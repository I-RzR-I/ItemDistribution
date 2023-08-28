// ***********************************************************************
//  Assembly         : RzR.Shared.Entity.ItemDistribution
//  Author           : RzR
//  Created On       : 2023-08-23 15:39
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-23 16:06
// ***********************************************************************
//  <copyright file="RandomHelper.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Threading;

#pragma warning disable SCS0005
#endregion

namespace ItemDistribution.Helpers
{
    /// <summary>
    ///     Random number generator
    /// </summary>
    internal class RandomHelper
    {
        private static readonly Lazy<object> Locker = new Lazy<object>(() => new object(), LazyThreadSafetyMode.ExecutionAndPublication);
        private readonly Random _localSeed;
        private readonly Random _seed = new Random();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemDistribution.Helpers.RandomHelper" /> class.
        /// </summary>
        /// <remarks></remarks>
        private RandomHelper() => _localSeed = _seed ?? new Random();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemDistribution.Helpers.RandomHelper" /> class.
        /// </summary>
        /// <param name="localSeed">Random seed</param>
        /// <remarks></remarks>
        public RandomHelper(Random localSeed) => _localSeed = localSeed;

        /// <summary>
        ///     Random instance
        /// </summary>
        public static RandomHelper Instance { get; } = new RandomHelper();

        /// <summary>
        ///     Generate new random number
        /// </summary>
        /// <param name="min">Optional. The default value is 0 (inclusive).</param>
        /// <param name="max">Optional. The default value is 1 (inclusive).</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal int Number(int min = 0, int max = 1)
        {
            lock (Locker.Value)
            {
                if (max < int.MaxValue) return _localSeed.Next(min, max + 1);

                if (min > int.MinValue) return 1 + _localSeed.Next(min - 1, max);

                var num = _localSeed.Next();
                var num2 = _localSeed.Next();
                var num3 = (num >> 8) & 0xFFFF;
                var num4 = (num2 >> 8) & 0xFFFF;

                return (num3 << 16) | num4;
            }
        }
    }
}