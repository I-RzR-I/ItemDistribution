// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-25 12:53
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-25 12:53
// ***********************************************************************
//  <copyright file="IntExtensions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ItemDistribution.Extensions
{
    /// <summary>
    ///     Integer extensions
    /// </summary>
    internal static class IntExtensions
    {
        /// <summary>
        ///     Check if value is equals with 0 or 1
        /// </summary>
        /// <param name="source">Source value to be checked</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal static bool IsZeroOne(this int source) => source == 0 || source == 1;
    }
}