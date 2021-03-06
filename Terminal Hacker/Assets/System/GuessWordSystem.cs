﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using System.Linq;

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

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        SelectRandomPasswordWhenNeeded(inputDependencies);
        inputDependencies = CheckPasswordIfNeeded(inputDependencies);
        DeleteFlashMessage(inputDependencies);
        return inputDependencies;
    }
    void DeleteFlashMessage(JobHandle inputDependencies)
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var handle = Entities.WithAny<BadGuess, GoodGuess, NewPasswordHintEvent>().ForEach((int entityInQueryIndex, Entity entity) =>
          {
              commandBuffer.DestroyEntity(entityInQueryIndex, entity);
          }).Schedule(inputDependencies);
        entityCommandBufferSystem.AddJobHandleForProducer(handle);
    }
    void DeleteCurrentPassword(JobHandle inputDependencies)
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var handle = Entities.WithAny<CurrentPassword>().ForEach((int entityInQueryIndex, Entity entity) =>
          {
              commandBuffer.RemoveComponent<CurrentPassword>(entityInQueryIndex, entity);
          }).Schedule(inputDependencies);
        entityCommandBufferSystem.AddJobHandleForProducer(handle);
        handle.Complete();
    }
    (JobHandle, NativeArray<Entity>) CreateFindWordJob(JobHandle inputDependencies)
    {
        var currentLevelEntity = GetSingletonEntity<SelectRandomPassword>();
        var currentLevel = EntityManager.GetComponentData<SelectRandomPassword>(currentLevelEntity);
        NativeArray<Entity> result = new NativeArray<Entity>(1, Allocator.TempJob);
        var job = new FindWordJobBuffer()
        {
            commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            level = currentLevel.level,
            componentDataFromEntity = GetComponentDataFromEntity<GamePassword>(true),
            result = result,
            random = GetRandom()
        };
        var findWordHandle = job.Schedule(this, inputDependencies);
        entityCommandBufferSystem.AddJobHandleForProducer(findWordHandle);
        return (findWordHandle, result);
    }
    JobHandle SelectRandomPasswordWhenNeeded(JobHandle inputDependencies)
    {
        if (HasSingleton<SelectRandomPassword>())
        {
            DeleteCurrentPassword(inputDependencies);
            var (findWordHandle, result) = CreateFindWordJob(inputDependencies);
            entityCommandBufferSystem.AddJobHandleForProducer(findWordHandle);
            findWordHandle.Complete();
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != Entity.Null)
                {
                    GenerateHint(result[i]);
                }
            }
            RemoveSelectRandomPassword();
            result.Dispose();

            return findWordHandle;
        }
        return default;
    }
    JobHandle CheckPasswordIfNeeded(JobHandle inputDependencies)
    {
        var currentPasswords =
        GetEntityQuery(typeof(CurrentPassword), ComponentType.Exclude<CrackedPassword>()).ToComponentDataArrayAsync<CurrentPassword>(Allocator.TempJob, out inputDependencies);
        var menuTexts =
       GetEntityQuery(typeof(MenuText)).ToEntityArray(Allocator.TempJob);
        var job = new CheckPasswordJob()
        {
            directoryPasswords = GetBufferFromEntity<DirectoryPasswordElement>(false),
            commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            currentPasswords = currentPasswords,
            gamePasswords = GetComponentDataFromEntity<GamePassword>(true),
            menuTexts = menuTexts
        };
        var handle = job.Schedule(this, inputDependencies);
        entityCommandBufferSystem.AddJobHandleForProducer(handle);
        return handle;
    }
    void GenerateHint(Entity passwordEntity)
    {
        var seedNext = (uint)UnityEngine.Random.Range(1, 100000);
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
        var password = EntityManager.GetComponentData<GamePassword>(passwordEntity);
        var hintEntity = EntityManager.CreateEntity();
        commandBuffer.AddComponent(hintEntity, new NewPasswordHintEvent() { Value = Scrabble(password.Value.ToString()) });
    }
    void RemoveSelectRandomPassword()
    {
        var currentLevelEntity = GetSingletonEntity<SelectRandomPassword>();
        EntityManager.RemoveComponent<SelectRandomPassword>(currentLevelEntity);
    }
    static string Scrabble(string input)
    {
        return new string(input.ToCharArray().OrderBy(x => System.Guid.NewGuid()).ToArray());
    }
    Random GetRandom()
    {
        return new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1000, 100000));
    }
}

[BurstCompile]
struct FindWordJobBuffer : IJobForEachWithEntity_EBCC<DirectoryPasswordElement, GameDictionnary, Level>
{
    public NativeArray<Entity> result;
    public EntityCommandBuffer.Concurrent commandBuffer;
    public int level;

    [ReadOnly] public ComponentDataFromEntity<GamePassword> componentDataFromEntity;
    public Unity.Mathematics.Random random;
    public void Execute(Entity entity,
    int index,
     DynamicBuffer<DirectoryPasswordElement> directoryPassword,
    [ReadOnly] ref GameDictionnary gameDictionnary,
    [ReadOnly] ref Level dictionnaryLevel)
    {
        if (dictionnaryLevel.Value == level && directoryPassword.Length > 0)
        {
            var randomIndex = random.NextInt(0, directoryPassword.Length);
            var currentPassword = directoryPassword[randomIndex];
            var password = componentDataFromEntity[currentPassword.Value];
            commandBuffer.AddComponent(index, currentPassword.Value, new CurrentPassword()
            {
                password = currentPassword.Value,
                dictionnary = entity,
                index = randomIndex,
            });
            result[0] = currentPassword.Value;
        }
        return;
    }
}


[BurstCompile]
struct CheckPasswordJob : IJobForEachWithEntity<PasswordGuess>
{
    public EntityCommandBuffer.Concurrent commandBuffer;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<CurrentPassword> currentPasswords;
    [DeallocateOnJobCompletion] public NativeArray<Entity> menuTexts;
    [ReadOnly] public ComponentDataFromEntity<GamePassword> gamePasswords;
    [NativeDisableParallelForRestriction] public BufferFromEntity<DirectoryPasswordElement> directoryPasswords;
    public Entity menu;

    public void Execute(
        Entity entity,
        int index,
        [ReadOnly] ref PasswordGuess passwordGuess
    )
    {
        for (int j = 0; j < currentPasswords.Length; j++)
        {
            var currentPassword = currentPasswords[j];
            var password = gamePasswords[currentPassword.password];
            if (passwordGuess.Value.Equals(password.Value))
            {
                var buffer = directoryPasswords[currentPassword.dictionnary];
                buffer.RemoveAt(currentPassword.index);
                var result = commandBuffer.CreateEntity(index);
                commandBuffer.AddComponent(index, result, new GoodGuess() { });
                commandBuffer.AddComponent(index, currentPassword.password, new CrackedPassword() { });
                commandBuffer.RemoveComponent<CurrentPassword>(index, currentPassword.password);
                for (int i = 0; i < menuTexts.Length; i++)
                {
                    commandBuffer.AddComponent(index, menuTexts[i], new ShowMenu() { });
                    UnityEngine.Debug.Log("Show menu");
                    UnityEngine.Debug.Log(menuTexts[i].Index);
                }
            }
            else
            {
                var result = commandBuffer.CreateEntity(index);
                commandBuffer.AddComponent(index, result, new BadGuess() { });
            }
        }
        commandBuffer.DestroyEntity(index, entity);
        return;
    }
}