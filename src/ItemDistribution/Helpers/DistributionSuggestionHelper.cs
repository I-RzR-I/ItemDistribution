// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 21:25
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-25 16:53
// ***********************************************************************
//  <copyright file="DistributionSuggestionHelper.cs" company="">
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
using ItemDistribution.Models.Dto.Result;
using ItemDistribution.Models.Internal.Dto;
using ItemDistribution.Models.Internal.Requests;

#endregion

namespace ItemDistribution.Helpers
{
    /// <summary>
    ///     Item distribution suggestion helper
    /// </summary>
    public sealed class DistributionSuggestionHelper : IDisposable
    {
        /// <summary>
        ///     Gets class/helper instance.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public static DistributionSuggestionHelper Instance { get; private set; } = new DistributionSuggestionHelper();

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Generate new item distribution suggestion.
        /// </summary>
        /// <param name="distributionParams">Distribution params.</param>
        /// <returns>Return new suggestion for item/documents distribution.</returns>
        /// <typeparam name="TUserId">Type of user id</typeparam>
        /// <remarks>The alternative user suggestion may be the same as user suggestion.</remarks>
        public IResult<DistributionSuggestionResultDto<TUserId>>
            GenerateNewDistributionSuggestion<TUserId>(DistributionParams<TUserId> distributionParams) where TUserId : struct
        {
            var usersLoadInformation = new List<PrepareUserDataSelectionDto<TUserId>>();

            try
            {
                // Prepare user information for selection
                var prepareUserInfoResult = UserDataAggregatorHelper
                    .PrepareUserInfoForSelection(new PrepareUserInfoForSelectionRequest<TUserId>()
                    {
                        FullWorkDayHours = distributionParams.FullWorkDayHours,
                        MaxAllowedInProcessDocuments = distributionParams.MaxAllowedInProcessDocuments,
                        EligibleRepartitionUsers = distributionParams.EligibleRepartitionUsers
                    });

                if (!prepareUserInfoResult.IsSuccess)
                    return Result<DistributionSuggestionResultDto<TUserId>>.Failure(prepareUserInfoResult.GetFirstMessage());

                // Find new distribution suggestion
                usersLoadInformation = prepareUserInfoResult.Response.ToList();

                // Find users grouping by their coefficient
                var userInfo = usersLoadInformation
                    .GroupBy(u => u.LoadCoefficient)
                    .Select(x => new { LoadCoefficient = x.Key, UsersCount = x.Count() })
                    .OrderBy(x => x.LoadCoefficient)
                    .FirstOrDefault();

                if (userInfo?.UsersCount == 1)
                {
                    var user = usersLoadInformation.Find(x => x.LoadCoefficient == userInfo.LoadCoefficient);
                    if (user.IsNull())
                        return Result<DistributionSuggestionResultDto<TUserId>>.Failure($"User with coefficient {userInfo.LoadCoefficient} not found!");

                    return Result<DistributionSuggestionResultDto<TUserId>>.Success(new DistributionSuggestionResultDto<TUserId>()
                    {
                        SuggestionUser = new DistributionSuggestionDto<TUserId>() { UserName = user.UserName, SuggestionUserId = user.UserId, InProcessItems = user.InProcessDocuments },
                        AlternativeUser = GenerateAlternativeUserSuggestion(usersLoadInformation, user, distributionParams.AvoidDuplicateResult).Response
                    });
                }

                // Exist multiple users with the same coefficient
                var userQueryCoefficient = usersLoadInformation
                    .Where(x => x.LoadCoefficient == userInfo?.LoadCoefficient)
                    .ToList();

                // Get user by priority
                var usersWithSameCoefficient = userQueryCoefficient
                    .GroupBy(u => u.UserPriority)
                    .Select(x => new { UserPriority = x.Key, UsersCount = x.Count() })
                    .OrderBy(x => x.UserPriority)
                    .First();
                if (usersWithSameCoefficient.UsersCount == 1)
                {
                    var user = userQueryCoefficient.First(x => x.UserPriority == usersWithSameCoefficient.UserPriority);

                    return Result<DistributionSuggestionResultDto<TUserId>>.Success(new DistributionSuggestionResultDto<TUserId>()
                    {
                        SuggestionUser = new DistributionSuggestionDto<TUserId>() { UserName = user.UserName, SuggestionUserId = user.UserId, InProcessItems = user.InProcessDocuments },
                        AlternativeUser = GenerateAlternativeUserSuggestion(usersLoadInformation, user, distributionParams.AvoidDuplicateResult).Response
                    });
                }
                else
                {
                    var user = userQueryCoefficient
                        .Where(x => x.UserPriority == usersWithSameCoefficient.UserPriority)
                        .OrderByDescending(x => x.LastActivityDate)
                        .First();

                    return Result<DistributionSuggestionResultDto<TUserId>>.Success(new DistributionSuggestionResultDto<TUserId>()
                    {
                        SuggestionUser = new DistributionSuggestionDto<TUserId>() { UserName = user.UserName, SuggestionUserId = user.UserId, InProcessItems = user.InProcessDocuments },
                        AlternativeUser = GenerateAlternativeUserSuggestion(usersLoadInformation, user, distributionParams.AvoidDuplicateResult).Response
                    });
                }
            }
            catch (Exception e)
            {
                var suggestion = GenerateAlternativeUserSuggestion(usersLoadInformation, null, distributionParams.AvoidDuplicateResult);
                var alternative = GenerateAlternativeUserSuggestion(usersLoadInformation,
                    new PrepareUserDataSelectionDto<TUserId>()
                    {
                        UserId = suggestion.Response.SuggestionUserId,
                        InProcessDocuments = suggestion.Response.InProcessItems,
                        UserName = suggestion.Response.UserName
                    }, distributionParams.AvoidDuplicateResult);

                return Result<DistributionSuggestionResultDto<TUserId>>
                    .Success(new DistributionSuggestionResultDto<TUserId>() { SuggestionUser = suggestion.Response, AlternativeUser = alternative.Response })
                    .WithError(e);
            }
        }

