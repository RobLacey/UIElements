using System.Collections.Generic;
using UIElements;
using UnityEngine;

public interface IDataHub : ISwitchData, IPopUpManage, ITrunkManagement,  ITrackNodesAndBranches, 
                            IPausedAndEscapeTracker, ISaveState
{
    RectTransform MainCanvasRect { get; }
    bool SceneStarted { get; }
    bool InMenu { get;}
    void SetInMenu(bool inMenu);
    bool AllowKeys { get;}
    void SetAllowKeys(bool newAllowKeysSetting);
    bool NoHistory { get; }
    List<GameObject> SelectedGOs { get; }
    List<INode> History { get; }
    void SwitchPressed(bool pressed);
    List<IBranch> ActiveResolvePopUps { get; }
    List<IBranch> ActiveOptionalPopUps { get; }
    void ManageHistory(INode node);
}
public interface ISwitchData
{
    ISwitch CurrentSwitcher { get; }
    void SetSwitcher(ISwitch newSwitcher);
    List<Trunk> ActiveTrunks { get; }
    public Trunk CurrentTrunk { get; }
    Trunk RootTrunk { get; }
}

public interface IPopUpManage: IRemoveOptionalPopUp, IRemoveResolvePopUp, IAddOptionalPopUp, IAddResolvePopUp
{
    bool NoResolvePopUp { get; }
    bool NoPopups { get; }

    void AddResolvePopUp(IBranch popUpToAdd);

    void AddOptionalPopUp(IBranch popUpToAdd);

    void RemoveResolvePopUp(IBranch popUpToRemove);

    void RemoveOptionalPopUp(IBranch popUpToRemove);
}

public interface ITrunkManagement : /*IAddTrunk, */IIsAtRootTrunk
{
    bool IsAtRoot { get; }
    void AddTrunk(Trunk trunk);
    void RemoveTrunk(Trunk trunk);
}

public interface ITrackNodesAndBranches: IHighlightedNode, ISelectedNode /*, IActiveBranch*/
{
    void SetSelected(INode newNode);
    void SetHighLighted(INode newNode);
    IBranch ActiveBranch { get; }
    void SetActiveBranch(Branch activeBranch);
}

public interface IPausedAndEscapeTracker
{
    Trunk PausedTrunk { get; }
    void SetPausedTrunk(Trunk pausedTrunk);
    bool GamePaused { get; }
    void SetIfGamePaused(bool paused);
    void SetGlobalEscapeSetting(EscapeKey setting);
    EscapeKey GlobalEscapeSetting { get; }
}

public interface ISaveState
{
    void SaveState();
    void RestoreState();
}
