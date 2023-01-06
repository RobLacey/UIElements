using System.Collections.Generic;
using UIElements;
using UnityEngine;

public interface IDataHub : ISwitchData, IPopUpManage, ITrunkManagement,  ITrackNodesAndBranches, 
                            IPausedAndEscapeTracker, ISaveState, TweenControl, IHotKeyTracker
{
    RectTransform MainCanvasRect { get; }
    void SetMasterRectTransform(RectTransform mainRect);
    bool SceneStarted { get; }
    bool InMenu { get;}
    void SetInMenu(bool inMenu);
    bool AllowKeys { get;}
    void SetAllowKeys(bool newAllowKeysSetting);
    bool NoHistory { get; }
    List<GameObject> SelectedGOs { get; }
    List<INode> History { get; }
    List<Node> CurrentSwitchHistory { get; }
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
}

public interface IPopUpManage /*IRemoveOptionalPopUp, IRemoveResolvePopUp, IAddOptionalPopUp, IAddResolvePopUp*/
{
    bool NoResolvePopUp { get; }
    bool HasResolvePopUp { get; }
    bool NoPopUps { get; }
    bool HasPopUps { get; }

    void AddResolvePopUp(IBranch popUpToAdd);

    void AddOptionalPopUp(IBranch popUpToAdd);

    void RemoveResolvePopUp(IBranch popUpToRemove);

    void RemoveOptionalPopUp(IBranch popUpToRemove);
}

public interface ITrunkManagement : /*IAddTrunk, */IIsAtRootTrunk
{
    Trunk RootTrunk { get; }
    bool IsAtRoot { get; }
    void SetRootTrunk(Trunk root);
    void AddTrunk(Trunk trunk);
    void RemoveTrunk(Trunk trunk);
}

public interface ITrackNodesAndBranches: IHighlightedNode, ISelectedNode, IMultiSelect
{
    void SetSelected(INode newNode);
    void SetAsHighLighted(INode newNode);
    bool CanSetAsHighlighted(INode newNode);
    IBranch ActiveBranch { get; }
    void SetActiveBranch(Branch activeBranch);
}

public interface IPausedAndEscapeTracker
{
    bool PausedOrEscapeTrunk(Trunk compare);
    void SetPausedTrunk(Trunk pausedTrunk);
    void SetEscapeTrunk(Trunk escapeTrunk);
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

public interface IMultiSelect
{
    bool MultiSelectActive { get; }
    void SetMultiSelect(bool active);
}

public interface TweenControl
{
    int PlayingTweens { get; }
    void AddPlayingTween();
    void RemovePlayingTween();

}

public interface IHotKeyTracker
{
    bool WasLastHotKeyPressed(Branch thisHotKey);
    void SetLastHotKeyPressed(Branch thisHotKey);
}
