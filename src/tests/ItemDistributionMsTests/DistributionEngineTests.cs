#region U S A G E S

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.ItemDistribution.Abstractions;
using RzR.ItemDistribution.Abstractions.Pipeline;
using RzR.ItemDistribution.Engine.Pipeline;
using RzR.ItemDistribution.Models.Config;
using RzR.ItemDistribution.Models.Domain;
using RzR.ItemDistribution.Public;
using RzR.ItemDistribution.Strategies.Allocation;
using RzR.ItemDistribution.Strategies.Scoring;
using RzR.ResultMessage;
using RzR.ResultMessage.Abstractions;

#endregion

namespace ItemDistributionMsTests
{
    internal sealed class BlockUserConstraint<TUserId> : IConstraint<TUserId>
    {
        private readonly TUserId _blockedUser;
        public BlockUserConstraint(TUserId blockedUser) => _blockedUser = blockedUser;

        public bool CanAllocate(TUserId userId, int currentAllocated)
            => !userId.Equals(_blockedUser);
    }

    internal sealed class ZeroScoreStep<TUserId> : IDistributionStep<TUserId>
        where TUserId : struct
    {
        private readonly TUserId _targetUser;
        public ZeroScoreStep(TUserId targetUser) => _targetUser = targetUser;

        public IResult Execute(IDistributionContext<TUserId> context)
        {
            if (context.Scores.ContainsKey(_targetUser))
                context.SetScore(_targetUser, 0m);

            return Result.Success();
        }
    }

