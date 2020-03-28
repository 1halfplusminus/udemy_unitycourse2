using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ConvertMenuText : MonoBehaviour, IConvertGameObjectToEntity
{
    [TextArea(3, 10)]
    public string text;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        /*  dstManager.RemoveComponent<LocalToWorld>(entity);
         dstManager.RemoveComponent<Rotation>(entity);
         dstManager.RemoveComponent<Translation>(entity); */
        dstManager.AddComponentData(entity, new MenuText() { text = text, length = text.Length });
    }
}