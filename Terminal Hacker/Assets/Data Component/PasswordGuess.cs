using System;
using Unity.Collections;
using Unity.Entities;

[Serializable]
public struct PasswordGuess : IComponentData
{
    public NativeString64 Value;
}
