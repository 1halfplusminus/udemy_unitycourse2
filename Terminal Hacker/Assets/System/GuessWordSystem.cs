using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class GuessWordSystem : JobComponentSystem
{
    [BurstCompile]
    struct FindWordJob : IJobForEachWithEntity_EBCC<DirectoryPasswordElement, GameDictionnary, Level>
    {
        public EntityCommandBuffer.Concurrent commandBuffer;
        public int level;

        public uint seed;

        public void Execute(Entity entity,
        int index,
         DynamicBuffer<DirectoryPasswordElement> directoryPassword,
        [ReadOnly] ref GameDictionnary gameDictionnary,
        [ReadOnly] ref Level dictionnaryLevel)
        {
            if (dictionnaryLevel.Value == level)
            {
                var random = new Unity.Mathematics.Random((uint)seed);
                var randomIndex = random.NextInt(0, directoryPassword.Length - 1);
                var currentPassword = directoryPassword[randomIndex];
                commandBuffer.AddComponent<CurrentPassword>(index, currentPassword.Value, new CurrentPassword() { password = currentPassword.Value });
            }
            return;
        }
    }

    [BurstCompile]
    struct LoadWordJob : IJobForEachWithEntity_EBCC<DirectoryPasswordElement, GameDictionnary, Level>
    {
        public EntityCommandBuffer.Concurrent commandBuffer;
        public int level;

        [ReadOnly] public NativeArray<NativeString64> words;
        public void Execute(Entity entity,
        int index,
         DynamicBuffer<DirectoryPasswordElement> directoryPassword,
        [ReadOnly] ref GameDictionnary gameDictionnary,
        [ReadOnly] ref Level dictionnaryLevel)
        {
            if (dictionnaryLevel.Value == level)
            {
                for (int i = 0; i < words.Length; i++)
                {
                    var wordEntity = commandBuffer.CreateEntity(index);
                    commandBuffer.AddComponent(index, entity, new GamePassword() { Value = words[i] });
                    directoryPassword.Add(new DirectoryPasswordElement() { Value = wordEntity });
                }
            }
            return;
        }
    }
    EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;
    System.Random seed;

    override protected void OnCreate()
    {
        entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        seed = new System.Random();
        RequireForUpdate(GetEntityQuery(typeof(GamePassword)));
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
            /*    var gamePasswords = new NativeList<Entity>(countNotCrackedPassword.CalculateEntityCount(), Allocator.TempJob); */
            var job = new FindWordJob()
            {
                commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                level = currentLevel.level,
                seed = (uint)seed.Next()
            };
            var findWordHandle = job.Schedule(this, inputDependencies);
            entityCommandBufferSystem.AddJobHandleForProducer(inputDependencies);
            findWordHandle.Complete();
            EntityManager.RemoveComponent<SelectRandomPassword>(currentLevelEntity);
            /*   if (gamePasswords.Length > 0)
              {
                  MarkOneAsCurrent(currentLevelEntity, gamePasswords);

              }
              gamePasswords.Dispose(); */
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