        /// <inheritdoc cref="GenerateNewDistributionSuggestion{TUserId}" />
        public async Task<IResult<DistributionSuggestionResultDto<TUserId>>>
            GenerateNewDistributionSuggestionAsync<TUserId>(DistributionParams<TUserId> distributionParams) where TUserId : struct
        {
            var usersLoadInformation = new List<PrepareUserDataSelectionDto<TUserId>>();

            try
            {
                // Prepare user information for selection
                var prepareUserInfoResult = await UserDataAggregatorHelper
                    .PrepareUserInfoForSelectionAsync(new PrepareUserInfoForSelectionRequest<TUserId>()
                    {
                        FullWorkDayHours = distributionParams.FullWorkDayHours,
                        MaxAllowedInProcessDocuments = distributionParams.MaxAllowedInProcessDocuments,
                        EligibleRepartitionUsers = distributionParams.EligibleRepartitionUsers
                    });

                if (!prepareUserInfoResult.IsSuccess)
                    return Result<DistributionSuggestionResultDto<TUserId>>.Failure(prepareUserInfoResult.GetFirstMessage());

                // Find new distribution suggestion
                usersLoadInformation = prepareUserInfoResult.Response.ToList();

                // Find users grouping by their coefficient
                var userInfo = usersLoadInformation
                    .GroupBy(u => u.LoadCoefficient)
                    .Select(x => new { LoadCoefficient = x.Key, UsersCount = x.Count() })
                    .OrderBy(x => x.LoadCoefficient)
                    .FirstOrDefault();

                if (userInfo?.UsersCount == 1)
                {
                    var user = usersLoadInformation.Find(x => x.LoadCoefficient == userInfo.LoadCoefficient);
                    if (user.IsNull())
                        return Result<DistributionSuggestionResultDto<TUserId>>.Failure($"User with coefficient {userInfo.LoadCoefficient} not found!");

                    return Result<DistributionSuggestionResultDto<TUserId>>.Success(new DistributionSuggestionResultDto<TUserId>()
                    {
                        SuggestionUser = new DistributionSuggestionDto<TUserId>() { UserName = user.UserName, SuggestionUserId = user.UserId, InProcessItems = user.InProcessDocuments },
                        AlternativeUser = (await GenerateAlternativeUserSuggestionAsync(usersLoadInformation, user, distributionParams.AvoidDuplicateResult)).Response
                    });
                }

                // Exist multiple users with the same coefficient
                var userQueryCoefficient = usersLoadInformation
                    .Where(x => x.LoadCoefficient == userInfo?.LoadCoefficient)
                    .ToList();

                // Get user by priority
                var usersWithSameCoefficient = userQueryCoefficient
                    .GroupBy(u => u.UserPriority)
                    .Select(x => new { UserPriority = x.Key, UsersCount = x.Count() })
                    .OrderBy(x => x.UserPriority)
                    .First();
                if (usersWithSameCoefficient.UsersCount == 1)
                {
                    var user = userQueryCoefficient.First(x => x.UserPriority == usersWithSameCoefficient.UserPriority);

                    return Result<DistributionSuggestionResultDto<TUserId>>.Success(new DistributionSuggestionResultDto<TUserId>()
                    {
                        SuggestionUser = new DistributionSuggestionDto<TUserId>() { UserName = user.UserName, SuggestionUserId = user.UserId, InProcessItems = user.InProcessDocuments },
                        AlternativeUser = (await GenerateAlternativeUserSuggestionAsync(usersLoadInformation, user, distributionParams.AvoidDuplicateResult)).Response
                    });
                }
                else
                {
                    var user = userQueryCoefficient
                        .Where(x => x.UserPriority == usersWithSameCoefficient.UserPriority)
                        .OrderByDescending(x => x.LastActivityDate)
                        .First();

                    return Result<DistributionSuggestionResultDto<TUserId>>.Success(new DistributionSuggestionResultDto<TUserId>()
                    {
                        SuggestionUser = new DistributionSuggestionDto<TUserId>() { UserName = user.UserName, SuggestionUserId = user.UserId, InProcessItems = user.InProcessDocuments },
                        AlternativeUser = (await GenerateAlternativeUserSuggestionAsync(usersLoadInformation, user, distributionParams.AvoidDuplicateResult)).Response
                    });
                }
            }
            catch (Exception e)
            {
                var suggestion = await GenerateAlternativeUserSuggestionAsync(usersLoadInformation, null, distributionParams.AvoidDuplicateResult);
                var alternative = await GenerateAlternativeUserSuggestionAsync(usersLoadInformation,
                    new PrepareUserDataSelectionDto<TUserId>()
                    {
                        UserId = suggestion.Response.SuggestionUserId,
                        InProcessDocuments = suggestion.Response.InProcessItems,
                        UserName = suggestion.Response.UserName
                    }, distributionParams.AvoidDuplicateResult);

                return Result<DistributionSuggestionResultDto<TUserId>>
                    .Success(new DistributionSuggestionResultDto<TUserId>() { SuggestionUser = suggestion.Response, AlternativeUser = alternative.Response })
                    .WithError(e);
            }
        }

