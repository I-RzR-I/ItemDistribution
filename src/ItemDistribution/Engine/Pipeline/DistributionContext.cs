// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="DistributionContext.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using ItemDistribution.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ItemDistribution.Engine.Pipeline
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A distribution context.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// =================================================================================================
    public class DistributionContext<TUserId>
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="DistributionContext{TUserId}" /> class.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="documents">The documents.</param>
        /// =================================================================================================
        public DistributionContext(IReadOnlyList<UserLoad<TUserId>> users, int documents)
        {
            var duplicateIds = users.GroupBy(u => u.UserId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateIds.Count > 0)
                throw new ArgumentException(
                    $"Duplicate user IDs detected: {string.Join(", ", duplicateIds)}.", nameof(users));

            Users = users;
            Documents = documents;
            Allocation = users.ToDictionary(u => u.UserId, _ => 0);
            Scores = users.ToDictionary(u => u.UserId, _ => 0m);
            Trace = new List<DistributionTrace<TUserId>>();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the users.
        /// </summary>
        /// <value>
        ///     The users.
        /// </value>
        /// =================================================================================================
        public IReadOnlyList<UserLoad<TUserId>> Users { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the documents.
        /// </summary>
        /// <value>
        ///     The documents.
        /// </value>
        /// =================================================================================================
        public int Documents { get; /* set; */ }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the allocation.
        /// </summary>
        /// <value>
        ///     The allocation.
        /// </value>
        /// =================================================================================================
        public IDictionary<TUserId, int> Allocation { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the scores.
        /// </summary>
        /// <value>
        ///     The scores.
        /// </value>
        /// =================================================================================================
        public IDictionary<TUserId, decimal> Scores { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the trace.
        /// </summary>
        /// <value>
        ///     The trace.
        /// </value>
        /// =================================================================================================
        public IList<DistributionTrace<TUserId>> Trace { get; }
    }
}