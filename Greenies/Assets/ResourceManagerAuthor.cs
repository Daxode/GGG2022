using Unity.Entities;
using UnityEngine;

public class ResourceManagerAuthor : MonoBehaviour
{
    public GameObject resourcePrefab;
    public GameObject greeniePrefab;
    
    class Baker : Baker<ResourceManagerAuthor>
    {
        public override void Bake(ResourceManagerAuthor author)
        {
            AddComponent(new ResourceManager
            {
                ResourcePrefab = GetEntity(author.resourcePrefab),
                GreeniePrefab = GetEntity(author.greeniePrefab)
            });
        }
    }
}

public struct ResourceManager : IComponentData
{
    public Entity ResourcePrefab;
    public Entity GreeniePrefab;
}
