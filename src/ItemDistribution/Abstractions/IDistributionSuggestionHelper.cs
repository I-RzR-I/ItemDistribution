// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-04-01 21:04
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="IDistributionSuggestionHelper.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.ItemDistribution.Models.Domain;
using RzR.ResultMessage.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.ItemDistribution.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Public contract for the distribution suggestion engine.
    ///     Implement this interface to swap allocation strategies or mock in tests.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier (must be a value type).</typeparam>
    /// =================================================================================================
    public interface IDistributionSuggestionHelper<TUserId>
        where TUserId : struct
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Synchronously distributes <paramref name="documents" /> items across <paramref name="users" />.
        /// </summary>
        /// <param name="users">Eligible users with their current load and capacity.</param>
        /// <param name="documents">Total number of items to distribute.</param>
        /// <returns>
        ///     A <see cref="DistributionResult{TUserId}" /> containing the full allocation map,
        ///     the primary and alternative user recommendations, and an audit trace;
        ///     or a failure result with an error message.
        /// </returns>
        /// =================================================================================================
        IResult<DistributionResult<TUserId>> Generate(
            IEnumerable<UserLoad<TUserId>> users,
            int documents);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Asynchronously distributes <paramref name="documents" /> items across <paramref name="users" />.
        /// </summary>
        /// <param name="users">Eligible users with their current load and capacity.</param>
        /// <param name="documents">Total number of items to distribute.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>
        ///     A <see cref="DistributionResult{TUserId}" /> containing the full allocation map,
        ///     the primary and alternative user recommendations, and an audit trace;
        ///     or a failure result with an error message.
        /// </returns>
        /// =================================================================================================
        Task<IResult<DistributionResult<TUserId>>> GenerateAsync(
            IEnumerable<UserLoad<TUserId>> users,
            int documents,
            CancellationToken cancellationToken = default);
    }
}