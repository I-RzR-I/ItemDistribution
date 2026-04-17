// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:36
// ***********************************************************************
//  <copyright file="FairAllocationStep.cs" company="RzR SOFT & TECH">
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
using ItemDistribution.Abstractions;
using ItemDistribution.Abstractions.Pipeline;
using ItemDistribution.Engine.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ItemDistribution.Strategies.Allocation
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A fair allocation step.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// <seealso cref="T:ItemDistribution.Abstractions.Pipeline.IDistributionStep{TUserId}" />
    /// =================================================================================================
    public class FairAllocationStep<TUserId> : IDistributionStep<TUserId>
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the constraints.
        /// </summary>
        /// =================================================================================================
        private readonly IEnumerable<IConstraint<TUserId>> _constraints;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="FairAllocationStep{TUserId}" /> class.
        /// </summary>
        /// <param name="constraints">The constraints.</param>
        /// =================================================================================================
        public FairAllocationStep(IEnumerable<IConstraint<TUserId>> constraints) => _constraints = constraints;

        /// <inheritdoc />
        public IResult Execute(DistributionContext<TUserId> context)
        {
            try
            {
                for (var i = 0; i < context.Documents; i++)
                {
                    var eligible = context.Users
                        .Where(u => u.Capacity > 0
                                    && (u.CurrentLoad + context.Allocation[u.UserId]) < u.Capacity
                                    && _constraints.All(c => c.CanAllocate(u.UserId, context.Allocation[u.UserId])));

                    var best = eligible
                        .OrderByDescending(u => context.Scores[u.UserId] - context.Allocation[u.UserId])
                        .ThenBy(u => u.UserId) // deterministic tie-breaking
                        .FirstOrDefault();

                    if (best == null)
                        break;

                    context.Allocation[best.UserId]++;
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