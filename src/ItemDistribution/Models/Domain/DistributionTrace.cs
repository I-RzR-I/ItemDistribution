// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-07 20:21
// ***********************************************************************
//  <copyright file="DistributionTrace.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace RzR.ItemDistribution.Models.Domain
{
    /// <summary>
    ///     A single audit-trail entry written by a pipeline step during distribution. The full
    ///     ordered list is exposed via
    ///     <see cref="DistributionResult{TUserId}.Trace" />.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    public sealed class DistributionTrace<TUserId>
    {
        /// <summary>
        ///     The user this trace entry relates to.
        /// </summary>
        /// <value>
        ///     The identifier of the user.
        /// </value>
        public TUserId UserId { get; set; }

        /// <summary>
        ///     The computed score for the user at this pipeline step.
        /// </summary>
        /// <value>
        ///     The score.
        /// </value>
        public decimal Score { get; set; }

        /// <summary>
        ///     Human-readable explanation of how the score was derived.
        /// </summary>
        /// <value>
        ///     The reason.
        /// </value>
        public string Reason { get; set; }
    }
}