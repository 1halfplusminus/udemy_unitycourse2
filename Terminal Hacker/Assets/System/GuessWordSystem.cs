using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System.Linq;

public class GuessWordSystem : JobComponentSystem
{
    [BurstCompile]
    struct FindWordJob : IJobForEachWithEntity_EBCC<DirectoryPasswordElement, GameDictionnary, Level>
    {
        public NativeArray<Entity> result;
        public EntityCommandBuffer.Concurrent commandBuffer;
        public int level;

        public uint seed;

        [ReadOnly] public ComponentDataFromEntity<GamePassword> componentDataFromEntity;

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
                var password = componentDataFromEntity[currentPassword.Value];
                commandBuffer.AddComponent(index, currentPassword.Value, new CurrentPassword()
                {
                    password = currentPassword.Value,
                });
                result[0] = currentPassword.Value;
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
        static string Scrabble(uint seed, string input)
        {
            var chars = input.ToArray();
            var r = new Unity.Mathematics.Random(seed); ;
            for (int i = 0; i < chars.Length; i++)
            {
                int randomIndex = r.NextInt(0, input.Length);
                char temp = input[randomIndex];
                chars[randomIndex] = chars[i];
                chars[i] = temp;
            }
            return new string(chars);
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
            NativeArray<Entity> result = new NativeArray<Entity>(1, Allocator.TempJob);
            var job = new FindWordJob()
            {
                commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                level = currentLevel.level,
                seed = (uint)seed.Next(),
                componentDataFromEntity = GetComponentDataFromEntity<GamePassword>(true),
                result = result
            };
            var findWordHandle = job.Schedule(this, inputDependencies);
            entityCommandBufferSystem.AddJobHandleForProducer(inputDependencies);
            findWordHandle.Complete();
            EntityManager.RemoveComponent<SelectRandomPassword>(currentLevelEntity);
            for (int i = 0; i < result.Length; i++)
            {
                GenerateHint(result[i]);
            }
            result.Dispose();
            return findWordHandle;
        }
        return default;
    }
    void GenerateHint(Entity entity)
    {
        var seedNext = (uint)seed.Next();
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
        var password = EntityManager.GetComponentData<GamePassword>(entity);
        EntityManager.AddComponentData(entity, new NewPasswordHintEvent() { Value = Scrabble(seedNext, password.Value.ToString()) });
    }
    static string Scrabble(uint seed, string input)
    {
        return new string(input.ToCharArray().OrderBy(x => System.Guid.NewGuid()).ToArray());
    }
    Random GetRandom()
    {
        return new Unity.Mathematics.Random((uint)seed.Next());
    }
}