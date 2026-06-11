// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-04-01 21:04
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-11 21:20
// ***********************************************************************
//  <copyright file="PriorityActivityScoringStep.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.ItemDistribution.Abstractions.Pipeline;
using RzR.ItemDistribution.Models.Domain;
using RzR.ResultMessage;
using RzR.ResultMessage.Abstractions;
using RzR.ResultMessage.Extensions.Result;
using System;
using System.Linq;

#endregion

namespace RzR.ItemDistribution.Strategies.Scoring
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Scoring step that adjusts each user's base load-score by their priority and
    ///     how recently they were active.
    ///     Adjustment formula (applied multiplicatively after <see cref="LoadScoringStep{TUserId}" />):
    ///     <list type="bullet">
    ///         <item>
    ///             Priority score — range = maxPriority − minPriority;
    ///             range == 0 ? 1 : (maxPriority − userPriority + 1) / (range + 1)
    ///             -> closer to minPriority = higher priority = higher multiplier.
    ///             Result is always in (0, 1]. Denominator is always >= 1 regardless of sign.
    ///         </item>
    ///         <item>
    ///             Activity score — (maxAgeDays − clampedAgeDays) / maxAgeDays
    ///             -> more recent user gets closer to 1.
    ///             ageDays is clamped to [0, maxAgeDays] before use so that a
    ///             future-dated <c>LastActivityDate</c> (e.g. local vs UTC mismatch)
    ///             cannot produce a negative age, which would push activityScore
    ///             above 1 and violate the documented [0, 1] range.
    ///         </item>
    ///         <item>Combined — baseScore × (priorityScore + activityScore) / 2</item>
    ///     </list>
    ///     When all users have the same priority / activity the multiplier is neutral (0.5)
    ///     and does not change ranking.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// <seealso cref="T:RzR.ItemDistribution.Abstractions.Pipeline.IDistributionStep{TUserId}" />
    /// =================================================================================================
    public class PriorityActivityScoringStep<TUserId> : IDistributionStep<TUserId>
        where TUserId : struct
    {
        /// <summary>
        ///     Clock delegate used to retrieve the current UTC time.
        ///     Defaults to <see cref="DateTime.UtcNow" />.
        ///     Provided as an internal seam so that unit tests can inject a deterministic time
        ///     without changing the public constructor signature.
        /// </summary>
        internal Func<DateTime> Clock { get; set; } = () => DateTime.UtcNow;

        /// <inheritdoc />
        public IResult Execute(IDistributionContext<TUserId> context)
        {
            if (context.Users.Count == 0)
                return Result.Success();

            try
            {
                var now = Clock();

                var minPriority = context.Users.Min(u => u.Priority);
                var maxPriority = context.Users.Max(u => u.Priority);
                var priorityRange = maxPriority - minPriority;
                var maxAgeDays = context.Users.Max(u => (now - u.LastActivityDate).TotalDays);

                foreach (var user in context.Users)
                {
                    // Priority multiplier: lower priority number, higher multiplier (range (0, 1]).
                    // Normalised against the full range so the denominator is always >= 1
                    // and result stays in (0, 1] for any mix of positive, zero, or negative priorities.
                    var priorityScore = priorityRange == 0
                        ? 1m
                        : (decimal)(maxPriority - user.Priority + 1) / (priorityRange + 1);

                    // Activity multiplier: more recent, higher (range [0, 1]).
                    // Clamp ageDays to [0, maxAgeDays] so a future-dated LastActivityDate
                    // (e.g. caller uses local time while clock uses UtcNow) cannot make
                    // ageDays negative and push activityScore above 1.
                    var ageDays = (now - user.LastActivityDate).TotalDays;
                    var clampedAge = Math.Min(Math.Max(ageDays, 0), maxAgeDays);
                    var activityScore = maxAgeDays <= 0
                        ? 1m
                        : (decimal)(maxAgeDays - clampedAge) / (decimal)maxAgeDays;

                    var adjustment = (priorityScore + activityScore) / 2m;
                    var adjustedScore = context.Scores[user.UserId] * adjustment;

                    context.SetScore(user.UserId, adjustedScore);

                    context.AddTrace(new DistributionTrace<TUserId>()
                    {
                        UserId = user.UserId,
                        Score = adjustedScore,
                        Reason = $"Priority/activity adjusted: priority={user.Priority}, " +
                                 $"lastActive={ageDays:F1}d ago (clamped={clampedAge:F1}d), multiplier={adjustment:F4}"
                    });
                }

                return Result.Success();
            }
            catch (Exception e)
            {
                return Result
                    .Failure(e.Message).WithError(e);
            }
        }
    }
}