    internal sealed class SetUnknownScoreStep<TUserId> : IDistributionStep<TUserId>
        where TUserId : struct
    {
        private readonly TUserId _unknownUser;
        public SetUnknownScoreStep(TUserId unknownUser) => _unknownUser = unknownUser;

        public IResult Execute(IDistributionContext<TUserId> context)
        {
            context.SetScore(_unknownUser, 1m);
            return Result.Success();
        }
    }

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
            var users = new[]
            {
                Load(userA, 0m, 10m),
                Load(userB, 8m, 10m)
            };

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
            var users = new[]
            {
                Load(userA, 0m, 10m),
                Load(userB, 8m, 10m)
            };

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
            var users = new[]
            {
                Load(emptyId, 0m, 0m), 
                Load(realId, 0m, 10m)
            };

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
            var users = new[]
            {
                Load(Guid.NewGuid(), 0m, 10m), 
                Load(Guid.NewGuid(), 0m, 10m)
            };

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
            var users = new[]
            {
                Load(Guid.NewGuid(), 0m, 10m)
            };
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
            var result = sut.Generate(new[]
            {
                Load(userId, 0m, 10m)
            }, 7);

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
            var users = new[]
            {
                Load(userId, 0m, 10m, workingHours: 0m)
            };

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
            var result = sut.Generate(new[]
            {
                Load(userId, 2m, 15m)
            }, 3);

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
            var result = await sut.GenerateAsync(new[]
            {
                Load(userId, 2m, 15m)
            }, 3);

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
            Assert.IsNull(result.Response.PrimaryUserId, "No users -> no primary.");
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
                Load(Guid.NewGuid(), 0m, 15m),
                Load(Guid.NewGuid(), 0m, 15m),
                Load(Guid.NewGuid(), 0m, 15m)
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
            // Users start with equal load and capacity so items spread across all three.
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 0m, 15m),
                Load(Guid.NewGuid(), 0m, 15m),
                Load(Guid.NewGuid(), 0m, 15m)
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
                Load(userA, 0m, 15m),
                Load(userB, 0m, 15m)
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

        [TestMethod]
        [TestCategory("New.Correctness")]
        public void Sync_DocumentsExceedTotalRemainingCapacity_AllocatesOnlyAvailableSlots()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 8m, 10m),   // 2 remaining
                Load(Guid.NewGuid(), 7m, 10m),   // 3 remaining
            };
            // Total remaining = 5; requesting 100

            // Act
            var result = sut.Generate(users, 100);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(5, result.Response.TotalAllocated,
                "TotalAllocated must equal the sum of remaining slots (5), not the requested count (100).");
        }

        [TestMethod]
        [TestCategory("New.Correctness")]
        public void Sync_AllUsersAtFullCapacity_TotalAllocatedIsZeroAndPrimaryIsNull()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 10m, 10m),
                Load(Guid.NewGuid(), 5m, 5m),
            };

            // Act
            var result = sut.Generate(users, 10);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Response.TotalAllocated,
                "No items should be allocated when all users are at full capacity.");
            Assert.IsNull(result.Response.PrimaryUserId,
                "PrimaryUserId must be null when nothing was allocated.");
        }

        [TestMethod]
        [TestCategory("New.Correctness")]
        public void Sync_MaxPerUserOne_EachUserReceivesAtMostOne()
        {
            // Arrange
            var sut = MakeSut(maxPerUser: 1);
            var idA = Guid.NewGuid();
            var idB = Guid.NewGuid();
            var idC = Guid.NewGuid();
            var users = new[]
            {
                Load(idA, 0m, 10m),
                Load(idB, 0m, 10m),
                Load(idC, 0m, 10m),
            };

            // Act — request more docs than users so cap is the binding constraint
            var result = sut.Generate(users, 10);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.Response.TotalAllocated,
                "With MaxPerUser=1 and 3 users, total must be capped at 3.");
            Assert.AreEqual(1, result.Response.Allocation[idA], "UserA must receive exactly 1.");
            Assert.AreEqual(1, result.Response.Allocation[idB], "UserB must receive exactly 1.");
            Assert.AreEqual(1, result.Response.Allocation[idC], "UserC must receive exactly 1.");
        }

        [TestMethod]
        [TestCategory("New.Correctness")]
        public void Sync_MaxPerUserThree_FiveUsers_TotalCappedAtFifteen()
        {
            // Arrange
            var sut = MakeSut(maxPerUser: 3);
            var users = Enumerable.Range(0, 5)
                .Select(_ => Load(Guid.NewGuid(), 0m, 20m))
                .ToArray();

            // Act
            var result = sut.Generate(users, 100);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(15, result.Response.TotalAllocated,
                "TotalAllocated must be min(100, 5*3) = 15 when MaxPerUser=3.");
            foreach (var kv in result.Response.Allocation)
                Assert.IsTrue(kv.Value <= 3,
                    $"User {kv.Key} received {kv.Value} items, which exceeds MaxPerUser=3.");
        }

        [TestMethod]
        [TestCategory("New.WorkingHours")]
        public void Sync_WorkingHoursCap_IsEnforced_UserCannotExceedEffectiveCapacity()
        {
            // Arrange
            // FullWorkDayHours=8, MaxAllowedDocs=8
            // User: WorkingHours=2, effectiveCapacity=Truncate(2*8/8)=2
            // Even though raw Capacity=100, user must receive at most 2 items.
            var sut = MakeSutWithWorkingHours(fullWorkDayHours: 8, maxDocs: 8);
            var constrainedId = Guid.NewGuid();
            var users = new[]
            {
                Load(constrainedId, 0m, 100m, workingHours: 2m)
            };

            // Act
            var result = sut.Generate(users, 50);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Response.Allocation[constrainedId] <= 2,
                $"User with effectiveCapacity=2 must not exceed 2, got {result.Response.Allocation[constrainedId]}.");
            Assert.AreEqual(2, result.Response.TotalAllocated,
                "Total allocated must be 2 (the working-hours effective capacity), not 50.");
        }

        [TestMethod]
        [TestCategory("New.WorkingHours")]
        public void Sync_WorkingHoursFractional_EffectiveCapacityTruncated()
        {
            // Arrange
            // WorkingHours=4.5, MaxAllowedDocs=20, FullWorkDayHours=8
            // effectiveCapacity = Truncate(4.5*20/8) = Truncate(11.25) = 11
            var sut = MakeSutWithWorkingHours(fullWorkDayHours: 8, maxDocs: 20);
            var userId = Guid.NewGuid();
            var users = new[]
            {
                Load(userId, 0m, 100m, workingHours: 4.5m)
            };

            // Act
            var result = sut.Generate(users, 50);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(11, result.Response.Allocation[userId],
                "Effective capacity should be Truncate(4.5*20/8) = 11, and all 11 slots should be filled.");
        }

        [TestMethod]
        [TestCategory("New.WorkingHours")]
        public void Sync_WorkingHoursTruncatesToZero_UserReceivesNothing()
        {
            // Arrange
            // WorkingHours=0.5, MaxAllowedDocs=1, FullWorkDayHours=8
            // effectiveCapacity = Truncate(0.5*1/8) = Truncate(0.0625) = 0
            var sut = MakeSutWithWorkingHours(fullWorkDayHours: 8, maxDocs: 1);
            var zeroCapId = Guid.NewGuid();
            var normalId = Guid.NewGuid();
            var users = new[]
            {
                Load(zeroCapId, 0m, 10m, workingHours: 0.5m),
                Load(normalId,  0m, 10m, workingHours: 8m)
            };

            // Act
            var result = sut.Generate(users, 5);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Response.Allocation[zeroCapId],
                "User with effectiveCapacity=0 must receive 0 items.");
        }

        [TestMethod]
        [TestCategory("New.Correctness")]
        public void Sync_AllUsersNegativePriority_ReturnsSuccessWithAllocation()
        {
            // Arrange
            var sut = MakeSut(usePriorityAndActivity: true);
            var users = new[]
            {
                Load(Guid.NewGuid(), 0m, 10m, priority: -3),
                Load(Guid.NewGuid(), 0m, 10m, priority: -1),
                Load(Guid.NewGuid(), 0m, 10m, priority: -1),
            };

            // Act — must not throw DivideByZeroException
            var result = sut.Generate(users, 6);

            // Assert
            Assert.IsTrue(result.IsSuccess,
                "Engine must succeed even when all priorities are negative.");
            Assert.AreEqual(6, result.Response.TotalAllocated,
                "All 6 documents should be allocated despite negative priorities.");
        }

        [TestMethod]
        [TestCategory("New.Correctness")]
        public void Sync_TiedUsers_IntUserId_LowerIdIsConsistentlyPrimary()
        {
            // Arrange — identical load/capacity, int UserIds so tie-break is obvious.
            // UserId 1 < UserId 2, so user 1 should always be PrimaryUserId when tied.
            var sut = new DistributionSuggestionHelper<int>(new EngineConfig());
            var users = new[]
            {
                new UserLoad<int>(2, 5m, 10m),
                new UserLoad<int>(1, 5m, 10m),
            };

            // Act — request 1 doc; both users have identical scores so tie-break by UserId
            var result1 = sut.Generate(users, 1);
            var result2 = sut.Generate(users, 1);

            // Assert
            Assert.IsTrue(result1.IsSuccess);
            Assert.IsTrue(result2.IsSuccess);
            Assert.AreEqual(1, result1.Response.PrimaryUserId,
                "Lower UserId (1) must win tie-break consistently (first run).");
            Assert.AreEqual(result1.Response.PrimaryUserId, result2.Response.PrimaryUserId,
                "Both runs must produce the same PrimaryUserId — the result is deterministic.");
        }

        [TestMethod]
        [TestCategory("New.Correctness")]
        public void Sync_GreedyConcentration_MostAvailableUserReceivesMost()
        {
            // Arrange
            // User loads: 1, 5, 8 — capacity 15 for all.
            // Remaining headroom: 14, 10, 7 respectively.
            // With 3 docs and greedy score*headroom, user with load=1 should win the most.
            var sut = MakeSut();
            var mostAvailableId = Guid.NewGuid();
            var users = new[]
            {
                Load(mostAvailableId, 1m, 15m),   // headroom=14, score=14/15
                Load(Guid.NewGuid(),  5m, 15m),   // headroom=10
                Load(Guid.NewGuid(),  8m, 15m),   // headroom=7
            };

            // Act
            var result = sut.Generate(users, 3);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(mostAvailableId, result.Response.PrimaryUserId,
                "The most-available user (load=1) should be the primary suggestion.");
            Assert.IsTrue(result.Response.Allocation[mostAvailableId] >= 1,
                "The most-available user must receive at least 1 document.");
        }

        [TestMethod]
        [TestCategory("New.Validation")]
        public void Sync_NegativeCapacity_UserLoadConstructor_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new UserLoad<Guid>(Guid.NewGuid(), 0m, -1m),
                "Negative capacity must throw ArgumentOutOfRangeException.");
        }

        [TestMethod]
        [TestCategory("New.Validation")]
        public void Sync_NegativeCurrentLoad_UserLoadConstructor_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new UserLoad<Guid>(Guid.NewGuid(), -1m, 10m),
                "Negative currentLoad must throw ArgumentOutOfRangeException.");
        }

        [TestMethod]
        [TestCategory("New.Validation")]
        public void Sync_NegativeWorkingHours_UserLoadConstructor_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new UserLoad<Guid>(Guid.NewGuid(), 0m, 10m, workingHours: -1m),
                "Negative workingHours must throw ArgumentOutOfRangeException.");
        }

        [TestMethod]
        [TestCategory("New.Validation")]
        public void Sync_UserLoadThrowsOnNegativeCapacity_HelperReturnsFailureIResult()
        {
            var sut = MakeSut();
            var users = Enumerable.Range(0, 1)
                .Select<int, UserLoad<Guid>>(_ =>
                    throw new ArgumentOutOfRangeException("capacity", "must be non-negative."));

            // Act
            var result = sut.Generate(users, 1);

            // Assert
            Assert.IsFalse(result.IsSuccess,
                "A sequence that throws during enumeration must produce a failure IResult.");
        }

        [TestMethod]
        [TestCategory("New.ResultContract")]
        public void Sync_AllocationIsDefensiveCopy_MutatingDoesNotAffectRerun()
        {
            // Arrange
            var sut = MakeSut();
            var userId = Guid.NewGuid();
            var users = new[] { Load(userId, 0m, 10m) };

            // Act
            var result = sut.Generate(users, 3);
            Assert.IsTrue(result.IsSuccess);

            // Mutate the returned allocation
            result.Response.Allocation[userId] = 9999;

            // Run again — the engine must be unaffected
            var result2 = sut.Generate(users, 3);

            // Assert
            Assert.IsTrue(result2.IsSuccess);
            Assert.AreEqual(3, result2.Response.TotalAllocated,
                "Second run must produce the correct allocation; first run's mutation must have no effect.");
        }

        [TestMethod]
        [TestCategory("New.ResultContract")]
        public void Sync_TraceIsReadOnly_AddingEntryThrowsNotSupportedException()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[] { Load(Guid.NewGuid(), 0m, 10m) };
            var result = sut.Generate(users, 1);

            Assert.IsTrue(result.IsSuccess);
            var trace = result.Response.Trace;

            // Assert — ReadOnlyCollection<T> throws NotSupportedException on mutation attempts
            Assert.IsInstanceOfType(trace, typeof(ReadOnlyCollection<DistributionTrace<Guid>>),
                "Trace must be a ReadOnlyCollection<T>.");
            Assert.ThrowsException<NotSupportedException>(
                () => ((IList<DistributionTrace<Guid>>)trace).Add(new DistributionTrace<Guid>()),
                "Adding to the trace must throw NotSupportedException.");
        }

        [TestMethod]
        [TestCategory("New.Validation")]
        public void Sync_DuplicateUserIds_ReturnsFailureIResult()
        {
            // Arrange
            var sut = MakeSut();
            var duplicateId = Guid.NewGuid();
            var users = new[]
            {
                Load(duplicateId, 0m, 10m),
                Load(duplicateId, 2m, 10m), // same ID
            };

            // Act
            var result = sut.Generate(users, 5);

            // Assert
            Assert.IsFalse(result.IsSuccess,
                "Duplicate user IDs must produce a failure IResult.");
        }

        [TestMethod]
        [TestCategory("New.Validation")]
        public async Task Async_PreCancelledToken_ReturnsFailureIResult_DoesNotThrow()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[] { Load(Guid.NewGuid(), 0m, 10m) };
            var cancelledToken = new CancellationToken(canceled: true);

            // Act — must NOT throw; must return a failure IResult
            var result = await sut.GenerateAsync(users, 5, cancelledToken);

            // Assert
            Assert.IsFalse(result.IsSuccess,
                "A pre-cancelled token must produce a failure IResult, not throw.");
        }

        [TestMethod]
        [TestCategory("New.Correctness")]
        public void Sync_LazyIEnumerableInput_AllocationIsCorrectNotEmpty()
        {
            // Arrange
            var sut = new DistributionSuggestionHelper<int>(new EngineConfig());
            // Build a lazy sequence — never materialised before engine sees it
            IEnumerable<UserLoad<int>> LazyUsers()
            {
                yield return new UserLoad<int>(1, 0m, 10m);
                yield return new UserLoad<int>(2, 0m, 10m);
            }

            // Act
            var result = sut.Generate(LazyUsers(), 4);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(4, result.Response.TotalAllocated,
                "All 4 documents must be allocated even when input is a lazy IEnumerable.");
            Assert.IsTrue(result.Response.Allocation.ContainsKey(1),
                "Lazy sequence user 1 must appear in the allocation.");
            Assert.IsTrue(result.Response.Allocation.ContainsKey(2),
                "Lazy sequence user 2 must appear in the allocation.");
        }

        [TestMethod]
        [TestCategory("New.Extensibility")]
        public void Sync_GenericConfigWithNoExtensions_ProducesIdenticalResultToBaseConfig()
        {
            // Arrange
            var baseConfig = new EngineConfig();
            var genericConfig = new EngineConfig<Guid>
            {
                AdditionalSteps = null,
                AdditionalConstraints = null
            };

            var sutBase = new DistributionSuggestionHelper<Guid>(baseConfig);
            var sutGeneric = new DistributionSuggestionHelper<Guid>(genericConfig);

            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();
            var users = new[] { Load(userA, 1m, 10m), Load(userB, 3m, 10m) };

            // Act
            var baseResult = sutBase.Generate(users, 6);
            var genericResult = sutGeneric.Generate(users, 6);

            // Assert
            Assert.IsTrue(baseResult.IsSuccess);
            Assert.IsTrue(genericResult.IsSuccess);
            Assert.AreEqual(baseResult.Response.TotalAllocated, genericResult.Response.TotalAllocated,
                "Generic config with no extensions must produce the same total as the base config.");
            Assert.AreEqual(baseResult.Response.Allocation[userA], genericResult.Response.Allocation[userA],
                "Allocation for userA must match between base and generic configs.");
            Assert.AreEqual(baseResult.Response.Allocation[userB], genericResult.Response.Allocation[userB],
                "Allocation for userB must match between base and generic configs.");
        }

        [TestMethod]
        [TestCategory("New.Extensibility")]
        public void Sync_CustomConstraintBlocksUser_BlockedUserReceivesZero()
        {
            // Arrange
            var blockedId = Guid.NewGuid();
            var allowedId = Guid.NewGuid();
            var config = new EngineConfig<Guid>
            {
                AdditionalConstraints = new List<IConstraint<Guid>>
                {
                    new BlockUserConstraint<Guid>(blockedId)
                }
            };
            var sut = new DistributionSuggestionHelper<Guid>(config);
            var users = new[]
            {
                Load(blockedId, 0m, 10m),
                Load(allowedId, 0m, 10m),
            };

            // Act
            var result = sut.Generate(users, 5);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Response.Allocation[blockedId],
                "Blocked user must receive 0 items.");
            Assert.AreEqual(5, result.Response.Allocation[allowedId],
                "Allowed user must receive all 5 items.");
        }

        [TestMethod]
        [TestCategory("New.Extensibility")]
        public void Sync_CustomStepZeroesScore_DeprioritisedUserReceivesFewest()
        {
            // Arrange
            // User A and B have identical load/capacity. Without the custom step they'd tie.
            // The custom step zeroes User A's score. User B should then receive all docs when
            // the headroom*score ranking is applied (0 * headroom = 0 for A).
            // Note: a score of 0 means score*headroom = 0 for A; B's product is positive, so B wins.
            var deprioritisedId = Guid.NewGuid();
            var normalId = Guid.NewGuid();
            var config = new EngineConfig<Guid>
            {
                AdditionalSteps = new List<IDistributionStep<Guid>>
                {
                    new ZeroScoreStep<Guid>(deprioritisedId)
                }
            };
            var sut = new DistributionSuggestionHelper<Guid>(config);
            var users = new[]
            {
                Load(deprioritisedId, 0m, 10m),
                Load(normalId,        0m, 10m),
            };

            // Act
            var result = sut.Generate(users, 5);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Response.Allocation[deprioritisedId],
                "User with zeroed score (score*headroom=0) must be ranked below normalId and receive no items.");
            Assert.AreEqual(5, result.Response.Allocation[normalId],
                "Normal user must receive all 5 items because the deprioritised user's score is 0.");
        }

        [TestMethod]
        [TestCategory("New.AllocationStatus")]
        public void Sync_PartialAllocation_IsFullyAllocatedFalse_UnallocatedMatchesDifference()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 5m, 10m),  // remaining = 5
                Load(Guid.NewGuid(), 5m, 10m),  // remaining = 5 -> combined = 10
            };
            const int requested = 15;

            // Act
            var result = sut.Generate(users, requested);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var r = result.Response;
            Assert.AreEqual(requested, r.RequestedDocuments,
                "RequestedDocuments must echo the value passed to Generate.");
            Assert.AreEqual(10, r.TotalAllocated,
                "TotalAllocated must equal total remaining capacity (10), not the requested count (15).");
            Assert.AreEqual(5, r.UnallocatedDocuments,
                "UnallocatedDocuments must equal RequestedDocuments - TotalAllocated = 15 - 10 = 5.");
            Assert.IsFalse(r.IsFullyAllocated,
                "IsFullyAllocated must be false when UnallocatedDocuments > 0.");
        }

        [TestMethod]
        [TestCategory("New.AllocationStatus")]
        public async Task Async_PartialAllocation_IsFullyAllocatedFalse_UnallocatedMatchesDifference()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 5m, 10m),
                Load(Guid.NewGuid(), 5m, 10m),
            };
            const int requested = 15;

            // Act
            var result = await sut.GenerateAsync(users, requested);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var r = result.Response;
            Assert.AreEqual(requested, r.RequestedDocuments,
                "RequestedDocuments must echo the value passed to GenerateAsync.");
            Assert.AreEqual(10, r.TotalAllocated,
                "TotalAllocated must equal total remaining capacity (10), not the requested count (15).");
            Assert.AreEqual(5, r.UnallocatedDocuments,
                "UnallocatedDocuments must equal RequestedDocuments - TotalAllocated = 15 - 10 = 5.");
            Assert.IsFalse(r.IsFullyAllocated,
                "IsFullyAllocated must be false when UnallocatedDocuments > 0.");
        }

        [TestMethod]
        [TestCategory("New.AllocationStatus")]
        public void Sync_FullAllocation_IsFullyAllocatedTrue_UnallocatedIsZero()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 0m, 20m),
                Load(Guid.NewGuid(), 0m, 20m),
            };
            const int requested = 10;

            // Act
            var result = sut.Generate(users, requested);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var r = result.Response;
            Assert.AreEqual(requested, r.RequestedDocuments,
                "RequestedDocuments must equal the value passed to Generate.");
            Assert.AreEqual(requested, r.TotalAllocated,
                "TotalAllocated must equal the requested count when capacity is sufficient.");
            Assert.AreEqual(0, r.UnallocatedDocuments,
                "UnallocatedDocuments must be 0 when all documents were placed.");
            Assert.IsTrue(r.IsFullyAllocated,
                "IsFullyAllocated must be true when UnallocatedDocuments == 0.");
        }

        [TestMethod]
        [TestCategory("New.AllocationStatus")]
        public void Sync_ZeroDocuments_RequestedIsZero_IsFullyAllocatedTrue_UnallocatedIsZero()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 0m, 10m),
                Load(Guid.NewGuid(), 0m, 10m),
            };

            // Act
            var result = sut.Generate(users, 0);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var r = result.Response;
            Assert.AreEqual(0, r.RequestedDocuments,
                "RequestedDocuments must be 0 when 0 was passed to Generate.");
            Assert.AreEqual(0, r.TotalAllocated,
                "TotalAllocated must be 0 when 0 documents were requested.");
            Assert.AreEqual(0, r.UnallocatedDocuments,
                "UnallocatedDocuments must be 0 when no documents were requested.");
            Assert.IsTrue(r.IsFullyAllocated,
                "IsFullyAllocated must be true when 0 requested and 0 unallocated.");
        }

        [TestMethod]
        [TestCategory("New.AllocationStatus")]
        public void Sync_AllUsersAtFullCapacity_NonZeroRequest_UnallocatedEqualsRequested_IsFullyAllocatedFalse()
        {
            // Arrange
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 10m, 10m),  // no headroom
                Load(Guid.NewGuid(), 5m,  5m),   // no headroom
            };
            const int requested = 7;

            // Act
            var result = sut.Generate(users, requested);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var r = result.Response;
            Assert.AreEqual(requested, r.RequestedDocuments,
                "RequestedDocuments must echo the value passed to Generate.");
            Assert.AreEqual(0, r.TotalAllocated,
                "TotalAllocated must be 0 when all users are at full capacity.");
            Assert.AreEqual(requested, r.UnallocatedDocuments,
                "UnallocatedDocuments must equal RequestedDocuments when nothing could be allocated.");
            Assert.IsFalse(r.IsFullyAllocated,
                "IsFullyAllocated must be false when no documents could be placed.");
        }

        [TestMethod]
        [TestCategory("New.AllocationStatus")]
        public void Sync_RequestedDocumentsAlwaysEchoesInput_DistinctFromTotalAllocatedOnPartialRun()
        {
            // Arrange — partial allocation scenario: 3 users with 2 slots each -> 6 total, request 9.
            var sut = MakeSut();
            var users = new[]
            {
                Load(Guid.NewGuid(), 8m, 10m),  // remaining = 2
                Load(Guid.NewGuid(), 8m, 10m),  // remaining = 2
                Load(Guid.NewGuid(), 8m, 10m),  // remaining = 2 -> combined = 6
            };
            const int requested = 9;

            // Act
            var result = sut.Generate(users, requested);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var r = result.Response;
            Assert.AreEqual(requested, r.RequestedDocuments,
                "RequestedDocuments must be exactly the int passed to Generate (9).");
            // On a partial run, RequestedDocuments != TotalAllocated — confirm they differ.
            Assert.AreNotEqual(r.RequestedDocuments, r.TotalAllocated,
                "On a partial run, RequestedDocuments must differ from TotalAllocated.");
            Assert.AreEqual(r.RequestedDocuments - r.TotalAllocated, r.UnallocatedDocuments,
                "UnallocatedDocuments must equal the arithmetic difference.");
        }

        [TestMethod]
        [TestCategory("New.BuildPipelineGuards")]
        public void Ctor_GenericConfig_AdditionalStepsContainsNullEntry_ThrowsArgumentException()
        {
            // Arrange
            var config = new EngineConfig<Guid>
            {
                AdditionalSteps = new List<IDistributionStep<Guid>> { null }
            };

            // Act / Assert
            Assert.ThrowsException<ArgumentException>(
                () => new DistributionSuggestionHelper<Guid>(config),
                "A null entry in AdditionalSteps must throw ArgumentException.");
        }

        [TestMethod]
        [TestCategory("New.BuildPipelineGuards")]
        public void Ctor_GenericConfig_AdditionalStepsContainsFairAllocationStep_ThrowsArgumentException()
        {
            // Arrange
            var config = new EngineConfig<Guid>
            {
                AdditionalSteps = new List<IDistributionStep<Guid>>
                {
                    new FairAllocationStep<Guid>(new List<IConstraint<Guid>>())
                }
            };

            // Act / Assert
            Assert.ThrowsException<ArgumentException>(
                () => new DistributionSuggestionHelper<Guid>(config),
                "A FairAllocationStep entry in AdditionalSteps must throw ArgumentException.");
        }

        [TestMethod]
        [TestCategory("New.BuildPipelineGuards")]
        public void Ctor_GenericConfig_AdditionalConstraintsContainsNullEntry_ThrowsArgumentException()
        {
            // Arrange
            var config = new EngineConfig<Guid>
            {
                AdditionalConstraints = new List<IConstraint<Guid>> { null }
            };

            // Act / Assert
            Assert.ThrowsException<ArgumentException>(
                () => new DistributionSuggestionHelper<Guid>(config),
                "A null entry in AdditionalConstraints must throw ArgumentException.");
        }

        [TestMethod]
        [TestCategory("New.KeyMissingGuard")]
        public void Sync_CustomStepSetsScoreForUnknownUser_ContextThrows_ReturnsFailure()
        {
            // Arrange
            var knownId = Guid.NewGuid();
            var unknownId = Guid.NewGuid(); // not in the users collection
            var config = new EngineConfig<Guid>
            {
                AdditionalSteps = new List<IDistributionStep<Guid>>
                {
                    new SetUnknownScoreStep<Guid>(unknownId)
                }
            };
            var sut = new DistributionSuggestionHelper<Guid>(config);
            var users = new[]
            {
                Load(knownId, 0m, 10m),
            };

            // Act
            var result = sut.Generate(users, 3);

            // Assert — IDistributionContext.SetScore throws InvalidOperationException for unknownId,
            // which DistributionSuggestionHelper.Generate catches and wraps as a failure IResult.
            Assert.IsFalse(result.IsSuccess,
                "Calling SetScore with a user ID not in the context must cause the step to throw, " +
                "which the helper must surface as IsSuccess == false.");
        }

        [TestMethod]
        [TestCategory("New.ClockSkewClamp")]
        public void Sync_FutureDatedUser_ClampedToZeroAge_ReceivesMoreThanOldUser()
        {
            // Arrange
            var sut = MakeSut(usePriorityAndActivity: true);
            var futureId = Guid.NewGuid();  // LastActivityDate = 30 days in the future; clamped to age 0
            var nowId = Guid.NewGuid();  // LastActivityDate = UtcNow; raw age ≈ 0, clamp has no effect
            var oldId = Guid.NewGuid();  // LastActivityDate = 90 days in the past; high age, low score

            var users = new[]
            {
                // activityDaysAgo=-30 means future date; Load() passes DateTime.UtcNow.AddDays(-(-30))
                // = AddDays(+30) which is genuinely future.
                new UserLoad<Guid>(futureId, 0m, 20m, 1,
                    DateTime.UtcNow.AddDays(30), 0m),
                new UserLoad<Guid>(nowId, 0m, 20m, 1,
                    DateTime.UtcNow, 0m),
                new UserLoad<Guid>(oldId, 0m, 20m, 1,
                    DateTime.UtcNow.AddDays(-90), 0m),
            };

            // Act
            var result = sut.Generate(users, 9);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            // The future-dated user is clamped to age 0 -> activityScore = 1.
            // The old user (90 days ago) has a substantially lower activityScore.
            // Both the future-dated and now-dated users must each receive more than the old user.
            Assert.IsTrue(result.Response.Allocation[futureId] > result.Response.Allocation[oldId],
                $"Future-dated user (clamped age=0, score~1) must receive more items than old user (score<1). " +
                $"future={result.Response.Allocation[futureId]}, old={result.Response.Allocation[oldId]}.");
            Assert.IsTrue(result.Response.Allocation[nowId] > result.Response.Allocation[oldId],
                $"Now-dated user (age~0, score~1) must receive more items than old user. " +
                $"now={result.Response.Allocation[nowId]}, old={result.Response.Allocation[oldId]}.");
        }

        [TestMethod]
        [TestCategory("New.Cancellation")]
        public async Task Async_PreCancelledToken_LargeDocumentCount_ReturnsFailureIResult_DoesNotThrow()
        {
            // Arrange
            var sut = MakeSut();
            // Ten users with large capacity — without cancellation all 1000 docs would be allocated.
            var users = Enumerable.Range(0, 10)
                .Select(_ => Load(Guid.NewGuid(), 0m, 200m))
                .ToArray();
            var cancelledToken = new CancellationToken(canceled: true);

            // Act — must NOT throw; cancellation must be surfaced as a failure IResult.
            var result = await sut.GenerateAsync(users, 1000, cancelledToken);

            // Assert
            Assert.IsFalse(result.IsSuccess,
                "A pre-cancelled token with a large document count must produce IsSuccess == false; " +
                "the allocation loop's ThrowIfCancellationRequested must short-circuit immediately.");
        }

        // Setup (fixed clock t = 2026-01-01 00:00:00 UTC):
        //   User A: lastActivityDate = t + 30d, raw ageDays = -30  (future-dated)
        //   User B: lastActivityDate = t − 90d, raw ageDays = +90
        //
        // maxAgeDays = max(-30, 90) = 90 (computed by the step over all users before clamping)
        //
        // Clamping for A:  clampedAge = max(-30, 0) then min(0, 90) = 0
        //   activityScore_A = (90 − 0) / 90 = 1.0
        //   WITHOUT the clamp it would be (90 − (−30)) / 90 = 120/90 ≈ 1.333, violation of [0,1]
        //
        // Clamping for B:  clampedAge = 90
        //   activityScore_B = (90 − 90) / 90 = 0.0
        //
        // Both users share the same priority (0), so priorityRange = 0, priorityScore = 1m.
        //   adjustment_A = (1 + 1) / 2 = 1.0, adjustedScore_A = baseScore 1.0 × 1.0 = 1.0m
        //   adjustment_B = (1 + 0) / 2 = 0.5, adjustedScore_B = baseScore 1.0 × 0.5 = 0.5m
        //
        // The double-to-decimal cast for activity scores uses whole-number double operands
        // (90.0 / 90.0 = 1.0, 0.0 / 90.0 = 0.0), so the exact decimal assertion is safe.
        [TestMethod]
        [TestCategory("New.ClockSkewClamp.WhiteBox")]
        public void Sync_PriorityActivityScoringStep_FutureDatedUser_ClampedToZeroAge_ExactScoreAssertion()
        {
            // Arrange
            var t = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var idA = Guid.NewGuid(); // future-dated
            var idB = Guid.NewGuid(); // old

            var userA = new UserLoad<Guid>(idA, currentLoad: 0m, capacity: 10m, priority: 0,
                lastActivityDate: t.AddDays(30), workingHours: 0m);
            var userB = new UserLoad<Guid>(idB, currentLoad: 0m, capacity: 10m, priority: 0,
                lastActivityDate: t.AddDays(-90), workingHours: 0m);

            var ctx = new DistributionContext<Guid>(
                new[] { userA, userB },
                documents: 5);

            // Base load scoring: capacity=10, currentLoad=0 -> score = 1.0m for both
            new LoadScoringStep<Guid>().Execute(ctx);

            Assert.AreEqual(1.0m, ctx.Scores[idA], "LoadScoringStep must set base score 1.0m for user A.");
            Assert.AreEqual(1.0m, ctx.Scores[idB], "LoadScoringStep must set base score 1.0m for user B.");

            // Inject fixed clock into PriorityActivityScoringStep via the internal seam
            var step = new PriorityActivityScoringStep<Guid>();
            step.Clock = () => t;

            // Act
            var stepResult = step.Execute(ctx);

            // Assert
            Assert.IsTrue(stepResult.IsSuccess, "PriorityActivityScoringStep.Execute must succeed.");

            // User A (future-dated): ageDays raw = -30, clamped to 0.
            // activityScore = (90 − 0) / 90 = 1.0 (double 1.0 -> decimal 1.0m, exact).
            // adjustment = (priorityScore 1 + activityScore 1) / 2 = 1.0m.
            // adjustedScore = 1.0m × 1.0m = 1.0m.
            // Key invariant: the clamp prevents the over-boost that would result from a negative ageDays.
            Assert.AreEqual(1.0m, ctx.Scores[idA],
                "Future-dated user A must be clamped to ageDays=0; adjustedScore must be exactly 1.0m (not >1).");

            // User B (90 days old): activityScore = 0.0, adjustment = 0.5m, adjustedScore = 0.5m.
            Assert.AreEqual(0.5m, ctx.Scores[idB],
                "Old user B (90 days ago) must have activityScore=0, adjustment=0.5, adjustedScore=0.5m.");

            // The clamp guarantee: the future-dated user must not have received a score above 1.
            Assert.IsTrue(ctx.Scores[idA] <= 1.0m,
                "Clamp must prevent adjustedScore from exceeding 1.0m (no over-boost from negative ageDays).");

            // Relative ordering: A was clamped to max activity (score 1.0m); B has low activity (score 0.5m).
            Assert.IsTrue(ctx.Scores[idA] > ctx.Scores[idB],
                "Future-dated (clamped) user A must outscore old user B.");
        }

        [TestMethod]
        [TestCategory("New.UseWorkingHoursCapacity")]
        public void Sync_UseWorkingHoursCapacity_True_ValidParams_CapIsEnforced()
        {
            // Arrange — enable via the explicit flag (not just MaxAllowedInProcessDocuments > 0)
            var sut = new DistributionSuggestionHelper<Guid>(new EngineConfig
            {
                UseWorkingHoursCapacity = true,
                MaxAllowedInProcessDocuments = 8,
                FullWorkDayHours = 8
            });
            var constrainedId = Guid.NewGuid();
            var users = new[]
            {
                Load(constrainedId, 0m, 100m, workingHours: 2m)
            };

            // Act
            var result = sut.Generate(users, 50);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            // effectiveCapacity = Truncate(2 * 8 / 8) = 2
            Assert.IsTrue(result.Response.Allocation[constrainedId] <= 2,
                $"User with effectiveCapacity=2 must not exceed 2, got {result.Response.Allocation[constrainedId]}.");
            Assert.AreEqual(2, result.Response.TotalAllocated,
                "Working-hours cap must limit total allocation to 2 when UseWorkingHoursCapacity=true.");
        }

        [TestMethod]
        [TestCategory("New.UseWorkingHoursCapacity")]
        public void Ctor_UseWorkingHoursCapacity_True_MaxDocsZero_ThrowsArgumentOutOfRange()
        {
            // Arrange
            var config = new EngineConfig
            {
                UseWorkingHoursCapacity = true,
                MaxAllowedInProcessDocuments = 0,  // invalid when feature is active
                FullWorkDayHours = 8
            };

            // Act / Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new DistributionSuggestionHelper<Guid>(config),
                "UseWorkingHoursCapacity=true with MaxAllowedInProcessDocuments=0 must throw ArgumentOutOfRangeException.");
        }

        [TestMethod]
        [TestCategory("New.UseWorkingHoursCapacity")]
        public void Ctor_UseWorkingHoursCapacity_True_FullWorkDayHoursZero_ThrowsArgumentOutOfRange()
        {
            // Arrange
            var config = new EngineConfig
            {
                UseWorkingHoursCapacity = true,
                MaxAllowedInProcessDocuments = 20,
                FullWorkDayHours = 0  // invalid when feature is active
            };

            // Act / Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new DistributionSuggestionHelper<Guid>(config),
                "UseWorkingHoursCapacity=true with FullWorkDayHours=0 must throw ArgumentOutOfRangeException.");
        }

        [TestMethod]
        [TestCategory("New.UseWorkingHoursCapacity")]
        public void Sync_UseWorkingHoursCapacity_False_MaxDocsPositive_CapStillApplied()
        {
            // Arrange — old-style activation: UseWorkingHoursCapacity stays false (default),
            // MaxAllowedInProcessDocuments > 0 alone triggers the step.
            var sut = new DistributionSuggestionHelper<Guid>(new EngineConfig
            {
                UseWorkingHoursCapacity = false,  // not set — rely on MaxAllowedInProcessDocuments
                MaxAllowedInProcessDocuments = 8,
                FullWorkDayHours = 8
            });
            var constrainedId = Guid.NewGuid();
            var users = new[]
            {
                Load(constrainedId, 0m, 100m, workingHours: 2m)
            };

            // Act
            var result = sut.Generate(users, 50);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            // effectiveCapacity = Truncate(2 * 8 / 8) = 2; same cap as Scenario 31
            Assert.IsTrue(result.Response.Allocation[constrainedId] <= 2,
                $"Back-compat: MaxAllowedInProcessDocuments>0 alone must still enforce the working-hours cap. " +
                $"Got {result.Response.Allocation[constrainedId]}, expected <= 2.");
            Assert.AreEqual(2, result.Response.TotalAllocated,
                "OR-gate must still activate the step when UseWorkingHoursCapacity=false but MaxAllowedInProcessDocuments>0.");
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

        private static UserLoad<Guid> Load(Guid id, decimal currentLoad, decimal capacity,
            short priority = 0, double activityDaysAgo = 0, decimal workingHours = 0)
        {
            return new UserLoad<Guid>(id, currentLoad, capacity, priority,
                activityDaysAgo == 0 ? default : DateTime.UtcNow.AddDays(-activityDaysAgo),
                workingHours);
        }
    }
}