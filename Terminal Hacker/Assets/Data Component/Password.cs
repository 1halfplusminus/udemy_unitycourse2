using System;
using Unity.Entities;

[Serializable]
public struct Password : IComponentData
{
    public string Value;
}