        /// <summary>
        ///     Generate alternative user suggestion.
        /// </summary>
        /// <param name="userInfo">List of eligible users with details.</param>
        /// <param name="excludedUser">Excluded user</param>
        /// <param name="avoidDuplicateResult">Avoid or not duplicate</param>
        /// <returns>Return an alternative user suggestion based on random generation.</returns>
        /// <typeparam name="TUserId">Type of user id.</typeparam>
        /// <remarks>Generate a random user suggestion.</remarks>
        private static IResult<DistributionSuggestionDto<TUserId>>
            GenerateAlternativeUserSuggestion<TUserId>(
                IReadOnlyList<PrepareUserDataSelectionDto<TUserId>> userInfo,
                PrepareUserDataSelectionDto<TUserId> excludedUser, bool avoidDuplicateResult = false)
        {
            var userCount = userInfo.Count;
            if (userCount.IsZeroOne())
                return Result<DistributionSuggestionDto<TUserId>>.Success();

            // Get random user
            var idx = RandomHelper.Instance.Number(0, userCount - 1);
            PrepareUserDataSelectionDto<TUserId> user;
            if (avoidDuplicateResult.Equals(true) && !excludedUser.IsNull())
            {
                if (userInfo.Count(x => !x.UserId.Equals(excludedUser.UserId)).IsZeroOne())
                    return Result<DistributionSuggestionDto<TUserId>>.Success();
                else
                    user = userInfo.Where(x => !x.UserId.Equals(excludedUser.UserId)).ToList()[idx];
            }
            else
                user = userInfo[idx];

            return Result<DistributionSuggestionDto<TUserId>>
                .Success(new DistributionSuggestionDto<TUserId>() { UserName = user.UserName, InProcessItems = user.InProcessDocuments, SuggestionUserId = user.UserId });
        }

        /// <inheritdoc cref="GenerateAlternativeUserSuggestion{TUserId}" />
        private static async Task<IResult<DistributionSuggestionDto<TUserId>>>
            GenerateAlternativeUserSuggestionAsync<TUserId>(
                IReadOnlyList<PrepareUserDataSelectionDto<TUserId>> userInfo,
                PrepareUserDataSelectionDto<TUserId> excludedUser, bool avoidDuplicateResult = false)
            => await Task.Run(() => GenerateAlternativeUserSuggestion(userInfo, excludedUser, avoidDuplicateResult));

        /// <summary>
        ///     Releases the unmanaged resources used by the
        ///     <see cref="ItemDistribution.Helpers.DistributionSuggestionHelper" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">If set to <see langword="true" />, then dispose elements; otherwise, elements will be not disposed.</param>
        /// <remarks></remarks>
        private static void Dispose(bool disposing)
        {
            if (!disposing || Instance.IsNull())
                return;

            Instance = null;
        }

        /// <inheritdoc />
        ~DistributionSuggestionHelper() => Dispose(false);
    }
}