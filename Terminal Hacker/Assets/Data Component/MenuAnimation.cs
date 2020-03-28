using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]
[GenerateAuthoringComponent]
public struct MenuAnimation : IComponentData
{
    [NonSerialized] public float elapsedTime;
    [NonSerialized] public int maxVisibleChar;
    public float delay;
}
