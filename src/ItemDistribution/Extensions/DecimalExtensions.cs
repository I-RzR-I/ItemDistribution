// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 18:31
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-21 18:32
// ***********************************************************************
//  <copyright file="DecimalExtensions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ItemDistribution.Extensions
{
    /// <summary>
    ///     Decimal extensions
    /// </summary>
    internal static class DecimalExtensions
    {
        /// <summary>
        ///     Check if user have full day working program
        /// </summary>
        /// <param name="userWorkingProgram">Current user working hours.</param>
        /// <param name="fullWorkingProgram">Full day working hours.</param>
        /// <returns>Return bool value, meaning if is full working day or not.</returns>
        /// <remarks></remarks>
        internal static bool IsFullWorkingDay(this decimal userWorkingProgram, decimal fullWorkingProgram) =>
            userWorkingProgram == fullWorkingProgram;
    }
}