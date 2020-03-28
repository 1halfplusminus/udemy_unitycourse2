using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]

public struct MenuText : IComponentData
{
    public int id;
    public int length;
    public NativeString4096 text;
}
