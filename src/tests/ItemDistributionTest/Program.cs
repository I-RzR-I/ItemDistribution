using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ItemDistribution.Models.Config;
using ItemDistribution.Models.Domain;
using ItemDistribution.Public;

namespace ItemDistributionTest
{
    internal static class Program
    {
        private static void Main()
        {
            var sut = new DistributionSuggestionHelper<Guid>(new EngineConfig
            {
                UsePriorityAndActivity = true,
                FullWorkDayHours = 8,
                MaxAllowedInProcessDocuments = 15
            });

            RunT0(sut);
            Console.WriteLine("");
            Console.WriteLine("↑ Executed RunT0");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT1(sut);
            Console.WriteLine("");
            RunT1Async(sut).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT1");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT2(sut);
            Console.WriteLine("");
            RunT2Async(sut).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT2");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT3(sut);
            Console.WriteLine("");
            RunT3Async(sut).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT3");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT4(sut);
            Console.WriteLine("");
            RunT4Async(sut).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT4");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT5(sut);
            Console.WriteLine("");
            RunT5Async(sut).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT5");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT6(sut);
            Console.WriteLine("");
            RunT6Async(sut).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT6");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            RunT7(sut);
            Console.WriteLine("");
            RunT7Async(sut).GetAwaiter().GetResult();
            Console.WriteLine("↑ Executed RunT7");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------");

            Console.ReadKey();
        }

        private static void RunT0(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 8, 8, 5, -1.2));

            var time = Stopwatch.StartNew();
            var result = sut.Generate(users, documents: 1);
            time.Stop();

