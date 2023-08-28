// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-23 16:57
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 21:16
// ***********************************************************************
//  <copyright file="DistributionSuggestionResultDto.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ItemDistribution.Models.Dto.Result
{
    /// <summary>
    ///     New item distribution suggestion
    /// </summary>
    /// <typeparam name="TUserId">Type of user id</typeparam>
    public class DistributionSuggestionResultDto<TUserId>
    {
        /// <summary>
        ///     Suggested user
        /// </summary>
        public DistributionSuggestionDto<TUserId> SuggestionUser { get; set; }

        /// <summary>
        ///     Alternative user suggested (random user)
        /// </summary>
        public DistributionSuggestionDto<TUserId> AlternativeUser { get; set; }

        /// <inheritdoc />
        public override string ToString()
            => $"At the current iteration user suggestion is: '{SuggestionUser}'; alternative user suggestion is: '{AlternativeUser}'";
    }
}