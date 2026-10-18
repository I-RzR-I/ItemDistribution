// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-06-11 21:20
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-11 21:20
// ***********************************************************************
//  <copyright file="IDistributionContext.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.ItemDistribution.Models.Domain;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace RzR.ItemDistribution.Abstractions.Pipeline
{
    /// <summary>
    ///     Read-only-plus-explicit-mutation view of a distribution context exposed to pipeline
    ///     steps. Steps read state through the properties and mutate it exclusively via the named
    ///     mutation methods. This contract prevents a custom step from adding or removing keys from
    ///     any internal dictionary or from desyncing the key sets, which was possible when steps
    ///     received the raw <c>DistributionContext</c> directly.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier. Must be a value type.</typeparam>
    public interface IDistributionContext<TUserId>
        where TUserId : struct
    {
        /// <summary>
        ///     Gets the read-only list of users participating in this distribution run.
        /// </summary>
        /// <value>
        ///     Ordered list of user-load snapshots passed to the engine for this run.
        /// </value>
        IReadOnlyList<UserLoad<TUserId>> Users { get; }

        /// <summary>
        ///     Gets the total number of documents to distribute across users.
        /// </summary>
        /// <value>
        ///     Non-negative integer representing how many items must be allocated.
        /// </value>
        int Documents { get; }

        /// <summary>
        ///     Gets the cancellation token that pipeline steps may observe to abort long-running work.
        ///     Defaults to <see cref="CancellationToken.None" /> when not provided to the engine.
        /// 
        /// </summary>
        /// <value>
        ///     The cancellation token for this pipeline run.
        /// </value>
        CancellationToken CancellationToken { get; }

        /// <summary>
        ///     Gets a read-only view of the current per-user scores. All keys are initialised at context
        ///     construction time from <see cref="Users" />;
        ///     the key set is fixed for the lifetime of the context. Use <see cref="SetScore" /> to
        ///     update a value.
        /// </summary>
        /// <value>
        ///     Read-only dictionary mapping user ID to the user's current score (in the range [0, 1]).
        /// </value>
        IReadOnlyDictionary<TUserId, decimal> Scores { get; }

        /// <summary>
        ///     Gets a read-only view of the current per-user effective capacities. All keys are
        ///     initialised at context construction time from <see cref="Users" /> using each user's raw <c>
        ///     Capacity</c>. Use <see cref="SetEffectiveCapacity" /> to update a value.
        /// </summary>
        /// <value>
        ///     Read-only dictionary mapping user ID to effective capacity (documents the user can still
        ///     receive).
        /// </value>
        IReadOnlyDictionary<TUserId, decimal> EffectiveCapacity { get; }

        /// <summary>
        ///     Gets a read-only view of the current per-user allocation counts. All keys are initialised
        ///     to zero at context construction time from <see cref="Users" />. Use <see cref="IncrementAllocation" />
        ///     to increment a value.
        /// </summary>
        /// <value>
        ///     Read-only dictionary mapping user ID to the number of items allocated to the user so far.
        /// </value>
        IReadOnlyDictionary<TUserId, int> Allocation { get; }

        /// <summary>
        ///     Sets the score for an existing user. The user must already be present in <see cref="Users" />
        ///     ; the key set is fixed after context construction. Calling this method with an unknown <paramref name="userId" />
        ///     throws <see cref="System.InvalidOperationException" />.
        /// </summary>
        ///
        /// <exception cref="System.InvalidOperationException">
        ///     Thrown when <paramref name="userId" /> is not part of this distribution context.
        /// </exception>
        /// <param name="userId">
        ///     The identifier of the user whose score is to be updated. Must be a key present in <see cref="Scores" />
        ///     .
        /// </param>
        /// <param name="score">The new score value to assign.</param>
        void SetScore(TUserId userId, decimal score);

        /// <summary>
        ///     Sets the effective capacity for an existing user. The user must already be present in <see cref="Users" />
        ///     ; the key set is fixed after context construction. Calling this method with an unknown <paramref name="userId" />
        ///     throws <see cref="System.InvalidOperationException" />.
        /// </summary>
        ///
        /// <exception cref="System.InvalidOperationException">
        ///     Thrown when <paramref name="userId" /> is not part of this distribution context.
        /// </exception>
        /// <param name="userId">
        ///     The identifier of the user whose effective capacity is to be updated. Must be a key
        ///     present in <see cref="EffectiveCapacity" />.
        /// </param>
        /// <param name="capacity">The new effective capacity value to assign.</param>
        void SetEffectiveCapacity(TUserId userId, decimal capacity);

        /// <summary>
        ///     Increments the allocation counter for an existing user by one. Intended for use by the
        ///     terminal allocation step; each call represents one document assigned to <paramref name="userId" />
        ///     . The user must already be present in <see cref="Users" />; calling with an unknown
        ///     <paramref name="userId" /> throws <see cref="System.InvalidOperationException" />.
        /// 
        /// </summary>
        ///
        /// <exception cref="System.InvalidOperationException">
        ///     Thrown when <paramref name="userId" /> is not part of this distribution context.
        /// </exception>
        /// <param name="userId">
        ///     The identifier of the user whose allocation count is to be incremented. Must be a key
        ///     present in <see cref="Allocation" />.
        /// </param>
        void IncrementAllocation(TUserId userId);

        /// <summary>
        ///     Appends a trace entry to the audit log for this distribution run.
        /// </summary>
        ///
        /// <exception cref="System.ArgumentNullException">
        ///     Thrown when <paramref name="trace" /> is <see langword="null" />.
        /// </exception>
        /// <param name="trace">The trace entry to append. Must not be <see langword="null" />.</param>
        void AddTrace(DistributionTrace<TUserId> trace);
    }
}
