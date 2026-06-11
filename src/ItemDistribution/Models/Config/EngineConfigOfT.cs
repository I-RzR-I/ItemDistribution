// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-06-11 21:20
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-11 21:20
// ***********************************************************************
//  <copyright file="EngineConfigOfT.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.ItemDistribution.Abstractions;
using RzR.ItemDistribution.Abstractions.Pipeline;
using System.Collections.Generic;

#endregion

namespace RzR.ItemDistribution.Models.Config
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Engine configuration with support for custom pipeline steps and constraints.
    ///     Extends <see cref="EngineConfig" /> with consumer-supplied extensibility points.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier (must be a value type).</typeparam>
    /// <remarks>
    ///     <b>Pipeline ordering contract:</b>
    ///     Custom steps added via <see cref="AdditionalSteps" /> are inserted after the built-in
    ///     load-scoring and capacity steps but <em>before</em> the terminal
    ///     <c>FairAllocationStep</c>, which always runs last. Custom constraints from
    ///     <see cref="AdditionalConstraints" /> are merged with the built-in
    ///     <c>MaxPerUserConstraint</c> (when <see cref="EngineConfig.MaxPerUser" /> is set) and
    ///     passed to <c>FairAllocationStep</c> together.
    ///     When neither collection is populated the pipeline behaves identically to the
    ///     non-generic <see cref="EngineConfig" />.
    /// </remarks>
    /// =================================================================================================
    public class EngineConfig<TUserId> : EngineConfig
        where TUserId : struct
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Optional custom pipeline steps to be inserted after the built-in scoring/capacity
        ///     steps and before the terminal <c>FairAllocationStep</c>.
        ///     When <see langword="null" /> or empty, no extra steps are inserted.
        /// </summary>
        /// =================================================================================================
        public IReadOnlyList<IDistributionStep<TUserId>> AdditionalSteps { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Optional custom allocation constraints to be merged with the built-in
        ///     <c>MaxPerUserConstraint</c> (when <see cref="EngineConfig.MaxPerUser" /> is set)
        ///     and applied by <c>FairAllocationStep</c>.
        ///     When <see langword="null" /> or empty, no extra constraints are applied.
        /// </summary>
        /// =================================================================================================
        public IReadOnlyList<IConstraint<TUserId>> AdditionalConstraints { get; set; }
    }
}
