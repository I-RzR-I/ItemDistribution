// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-11 21:20
// ***********************************************************************
//  <copyright file="LoadScoringStep.cs" company="RzR SOFT & TECH">
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

namespace RzR.ItemDistribution.Strategies.Scoring
{
    /// <summary>
    ///     A load scoring step.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// <seealso cref="T:RzR.ItemDistribution.Abstractions.Pipeline.IDistributionStep{TUserId}">
    ///     =================================================================================================
    /// </seealso>
    public class LoadScoringStep<TUserId> : IDistributionStep<TUserId>
        where TUserId : struct
    {
        /// <inheritdoc />
        public IResult Execute(IDistributionContext<TUserId> context)
        {
            try
            {
                foreach (var user in context.Users)
                {
                    var score = user.Capacity == 0 
                        ? 0
                        : (user.Capacity - user.CurrentLoad) / user.Capacity;

                    context.SetScore(user.UserId, score);

                    context.AddTrace(new DistributionTrace<TUserId>()
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
