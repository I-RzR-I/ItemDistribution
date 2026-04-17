// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="EngineConfig.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ItemDistribution.Models.Config
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An engine configuration.
    /// </summary>
    /// =================================================================================================
    public class EngineConfig
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the maximum items that can be allocated to a single user in one run.
        ///     Must be greater than 0 when set.
        /// </summary>
        /// =================================================================================================
        public int? MaxPerUser { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     When <see langword="true" />, a <see cref="Strategies.Scoring.PriorityActivityScoringStep{TUserId}" />
        ///     is inserted after load scoring to factor in user priority and last-activity date.
        /// </summary>
        /// =================================================================================================
        public bool UsePriorityAndActivity { get; set; } = false;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Number of hours in a standard full working day (e.g. 8).
        ///     Required together with <see cref="MaxAllowedInProcessDocuments" /> to enable
        ///     part-time capacity adjustment via <c>WorkingHoursCapacityStep</c>.
        /// </summary>
        /// =================================================================================================
        public decimal FullWorkDayHours { get; set; } = 8;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Maximum number of documents a full-day user can have in process at once (e.g. 20).
        ///     Required together with <see cref="FullWorkDayHours" /> to enable part-time capacity adjustment.
        ///     When 0 (default), the engine uses <see cref="Domain.UserLoad{TUserId}.Capacity" /> directly.
        /// </summary>
        /// =================================================================================================
        public int MaxAllowedInProcessDocuments { get; set; } = 0;
    }
}