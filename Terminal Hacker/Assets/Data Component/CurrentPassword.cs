﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct CurrentPassword : IComponentData
{
    public Entity password;
}