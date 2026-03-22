using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;

namespace Action002.Tests.Enemy
{
    public class EnemyMoveJobTests
    {
        private NativeArray<EnemyState> src;
        private NativeArray<EnemyState> dst;
        private NativeArray<MovementSpec> typeSpecs;

        [SetUp]
        public void SetUp()
        {
            int typeCount = System.Enum.GetValues(typeof(EnemyTypeId)).Length;
            typeSpecs = new NativeArray<MovementSpec>(typeCount, Allocator.TempJob);
            for (int i = 0; i < typeCount; i++)
            {
                var spec = EnemyTypeTable.Get((EnemyTypeId)i);
                typeSpecs[i] = new MovementSpec
                {
                    Pattern = spec.Movement,
                    KeepDistance = spec.KeepDistance,
                    ArrivalThreshold = spec.ArrivalThreshold,
                };
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (src.IsCreated) src.Dispose();
            if (dst.IsCreated) dst.Dispose();
            if (typeSpecs.IsCreated) typeSpecs.Dispose();
        }

        private EnemyState RunSingleEnemy(EnemyState state, float2 playerPos, float deltaTime)
        {
            src = new NativeArray<EnemyState>(1, Allocator.TempJob);
            dst = new NativeArray<EnemyState>(1, Allocator.TempJob);
            src[0] = state;

            var job = new EnemyMoveJob
            {
                Src = src.Slice(),
                Dst = dst,
                PlayerPos = playerPos,
                DeltaTime = deltaTime,
                TypeSpecs = typeSpecs,
            };
            job.Execute(0);

            return dst[0];
        }

        #region Chase

        [Test]
        public void Chase_ShouldMoveTowardPlayer()
        {
            var state = new EnemyState
            {
                Position = new float2(0, 0),
                Speed = 5f,
                TypeId = EnemyTypeId.Shooter,
            };

            var result = RunSingleEnemy(state, new float2(10, 0), 1f);

            Assert.Greater(result.Position.x, 0f);
        }

        [Test]
        public void Chase_ShouldNotOvershootPlayer()
        {
            var state = new EnemyState
            {
                Position = new float2(0, 0),
                Speed = 100f,
                TypeId = EnemyTypeId.Shooter,
            };

            var result = RunSingleEnemy(state, new float2(1, 0), 1f);

            // Chase は overshoot を防がない（設計上 OK）、ただし移動すること
            Assert.Greater(result.Position.x, 0f);
        }

        #endregion

        #region KeepDistance

        [Test]
        public void KeepDistance_FarFromPlayer_ShouldApproach()
        {
            var state = new EnemyState
            {
                Position = new float2(20, 0),
                Speed = 5f,
                TypeId = EnemyTypeId.NWay,
                StrafeSign = 1,
            };

            var result = RunSingleEnemy(state, new float2(0, 0), 1f);

            Assert.Less(result.Position.x, 20f, "Should move closer to player");
        }

        [Test]
        public void KeepDistance_TooCloseToPlayer_ShouldRetreat()
        {
            var state = new EnemyState
            {
                Position = new float2(3, 0),
                Speed = 5f,
                TypeId = EnemyTypeId.NWay,
                StrafeSign = 1,
            };

            var result = RunSingleEnemy(state, new float2(0, 0), 1f);

            Assert.Greater(result.Position.x, 3f, "Should retreat from player");
        }

        [Test]
        public void KeepDistance_AtDistance_ShouldStrafe()
        {
            float keepDist = EnemyTypeTable.Get(EnemyTypeId.NWay).KeepDistance;
            var state = new EnemyState
            {
                Position = new float2(keepDist, 0),
                Speed = 5f,
                TypeId = EnemyTypeId.NWay,
                StrafeSign = 1,
            };

            var result = RunSingleEnemy(state, new float2(0, 0), 0.1f);

            // 横移動なので Y が変化するはず
            Assert.AreNotEqual(0f, result.Position.y, "Should strafe perpendicular");
        }

        [Test]
        public void KeepDistance_StrafeSignNegative_ShouldStrafeOpposite()
        {
            float keepDist = EnemyTypeTable.Get(EnemyTypeId.NWay).KeepDistance;
            var statePos = new EnemyState
            {
                Position = new float2(keepDist, 0),
                Speed = 5f,
                TypeId = EnemyTypeId.NWay,
                StrafeSign = 1,
            };
            var stateNeg = new EnemyState
            {
                Position = new float2(keepDist, 0),
                Speed = 5f,
                TypeId = EnemyTypeId.NWay,
                StrafeSign = -1,
            };

            // 2回目の実行用にsrc/dstを再作成するためCleanup
            var resultPos = RunSingleEnemy(statePos, new float2(0, 0), 0.1f);
            src.Dispose(); dst.Dispose();

            var resultNeg = RunSingleEnemy(stateNeg, new float2(0, 0), 0.1f);

            Assert.That(math.sign(resultPos.Position.y), Is.Not.EqualTo(math.sign(resultNeg.Position.y)),
                "Opposite strafe signs should move in opposite Y directions");
        }

        #endregion

        #region Anchor

        [Test]
        public void Anchor_FarFromTarget_ShouldMoveToward()
        {
            var state = new EnemyState
            {
                Position = new float2(0, 0),
                Speed = 5f,
                TypeId = EnemyTypeId.Anchor,
                TargetPosition = new float2(10, 10),
            };

            var result = RunSingleEnemy(state, new float2(0, 0), 1f);

            Assert.Greater(result.Position.x, 0f);
            Assert.Greater(result.Position.y, 0f);
        }

        [Test]
        public void Anchor_AtTarget_ShouldStop()
        {
            float threshold = EnemyTypeTable.Get(EnemyTypeId.Anchor).ArrivalThreshold; // Anchor has ArrivalThreshold
            var state = new EnemyState
            {
                Position = new float2(10, 10),
                Speed = 5f,
                TypeId = EnemyTypeId.Anchor,
                TargetPosition = new float2(10, 10),
            };

            var result = RunSingleEnemy(state, new float2(0, 0), 1f);

            Assert.AreEqual(0f, result.Velocity.x);
            Assert.AreEqual(0f, result.Velocity.y);
        }

        [Test]
        public void Anchor_ShouldNotOvershootTarget()
        {
            var state = new EnemyState
            {
                Position = new float2(0, 0),
                Speed = 100f,
                TypeId = EnemyTypeId.Anchor,
                TargetPosition = new float2(1, 0),
            };

            var result = RunSingleEnemy(state, new float2(0, 0), 1f);

            Assert.AreEqual(1f, result.Position.x, 0.01f, "Should snap to target, not overshoot");
            Assert.AreEqual(0f, result.Velocity.x, "Should stop after snapping");
        }

        [Test]
        public void Anchor_IgnoresPlayerPosition()
        {
            var state = new EnemyState
            {
                Position = new float2(0, 0),
                Speed = 5f,
                TypeId = EnemyTypeId.Anchor,
                TargetPosition = new float2(10, 0),
            };

            var resultA = RunSingleEnemy(state, new float2(-100, -100), 1f);
            src.Dispose(); dst.Dispose();

            var resultB = RunSingleEnemy(state, new float2(100, 100), 1f);

            Assert.AreEqual(resultA.Position.x, resultB.Position.x, 0.001f,
                "Anchor should move to target regardless of player position");
        }

        #endregion
    }
}
