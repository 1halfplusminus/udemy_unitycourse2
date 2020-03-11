using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class GuessWordSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;
    System.Random seed;

    override protected void OnCreate()
    {
        entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        seed = new System.Random();
        RequireForUpdate(GetEntityQuery(typeof(GamePassword)));
    }
    [BurstCompile]
    [ExcludeComponent(typeof(CrackedPassword))]
    struct FindWordJob : IJobForEachWithEntity<GamePassword>
    {
        public EntityCommandBuffer.Concurrent commandBuffer;
        public int level;

        public NativeList<Entity>.ParallelWriter gamePasswords;
        public void Execute(Entity entity, int index, [ReadOnly]ref GamePassword gamePassword)
        {
            if (gamePassword.level == level)
            {
                gamePasswords.AddNoResize(entity);
            }
            return;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        SelectRandomPasswordWhenNeeded(inputDependencies);
        return inputDependencies;
    }
    JobHandle SelectRandomPasswordWhenNeeded(JobHandle inputDependencies)
    {
        if (HasSingleton<SelectRandomPassword>())
        {
            var currentLevelEntity = GetSingletonEntity<SelectRandomPassword>();
            var currentLevel = EntityManager.GetComponentData<SelectRandomPassword>(currentLevelEntity);
            var countNotCrackedPassword = GetEntityQuery(typeof(GamePassword), ComponentType.Exclude<CrackedPassword>());
            var gamePasswords = new NativeList<Entity>(countNotCrackedPassword.CalculateEntityCount(), Allocator.TempJob);
            var job = new FindWordJob()
            {
                commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                level = currentLevel.level,
                gamePasswords = gamePasswords.AsParallelWriter()
            };
            var findWordHandle = job.Schedule(this, inputDependencies);
            entityCommandBufferSystem.AddJobHandleForProducer(inputDependencies);
            findWordHandle.Complete();
            if (gamePasswords.Length > 0)
            {
                MarkOneAsCurrent(currentLevelEntity, gamePasswords);

            }
            gamePasswords.Dispose();
            return findWordHandle;
        }
        return default;
    }
    void MarkOneAsCurrent(Entity currentLevelEntity, NativeList<Entity> gamePasswords)
    {
        var rnd = GetRandom();
        var currentPasswordEntity = gamePasswords[rnd.NextInt(0, gamePasswords.Length)];
        var password = EntityManager.GetComponentData<GamePassword>(currentPasswordEntity);
        EntityManager.AddComponentData(currentLevelEntity, new CurrentPassword() { password = currentPasswordEntity });
        EntityManager.RemoveComponent<SelectRandomPassword>(currentLevelEntity);
    }
    Random GetRandom()
    {
        return new Unity.Mathematics.Random((uint)seed.Next());
    }
}