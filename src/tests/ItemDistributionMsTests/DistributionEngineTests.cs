#region U S A G E S

using System;
using System.Linq;
using System.Threading.Tasks;
using ItemDistribution.Models.Config;
using ItemDistribution.Models.Domain;
using ItemDistribution.Public;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace ItemDistributionMsTests
{
    [TestClass]
    public class DistributionEngineTests
    {
        [TestMethod]
        [TestCategory("Engine.Allocation")]
        public void Sync_TwoUsers_MostCapacityReceivesMore()
        {
            var sut = MakeSut();
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();
            var users = new[] { Load(userA, 0m, 10m), Load(userB, 8m, 10m) };

            var result = sut.Generate(users, 3); // odd count, higher-score user wins the extra item

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Response.Allocation[userA] > result.Response.Allocation[userB],
                $"UserA ({result.Response.Allocation[userA]}) should receive more items than UserB ({result.Response.Allocation[userB]}).");
        }

        [TestMethod]
        [TestCategory("Engine.Allocation")]
        public async Task Async_TwoUsers_MostCapacityReceivesMore()
        {
            var sut = MakeSut();
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();
            var users = new[] { Load(userA, 0m, 10m), Load(userB, 8m, 10m) };

            var result = await sut.GenerateAsync(users, 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Response.Allocation[userA] > result.Response.Allocation[userB]);
        }

        [TestMethod]
        [TestCategory("Engine.Allocation")]
        public void Sync_TotalAllocated_EqualsDocumentCount()
        {
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 2m, 10m),
                Load(Guid.NewGuid(), 5m, 10m),
                Load(Guid.NewGuid(), 3m, 10m)
            };

            var result = sut.Generate(users, 15);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(15, result.Response.TotalAllocated,
                "Total allocated items must equal requested document count.");
        }

        [TestMethod]
        [TestCategory("Engine.Allocation")]
        public async Task Async_TotalAllocated_EqualsDocumentCount()
        {
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 2m, 10m),
                Load(Guid.NewGuid(), 5m, 10m),
                Load(Guid.NewGuid(), 3m, 10m)
            };

            var result = await sut.GenerateAsync(users, 15);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(15, result.Response.TotalAllocated);
        }

        [TestMethod]
        [TestCategory("Engine.Constraints")]
        public void Sync_MaxPerUserConstraint_NobodyExceedsCap()
        {
            var sut = MakeSut(3);
            var users = new[]
            {
                Load(Guid.NewGuid(), 0m, 10m),
                Load(Guid.NewGuid(), 0m, 10m)
            };

            var result = sut.Generate(users, 20); // 20 docs, cap=3, 2 users, only 6 can be allocated

            Assert.IsTrue(result.IsSuccess);
            foreach (var kv in result.Response.Allocation)
                Assert.IsTrue(kv.Value <= 3, $"User {kv.Key} got {kv.Value} items, exceeding cap of 3.");
        }

        [TestMethod]
        [TestCategory("Engine.Allocation")]
        public void Sync_ZeroCapacityUser_ReceivesNoAllocation()
        {
            var sut = MakeSut();
            var emptyId = Guid.NewGuid();
            var realId = Guid.NewGuid();
            var users = new[] { Load(emptyId, 0m, 0m), Load(realId, 0m, 10m) };

            var result = sut.Generate(users, 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Response.Allocation[emptyId], "User with zero capacity should receive 0 items.");
            Assert.AreEqual(5, result.Response.Allocation[realId]);
        }

        [TestMethod]
        [TestCategory("Engine.Allocation")]
        public void Sync_ZeroDocuments_AllUsersGetZero()
        {
            var sut = MakeSut();
            var users = new[] { Load(Guid.NewGuid(), 0m, 10m), Load(Guid.NewGuid(), 0m, 10m) };

            var result = sut.Generate(users, 0);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Response.TotalAllocated);
        }

        [TestMethod]
        [TestCategory("Engine.Validation")]
        public void Sync_NullUsers_ReturnsFailure()
        {
            var sut = MakeSut();
            var result = sut.Generate(null, 5);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        [TestCategory("Engine.Validation")]
        public async Task Async_NullUsers_ReturnsFailure()
        {
            var sut = MakeSut();
            var result = await sut.GenerateAsync(null, 5);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        [TestCategory("Engine.Validation")]
        public void Sync_NegativeDocuments_ReturnsFailure()
        {
            var sut = MakeSut();
            var users = new[] { Load(Guid.NewGuid(), 0m, 10m) };
            var result = sut.Generate(users, -1);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        [TestCategory("Engine.Validation")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Ctor_MaxPerUserZero_ThrowsArgumentOutOfRange()
        {
            _ = new DistributionSuggestionHelper<Guid>(new EngineConfig { MaxPerUser = 0 });
        }

        [TestMethod]
        [TestCategory("Engine.Validation")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_NullConfig_ThrowsArgumentNull()
        {
            _ = new DistributionSuggestionHelper<Guid>(null);
        }

        [TestMethod]
        [TestCategory("Engine.PriorityActivity")]
        public void Sync_PriorityScoring_LowerPriorityNumberReceivesMore()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var highPriId = Guid.NewGuid(); // priority=1, higher rank
            var lowPriId = Guid.NewGuid(); // priority=5, lower rank

            var users = new[]
            {
                Load(highPriId, 0m, 10m, 1, 1),
                Load(lowPriId, 0m, 10m, 5, 1)
            };

            var result = sut.Generate(users, 10);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Response.Allocation[highPriId] >= result.Response.Allocation[lowPriId],
                $"High-priority user ({result.Response.Allocation[highPriId]}) should receive >= items than low-priority ({result.Response.Allocation[lowPriId]}).");
        }

        [TestMethod]
        [TestCategory("Engine.PriorityActivity")]
        public void Sync_ActivityScoring_MoreRecentUserReceivesMore()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var recentId = Guid.NewGuid(); // active 1 day ago
            var oldId = Guid.NewGuid(); // active 30 days ago

            var users = new[]
            {
                Load(recentId, 0m, 10m, 1, 1),
                Load(oldId, 0m, 10m, 1, 30)
            };

            var result = sut.Generate(users, 10);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Response.Allocation[recentId] >= result.Response.Allocation[oldId],
                $"Recent user ({result.Response.Allocation[recentId]}) should receive >= items than old user ({result.Response.Allocation[oldId]}).");
        }

        [TestMethod]
        [TestCategory("Engine.Allocation")]
        public void Sync_SingleUser_ReceivesAllDocuments()
        {
            var sut = MakeSut();
            var userId = Guid.NewGuid();
            var result = sut.Generate(new[] { Load(userId, 0m, 10m) }, 7);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(7, result.Response.Allocation[userId]);
        }

        [TestMethod]
        [TestCategory("Engine.Allocation")]
        public void Sync_AllInputUsers_PresentInResult()
        {
            var sut = MakeSut();
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var users = ids.Select(id => Load(id, 1m, 10m)).ToArray();

            var result = sut.Generate(users, 6);

            Assert.IsTrue(result.IsSuccess);
            foreach (var id in ids)
                Assert.IsTrue(result.Response.Allocation.ContainsKey(id),
                    $"Result should contain entry for user {id}.");
        }

        [TestMethod]
        [TestCategory("Engine.WorkingHours")]
        public void Sync_PartTimeUser_ReceivesLess_WhenMoreLoaded()
        {
            var sut = MakeSutWithWorkingHours(8, 20);
            var partTimeId = Guid.NewGuid();
            var fullTimeId = Guid.NewGuid();
            var users = new[]
            {
                Load(partTimeId, 3m, 20m, workingHours: 4m),
                Load(fullTimeId, 0m, 20m, workingHours: 8m)
            };

            var result = sut.Generate(users, 9);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Response.Allocation[fullTimeId] > result.Response.Allocation[partTimeId],
                $"Full-timer ({result.Response.Allocation[fullTimeId]}) should receive more than part-timer ({result.Response.Allocation[partTimeId]}).");
        }

        [TestMethod]
        [TestCategory("Engine.WorkingHours")]
        public void Sync_WorkingHoursZero_StepSkipsUser_ReceivesFullAllocation()
        {
            var sut = MakeSutWithWorkingHours(8, 20);
            var userId = Guid.NewGuid();
            var users = new[] { Load(userId, 0m, 10m, workingHours: 0m) };

            var result = sut.Generate(users, 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(5, result.Response.Allocation[userId],
                "User with WorkingHours=0 should receive all 5 documents (step skipped).");
        }

        [TestMethod]
        [TestCategory("Engine.WorkingHours")]
        public void Sync_MaxDocsZero_WorkingHoursHasNoEffect_AllDocsAllocated()
        {
            var sut = MakeSut(); // default config: MaxAllowedInProcessDocuments=0
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();
            var users = new[]
            {
                Load(userA, 0m, 10m, workingHours: 4m),
                Load(userB, 0m, 10m, workingHours: 8m)
            };

            var result = sut.Generate(users, 4);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(4, result.Response.TotalAllocated,
                "All documents should be allocated regardless of WorkingHours when step is not active.");
        }

        [TestMethod]
        [TestCategory("Engine.Validation")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Ctor_MaxDocsSetButFullWorkDayHoursZero_ThrowsArgumentOutOfRange()
        {
            _ = new DistributionSuggestionHelper<Guid>(new EngineConfig
            {
                MaxAllowedInProcessDocuments = 20,
                FullWorkDayHours = 0
            });
        }

        [TestMethod]
        [TestCategory("Migrated.Coefficient")]
        public void Sync_UniqueLowestLoad_PrimaryUserIdMatchesBestUser()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var te1 = Guid.NewGuid();
            var users = new[]
            {
                Load(te1, 1m, 15m, 5, 1),
                Load(Guid.NewGuid(), 8m, 15m, 5, 1.2),
                Load(Guid.NewGuid(), 5m, 15m, 1, 2),
                Load(Guid.NewGuid(), 3m, 15m, 2, 2.5)
            };

            var result = sut.Generate(users, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(te1, result.Response.PrimaryUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.Coefficient")]
        public async Task Async_UniqueLowestLoad_PrimaryUserIdMatchesBestUser()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var te1 = Guid.NewGuid();
            var users = new[]
            {
                Load(te1, 1m, 15m, 5, 1),
                Load(Guid.NewGuid(), 8m, 15m, 5, 1.2),
                Load(Guid.NewGuid(), 5m, 15m, 1, 2),
                Load(Guid.NewGuid(), 3m, 15m, 2, 2.5)
            };

            var result = await sut.GenerateAsync(users, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(te1, result.Response.PrimaryUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.Priority")]
        public void Sync_SameLoad_DifferentPriority_PrimaryIsHigherPriority()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var te2 = Guid.NewGuid();
            var users = new[]
            {
                Load(Guid.NewGuid(), 1m, 15m, 5, 1),
                Load(te2, 1m, 15m, 1, 1.2),
                Load(Guid.NewGuid(), 5m, 15m, 1, 2),
                Load(Guid.NewGuid(), 3m, 15m, 2, 2.5)
            };

            var result = sut.Generate(users, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(te2, result.Response.PrimaryUserId,
                "User with lower priority number should be the primary suggestion.");
        }

        [TestMethod]
        [TestCategory("Migrated.Priority")]
        public async Task Async_SameLoad_DifferentPriority_PrimaryIsHigherPriority()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var te2 = Guid.NewGuid();
            var users = new[]
            {
                Load(Guid.NewGuid(), 1m, 15m, 5, 1),
                Load(te2, 1m, 15m, 1, 1.2),
                Load(Guid.NewGuid(), 5m, 15m, 1, 2),
                Load(Guid.NewGuid(), 3m, 15m, 2, 2.5)
            };

            var result = await sut.GenerateAsync(users, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(te2, result.Response.PrimaryUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.Priority")]
        public void Sync_SameLoadSamePriority_MostRecentIsPrimary()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var te1 = Guid.NewGuid();
            var users = new[]
            {
                Load(te1, 1m, 15m, 5, 1),
                Load(Guid.NewGuid(), 1m, 15m, 5, 1.2),
                Load(Guid.NewGuid(), 5m, 15m, 1, 2),
                Load(Guid.NewGuid(), 3m, 15m, 2, 2.5)
            };

            var result = sut.Generate(users, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(te1, result.Response.PrimaryUserId,
                "More recently active user should be the primary suggestion.");
        }

        [TestMethod]
        [TestCategory("Migrated.Priority")]
        public async Task Async_SameLoadSamePriority_MostRecentIsPrimary()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var te1 = Guid.NewGuid();
            var users = new[]
            {
                Load(te1, 1m, 15m, 5, 1),
                Load(Guid.NewGuid(), 1m, 15m, 5, 1.2),
                Load(Guid.NewGuid(), 5m, 15m, 1, 2),
                Load(Guid.NewGuid(), 3m, 15m, 2, 2.5)
            };

            var result = await sut.GenerateAsync(users, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(te1, result.Response.PrimaryUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.EdgeCase")]
        public void Sync_SingleUser_PrimarySet_AlternativeNull()
        {
            var sut = MakeSut();
            var userId = Guid.NewGuid();
            var result = sut.Generate(new[] { Load(userId, 2m, 15m) }, 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(userId, result.Response.PrimaryUserId);
            Assert.IsNull(result.Response.AlternativeUserId,
                "With only one user, AlternativeUserId should be null.");
        }

        [TestMethod]
        [TestCategory("Migrated.EdgeCase")]
        public async Task Async_SingleUser_PrimarySet_AlternativeNull()
        {
            var sut = MakeSut();
            var userId = Guid.NewGuid();
            var result = await sut.GenerateAsync(new[] { Load(userId, 2m, 15m) }, 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(userId, result.Response.PrimaryUserId);
            Assert.IsNull(result.Response.AlternativeUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.EdgeCase")]
        public void Sync_EmptyUserList_ReturnsSuccessWithNullPrimary()
        {
            var sut = MakeSut();
            var result = sut.Generate(Array.Empty<UserLoad<Guid>>(), 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Response.TotalAllocated);
            Assert.IsNull(result.Response.PrimaryUserId, "No users → no primary.");
            Assert.IsNull(result.Response.AlternativeUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.EdgeCase")]
        public async Task Async_EmptyUserList_ReturnsSuccessWithNullPrimary()
        {
            var sut = MakeSut();
            var result = await sut.GenerateAsync(Array.Empty<UserLoad<Guid>>(), 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Response.TotalAllocated);
            Assert.IsNull(result.Response.PrimaryUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.Coefficient")]
        public void Sync_PartialWorkDay_FullTimerIsPrimary()
        {
            var sut = MakeSutWithWorkingHours(8, 15);
            var te2 = Guid.NewGuid();
            var users = new[]
            {
                Load(Guid.NewGuid(), 2m, 15m, workingHours: 5m),
                Load(te2, 1m, 15m, workingHours: 8m),
                Load(Guid.NewGuid(), 5m, 15m, workingHours: 8m),
                Load(Guid.NewGuid(), 3m, 15m, workingHours: 8m)
            };

            var result = sut.Generate(users, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(te2, result.Response.PrimaryUserId,
                "Full-time user with lowest load should be primary.");
        }

        [TestMethod]
        [TestCategory("Migrated.Coefficient")]
        public async Task Async_PartialWorkDay_FullTimerIsPrimary()
        {
            var sut = MakeSutWithWorkingHours(8, 15);
            var te2 = Guid.NewGuid();
            var users = new[]
            {
                Load(Guid.NewGuid(), 2m, 15m, workingHours: 5m),
                Load(te2, 1m, 15m, workingHours: 8m),
                Load(Guid.NewGuid(), 5m, 15m, workingHours: 8m),
                Load(Guid.NewGuid(), 3m, 15m, workingHours: 8m)
            };

            var result = await sut.GenerateAsync(users, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(te2, result.Response.PrimaryUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.Alternative")]
        public void Sync_ThreeUsers_PrimaryAndAlternativeDiffer()
        {
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 1m, 15m),
                Load(Guid.NewGuid(), 5m, 15m),
                Load(Guid.NewGuid(), 8m, 15m)
            };

            var result = sut.Generate(users, 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Response.PrimaryUserId);
            Assert.IsNotNull(result.Response.AlternativeUserId);
            Assert.AreNotEqual(result.Response.PrimaryUserId, result.Response.AlternativeUserId,
                "Primary and alternative users must be different.");
        }

        [TestMethod]
        [TestCategory("Migrated.Alternative")]
        public async Task Async_ThreeUsers_PrimaryAndAlternativeDiffer()
        {
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 1m, 15m),
                Load(Guid.NewGuid(), 5m, 15m),
                Load(Guid.NewGuid(), 8m, 15m)
            };

            var result = await sut.GenerateAsync(users, 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Response.PrimaryUserId);
            Assert.IsNotNull(result.Response.AlternativeUserId);
            Assert.AreNotEqual(result.Response.PrimaryUserId, result.Response.AlternativeUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.Alternative")]
        public void Sync_TwoUsersWithItems_AlternativeIsTheOther()
        {
            var sut = MakeSut();
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();
            var users = new[]
            {
                Load(userA, 1m, 15m),
                Load(userB, 8m, 15m)
            };

            var result = sut.Generate(users, 4);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Response.PrimaryUserId);
            Assert.IsNotNull(result.Response.AlternativeUserId);
            Assert.AreNotEqual(result.Response.PrimaryUserId, result.Response.AlternativeUserId);
        }

        [TestMethod]
        [TestCategory("Migrated.EdgeCase")]
        public void Sync_ZeroDocs_PrimaryIsNull()
        {
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 3m, 15m),
                Load(Guid.NewGuid(), 9m, 15m)
            };

            var result = sut.Generate(users, 0);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Response.PrimaryUserId,
                "With 0 documents nobody receives anything, PrimaryUserId should be null.");
        }

        [TestMethod]
        [TestCategory("Migrated.ResultPayload")]
        public void Sync_TraceIsPopulated()
        {
            var sut = MakeSut(usePriorityAndActivity: true);
            var users = new[]
            {
                Load(Guid.NewGuid(), 1m, 15m, 1, 1),
                Load(Guid.NewGuid(), 5m, 15m, 2, 2)
            };

            var result = sut.Generate(users, 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Response.Trace.Count > 0, "Trace should contain at least one entry.");
        }

        private static DistributionSuggestionHelper<Guid> MakeSut(
            int? maxPerUser = null,
            bool usePriorityAndActivity = false)
        {
            return new DistributionSuggestionHelper<Guid>(new EngineConfig
            {
                MaxPerUser = maxPerUser,
                UsePriorityAndActivity = usePriorityAndActivity
            });
        }

        private static DistributionSuggestionHelper<Guid> MakeSutWithWorkingHours(
            decimal fullWorkDayHours, int maxDocs, int? maxPerUser = null)
        {
            return new DistributionSuggestionHelper<Guid>(new EngineConfig
            {
                MaxPerUser = maxPerUser,
                FullWorkDayHours = fullWorkDayHours,
                MaxAllowedInProcessDocuments = maxDocs
            });
        }

        private static UserLoad<Guid> Load(
            Guid id,
            decimal currentLoad,
            decimal capacity,
            short priority = 0,
            double activityDaysAgo = 0,
            decimal workingHours = 0)
        {
            return new UserLoad<Guid>(id, currentLoad, capacity, priority,
                activityDaysAgo == 0 ? default : DateTime.UtcNow.AddDays(-activityDaysAgo),
                workingHours);
        }
    }
}