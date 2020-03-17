using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
[Serializable]
public struct GamePassword : IComponentData
{
    public NativeString64 Value;

}