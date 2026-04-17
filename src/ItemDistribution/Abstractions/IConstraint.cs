// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="IConstraint.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ItemDistribution.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Interface for constraint.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// =================================================================================================
    public interface IConstraint<in TUserId>
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Determine if we can allocate.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="currentAllocated">The current allocated.</param>
        /// <returns>
        ///     True if we can allocate, false if not.
        /// </returns>
        /// =================================================================================================
        bool CanAllocate(TUserId userId, int currentAllocated);
    }
}