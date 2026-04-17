// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="DistributionSuggestionHelper.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using AggregatedGenericResultMessage;
using AggregatedGenericResultMessage.Abstractions;
using AggregatedGenericResultMessage.Extensions.Result;
using DomainCommonExtensions.CommonExtensions;
using ItemDistribution.Abstractions;
using ItemDistribution.Abstractions.Pipeline;
using ItemDistribution.Engine;
using ItemDistribution.Models.Config;
using ItemDistribution.Models.Domain;
using ItemDistribution.Strategies.Allocation;
using ItemDistribution.Strategies.Capacity;
using ItemDistribution.Strategies.Constraints;
using ItemDistribution.Strategies.Scoring;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ItemDistribution.Public
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     High-level entry point for the distribution engine.
    ///     Compose the pipeline via <see cref="EngineConfig" /> and call
    ///     <see cref="Generate" /> / <see cref="GenerateAsync" /> to allocate items across users.
    /// </summary>
    /// <typeparam name="TUserId">
    ///     Type of the user identifier (must be a value type, e.g. <see cref="Guid" /> or <see cref="int" />).
    /// </typeparam>
    /// <seealso cref="T:ItemDistribution.Abstractions.IDistributionSuggestionHelper{TUserId}" />
    /// =================================================================================================
    public class DistributionSuggestionHelper<TUserId> : IDistributionSuggestionHelper<TUserId>
        where TUserId : struct
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the distribution engine.
        /// </summary>
        /// =================================================================================================
        private readonly DistributionEngine<TUserId> _engine;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="DistributionSuggestionHelper{TUserId}" /> class.
        /// </summary>
        /// <param name="config">Engine configuration. Must not be <see langword="null" />.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <see cref="EngineConfig.MaxPerUser" /> is set to a value ≤ 0.
        /// </exception>
        /// =================================================================================================
        public DistributionSuggestionHelper(EngineConfig config)
        {
            if (config.IsNull())
                throw new ArgumentNullException(nameof(config));

            if (config.MaxPerUser.HasValue && config.MaxPerUser.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(config.MaxPerUser),
                    $"{nameof(EngineConfig.MaxPerUser)} must be greater than 0 when specified.");
            }

            if (config.MaxAllowedInProcessDocuments > 0 && config.FullWorkDayHours <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(config.FullWorkDayHours),
                    $"{nameof(EngineConfig.FullWorkDayHours)} must be greater than 0 when {nameof(EngineConfig.MaxAllowedInProcessDocuments)} is set.");
            }

            var constraints = new List<IConstraint<TUserId>>();

            if (config.MaxPerUser.HasValue)
                constraints.Add(new MaxPerUserConstraint<TUserId>(config.MaxPerUser.Value));

            var steps = new List<IDistributionStep<TUserId>> { new LoadScoringStep<TUserId>() };

            if (config.MaxAllowedInProcessDocuments > 0)
                steps.Add(new WorkingHoursCapacityStep<TUserId>(config.FullWorkDayHours, config.MaxAllowedInProcessDocuments));

            if (config.UsePriorityAndActivity)
                steps.Add(new PriorityActivityScoringStep<TUserId>());

            steps.Add(new FairAllocationStep<TUserId>(constraints));

            _engine = new DistributionEngine<TUserId>(steps);
        }

        /// <inheritdoc />
        public IResult<DistributionResult<TUserId>> Generate(
            IEnumerable<UserLoad<TUserId>> users,
            int documents)
        {
            try
            {
                if (users.IsNull())
                    return Result<DistributionResult<TUserId>>
                        .Failure($"{nameof(users)} cannot be null.");

                if (documents < 0)
                    return Result<DistributionResult<TUserId>>
                        .Failure($"{nameof(documents)} must be non-negative.");

                var (allocation, trace) = _engine.Execute(users, documents);

                return Result<DistributionResult<TUserId>>.Success(
                    new DistributionResult<TUserId>(allocation, trace));
            }
            catch (Exception e)
            {
                return Result<DistributionResult<TUserId>>
                    .Failure(e.Message).WithError(e);
            }
        }

        /// <inheritdoc />
        public async Task<IResult<DistributionResult<TUserId>>> GenerateAsync(
            IEnumerable<UserLoad<TUserId>> users,
            int documents,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (users.IsNull())
                    return Result<DistributionResult<TUserId>>
                        .Failure($"{nameof(users)} cannot be null.");

                if (documents < 0)
                    return Result<DistributionResult<TUserId>>
                        .Failure($"{nameof(documents)} must be non-negative.");

                var (allocation, trace) = await _engine.ExecuteAsync(users, documents, cancellationToken);

                return Result<DistributionResult<TUserId>>.Success(
                    new DistributionResult<TUserId>(allocation, trace));
            }
            catch (Exception e)
            {
                return Result<DistributionResult<TUserId>>
                    .Failure(e.Message).WithError(e);
            }
        }
    }
}