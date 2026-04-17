// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-04-01 22:04
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="DistributionResult.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using System.Linq;

#endregion

namespace ItemDistribution.Models.Domain
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     The result produced by a single run of the distribution engine.
    ///     <para>
    ///         <b>Quick-start guide for callers:</b>
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>
    ///                     <see cref="PrimaryUserId" />
    ///                 </term>
    ///                 <description>The user to assign the next work item to — highest-ranked by the engine.</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <see cref="AlternativeUserId" />
    ///                 </term>
    ///                 <description>Fallback user when the primary is unavailable (second-highest rank).</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <see cref="Allocation" />
    ///                 </term>
    ///                 <description>Full map of every user -> how many items the engine assigned to them.</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <see cref="TotalAllocated" />
    ///                 </term>
    ///                 <description>Total items distributed across all users this run.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier (must be a value type).</typeparam>
    /// =================================================================================================
    public sealed class DistributionResult<TUserId>
        where TUserId : struct
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="DistributionResult{TUserId}" /> class.
        /// </summary>
        /// <param name="allocation">Allocation map from the engine.</param>
        /// <param name="trace">Audit trace from the engine.</param>
        /// =================================================================================================
        internal DistributionResult(
            IDictionary<TUserId, int> allocation,
            IList<DistributionTrace<TUserId>> trace)
        {
            Allocation = allocation;
            Trace = trace as IReadOnlyList<DistributionTrace<TUserId>>
                    ?? ((List<DistributionTrace<TUserId>>)trace).AsReadOnly();
            TotalAllocated = allocation.Values.Sum();

            var ranked = allocation
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .ToList();

            PrimaryUserId = ranked.Count > 0 ? ranked[0].Key : null;
            AlternativeUserId = ranked.Count > 1 ? ranked[1].Key : null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The user ranked first by the engine — the primary recommendation for the next assignment.
        ///     <c>null</c> when no items could be allocated (e.g. all users are at full capacity,
        ///     or <c>documents = 0</c>).
        /// </summary>
        /// =================================================================================================
        public TUserId? PrimaryUserId { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The user ranked second by the engine — a fallback when
        ///     <see cref="PrimaryUserId" /> is unavailable.
        ///     <c>null</c> when fewer than two users received at least one item.
        /// </summary>
        /// =================================================================================================
        public TUserId? AlternativeUserId { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Full allocation map produced by the engine.
        ///     Every input user has an entry; the value is the number of items assigned to that user.
        ///     A value of <c>0</c> means the user received nothing in this run.
        /// </summary>
        /// =================================================================================================
        public IDictionary<TUserId, int> Allocation { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The sum of all values in <see cref="Allocation" />.
        ///     Always ≤ the <c>documents</c> count passed to the engine
        ///     (maybe less when all users are at capacity before all items are placed).
        /// </summary>
        /// =================================================================================================
        public int TotalAllocated { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Audit trace written by each pipeline step.
        ///     Useful for diagnostics and understanding why a particular score was assigned.
        /// </summary>
        /// =================================================================================================
        public IReadOnlyList<DistributionTrace<TUserId>> Trace { get; }
    }
}