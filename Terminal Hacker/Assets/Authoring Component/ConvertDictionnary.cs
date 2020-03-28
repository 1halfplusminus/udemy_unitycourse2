using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ConvertDictionnary : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private List<string> words;
    [SerializeField] private int level;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {/* 
        dstManager.RemoveComponent<LocalToWorld>(entity);
        dstManager.RemoveComponent<Rotation>(entity);
        dstManager.RemoveComponent<Translation>(entity); */
        dstManager.AddComponentData(entity, new Level() { Value = level });
        dstManager.AddComponentData(entity, new GameDictionnary());
        dstManager.AddBuffer<DirectoryPasswordElement>(entity);
        LoadWords(entity, dstManager);
    }

    void LoadWords(Entity dictionary, EntityManager entityManager)
    {
        var archetype = entityManager.CreateArchetype(typeof(GamePassword));
        var wordsArrays = new NativeArray<Entity>(words.Count, Allocator.Temp);
        entityManager.CreateEntity(archetype, wordsArrays);
        for (int i = 0; i < wordsArrays.Length; i++)
        {
            entityManager.AddComponentData(wordsArrays[i], new GamePassword() { Value = words[i] });
            var buffer = entityManager.GetBuffer<DirectoryPasswordElement>(dictionary).Add(new DirectoryPasswordElement() { Value = wordsArrays[i] });
        }
        wordsArrays.Dispose();
    }
}
