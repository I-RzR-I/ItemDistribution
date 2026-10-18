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

using RzR.Extensions.Domain.Models;
using RzR.Extensions.Domain.Primitives;
using RzR.ItemDistribution.Abstractions.Pipeline;
using RzR.ItemDistribution.Engine.Pipeline;
using RzR.ItemDistribution.Models.Domain;
using RzR.ResultMessage.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.ItemDistribution.Engine
{
    /// <summary>
    ///     A distribution engine. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    public sealed class DistributionEngine<TUserId>
        where TUserId : struct
    {
        /// <summary>
        ///     (Immutable) the steps.
        /// </summary>
        private readonly IEnumerable<IDistributionStep<TUserId>> _steps;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DistributionEngine{TUserId}" /> class.
        /// </summary>
        /// <param name="steps">The steps.</param>
        public DistributionEngine(IEnumerable<IDistributionStep<TUserId>> steps) => _steps = steps;

        /// <summary>
        ///     Synchronously executes all pipeline steps and returns the allocation result.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="users">The users.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="cancellationToken">
        ///     (Optional) Optional cancellation token; checked between pipeline steps.
        /// </param>
        /// <returns>
        ///     Allocation dictionary and audit trace.
        /// </returns>
        public TupleResult<IDictionary<TUserId, int>, IList<DistributionTrace<TUserId>>> Execute(
            IEnumerable<UserLoad<TUserId>> users, int documents, CancellationToken cancellationToken = default)
        {
            var context = new DistributionContext<TUserId>(users.ToList(), documents, cancellationToken);

            foreach (var step in _steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stepResult = step.Execute(context);
                if (stepResult.IsSuccess.IsFalse())
                {
                    var stepMessage = stepResult.GetFirstMessage();
                    var exceptionInfo = stepResult.Messages
                        .FirstOrDefault(m => m.MessageType == MessageType.Exception);
                    var innerMessage = exceptionInfo?.Message?.Info;
                    var innerException = innerMessage.IsNull()
                        ? null
                        : new InvalidOperationException(innerMessage);

                    throw new InvalidOperationException(
                        $"Distribution pipeline step '{step.GetType().Name}' failed: {stepMessage}",
                        innerException);
                }
            }

            return TupleResult.Create(context.AllocationMap, context.TraceList);
        }

        /// <summary>
        ///     Asynchronously executes all pipeline steps on a thread-pool thread.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="cancellationToken">(Optional) Optional cancellation token.</param>
        /// <returns>
        ///     Allocation dictionary and audit trace.
        /// </returns>
        public async Task<TupleResult<IDictionary<TUserId, int>, IList<DistributionTrace<TUserId>>>> ExecuteAsync(
            IEnumerable<UserLoad<TUserId>> users, int documents, CancellationToken cancellationToken = default)
            => await Task.Run(() => Execute(users, documents, cancellationToken), cancellationToken);
    }
}