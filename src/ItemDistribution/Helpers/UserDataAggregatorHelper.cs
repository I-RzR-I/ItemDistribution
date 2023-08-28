// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 19:45
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-24 00:23
// ***********************************************************************
//  <copyright file="UserDataAggregatorHelper.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AggregatedGenericResultMessage;
using AggregatedGenericResultMessage.Abstractions;
using AggregatedGenericResultMessage.Extensions.Result;
using ItemDistribution.Extensions;
using ItemDistribution.Models.Dto;
using ItemDistribution.Models.Internal.Dto;
using ItemDistribution.Models.Internal.Requests;

#endregion

namespace ItemDistribution.Helpers
{
    /// <summary>
    ///     User data aggregation
    /// </summary>
    internal static class UserDataAggregatorHelper
    {
        /// <summary>
        ///     Prepare users for selection
        /// </summary>
        /// <param name="request">Selection request</param>
        /// <returns></returns>
        /// <typeparam name="TUserId">Type of user id</typeparam>
        /// <remarks></remarks>
        internal static IResult<IEnumerable<PrepareUserDataSelectionDto<TUserId>>>
            PrepareUserInfoForSelection<TUserId>(PrepareUserInfoForSelectionRequest<TUserId> request)
        {
            try
            {
                var resultUserInfo = Task.WhenAll(request.EligibleRepartitionUsers
                    .Select(async c => await DoCalculationAsync(c, request.FullWorkDayHours, request.MaxAllowedInProcessDocuments)))
                    .Result
                    .Where(s => !s.IsNull())
                    .ToList();

                return Result<IEnumerable<PrepareUserDataSelectionDto<TUserId>>>
                    .Success(resultUserInfo);
            }
            catch (Exception e)
            {
                return Result<IEnumerable<PrepareUserDataSelectionDto<TUserId>>>
                    .Failure($"Internal error on prepare user data for selection. Message: {e.Message}")
                    .WithError(e);
            }
        }

        /// <inheritdoc cref="PrepareUserInfoForSelection{TUserId}"/>
        internal static async Task<IResult<IEnumerable<PrepareUserDataSelectionDto<TUserId>>>>
            PrepareUserInfoForSelectionAsync<TUserId>(PrepareUserInfoForSelectionRequest<TUserId> request)
            => await Task.Run(() => PrepareUserInfoForSelection(request));

        /// <summary>
        ///     Execute user calculation and inf aggregation.
        /// </summary>
        /// <param name="userInfo">USer information.</param>
        /// <param name="fullWorkDayHours">Full work hours program.</param>
        /// <param name="maxAllowedInProcessDocuments">Maximum allowed items in process.</param>
        /// <returns></returns>
        /// <typeparam name="TUserId">Type of user id</typeparam>
        /// <remarks></remarks>
        private static async Task<PrepareUserDataSelectionDto<TUserId>>
            DoCalculationAsync<TUserId>(UserInfoOptions<TUserId> userInfo, decimal fullWorkDayHours, int maxAllowedInProcessDocuments)
            => await Task.Run(() =>
            {
                var coefficient = LoadCoefficientCalcHelper.CalculateLoadCoefficient(new CalculateLoadCoefficientRequest
                {
                    ActiveNrOfDocuments = userInfo.InProcessDocuments,
                    NormallyDayWorkHours = fullWorkDayHours,
                    TotalNrOfDocuments = maxAllowedInProcessDocuments,
                    WorkHours = userInfo.WorkingHours
                });

                return new PrepareUserDataSelectionDto<TUserId>
                {
                    InProcessDocuments = userInfo.InProcessDocuments,
                    LastActivityDate = userInfo.LastActivityDate,
                    LoadCoefficient = coefficient.Response,
                    UserId = userInfo.UserId,
                    UserName = userInfo.UserName,
                    WorkingHours = userInfo.WorkingHours,
                    UserPriority = userInfo.UserPriority
                };
            });
    }
}