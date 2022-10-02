using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GreenieAuthor : MonoBehaviour
{
    class Baker : Baker<GreenieAuthor>
    {
        public override void Bake(GreenieAuthor author)
        {
            AddComponent(new GreenieData
            {
                State = GreenieState.Idling
            });
        }
    }
}

struct GreenieData : IComponentData
{
    public GreenieState State;
    public FixedList32Bytes<Resource> Resources; // Holds 30 resources
}

public enum GreenieState : byte
{
    Idling,
    Walking,
    Actioning,
}
