// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-11 21:20
// ***********************************************************************
//  <copyright file="IDistributionStep.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.ResultMessage.Abstractions;

#endregion

namespace RzR.ItemDistribution.Abstractions.Pipeline
{
    /// <summary>
    ///     Interface for distribution step.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier.</typeparam>
    public interface IDistributionStep<TUserId>
        where TUserId : struct
    {
        /// <summary>
        ///     Executes the step against the provided distribution context. Steps must read state via
        ///     the read-only properties of <see cref="IDistributionContext{TUserId}" />
        ///     and write state exclusively through its mutation methods (<c>SetScore</c>, <c>
        ///     SetEffectiveCapacity</c>, <c>IncrementAllocation</c>, <c>AddTrace</c>).
        /// </summary>
        /// <param name="context">
        ///     The narrowed distribution context. Only users already present at construction time may be
        ///     targeted; attempting to address an unknown user will throw
        ///     <see cref="System.InvalidOperationException" /> which the engine surfaces as a failure
        ///     result.
        /// </param>
        /// <returns>
        ///     <see cref="IResult" /> indicating success or a descriptive failure.
        /// </returns>
        IResult Execute(IDistributionContext<TUserId> context);
    }
}
