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

using RzR.Extensions.Domain.Primitives;
using RzR.ItemDistribution.Abstractions;
using RzR.ItemDistribution.Abstractions.Pipeline;
using RzR.ItemDistribution.Engine;
using RzR.ItemDistribution.Models.Config;
using RzR.ItemDistribution.Models.Domain;
using RzR.ItemDistribution.Strategies.Allocation;
using RzR.ItemDistribution.Strategies.Capacity;
using RzR.ItemDistribution.Strategies.Constraints;
using RzR.ItemDistribution.Strategies.Scoring;
using RzR.ResultMessage;
using RzR.ResultMessage.Abstractions;
using RzR.ResultMessage.Extensions.Result;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.ItemDistribution.Public
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     High-level entry point for the distribution engine.
    ///     Compose the pipeline via <see cref="EngineConfig" /> or <see cref="EngineConfig{TUserId}" />
    ///     and call <see cref="Generate" /> / <see cref="GenerateAsync" /> to allocate items across users.
    /// </summary>
    /// <typeparam name="TUserId">
    ///     Type of the user identifier (must be a value type, e.g. <see cref="Guid" /> or <see cref="int" />).
    /// </typeparam>
    /// <seealso cref="T:RzR.ItemDistribution.Abstractions.IDistributionSuggestionHelper{TUserId}" />
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
        ///     <see cref="DistributionSuggestionHelper{TUserId}" /> class with base configuration.
        ///     The pipeline uses only the built-in scoring, capacity, and allocation steps.
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

            _engine = new DistributionEngine<TUserId>(BuildPipeline(config, null, null));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="DistributionSuggestionHelper{TUserId}" /> class with consumer-supplied
        ///     custom steps and constraints.
        ///     <para>
        ///         <b>Pipeline ordering contract:</b> custom steps from
        ///         <see cref="EngineConfig{TUserId}.AdditionalSteps" /> are inserted after the
        ///         built-in scoring/capacity steps and <em>before</em> the terminal
        ///         <c>FairAllocationStep</c>. Custom constraints from
        ///         <see cref="EngineConfig{TUserId}.AdditionalConstraints" /> are merged with the
        ///         built-in <c>MaxPerUserConstraint</c> and passed to <c>FairAllocationStep</c>.
        ///     </para>
        ///     When <see cref="EngineConfig{TUserId}.AdditionalSteps" /> and
        ///     <see cref="EngineConfig{TUserId}.AdditionalConstraints" /> are both empty the
        ///     pipeline behaves identically to the <see cref="DistributionSuggestionHelper{TUserId}(EngineConfig)" />
        ///     constructor.
        /// </summary>
        /// <param name="config">
        ///     Generic engine configuration with optional custom steps and constraints.
        ///     Must not be <see langword="null" />.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <see cref="EngineConfig.MaxPerUser" /> is set to a value ≤ 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <see cref="EngineConfig{TUserId}.AdditionalSteps" /> contains a
        ///     <see langword="null" /> entry or a <c>FairAllocationStep</c> instance (the terminal
        ///     step must remain the single built-in one), or when
        ///     <see cref="EngineConfig{TUserId}.AdditionalConstraints" /> contains a
        ///     <see langword="null" /> entry.
        /// </exception>
        /// =================================================================================================
        public DistributionSuggestionHelper(EngineConfig<TUserId> config)
        {
            if (config.IsNull())
                throw new ArgumentNullException(nameof(config));

            _engine = new DistributionEngine<TUserId>(
                BuildPipeline(config, config.AdditionalSteps, config.AdditionalConstraints));
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
                    new DistributionResult<TUserId>(allocation, trace, documents));
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
                    new DistributionResult<TUserId>(allocation, trace, documents));
            }
            catch (Exception e)
            {
                return Result<DistributionResult<TUserId>>
                    .Failure(e.Message).WithError(e);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds the ordered pipeline list from base config, optional extra steps, and optional
        ///     extra constraints. Validates shared config fields that apply regardless of the constructor
        ///     overload used.
        /// </summary>
        /// <param name="config">Base engine configuration.</param>
        /// <param name="additionalSteps">
        ///     Consumer-supplied steps to insert after scoring/capacity and before allocation.
        ///     May be null or empty.
        /// </param>
        /// <param name="additionalConstraints">
        ///     Consumer-supplied constraints merged with built-in ones for the allocation step.
        ///     May be null or empty.
        /// </param>
        /// <returns>Ordered list of pipeline steps.</returns>
        /// =================================================================================================
        private static IEnumerable<IDistributionStep<TUserId>> BuildPipeline(
            EngineConfig config,
            IReadOnlyList<IDistributionStep<TUserId>> additionalSteps,
            IReadOnlyList<IConstraint<TUserId>> additionalConstraints)
        {
            if (config.MaxPerUser.HasValue && config.MaxPerUser.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(config.MaxPerUser),
                    $"{nameof(EngineConfig.MaxPerUser)} must be greater than 0 when specified.");
            }

            var workingHoursEnabled = config.UseWorkingHoursCapacity || config.MaxAllowedInProcessDocuments > 0;

            if (workingHoursEnabled && config.MaxAllowedInProcessDocuments <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(config.MaxAllowedInProcessDocuments),
                    $"{nameof(EngineConfig.MaxAllowedInProcessDocuments)} must be greater than 0 when {nameof(EngineConfig.UseWorkingHoursCapacity)} is enabled.");
            }

            if (workingHoursEnabled && config.FullWorkDayHours <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(config.FullWorkDayHours),
                    $"{nameof(EngineConfig.FullWorkDayHours)} must be greater than 0 when {nameof(EngineConfig.MaxAllowedInProcessDocuments)} is set.");
            }

            var constraints = new List<IConstraint<TUserId>>();

            if (config.MaxPerUser.HasValue)
                constraints.Add(new MaxPerUserConstraint<TUserId>(config.MaxPerUser.Value));

            if (additionalConstraints.IsNotNull())
            {
                foreach (var constraint in additionalConstraints)
                {
                    if (constraint.IsNull())
                        throw new ArgumentException(
                            "additionalConstraints must not contain null entries.", nameof(additionalConstraints));
                }

                constraints.AddRange(additionalConstraints);
            }

            var steps = new List<IDistributionStep<TUserId>> { new LoadScoringStep<TUserId>() };

            if (workingHoursEnabled)
                steps.Add(new WorkingHoursCapacityStep<TUserId>(config.FullWorkDayHours, config.MaxAllowedInProcessDocuments));

            if (config.UsePriorityAndActivity)
                steps.Add(new PriorityActivityScoringStep<TUserId>());

            if (additionalSteps.IsNotNull())
            {
                foreach (var step in additionalSteps)
                {
                    if (step.IsNull())
                        throw new ArgumentException(
                            "additionalSteps must not contain null entries.", nameof(additionalSteps));

                    if (step is FairAllocationStep<TUserId>)
                        throw new ArgumentException(
                            $"additionalSteps must not contain a {nameof(FairAllocationStep<TUserId>)} instance. " +
                            "The terminal allocation step is built in and must remain the single, last step.",
                            nameof(additionalSteps));
                }

                steps.AddRange(additionalSteps);
            }

            steps.Add(new FairAllocationStep<TUserId>(constraints));

            return steps;
        }
    }
}
