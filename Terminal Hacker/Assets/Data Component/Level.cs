using System;
using Unity.Entities;

[Serializable]
public struct Level : IComponentData
{
    public int Value;
}