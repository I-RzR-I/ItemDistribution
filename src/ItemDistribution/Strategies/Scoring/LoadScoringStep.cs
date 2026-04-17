// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:36
// ***********************************************************************
//  <copyright file="LoadScoringStep.cs" company="RzR SOFT & TECH">
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

#endregion

namespace ItemDistribution.Strategies.Scoring
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A load scoring step.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// <seealso cref="T:ItemDistribution.Abstractions.Pipeline.IDistributionStep{TUserId}" />
    /// =================================================================================================
    public class LoadScoringStep<TUserId> : IDistributionStep<TUserId>
    {
        /// <inheritdoc />
        public IResult Execute(DistributionContext<TUserId> context)
        {
            try
            {
                foreach (var user in context.Users)
                {
                    var score = user.Capacity == 0 ? 0 : (user.Capacity - user.CurrentLoad) / user.Capacity;

                    context.Scores[user.UserId] = score;

                    context.Trace.Add(new DistributionTrace<TUserId>()
                    {
                        UserId = user.UserId, 
                        Score = score,
                        Reason = "Load-based scoring"
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