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

namespace RzR.ItemDistribution.Models.Config
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
        ///     When <see langword="true" />, the working-hours capacity step is explicitly enabled.
        ///     The step is also enabled implicitly when <see cref="MaxAllowedInProcessDocuments" /> &gt; 0,
        ///     so existing configurations that set <see cref="MaxAllowedInProcessDocuments" /> continue to
        ///     work without change.
        ///     When the feature is active (either by this flag or by <see cref="MaxAllowedInProcessDocuments" /> &gt; 0),
        ///     both <see cref="MaxAllowedInProcessDocuments" /> &gt; 0 and <see cref="FullWorkDayHours" /> &gt; 0
        ///     are required; otherwise an <see cref="System.ArgumentOutOfRangeException" /> is thrown during
        ///     construction.
        /// </summary>
        /// =================================================================================================
        public bool UseWorkingHoursCapacity { get; set; } = false;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Number of hours in a standard full working day (e.g. 8).
        ///     Required (must be &gt; 0) whenever the working-hours capacity step is active, which happens
        ///     when <see cref="UseWorkingHoursCapacity" /> is <see langword="true" /> or
        ///     <see cref="MaxAllowedInProcessDocuments" /> &gt; 0.
        /// </summary>
        /// =================================================================================================
        public decimal FullWorkDayHours { get; set; } = 8;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Maximum number of documents a full-day user can have in process at once (e.g. 20).
        ///     Required (must be &gt; 0) whenever the working-hours capacity step is active, which happens
        ///     when <see cref="UseWorkingHoursCapacity" /> is <see langword="true" /> or this value itself
        ///     is &gt; 0. When 0 (default) and <see cref="UseWorkingHoursCapacity" /> is
        ///     <see langword="false" />, the engine uses <see cref="Domain.UserLoad{TUserId}.Capacity" />
        ///     directly.
        /// </summary>
        /// =================================================================================================
        public int MaxAllowedInProcessDocuments { get; set; } = 0;
    }
}