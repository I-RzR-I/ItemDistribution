// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 20:33
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 21:16
// ***********************************************************************
//  <copyright file="PrepareUserInfoForSelectionRequest.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using ItemDistribution.Models.Dto;

#endregion

namespace ItemDistribution.Models.Internal.Requests
{
    /// <summary>
    ///     Prepare user information for selection
    /// </summary>
    /// <typeparam name="TUserId">Type of user id</typeparam>
    /// <remarks></remarks>
    internal class PrepareUserInfoForSelectionRequest<TUserId>
    {
        /// <summary>
        ///     Maximum allowed number of document at the same time in processing
        /// </summary>
        internal int MaxAllowedInProcessDocuments { get; set; }

        /// <summary>
        ///     Number of hours for full working day
        /// </summary>
        internal decimal FullWorkDayHours { get; set; }

        /// <summary>
        ///     Eligible users for current selection
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        internal IEnumerable<UserInfoOptions<TUserId>> EligibleRepartitionUsers { get; set; }
    }
}