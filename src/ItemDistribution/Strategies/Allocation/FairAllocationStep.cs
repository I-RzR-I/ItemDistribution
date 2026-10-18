// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-11 21:20
// ***********************************************************************
//  <copyright file="FairAllocationStep.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.Extensions.Domain.Primitives;
using RzR.ItemDistribution.Abstractions;
using RzR.ItemDistribution.Abstractions.Pipeline;
using RzR.ItemDistribution.Models.Domain;
using RzR.ResultMessage;
using RzR.ResultMessage.Abstractions;
using RzR.ResultMessage.Extensions.Result;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace RzR.ItemDistribution.Strategies.Allocation
{
    /// <summary>
    ///     A fair allocation step.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// <seealso cref="T:RzR.ItemDistribution.Abstractions.Pipeline.IDistributionStep{TUserId}">
    ///     =================================================================================================
    /// </seealso>
    public class FairAllocationStep<TUserId> : IDistributionStep<TUserId>
        where TUserId : struct
    {
        /// <summary>
        ///     (Immutable) the constraints.
        /// </summary>
        private readonly IEnumerable<IConstraint<TUserId>> _constraints;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FairAllocationStep{TUserId}" /> class.
        /// </summary>
        /// <param name="constraints">The constraints.</param>
        public FairAllocationStep(IEnumerable<IConstraint<TUserId>> constraints) => _constraints = constraints;

        /// <inheritdoc />
        public IResult Execute(IDistributionContext<TUserId> context)
        {
            try
            {
                ValidateContextDictionaries(context);

                var userIdComparer = Comparer<TUserId>.Default;

                for (var i = 0; i < context.Documents; i++)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    UserLoad<TUserId> best = null;
                    var bestKey = decimal.MinValue;

                    foreach (var u in context.Users)
                    {
                        if (context.EffectiveCapacity[u.UserId] <= 0)
                            continue;

                        if ((u.CurrentLoad + context.Allocation[u.UserId]) >= context.EffectiveCapacity[u.UserId])
                            continue;

                        if (!_constraints.All(c => c.CanAllocate(u.UserId, context.Allocation[u.UserId])))
                            continue;

                        var key = context.Scores[u.UserId]
                            * (context.EffectiveCapacity[u.UserId] - u.CurrentLoad - context.Allocation[u.UserId]);

                        if (best.IsNull()
                            || key > bestKey
                            || (key == bestKey && userIdComparer.Compare(u.UserId, best.UserId) < 0))
                        {
                            best = u;
                            bestKey = key;
                        }
                    }

                    if (best.IsNull())
                        break;

                    context.IncrementAllocation(best.UserId);
                }

                return Result.Success();
            }
            catch (Exception e)
            {
                return Result
                    .Failure(e.Message).WithError(e);
            }
        }

        /// <summary>
        ///     Validates that every user in <paramref name="context" /> has an entry in all three
        ///     context dictionaries (Scores, EffectiveCapacity, Allocation).
        ///     With the narrowed <see cref="IDistributionContext{TUserId}" /> API, custom steps can no
        ///     longer add or remove keys, so a desync is structurally prevented. This check is kept as
        ///     defence-in-depth — for example, to catch any future implementation of
        ///     <see cref="IDistributionContext{TUserId}" /> that does not enforce key-set integrity.
        /// </summary>
        /// <param name="context">The distribution context to validate.</param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a user ID is missing from any of the required context dictionaries.
        /// </exception>
        private static void ValidateContextDictionaries(IDistributionContext<TUserId> context)
        {
            foreach (var user in context.Users)
            {
                if (!context.Scores.ContainsKey(user.UserId))
                    throw new InvalidOperationException(
                        $"User '{user.UserId}' is present in context.Users but missing from context.Scores. " +
                        "A custom pipeline step may have modified the context dictionaries inconsistently.");

                if (!context.EffectiveCapacity.ContainsKey(user.UserId))
                    throw new InvalidOperationException(
                        $"User '{user.UserId}' is present in context.Users but missing from context.EffectiveCapacity. " +
                        "A custom pipeline step may have modified the context dictionaries inconsistently.");

                if (!context.Allocation.ContainsKey(user.UserId))
                    throw new InvalidOperationException(
                        $"User '{user.UserId}' is present in context.Users but missing from context.Allocation. " +
                        "A custom pipeline step may have modified the context dictionaries inconsistently.");
            }
        }
    }
}
