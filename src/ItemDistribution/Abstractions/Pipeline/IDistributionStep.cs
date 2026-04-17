// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="IDistributionStep.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using AggregatedGenericResultMessage.Abstractions;
using ItemDistribution.Engine.Pipeline;

#endregion

namespace ItemDistribution.Abstractions.Pipeline
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Interface for distribution step.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    /// =================================================================================================
    public interface IDistributionStep<TUserId>
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Executes the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// =================================================================================================
        IResult Execute(DistributionContext<TUserId> context);
    }
}