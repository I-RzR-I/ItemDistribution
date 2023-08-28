// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-23 16:55
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 21:16
// ***********************************************************************
//  <copyright file="DistributionSuggestionDto.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ItemDistribution.Models.Dto.Result
{
    /// <summary>
    ///     Distribution suggestion
    /// </summary>
    public class DistributionSuggestionDto<TUserId>
    {
        /// <summary>
        ///     Suggested user id
        /// </summary>
        public TUserId SuggestionUserId { get; set; }

        /// <summary>
        ///     Suggestion User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Suggestion user in process number of items
        /// </summary>
        public int InProcessItems { get; set; }

        /// <inheritdoc />
        public override string ToString()
            => $"Suggestion for current iteration is user '{UserName}' with id '{SuggestionUserId}' and '{InProcessItems}' in process items.";
    }
}