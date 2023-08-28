// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 18:31
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-21 18:32
// ***********************************************************************
//  <copyright file="MaximumAllowedDocCalcHelper.cs" company="">
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
using ItemDistribution.Models.Internal.Requests;

#endregion

namespace ItemDistribution.Helpers
{
    /// <summary>
    ///     Maximum allowed in process/active documents calculation helper.
    /// </summary>
    internal static class MaximumAllowedDocCalcHelper
    {
        /// <summary>
        ///     Calculate number of maximum allowed to process by user with specific working program.
        /// </summary>
        /// <param name="request">User work program.</param>
        /// <returns>Return int value, number of maximum allowed in process documents.</returns>
        /// <remarks></remarks>
        internal static IResult<int> CalculateMaxAllowedNrDocs(CalculateMaxAllowedNrDocsRequest request)
        {
            try
            {
                var docMarNr = (request.UserWorkHours * request.DayMaxAllowedDocs) / request.DayWorkHours;

                return Result<int>.Success((int)decimal.Truncate(docMarNr));
            }
            catch (Exception e)
            {
                return Result<int>
                    .Failure($"Internal error on calculate maximum allowed documents for processing in one day. Message: {e.Message}")
                    .WithError(e);
            }
        }

        /// <inheritdoc cref="CalculateMaxAllowedNrDocs"/>
        internal static async Task<IResult<int>> CalculateMaxAllowedNrDocsAsync(CalculateMaxAllowedNrDocsRequest request)
            => await Task.Run(() => CalculateMaxAllowedNrDocs(request));
    }
}