using Unity.Entities;

public struct ActionRecipe
{
    
}


// Left click to pick blob
// Left click another blob to continue blob selection
// Right click directly to do `insta-action` with blobs
// - Insta-action is the action for the tile hit, so walk and do action, or 
// Left click resource to pick up resource
// Right click to start a craft
// Left click block 1. walk to it 2. does action on it empty resources
// Left click block with resource at hand will 1. walk to it. 2. does action on it with resources
// Right click 
// OnBlock x2 = Walk, Action, PickUpResource.
public enum Command
{
    DoAction, 
    WalkToDestination,
    PickUpResource
}

public struct CommandQueue
{
    
}

public enum GreenyState
{
    
}

public enum Action
{
    OnBlock,
    OnResource,
    OnEmptySpace,
    OnGreeny
}

public struct ResourceRecipe
{
    
}

public enum Resource
{
    CO2,
    H2,
    H2O,
    O2,
}