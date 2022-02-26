

using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface IDataHub : IMonoAwake, IMonoEnable
{
    bool SceneStarted { get; }
    void SetStarted();
    bool InMenu { get; set; }
    bool GamePaused { get; }
    bool OnHomeScreen { get; }
    void SetOnHomeScreen(bool IsOnHomeScreen);
    bool AllowKeys { get;}
    void SetAllowKeys(bool newAllowKeysSetting);
    bool NoResolvePopUp { get; }
    bool NoPopups { get; }
    bool NoHistory { get; }
    IBranch ActiveBranch { get; }
    RectTransform MainCanvasRect { get; }
    IBranch[] AllBranches { get; }
    List<GameObject> SelectedGOs { get; }
    List<INode> History { get; }
    List<IBranch> ActiveResolvePopUps { get; }
    List<IBranch> ActiveOptionalPopUps { get; }
    INode Highlighted { get; }

}

public class DataHub: IEZEventUser, IIsAService, IDataHub
{
    public DataHub(RectTransform mainCanvasRect)
    {
        MainCanvasRect = mainCanvasRect;
    }

    public bool SceneStarted { get; private set; }
    public bool InMenu { get;  set; }
    public bool GamePaused { get; private set; }
    public bool OnHomeScreen { get; set; } = true;
    public bool AllowKeys { get; private set; }
    public bool NoPopups  => ActiveOptionalPopUps.Count == 0 & ActiveResolvePopUps.Count == 0;
    public bool NoHistory => History.Count == 0;
    public IBranch ActiveBranch { get; private set; }
    public RectTransform MainCanvasRect { get; }
    public IBranch[] AllBranches => Object.FindObjectsOfType<UIBranch>().ToArray<IBranch>();
    public bool NoResolvePopUp => ActiveResolvePopUps.Count == 0;
    public List<IBranch> ActiveResolvePopUps { get; } = new List<IBranch>();
    public List<IBranch> ActiveOptionalPopUps { get; } = new List<IBranch>();
    public List<INode> History { get; private set; } = new List<INode>();

    public List<GameObject> SelectedGOs { get; private set; } = new List<GameObject>();
    public INode Highlighted { get; private set; }

    public void OnAwake() => AddService();

    public void OnEnable() => ObserveEvents();

    public void AddService() => EZService.Locator.AddNew<IDataHub>(this);

    public void OnRemoveService() { }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IStoreNodeHistoryData>(ManageHistory);
        HistoryEvents.Do.Subscribe<IInMenu>(SetIfInMenu);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(SetIfGamePaused);
        HistoryEvents.Do.Subscribe<IActiveBranch>(SetActiveBranch);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SetHighlighted);
        PopUpEvents.Do.Subscribe<IAddResolvePopUp>(AddResolvePopUp);
        PopUpEvents.Do.Subscribe<IAddOptionalPopUp>(AddOptionalPopUp);
        PopUpEvents.Do.Subscribe<IRemoveResolvePopUp>(RemoveResolvePopUp );
        PopUpEvents.Do.Subscribe<IRemoveOptionalPopUp>(RemoveOptionalPopUp);
    }

    private void AddResolvePopUp(IAddResolvePopUp args) => ActiveResolvePopUps.Add(args.ThisPopUp);

    private void AddOptionalPopUp(IAddOptionalPopUp args) => ActiveOptionalPopUps.Add(args.ThisPopUp);

    private void RemoveResolvePopUp(IRemoveResolvePopUp args) => ActiveResolvePopUps.Remove(args.ThisPopUp);

    private void RemoveOptionalPopUp(IRemoveOptionalPopUp args) => ActiveOptionalPopUps.Remove(args.ThisPopUp);

    public void UnObserveEvents() { }
    
    private void SetHighlighted(IHighlightedNode args) => Highlighted = args.Highlighted;

    private void SetActiveBranch(IActiveBranch args) => ActiveBranch = args.ActiveBranch;

    public void SetAllowKeys(bool newAllowKeysSetting) => AllowKeys = newAllowKeysSetting;
    public void SetOnHomeScreen(bool IsOnHomeScreen) => OnHomeScreen = IsOnHomeScreen;

    private void SetIfGamePaused(IGameIsPaused args) => GamePaused = args.IsPaused;

    private void SetIfInMenu(IInMenu args) => InMenu = args.InTheMenu;

    public void SetStarted() => SceneStarted = true;
    
    private void ManageHistory(IStoreNodeHistoryData args)
    {
        if (args.NodeToUpdate is null)
        {
            History.Clear();
            SelectedGOs.Clear();
            return;
        }
        if (History.Contains((UINode)args.NodeToUpdate))
        {
            History.Remove((UINode) args.NodeToUpdate);
            SelectedGOs.Remove(args.NodeToUpdate.InGameObject);
        }
        else
        {
            History.Add((UINode) args.NodeToUpdate);
            if(args.NodeToUpdate.InGameObject.IsNull())return;
            SelectedGOs.Add(args.NodeToUpdate.InGameObject);
        }
    }


}
