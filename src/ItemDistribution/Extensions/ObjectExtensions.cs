// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 21:31
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-21 21:34
// ***********************************************************************
//  <copyright file="ObjectExtensions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using CodeSource;

#endregion

namespace ItemDistribution.Extensions
{
    /// <summary>
    ///     Object extensions
    /// </summary>
    internal static class ObjectExtensions
    {
        /// <summary>
        ///     Is null
        /// </summary>
        /// <param name="obj">Source data</param>
        /// <returns></returns>
        [CodeSource("https://github.com/I-RzR-I/DomainCommonExtensions", "RzR",
            "DomainCommonExtensions.CommonExtensions.NullExtensions.IsNull", 1)]
        internal static bool IsNull(this object obj) => obj == null;
    }
}