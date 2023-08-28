// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 18:31
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-21 18:32
// ***********************************************************************
//  <copyright file="LoadCoefficientCalcHelper.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Threading.Tasks;
using AggregatedGenericResultMessage;
using AggregatedGenericResultMessage.Abstractions;
using AggregatedGenericResultMessage.Extensions.Result;
using AggregatedGenericResultMessage.Extensions.Result.Messages;
using ItemDistribution.Extensions;
using ItemDistribution.Models.Internal.Requests;

#endregion

namespace ItemDistribution.Helpers
{
    /// <summary>
    ///     User load coefficient calculation helper
    /// </summary>
    /// <remarks></remarks>
    internal static class LoadCoefficientCalcHelper
    {
        /// <summary>
        ///     Calculate user load coefficient
        /// </summary>
        /// <param name="request">User working data.</param>
        /// <returns>Return user load coefficient.</returns>
        /// <remarks></remarks>
        internal static IResult<decimal> CalculateLoadCoefficient(CalculateLoadCoefficientRequest request)
        {
            try
            {
                return request.WorkHours.IsFullWorkingDay(request.NormallyDayWorkHours)
                    ? CalculateFullWorkDayLoadCoefficient(request.ActiveNrOfDocuments, request.TotalNrOfDocuments)
                    : CalculateCustomWorkDayLoadCoefficient(request.ActiveNrOfDocuments, request.TotalNrOfDocuments,
                        request.WorkHours, request.NormallyDayWorkHours);
            }
            catch (Exception e)
            {
                return Result<decimal>
                    .Failure($"Internal error on calculate load coefficient. Message: {e.Message}")
                    .WithError(e);
            }
        }

        /// <summary>
        ///     Calculate load coefficient for full working day.
        /// </summary>
        /// <param name="currentActiveDocuments">Number of currently active documents for user X.</param>
        /// <param name="maxAllowedDocuments">Number of maximum allowed documents for processing in one day.</param>
        /// <returns>Return user load coefficient.</returns>
        /// <remarks></remarks>
        private static IResult<decimal> CalculateFullWorkDayLoadCoefficient(
            int currentActiveDocuments, int maxAllowedDocuments)
        {
            try
            {
                return Result<decimal>.Success(currentActiveDocuments / (decimal)maxAllowedDocuments);
            }
            catch (Exception e)
            {
                return Result<decimal>
                    .Failure($"Internal error on calculate load coefficient for full work day. Message: {e.Message}")
                    .WithError(e);
            }
        }

        /// <summary>
        ///     Calculate load coefficient for user custom working day.
        /// </summary>
        /// <param name="currentActiveDocuments">Number of currently active documents for user X.</param>
        /// <param name="maxAllowedDocuments">Number of maximum allowed documents for processing in one day.</param>
        /// <param name="userWorkingHours">Number of user working hours.</param>
        /// <param name="normallyWorkHours">Number of hours full working day.</param>
        /// <returns>Return user load coefficient.</returns>
        /// <remarks></remarks>
        private static IResult<decimal> CalculateCustomWorkDayLoadCoefficient(
            int currentActiveDocuments,
            int maxAllowedDocuments, decimal userWorkingHours, decimal normallyWorkHours)
        {
            try
            {
                var maxUserAllowedDocuments =
                    MaximumAllowedDocCalcHelper.CalculateMaxAllowedNrDocs(new CalculateMaxAllowedNrDocsRequest
                    {
                        DayWorkHours = normallyWorkHours,
                        DayMaxAllowedDocs = maxAllowedDocuments,
                        UserWorkHours = userWorkingHours
                    });
                if (!maxUserAllowedDocuments.IsSuccess)
                    return Result<decimal>.Failure().AddError(maxUserAllowedDocuments.GetFirstMessage());

                return Result<decimal>.Success(currentActiveDocuments / (decimal)maxUserAllowedDocuments.Response);
            }
            catch (Exception e)
            {
                return Result<decimal>
                    .Failure($"Internal error on calculate custom user load coefficient. Message: {e.Message}")
                    .WithError(e);
            }
        }

        #region A S Y N C
        
        /// <inheritdoc cref="CalculateLoadCoefficient"/>
        internal static async Task<IResult<decimal>> CalculateLoadCoefficientAsync(CalculateLoadCoefficientRequest request)
            => await Task.Run(async () =>
            {
                try
                {
                    return request.WorkHours.IsFullWorkingDay(request.NormallyDayWorkHours)
                        ? await CalculateFullWorkDayLoadCoefficientAsync(request.ActiveNrOfDocuments, request.TotalNrOfDocuments)
                        : await CalculateCustomWorkDayLoadCoefficientAsync(request.ActiveNrOfDocuments, request.TotalNrOfDocuments,
                            request.WorkHours, request.NormallyDayWorkHours);
                }
                catch (Exception e)
                {
                    return Result<decimal>
                        .Failure($"Internal error on calculate load coefficient. Message: {e.Message}")
                        .WithError(e);
                }
            });

        /// <inheritdoc cref="CalculateFullWorkDayLoadCoefficient"/>
        private static async Task<IResult<decimal>> CalculateFullWorkDayLoadCoefficientAsync(
            int currentActiveDocuments, int maxAllowedDocuments)
            => await Task.Run(()
                => CalculateFullWorkDayLoadCoefficient(currentActiveDocuments, maxAllowedDocuments));

        /// <inheritdoc cref="CalculateCustomWorkDayLoadCoefficient"/>
        private static async Task<IResult<decimal>> CalculateCustomWorkDayLoadCoefficientAsync(
            int currentActiveDocuments, int maxAllowedDocuments, decimal userWorkingHours, decimal normallyWorkHours)
            => await Task.Run(async () =>
            {
                try
                {
                    var maxUserAllowedDocuments =
                        await MaximumAllowedDocCalcHelper.CalculateMaxAllowedNrDocsAsync(new CalculateMaxAllowedNrDocsRequest
                        {
                            DayWorkHours = normallyWorkHours,
                            DayMaxAllowedDocs = maxAllowedDocuments,
                            UserWorkHours = userWorkingHours
                        });
                    if (!maxUserAllowedDocuments.IsSuccess)
                        return Result<decimal>.Failure().AddError(maxUserAllowedDocuments.GetFirstMessage());

                    return Result<decimal>.Success(currentActiveDocuments / (decimal)maxUserAllowedDocuments.Response);
                }
                catch (Exception e)
                {
                    return Result<decimal>
                        .Failure($"Internal error on calculate custom user load coefficient. Message: {e.Message}")
                        .WithError(e);
                }
            });
        
        #endregion
    }
}