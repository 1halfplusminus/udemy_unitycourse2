using Unity.Collections;
using Unity.Entities;
using System.Linq;

public class DotsMenu
{
    private static EntityQueryDesc GetEntityDescription()
    {
        var queryDesc = new EntityQueryDesc
        {
            Any = new ComponentType[] { typeof(ShowMenu), typeof(MenuAnimation) },
            All = new ComponentType[] { typeof(MenuText) }
        };
        return queryDesc;
    }
    public static (bool found, MenuText text, Entity entity) TryGetMenuToShow(EntityManager em)
    {
        return QueryMenu(em, GetEntityDescription());
    }
    public static (bool found, MenuAnimation animation) GetAnimation(EntityManager em, Entity entity)
    {
        if (em.HasComponent<MenuAnimation>(entity))
        {
            return (true, em.GetComponentData<MenuAnimation>(entity));
        }
        return (false, default);
    }
    public static (bool found, MenuText text, Entity entity) TryGetMenuToShow(EntityManager em, ComponentType tag)
    {
        var desc = GetEntityDescription();
        desc.All.Append(tag);
        return QueryMenu(em, desc);
    }
    private static (bool found, MenuText text, Entity entity) QueryMenu(EntityManager em, EntityQueryDesc queryDesc)
    {
        var query = em.CreateEntityQuery(queryDesc);
        var entities = query.ToEntityArray(Allocator.TempJob);
        var entity = entities.FirstOrDefault();
        entities.Dispose();
        if (entity != Entity.Null)
        {
            var menuText = em.GetComponentData<MenuText>(entity);
            var animation = em.HasComponent<MenuAnimation>(entity) ? em.GetComponentData<MenuAnimation>(entity) : default;
            em.RemoveComponent<ShowMenu>(entity);
            return (found: true, text: menuText, entity: entity);
        }
        return (false, default, default);
    }
}