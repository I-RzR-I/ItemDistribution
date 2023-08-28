// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-21 21:59
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 21:16
// ***********************************************************************
//  <copyright file="UserInfoOptions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;

#endregion

namespace ItemDistribution.Models.Dto
{
    /// <summary>
    ///     User info options
    /// </summary>
    /// <typeparam name="TUserId">Type of user id</typeparam>
    public class UserInfoOptions<TUserId>
    {
        /// <summary>
        ///     User id
        /// </summary>
        public TUserId UserId { get; set; }

        /// <summary>
        ///     User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     User working hours
        /// </summary>
        public decimal WorkingHours { get; set; }

        /// <summary>
        ///     Number of documents/items in processing at this moment
        /// </summary>
        public int InProcessDocuments { get; set; }

        /// <summary>
        ///     Last user activity date
        /// </summary>
        public DateTime LastActivityDate { get; set; }

        /// <summary>
        ///     User select priority.
        ///     The priority is higher as close to 0 as possible is the value.
        /// </summary>
        public short UserPriority { get; set; }
    }
}