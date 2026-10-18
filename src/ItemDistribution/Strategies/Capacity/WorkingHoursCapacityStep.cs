// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-04-01 21:04
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-11 21:20
// ***********************************************************************
//  <copyright file="WorkingHoursCapacityStep.cs" company="RzR SOFT & TECH">
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

#endregion

namespace RzR.ItemDistribution.Strategies.Capacity
{
    /// <summary>
    ///     Pipeline step that recalculates the load score for part-time users. For each user whose <c>
    ///     UserLoad.WorkingHours</c> is greater than zero, the step computes an effective capacity
    ///     proportional to their working hours: effectiveCapacity = Truncate( (WorkingHours ×
    ///     MaxAllowedDocs) / FullWorkDayHours )
    ///     and then overwrites the score that <see cref="Scoring.LoadScoringStep{TUserId}" /> wrote
    ///     with: adjustedScore = (effectiveCapacity − CurrentLoad) / effectiveCapacity Users whose <c>
    ///     WorkingHours</c> is 0 ("not set") are skipped — their score from <see cref="Scoring.LoadScoringStep{TUserId}" />
    ///     is left unchanged. This step must be placed AFTER <see cref="Scoring.LoadScoringStep{TUserId}" />
    ///     in the pipeline.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// <seealso cref="T:RzR.ItemDistribution.Abstractions.Pipeline.IDistributionStep{TUserId}">
    ///     =================================================================================================
    /// </seealso>
    public class WorkingHoursCapacityStep<TUserId> : IDistributionStep<TUserId>
        where TUserId : struct
    {
        private readonly decimal _fullWorkDayHours;
        private readonly int _maxAllowedDocs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WorkingHoursCapacityStep{TUserId}" /> class.
        /// </summary>
        /// <param name="fullWorkDayHours">Hours in a standard full working day (e.g. 8).</param>
        /// <param name="maxAllowedDocs">Max documents a full-day user can process (e.g. 20).</param>
        public WorkingHoursCapacityStep(decimal fullWorkDayHours, int maxAllowedDocs)
        {
            _fullWorkDayHours = fullWorkDayHours;
            _maxAllowedDocs = maxAllowedDocs;
        }

        /// <inheritdoc />
        public IResult Execute(IDistributionContext<TUserId> context)
        {
            try
            {
                foreach (var user in context.Users)
                {
                    // Skip users where WorkingHours is not set — Capacity is used as-is
                    if (user.WorkingHours <= 0)
                        continue;

                    var effectiveCapacity = decimal.Truncate(
                        user.WorkingHours * _maxAllowedDocs / _fullWorkDayHours);

                    var adjustedScore = effectiveCapacity == 0
                        ? 0m
                        : (effectiveCapacity - user.CurrentLoad) / effectiveCapacity;

                    context.SetEffectiveCapacity(user.UserId, effectiveCapacity);
                    context.SetScore(user.UserId, adjustedScore);

                    context.AddTrace(new DistributionTrace<TUserId>()
                    {
                        UserId = user.UserId,
                        Score = adjustedScore,
                        Reason = $"WorkingHours adjusted: hours={user.WorkingHours}, " +
                                 $"effectiveCapacity={effectiveCapacity}, score={adjustedScore:F4}"
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
