// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 20:32
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 21:16
// ***********************************************************************
//  <copyright file="PrepareUserDataSelectionDto.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using ItemDistribution.Models.Dto;

#endregion

namespace ItemDistribution.Models.Internal.Dto
{
    /// <summary>
    ///     Prepare user data for distribution selection
    /// </summary>
    /// <typeparam name="TUserId"></typeparam>
    internal class PrepareUserDataSelectionDto<TUserId> : UserInfoOptions<TUserId>
    {
        /// <summary>
        ///     User load coefficient
        /// </summary>
        internal decimal LoadCoefficient { get; set; }
    }
}