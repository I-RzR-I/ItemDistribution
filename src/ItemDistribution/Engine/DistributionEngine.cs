// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="DistributionEngine.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using DomainCommonExtensions.Models;
using ItemDistribution.Abstractions.Pipeline;
using ItemDistribution.Engine.Pipeline;
using ItemDistribution.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ItemDistribution.Engine
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A distribution engine. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// =================================================================================================
    public sealed class DistributionEngine<TUserId>
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the steps.
        /// </summary>
        /// =================================================================================================
        private readonly IEnumerable<IDistributionStep<TUserId>> _steps;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="DistributionEngine{TUserId}" /> class.
        /// </summary>
        /// <param name="steps">The steps.</param>
        /// =================================================================================================
        public DistributionEngine(IEnumerable<IDistributionStep<TUserId>> steps) => _steps = steps;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Synchronously executes all pipeline steps and returns the allocation result.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="documents">The documents.</param>
        /// <returns>
        ///     Allocation dictionary and audit trace.
        /// </returns>
        /// =================================================================================================
        public TupleResult<IDictionary<TUserId, int>, IList<DistributionTrace<TUserId>>> Execute(
            IEnumerable<UserLoad<TUserId>> users,
            int documents)
        {
            var context = new DistributionContext<TUserId>(users.ToList(), documents);

            foreach (var step in _steps)
            {
                var stepResult = step.Execute(context);
                if (!stepResult.IsSuccess)
                    throw new InvalidOperationException(
                        $"Distribution pipeline step '{step.GetType().Name}' failed.");
            }

            return TupleResult.Create(context.Allocation, context.Trace);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Asynchronously executes all pipeline steps on a thread-pool thread.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>
        ///     Allocation dictionary and audit trace.
        /// </returns>
        /// =================================================================================================
        public async Task<TupleResult<IDictionary<TUserId, int>, IList<DistributionTrace<TUserId>>>> ExecuteAsync(
            IEnumerable<UserLoad<TUserId>> users,
            int documents,
            CancellationToken cancellationToken = default)
            => await Task.Run(() => Execute(users, documents), cancellationToken);
    }
}