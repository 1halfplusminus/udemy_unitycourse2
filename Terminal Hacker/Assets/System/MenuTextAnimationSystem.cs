using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class MenuTextAnimationSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;
    override protected void OnCreate()
    {
        entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }
    [BurstCompile]
    struct MenuTextAnimationSystemJob : IJobForEachWithEntity<MenuAnimation, MenuText>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float deltaTime;
        public void Execute(Entity entity, int index, ref MenuAnimation menuAnimation, ref MenuText menuText)
        {
            if (menuAnimation.maxVisibleChar >= menuText.length)
            {
                entityCommandBuffer.RemoveComponent<MenuAnimation>(index, entity);
                return;
            }
            menuAnimation.elapsedTime += deltaTime;
            if (menuAnimation.elapsedTime >= menuAnimation.delay)
            {
                menuAnimation.elapsedTime = 0;
                menuAnimation.maxVisibleChar += 1;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MenuTextAnimationSystemJob()
        {
            deltaTime = UnityEngine.Time.deltaTime,
            entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        var handle = job.Schedule(this, inputDependencies);
        entityCommandBufferSystem.AddJobHandleForProducer(handle);
        return handle;
    }

}