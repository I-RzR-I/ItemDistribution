// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-04-01 21:04
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:36
// ***********************************************************************
//  <copyright file="PriorityActivityScoringStep.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using AggregatedGenericResultMessage;
using AggregatedGenericResultMessage.Abstractions;
using AggregatedGenericResultMessage.Extensions.Result;
using ItemDistribution.Abstractions.Pipeline;
using ItemDistribution.Engine.Pipeline;
using ItemDistribution.Models.Domain;
using System;
using System.Linq;

#endregion

namespace ItemDistribution.Strategies.Scoring
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Scoring step that adjusts each user's base load-score by their priority and
    ///     how recently they were active.
    ///     Adjustment formula (applied multiplicatively after <see cref="LoadScoringStep{TUserId}" />):
    ///     <list type="bullet">
    ///         <item>
    ///             Priority score  — (maxPriority − userPriority + 1) / (maxPriority + 1)
    ///             -> closer to 0 = higher priority = higher multiplier.
    ///         </item>
    ///         <item>
    ///             Activity score — (maxAgeDays − userAgeDays) / maxAgeDays
    ///             -> more recent user gets closer to 1.
    ///         </item>
    ///         <item>Combined — baseScore × (priorityScore + activityScore) / 2</item>
    ///     </list>
    ///     When all users have the same priority / activity the multiplier is neutral (0.5)
    ///     and does not change ranking.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// <seealso cref="T:ItemDistribution.Abstractions.Pipeline.IDistributionStep{TUserId}" />
    /// =================================================================================================
    public class PriorityActivityScoringStep<TUserId> : IDistributionStep<TUserId>
    {
        /// <inheritdoc />
        public IResult Execute(DistributionContext<TUserId> context)
        {
            if (context.Users.Count == 0)
                return Result.Success();

            try
            {
                var now = DateTime.UtcNow;

                var maxPriority = context.Users.Max(u => u.Priority);
                var maxAgeDays = context.Users.Max(u => (now - u.LastActivityDate).TotalDays);

                foreach (var user in context.Users)
                {
                    // Priority multiplier: lower priority number, higher multiplier (range (0, 1])
                    var priorityScore = maxPriority == 0
                        ? 1m
                        : (decimal)(maxPriority - user.Priority + 1) / (maxPriority + 1);

                    // Activity multiplier: more recent, higher (range [0, 1])
                    var ageDays = (now - user.LastActivityDate).TotalDays;
                    var activityScore = maxAgeDays <= 0
                        ? 1m
                        : (decimal)(maxAgeDays - ageDays) / (decimal)maxAgeDays;

                    var adjustment = (priorityScore + activityScore) / 2m;
                    var adjustedScore = context.Scores[user.UserId] * adjustment;

                    context.Scores[user.UserId] = adjustedScore;

                    context.Trace.Add(new DistributionTrace<TUserId>()
                    {
                        UserId = user.UserId,
                        Score = adjustedScore,
                        Reason = $"Priority/activity adjusted: priority={user.Priority}, lastActive={ageDays:F1}d ago, multiplier={adjustment:F4}"
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