// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2026-03-24 23:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-03 01:37
// ***********************************************************************
//  <copyright file="UserLoad.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;

#endregion

namespace RzR.ItemDistribution.Models.Domain
{
    /// <summary>
    ///     Immutable snapshot of a single user's workload and scoring attributes. Passed to the
    ///     distribution engine as input.
    /// </summary>
    /// <typeparam name="TUserId">Type of the user identifier (must be a value type).</typeparam>
    public sealed class UserLoad<TUserId>
        where TUserId : struct
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserLoad{TUserId}" /> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when one or more arguments are outside the required range.
        /// </exception>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="currentLoad">Number of items currently assigned to the user.</param>
        /// <param name="capacity">Maximum number of items this user can handle.</param>
        /// <param name="priority">
        ///     (Optional)
        ///     Selection priority (lower = higher priority). Default is 0.
        /// </param>
        /// <param name="lastActivityDate">(Optional) Last recorded activity date/time.</param>
        /// <param name="workingHours">
        ///     (Optional)
        ///     Hours worked per day. Default is 0 (capacity used as-is).
        /// </param>
        public UserLoad(
            TUserId userId, decimal currentLoad, decimal capacity,
            short priority = 0, DateTime lastActivityDate = default,
            decimal workingHours = 0)
        {
            if (currentLoad < 0)
                throw new ArgumentOutOfRangeException(nameof(currentLoad),
                    $"{nameof(currentLoad)} must be non-negative.");

            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity),
                    $"{nameof(capacity)} must be non-negative.");

            if (workingHours < 0)
                throw new ArgumentOutOfRangeException(nameof(workingHours),
                    $"{nameof(workingHours)} must be non-negative.");

            UserId = userId;
            CurrentLoad = currentLoad;
            Capacity = capacity;
            Priority = priority;
            LastActivityDate = lastActivityDate;
            WorkingHours = workingHours;
        }

        /// <summary>
        ///     Unique identifier of the user.
        /// </summary>
        /// <value>
        ///     The identifier of the user.
        /// </value>
        public TUserId UserId { get; }

        /// <summary>
        ///     Number of items currently assigned to (in process by) this user.
        /// </summary>
        /// <value>
        ///     The current load.
        /// </value>
        public decimal CurrentLoad { get; }

        /// <summary>
        ///     Maximum number of items this user can handle. When <see cref="WorkingHours" /> is set and
        ///     the engine has
        ///     <see cref="Config.EngineConfig.MaxAllowedInProcessDocuments" /> configured,
        ///     the <c>WorkingHoursCapacityStep</c> derives an effective capacity automatically.
        /// </summary>
        /// <value>
        ///     The capacity.
        /// </value>
        public decimal Capacity { get; }

        /// <summary>
        ///     User selection priority. Lower value = higher priority (closer to 0 wins). Only
        ///     considered when <see cref="Config.EngineConfig.UsePriorityAndActivity" />
        ///     is <see langword="true" />.
        /// </summary>
        /// <value>
        ///     The priority.
        /// </value>
        public short Priority { get; }

        /// <summary>
        ///     Date/time of the user's last recorded activity. Used by the <c>
        ///     PriorityActivityScoringStep</c> to favor recently active users.
        /// </summary>
        /// <value>
        ///     The last activity date.
        /// </value>
        public DateTime LastActivityDate { get; }

        /// <summary>
        ///     Number of hours this user works per day (e.g. 4 for part-time, 8 for full-time). When the
        ///     engine has <see cref="Config.EngineConfig.FullWorkDayHours" /> and
        ///     <see cref="Config.EngineConfig.MaxAllowedInProcessDocuments" /> configured,
        ///     the <c>WorkingHoursCapacityStep</c> will derive the user's effective capacity from this
        ///     value automatically. Defaults to 0, meaning "not set" — capacity is taken from <see cref="Capacity" />
        ///     as-is.
        /// </summary>
        /// <value>
        ///     The working hours.
        /// </value>
        public decimal WorkingHours { get; }
    }
}