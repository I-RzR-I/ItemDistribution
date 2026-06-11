// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-11 21:20
// ***********************************************************************
//  <copyright file="DistributionContext.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.Extensions.Domain.Primitives;
using RzR.ItemDistribution.Abstractions.Pipeline;
using RzR.ItemDistribution.Models.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

#endregion

namespace RzR.ItemDistribution.Engine.Pipeline
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Concrete distribution context built by the engine before executing the pipeline.
    ///     Implements <see cref="IDistributionContext{TUserId}" /> so that steps receive only the
    ///     narrowed, read-only-plus-explicit-mutation view and cannot add or remove dictionary keys.
    ///     The engine accesses the mutable result collections through the internal
    ///     <see cref="AllocationMap" /> and <see cref="TraceList" /> accessors.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// =================================================================================================
    public class DistributionContext<TUserId> : IDistributionContext<TUserId>
        where TUserId : struct
    {
        private readonly Dictionary<TUserId, int> _allocation;
        private readonly Dictionary<TUserId, decimal> _scores;
        private readonly Dictionary<TUserId, decimal> _effectiveCapacity;
        private readonly IList<DistributionTrace<TUserId>> _trace;
        private readonly ReadOnlyDictionary<TUserId, decimal> _scoresView;
        private readonly ReadOnlyDictionary<TUserId, decimal> _effectiveCapacityView;
        private readonly ReadOnlyDictionary<TUserId, int> _allocationView;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="DistributionContext{TUserId}" /> class.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="cancellationToken">
        ///     Optional token that pipeline steps can observe to abort long-running work.
        ///     Defaults to <see cref="CancellationToken.None" />.
        /// </param>
        /// =================================================================================================
        public DistributionContext(
            IReadOnlyList<UserLoad<TUserId>> users,
            int documents,
            CancellationToken cancellationToken = default)
        {
            var duplicateIds = users
                .GroupBy(u => u.UserId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key).ToList();

            if (duplicateIds.Count > 0)
                throw new ArgumentException(
                    $"Duplicate user IDs detected: {string.Join(", ", duplicateIds)}.", nameof(users));

            Users = users;
            Documents = documents;
            CancellationToken = cancellationToken;

            _allocation = users.ToDictionary(u => u.UserId, _ => 0);
            _scores = users.ToDictionary(u => u.UserId, _ => 0m);
            _effectiveCapacity = users.ToDictionary(u => u.UserId, u => u.Capacity);
            _trace = new List<DistributionTrace<TUserId>>();
            _scoresView = new ReadOnlyDictionary<TUserId, decimal>(_scores);
            _effectiveCapacityView = new ReadOnlyDictionary<TUserId, decimal>(_effectiveCapacity);
            _allocationView = new ReadOnlyDictionary<TUserId, int>(_allocation);
        }

        /// <inheritdoc />
        public IReadOnlyList<UserLoad<TUserId>> Users { get; }

        /// <inheritdoc />
        public int Documents { get; }

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<TUserId, decimal> Scores => _scoresView;

        /// <inheritdoc />
        public IReadOnlyDictionary<TUserId, decimal> EffectiveCapacity => _effectiveCapacityView;

        /// <inheritdoc />
        public IReadOnlyDictionary<TUserId, int> Allocation => _allocationView;

        /// <inheritdoc />
        public void SetScore(TUserId userId, decimal score)
        {
            if (!_scores.ContainsKey(userId))
                throw new InvalidOperationException(
                    $"User '{userId}' is not part of this distribution context.");

            _scores[userId] = score;
        }

        /// <inheritdoc />
        public void SetEffectiveCapacity(TUserId userId, decimal capacity)
        {
            if (!_effectiveCapacity.ContainsKey(userId))
                throw new InvalidOperationException(
                    $"User '{userId}' is not part of this distribution context.");

            _effectiveCapacity[userId] = capacity;
        }

        /// <inheritdoc />
        public void IncrementAllocation(TUserId userId)
        {
            if (!_allocation.ContainsKey(userId))
                throw new InvalidOperationException(
                    $"User '{userId}' is not part of this distribution context.");

            _allocation[userId]++;
        }

        /// <inheritdoc />
        public void AddTrace(DistributionTrace<TUserId> trace)
        {
            if (trace.IsNull())
                throw new ArgumentNullException(nameof(trace));

            _trace.Add(trace);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Internal accessor used by <see cref="RzR.ItemDistribution.Engine.DistributionEngine{TUserId}" />
        ///     to extract the mutable allocation map for the <c>TupleResult</c> return value.
        ///     Not part of the step-facing <see cref="IDistributionContext{TUserId}" /> surface.
        /// </summary>
        /// =================================================================================================
        internal IDictionary<TUserId, int> AllocationMap => _allocation;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Internal accessor used by <see cref="RzR.ItemDistribution.Engine.DistributionEngine{TUserId}" />
        ///     to extract the mutable trace list for the <c>TupleResult</c> return value.
        ///     Not part of the step-facing <see cref="IDistributionContext{TUserId}" /> surface.
        /// </summary>
        /// =================================================================================================
        internal IList<DistributionTrace<TUserId>> TraceList => _trace;
    }
}
