using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[InternalBufferCapacity(100)]
public struct DirectoryPasswordElement : IBufferElementData
{
    public Entity Value;
}
