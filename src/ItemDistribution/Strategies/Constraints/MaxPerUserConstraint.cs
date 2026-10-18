// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:36
// ***********************************************************************
//  <copyright file="MaxPerUserConstraint.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.ItemDistribution.Abstractions;

#endregion

namespace RzR.ItemDistribution.Strategies.Constraints
{
    /// <summary>
    ///     A maximum per user constraint.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// <seealso cref="T:RzR.ItemDistribution.Abstractions.IConstraint{TUserId}">
    ///     =================================================================================================
    /// </seealso>
    public class MaxPerUserConstraint<TUserId> : IConstraint<TUserId>
    {
        /// <summary>
        ///     (Immutable) the maximum.
        /// </summary>
        private readonly int _max;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MaxPerUserConstraint{TUserId}" /> class.
        /// </summary>
        /// <param name="max">The maximum.</param>
        public MaxPerUserConstraint(int max) => _max = max;

        /// <inheritdoc />
        public bool CanAllocate(TUserId userId, int currentAllocated) => currentAllocated < _max;
    }
}