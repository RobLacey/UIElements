using System.Collections.Generic;
using EZ.Events;
using EZ.Inject;
using UIElements;

public class HistoryBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //History
       // AutoRemove().CreateEvent<IReturnToHome>();
        AutoRemove().CreateEvent<IIsAtRootTrunk>();
        AutoRemove().CreateEvent<IOnStart>();
        AutoRemove().CreateEvent<IGameIsPaused>();
        AutoRemove().CreateEvent<IInMenu>();
        //AutoRemove().CreateEvent<IReturnHomeGroupIndex>();
        // AutoRemove().CreateEvent<IAddTrunk>();
        // AutoRemove().CreateEvent<IRemoveTrunk>();
        
        //Node
        AutoRemove().CreateEvent<IHighlightedNode>();
        AutoRemove().CreateEvent<ISelectedNode>();
        AutoRemove().CreateEvent<IDisabledNode>();
       // AutoRemove().CreateEvent<IStoreNodeHistoryData>();
        AutoRemove().CreateEvent<ISceneIsChanging>();
        
        //Branch
        //AutoRemove().CreateEvent<IActiveBranch>();
        // AutoRemove().CreateEvent<IAddActiveBranch>();
        // AutoRemove().CreateEvent<IRemoveActiveBranch>();
    }
}

//public interface IReturnToHome { }

public interface IIsAtRootTrunk { }

public interface IOnStart { }

public interface IGameIsPaused
{
    bool IsPaused { get; }
}

public interface IInMenu
{
    bool InTheMenu { get; }
}

public interface IHighlightedNode
{
    INode Highlighted { get; }
}

public interface ISelectedNode
{
    INode SelectedNode { get; }
}

// public interface IActiveBranch
// {
//   //  IBranch ActiveBranch { get; }
// }
// public interface IAddActiveBranch
// {
//     IBranch ThisBranch { get; }
// }
// public interface IRemoveActiveBranch
// {
//     IBranch ThisBranch { get; }
// }

public interface IDisabledNode
{
    INode ThisNode { get; }
}

// public interface IStoreNodeHistoryData
// {
//     Node NodeToUpdate { get; }
// }

//public interface IReturnHomeGroupIndex { }

public interface ISceneIsChanging { }

// public interface IAddTrunk: IRemoveTrunk { }
// public interface IRemoveTrunk
// {
//     Trunk ThisTrunk { get; }
// }






