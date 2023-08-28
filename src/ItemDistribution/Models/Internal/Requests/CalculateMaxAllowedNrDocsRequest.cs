// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 20:33
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 21:16
// ***********************************************************************
//  <copyright file="CalculateMaxAllowedNrDocsRequest.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ItemDistribution.Models.Internal.Requests
{
    /// <summary>
    ///     Calculate maximum allowed number of documents request
    /// </summary>
    internal class CalculateMaxAllowedNrDocsRequest
    {
        /// <summary>
        ///     User working hours
        /// </summary>
        internal decimal UserWorkHours { get; set; }

        /// <summary>
        ///     Maximum allowed working hours per day
        /// </summary>
        internal decimal DayWorkHours { get; set; }

        /// <summary>
        ///     Maximum allowed number of documents per day
        /// </summary>
        internal int DayMaxAllowedDocs { get; set; }
    }
}