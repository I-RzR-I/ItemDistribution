using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ItemDistribution.Helpers;
using ItemDistribution.Models.Dto;

namespace ItemDistributionTest
{
    internal static class Program
    {
        private static void Main()
        {
            var suggestionInstance = DistributionSuggestionHelper.Instance;
            
            RunT0(suggestionInstance);
            Console.WriteLine("");
            Console.WriteLine("↑ Executed RunT0");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT1(suggestionInstance);
            Console.WriteLine("");
            RunT1Async(suggestionInstance).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT1");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT2(suggestionInstance);
            Console.WriteLine("");
            RunT2Async(suggestionInstance).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT2");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT3(suggestionInstance);
            Console.WriteLine("");
            RunT3Async(suggestionInstance).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT3");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT4(suggestionInstance);
            Console.WriteLine("");
            RunT4Async(suggestionInstance).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT4");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT5(suggestionInstance);
            Console.WriteLine("");
            RunT5Async(suggestionInstance).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT5");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT6(suggestionInstance);
            Console.WriteLine("");
            RunT6Async(suggestionInstance).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT6");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT7(suggestionInstance);
            Console.WriteLine("");
            RunT7Async(suggestionInstance).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT7");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            suggestionInstance.Dispose();
            Console.ReadKey();
        }

        private static void RunT0(DistributionSuggestionHelper suggestionInstance)
        {
            var normalUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>()
                {
                    InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(),
                    UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)
                },
                new UserInfoOptions<Guid>()
                {
                    InProcessDocuments = 8, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(),
                    UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)
                }
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = suggestionInstance.GenerateNewDistributionSuggestion(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = normalUsersInfo,
                AvoidDuplicateResult = true,
            });
            time.Stop();

            Console.WriteLine(
                $"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static void RunT1(DistributionSuggestionHelper suggestionInstance)
        {
            var normalUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 8, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = suggestionInstance.GenerateNewDistributionSuggestion(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = normalUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static async Task RunT1Async(DistributionSuggestionHelper suggestionInstance)
        {
            var normalUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 8, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = await suggestionInstance.GenerateNewDistributionSuggestionAsync(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = normalUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static void RunT2(DistributionSuggestionHelper suggestionInstance)
        {
            var sameNrOfDocsUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = suggestionInstance.GenerateNewDistributionSuggestion(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = sameNrOfDocsUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static async Task RunT2Async(DistributionSuggestionHelper suggestionInstance)
        {
            var sameNrOfDocsUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = await suggestionInstance.GenerateNewDistributionSuggestionAsync(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = sameNrOfDocsUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static void RunT3(DistributionSuggestionHelper suggestionInstance)
        {
            var sameNrOfDocsUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = suggestionInstance.GenerateNewDistributionSuggestion(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = sameNrOfDocsUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static async Task RunT3Async(DistributionSuggestionHelper suggestionInstance)
        {
            var sameNrOfDocsUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = await suggestionInstance.GenerateNewDistributionSuggestionAsync(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = sameNrOfDocsUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static void RunT4(DistributionSuggestionHelper suggestionInstance)
        {
            var samePriorityUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = suggestionInstance.GenerateNewDistributionSuggestion(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = samePriorityUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static async Task RunT4Async(DistributionSuggestionHelper suggestionInstance)
        {
            var samePriorityUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = await suggestionInstance.GenerateNewDistributionSuggestionAsync(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = samePriorityUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static void RunT5(DistributionSuggestionHelper suggestionInstance)
        {
            var customWorkUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 2, WorkingHours = 5, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 4, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = suggestionInstance.GenerateNewDistributionSuggestion(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = customWorkUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static async Task RunT5Async(DistributionSuggestionHelper suggestionInstance)
        {
            var customWorkUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 2, WorkingHours = 5, UserName = "Te1", UserId = Guid.NewGuid(), UserPriority = 4, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = await suggestionInstance.GenerateNewDistributionSuggestionAsync(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = customWorkUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static void RunT6(DistributionSuggestionHelper suggestionInstance)
        {
            var customWorkUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 2, WorkingHours = 5, UserName = "Te1*", UserId = Guid.NewGuid(), UserPriority = 4, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2*", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3*", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4*", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };
            var rnd = new Random();
            for (var i = 0; i < 10000; i++)
            {
                var rndNum = rnd.Next(1, 10);
                customWorkUsersInfo.Add(new UserInfoOptions<Guid>()
                {
                    UserName = $"TestName{i}",
                    InProcessDocuments = rndNum,
                    WorkingHours = 8,
                    LastActivityDate = DateTime.Now.AddDays(-rndNum),
                    UserPriority = (short)rndNum,
                    UserId = Guid.NewGuid()
                });
            }
            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = suggestionInstance.GenerateNewDistributionSuggestion(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = customWorkUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static async Task RunT6Async(DistributionSuggestionHelper suggestionInstance)
        {
            var customWorkUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 2, WorkingHours = 5, UserName = "Te1*", UserId = Guid.NewGuid(), UserPriority = 4, LastActivityDate = DateTime.Now.AddDays(-1)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 1, WorkingHours = 8, UserName = "Te2*", UserId = Guid.NewGuid(), UserPriority = 5, LastActivityDate = DateTime.Now.AddDays(-1.2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 5, WorkingHours = 8, UserName = "Te3*", UserId = Guid.NewGuid(), UserPriority = 1, LastActivityDate = DateTime.Now.AddDays(2)},
                new UserInfoOptions<Guid>() {InProcessDocuments = 3, WorkingHours = 8, UserName = "Te4*", UserId = Guid.NewGuid(), UserPriority = 2, LastActivityDate = DateTime.Now.AddDays(2.5)}
            };
            var rnd = new Random();
            for (var i = 0; i < 10000; i++)
            {
                var rndNum = rnd.Next(1, 10);
                customWorkUsersInfo.Add(new UserInfoOptions<Guid>()
                {
                    UserName = $"TestName{i}",
                    InProcessDocuments = rndNum,
                    WorkingHours = 8,
                    LastActivityDate = DateTime.Now.AddDays(-rndNum),
                    UserPriority = (short)rndNum,
                    UserId = Guid.NewGuid()
                });
            }
            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = await suggestionInstance.GenerateNewDistributionSuggestionAsync(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = customWorkUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static void RunT7(DistributionSuggestionHelper suggestionInstance)
        {
            var customWorkUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 2, WorkingHours = 5, UserName = "Te1*", UserId = Guid.NewGuid(), UserPriority = 4, LastActivityDate = DateTime.Now.AddDays(-1)},
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = suggestionInstance.GenerateNewDistributionSuggestion(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = customWorkUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }

        private static async Task RunT7Async(DistributionSuggestionHelper suggestionInstance)
        {
            var customWorkUsersInfo = new List<UserInfoOptions<Guid>>()
            {
                new UserInfoOptions<Guid>() {InProcessDocuments = 2, WorkingHours = 5, UserName = "Te1*", UserId = Guid.NewGuid(), UserPriority = 4, LastActivityDate = DateTime.Now.AddDays(-1)},
            };

            var time = new Stopwatch();
            time.Start();
            suggestionInstance ??= DistributionSuggestionHelper.Instance;
            var suggestion = await suggestionInstance.GenerateNewDistributionSuggestionAsync(new DistributionParams<Guid>()
            {
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15,
                EligibleRepartitionUsers = customWorkUsersInfo
            });
            time.Stop();

            Console.WriteLine($"Execution time:\nSeconds - {time.Elapsed.TotalSeconds}\nMilliseconds - {time.Elapsed.TotalMilliseconds}.\nResult:\n{suggestion.Response}");
        }
    }
}
