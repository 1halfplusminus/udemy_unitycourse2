using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class GameWorldInpuSystem : JobComponentSystem
{
    struct GameWorldInpuSystemJob : IJobForEach<Translation, Rotation>
    {

        public void Execute(ref Translation translation, [ReadOnly] ref Rotation rotation)
        {

        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        throw new System.NotImplementedException();
    }
}