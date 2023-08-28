// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 20:33
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 21:16
// ***********************************************************************
//  <copyright file="CalculateLoadCoefficientRequest.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ItemDistribution.Models.Internal.Requests
{
    /// <summary>
    ///     Calculation user load coefficient
    /// </summary>
    internal class CalculateLoadCoefficientRequest
    {
        /// <summary>
        ///     Number of active documents in processing/execution.
        /// </summary>
        internal int ActiveNrOfDocuments { get; set; }

        /// <summary>
        ///     Number of maximum allowed documents for processing
        /// </summary>
        internal int TotalNrOfDocuments { get; set; }

        /// <summary>
        ///     Current user working hours program
        /// </summary>
        internal decimal WorkHours { get; set; }

        /// <summary>
        ///     Normally working day program (hours)
        /// </summary>
        internal decimal NormallyDayWorkHours { get; set; }
    }
}