            PrintResult("T0 Sync", time, result, names);
        }

        private static void RunT1(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 8, 8, 5, -1.2),
                ("Te3", 5, 8, 5, 2),
                ("Te4", 3, 8, 5, 2.5));

            var time = Stopwatch.StartNew();
            var result = sut.Generate(users, documents: 1);
            time.Stop();

            PrintResult("T1 Sync", time, result, names);
        }

        private static async Task RunT1Async(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 8, 8, 5, -1.2),
                ("Te3", 5, 8, 5, 2),
                ("Te4", 3, 8, 5, 2.5));

            var time = Stopwatch.StartNew();
            var result = await sut.GenerateAsync(users, documents: 1);
            time.Stop();

            PrintResult("T1 Async", time, result, names);
        }

        private static void RunT2(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 1, 8, 1, -1.2),
                ("Te3", 5, 8, 1, 2),
                ("Te4", 3, 8, 2, 2.5));

            var time = Stopwatch.StartNew();
            var result = sut.Generate(users, documents: 1);
            time.Stop();

            PrintResult("T2 Sync", time, result, names);
        }

        private static async Task RunT2Async(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 1, 8, 1, -1.2),
                ("Te3", 5, 8, 1, 2),
                ("Te4", 3, 8, 2, 2.5));

            var time = Stopwatch.StartNew();
            var result = await sut.GenerateAsync(users, documents: 1);
            time.Stop();

            PrintResult("T2 Async", time, result, names);
        }

        private static void RunT3(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 1, 8, 5, -1.2),
                ("Te3", 5, 8, 1, 2),
                ("Te4", 3, 8, 2, 2.5));

            var time = Stopwatch.StartNew();
            var result = sut.Generate(users, documents: 1);
            time.Stop();

            PrintResult("T3 Sync", time, result, names);
        }

        private static async Task RunT3Async(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 1, 8, 5, -1.2),
                ("Te3", 5, 8, 1, 2),
                ("Te4", 3, 8, 2, 2.5));

            var time = Stopwatch.StartNew();
            var result = await sut.GenerateAsync(users, documents: 1);
            time.Stop();

            PrintResult("T3 Async", time, result, names);
        }

        private static void RunT4(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 1, 8, 5, -1.2),
                ("Te3", 5, 8, 1, 2),
                ("Te4", 3, 8, 2, 2.5));

            var time = Stopwatch.StartNew();
            var result = sut.Generate(users, documents: 1);
            time.Stop();

            PrintResult("T4 Sync", time, result, names);
        }

        private static async Task RunT4Async(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 1, 8, 5, -1),
                ("Te2", 1, 8, 5, -1.2),
                ("Te3", 5, 8, 1, 2),
                ("Te4", 3, 8, 2, 2.5));

            var time = Stopwatch.StartNew();
            var result = await sut.GenerateAsync(users, documents: 1);
            time.Stop();

            PrintResult("T4 Async", time, result, names);
        }

        private static void RunT5(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 2, 5, 4, -1),
                ("Te2", 1, 8, 5, -1.2),
                ("Te3", 5, 8, 1, 2),
                ("Te4", 3, 8, 2, 2.5));

            var time = Stopwatch.StartNew();
            var result = sut.Generate(users, documents: 1);
            time.Stop();

            PrintResult("T5 Sync", time, result, names);
        }

        private static async Task RunT5Async(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(
                ("Te1", 2, 5, 4, -1),
                ("Te2", 1, 8, 5, -1.2),
                ("Te3", 5, 8, 1, 2),
                ("Te4", 3, 8, 2, 2.5));

            var time = Stopwatch.StartNew();
            var result = await sut.GenerateAsync(users, documents: 1);
            time.Stop();

            PrintResult("T5 Async", time, result, names);
        }

        private static void RunT6(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildStressUsers();

            var time = Stopwatch.StartNew();
            var result = sut.Generate(users, documents: 1);
            time.Stop();

            PrintResult("T6 Sync", time, result, names);
        }

        private static async Task RunT6Async(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildStressUsers();

            var time = Stopwatch.StartNew();
            var result = await sut.GenerateAsync(users, documents: 1);
            time.Stop();

            PrintResult("T6 Async", time, result, names);
        }

        private static void RunT7(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(("Te1*", 2, 5, 4, -1));

            var time = Stopwatch.StartNew();
            var result = sut.Generate(users, documents: 1);
            time.Stop();

            PrintResult("T7 Sync", time, result, names);
        }

        private static async Task RunT7Async(DistributionSuggestionHelper<Guid> sut)
        {
            var (users, names) = BuildUsers(("Te1*", 2, 5, 4, -1));

            var time = Stopwatch.StartNew();
            var result = await sut.GenerateAsync(users, documents: 1);
            time.Stop();

            PrintResult("T7 Async", time, result, names);
        }

        private static (List<UserLoad<Guid>> Users, Dictionary<Guid, string> Names) BuildUsers(
            params (string Name, int Docs, decimal Hours, short Priority, double ActivityDaysOffset)[] specs)
        {
            var users = new List<UserLoad<Guid>>(specs.Length);
            var names = new Dictionary<Guid, string>(specs.Length);

            foreach (var s in specs)
            {
                var id = Guid.NewGuid();
                names[id] = s.Name;
                users.Add(new UserLoad<Guid>(
                    userId: id,
                    currentLoad: s.Docs,
                    capacity: 15,
                    priority: s.Priority,
                    lastActivityDate: DateTime.Now.AddDays(s.ActivityDaysOffset),
                    workingHours: s.Hours));
            }

            return (users, names);
        }

        private static (List<UserLoad<Guid>> Users, Dictionary<Guid, string> Names) BuildStressUsers()
        {
            var (users, names) = BuildUsers(
                ("Te1*", 2, 5, 4, -1),
                ("Te2*", 1, 8, 5, -1.2),
                ("Te3*", 5, 8, 1, 2),
                ("Te4*", 3, 8, 2, 2.5));

            var rnd = new Random();
            for (var i = 0; i < 10000; i++)
            {
                var rndNum = rnd.Next(1, 10);
                var id = Guid.NewGuid();
                names[id] = $"TestName{i}";
                users.Add(new UserLoad<Guid>(
                    userId: id,
                    currentLoad: rndNum,
                    capacity: 15,
                    priority: (short)rndNum,
                    lastActivityDate: DateTime.Now.AddDays(-rndNum),
                    workingHours: 8));
            }

            return (users, names);
        }

        private static void PrintResult(
            string label,
            Stopwatch time,
            AggregatedGenericResultMessage.Abstractions.IResult<DistributionResult<Guid>> result,
            Dictionary<Guid, string> names)
        {
            Console.WriteLine($"[{label}]  {time.Elapsed.TotalMilliseconds:F2} ms");

            if (!result.IsSuccess)
            {
                Console.WriteLine($"  FAILURE");
                return;
            }

            var r = result.Response;
            var primary = r.PrimaryUserId.HasValue && names.TryGetValue(r.PrimaryUserId.Value, out var pn) 
                ? pn 
                : r.PrimaryUserId?.ToString() ?? "(none)";
            var alt = r.AlternativeUserId.HasValue && names.TryGetValue(r.AlternativeUserId.Value, out var an) 
                ? an 
                : r.AlternativeUserId?.ToString() ?? "(none)";

            Console.WriteLine($"  Primary:     {primary}");
            Console.WriteLine($"  Alternative: {alt}");
            Console.WriteLine($"  Total alloc: {r.TotalAllocated}");

            var topAlloc = r.Allocation
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Take(5);
            foreach (var kv in topAlloc)
            {
                var name = names.TryGetValue(kv.Key, out var n) ? n : kv.Key.ToString();
                Console.WriteLine($"    {name} > {kv.Value}");
            }
        }
    }
}
