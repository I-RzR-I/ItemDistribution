// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 21:59
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 21:16
// ***********************************************************************
//  <copyright file="DistributionParams.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;

#endregion

namespace ItemDistribution.Models.Dto
{
    /// <summary>
    ///     Distribution suggestion parameters
    /// </summary>
    /// <typeparam name="TUserId">Type of user id</typeparam>
    public class DistributionParams<TUserId> where TUserId : struct
    {
        /// <summary>
        ///     Avoid or do not duplicate in result.
        ///     In case the value is 'true', then 'SuggestionUser' and 'AlternativeUser' must be different data.
        /// </summary>
        public bool AvoidDuplicateResult { get; set; } = false;

        /// <summary>
        ///     Gets or sets the maximum number of documents allowed at the same time in processing.
        /// </summary>
        /// <value></value>
        /// <remarks>Default value is set for maximum 20 documents/items per day.</remarks>
        public int MaxAllowedInProcessDocuments { get; set; } = 20;

        /// <summary>
        ///     Gets or sets number of hours in on a working day.
        /// </summary>
        /// <value></value>
        /// <remarks>Default value is set for 8 hours per day.</remarks>
        public decimal FullWorkDayHours { get; set; } = 8;

        /// <summary>
        ///     Gets or sets eligible users for a new selection.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public IEnumerable<UserInfoOptions<TUserId>> EligibleRepartitionUsers { get; set; } = new List<UserInfoOptions<TUserId>>();
    }
}