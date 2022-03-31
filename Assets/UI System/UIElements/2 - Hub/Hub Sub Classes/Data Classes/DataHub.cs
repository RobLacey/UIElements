using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;


[Serializable]
public class DataHub:  /*IEZEventUser,*/ IIsAService, IDataHub, IEZEventDispatcher/*, IStoreNodeHistoryData*/
{
    [SerializeField] private Node _lastHighlighted = default;
    [SerializeField] private GameObject _lastHighlightedGo = default;
    [SerializeField] private Node _lastSelected = default;
    [SerializeField] private GameObject _lastSelectedGo = default;
    [SerializeField] private Branch _activeBranch = default;
    [SerializeField] private Trunk _currentTrunk = default;
    [SerializeField] private Trunk _pausedTrunk = default;
    [SerializeField] [ReadOnly] private Trunk _rootTrunk;
    [SerializeField] [ReadOnly] private bool _sceneStarted;
    [SerializeField] [ReadOnly] private bool _onRootTrunk = true;
    [SerializeField] [ReadOnly] private bool _controllingWithKeys = default;
    [SerializeField] [ReadOnly] private bool _inMenu;
    [SerializeField] [ReadOnly] private bool _gameIsPaused;
    [SerializeField] [ReadOnly] private EscapeKey _globalEscapeKey;
    [SerializeField] private List<Trunk> _activeTrunks = default;
    [SerializeField] private List<Node> _history = default;
    [SerializeField] private List<Node> _currentSwitchHistory;
    [SerializeField] private List<GameObject> _selectedGOs = default;
    [SerializeField] private List<Branch> _activeResolvePopUps;
    [SerializeField] private List<Branch> _activeOptionalPopUps;

    //Variables
    private bool _switchPressed;
    private bool _wasAtRoot;
    private Node _lastSelectedBeforePause;
    private Branch _lastActiveBranchBeforePause;
    private Trunk _lastTrunkBeforePause;
    private ISwitch _lastSwitcher;
    private List<Node> _lastCurrentSwitchHistory = new List<Node>();

    //private readonly List<INode> _history = new List<INode>();

    //Events
    //private Action<IStoreNodeHistoryData> DoAddANode { get; set; }
    private Action<IAddOptionalPopUp> DoAddOptionalPopUp { get; set; }
    private Action<IRemoveOptionalPopUp> DoRemoveOptionalPopUp { get; set; }
    private Action<IAddResolvePopUp> DoAddResolvePopUp { get; set; }
    private Action<IRemoveResolvePopUp> DoRemoveResolvePopUps { get; set; }
    // private Action<IAddTrunk> DoAddTrunk { get; set; }
    // private Action<IRemoveTrunk> DoRemoveTrunk { get; set; }

    private Action<IHighlightedNode> DoHighlighted { get; set; } 

    private Action<ISelectedNode> DoSelected { get; set; }
    //private  Action<IActiveBranch> DoSetAsActiveBranch { get; set; } 
    private Action<IIsAtRootTrunk> DoSetIsAtRoot { get; set; }


    
    public RectTransform MainCanvasRect { get; private set; }

    public bool SceneStarted => _sceneStarted;
    public void SetStarted() => _sceneStarted = true;

    public bool InMenu => _inMenu;
    public void SetInMenu(bool inMenu) => _inMenu = inMenu;

    public Trunk PausedTrunk => _pausedTrunk;
    public void SetPausedTrunk(Trunk pausedTrunk) => _pausedTrunk = pausedTrunk;
    public bool GamePaused => _gameIsPaused;
    public void SetIfGamePaused(bool paused) => _gameIsPaused = paused;

    public void SetGlobalEscapeSetting(EscapeKey setting) => _globalEscapeKey = setting;
    public EscapeKey GlobalEscapeSetting => _globalEscapeKey;

    public bool IsAtRoot => _onRootTrunk;

    public bool AllowKeys => _controllingWithKeys;
    public void SetAllowKeys(bool newAllowKeysSetting) => _controllingWithKeys = newAllowKeysSetting;

    public bool NoPopups  => _activeOptionalPopUps.Count == 0 & _activeResolvePopUps.Count == 0;
    public bool NoHistory => History.Count == 0;
    
    public IBranch ActiveBranch => _activeBranch;
    public void SetActiveBranch(Branch activeBranch) => _activeBranch = activeBranch;


    public ISwitch CurrentSwitcher { get; private set; }
    public void SetSwitcher(ISwitch newSwitcher)
    {
        if(newSwitcher != CurrentSwitcher)
            _currentSwitchHistory.Clear();
        
        CurrentSwitcher = newSwitcher;
    }

    public List<Trunk> ActiveTrunks => _activeTrunks;
    public Trunk CurrentTrunk => _currentTrunk;
    public Trunk RootTrunk => _rootTrunk;
    public bool NoResolvePopUp => _activeResolvePopUps.IsEmpty();
    public List<IBranch> ActiveResolvePopUps => _activeResolvePopUps.ToList<IBranch>();
    public List<IBranch> ActiveOptionalPopUps => _activeOptionalPopUps.ToList<IBranch>();

    public void SwitchPressed(bool pressed) => _switchPressed = pressed;
    public List<INode> History
    {
        get
        {
            if (_switchPressed || GamePaused)
            {
                return _currentSwitchHistory.ToList<INode>();
            }
            return _history.ToList<INode>();
        }
    }

    public List<GameObject> SelectedGOs => _selectedGOs;
    public INode Highlighted => _lastHighlighted;
    
    public void SetHighLighted(INode newNode)
    {
        if(!SceneStarted) return;
        
        if(Highlighted.IsNotNull() && newNode != Highlighted)
            Highlighted.ThisNodeNotHighLighted();


        _lastHighlighted = (Node)newNode;
        
        if (_lastHighlighted.IsNotNull() && _lastHighlighted.InGameObject.IsNotNull())
            _lastHighlightedGo = _lastHighlighted.InGameObject;
        
        DoHighlighted?.Invoke(this);
    }

    public INode SelectedNode => _lastSelected;
    
    public void SetSelected(INode newNode)
    {
        _lastSelected = (Node)newNode;
        
        if (_lastSelected.IsNotNull() && _lastSelected.InGameObject.IsNotNull())
            _lastSelectedGo = _lastSelected.InGameObject;

        DoSelected?.Invoke(this);
    }


    public Node NodeToUpdate { get; private set; }
    public IBranch ThisPopUp{ get;  private set; }
    //public Trunk ThisTrunk { get; private set; }


    //Main
    public void OnAwake() => AddService();

    public void SetUpDataHub(Trunk rootTrunk, RectTransform mainRect)
    {
        _rootTrunk = rootTrunk;
        MainCanvasRect = mainRect;
    }

    public void OnEnable()
    {
        //ObserveEvents();
        FetchEvents();
    }

    public void FetchEvents()
    {
        //DoAddANode = HistoryEvents.Do.Fetch<IStoreNodeHistoryData>();
        DoAddOptionalPopUp = PopUpEvents.Do.Fetch<IAddOptionalPopUp>();
        DoRemoveOptionalPopUp = PopUpEvents.Do.Fetch<IRemoveOptionalPopUp>();
        DoAddResolvePopUp = PopUpEvents.Do.Fetch<IAddResolvePopUp>();
        DoRemoveResolvePopUps = PopUpEvents.Do.Fetch<IRemoveResolvePopUp>();
        // DoAddTrunk = HistoryEvents.Do.Fetch<IAddTrunk>();
        // DoRemoveTrunk = HistoryEvents.Do.Fetch<IRemoveTrunk>();
        DoHighlighted = HistoryEvents.Do.Fetch<IHighlightedNode>();
        DoSelected = HistoryEvents.Do.Fetch<ISelectedNode>();
        //DoSetAsActiveBranch = HistoryEvents.Do.Fetch<IActiveBranch>();
        DoSetIsAtRoot = HistoryEvents.Do.Fetch<IIsAtRootTrunk>();

    }

    public void AddService() => EZService.Locator.AddNew<IDataHub>(this);

    public void OnRemoveService() { }

    // public void ObserveEvents()
    // {
    //     //HistoryEvents.Do.Subscribe<IInMenu>(SetIfInMenu);
    //     //HistoryEvents.Do.Subscribe<IGameIsPaused>(SetIfGamePaused);
    // }

    public void OnStart()
    {
        _lastHighlighted = (Node)RootTrunk.GroupsBranches.First().DefaultStartOnThisNode;
        _activeBranch = (Branch)RootTrunk.GroupsBranches.First();
    }

    public void AddResolvePopUp(IBranch popUpToAdd)
    {
        _activeResolvePopUps.Add((Branch)popUpToAdd);
        ThisPopUp = popUpToAdd;
        DoAddResolvePopUp?.Invoke(this);
    }

    public void AddOptionalPopUp(IBranch popUpToAdd)
    {
        _activeOptionalPopUps.Add((Branch)popUpToAdd);
        ThisPopUp = popUpToAdd;
        DoAddOptionalPopUp?.Invoke(this);
    }

    public void RemoveResolvePopUp(IBranch popUpToRemove)
    {
        _activeResolvePopUps.Remove((Branch)popUpToRemove);
        ThisPopUp = popUpToRemove;
        DoRemoveResolvePopUps?.Invoke(this);
    }

    public void RemoveOptionalPopUp(IBranch popUpToRemove)
    {
        _activeOptionalPopUps.Remove((Branch)popUpToRemove);
        ThisPopUp = popUpToRemove;
        DoRemoveOptionalPopUp?.Invoke(this);
    }

    // public void UnObserveEvents()
    // {
    //    // HistoryEvents.Do.Unsubscribe<IInMenu>(SetIfInMenu);
    //    // HistoryEvents.Do.Unsubscribe<IGameIsPaused>(SetIfGamePaused);
    // }




    //private void SetIfInMenu(IInMenu args) => InMenu = args.InTheMenu;

    
    public void ManageHistory(INode node)
    {
        NodeToUpdate = (Node)node;
        //DoAddANode?.Invoke(this);
        
        if (_history.Contains(NodeToUpdate))
        {
            _history.Remove( NodeToUpdate);
            SelectedGOs.Remove(NodeToUpdate.InGameObject);
            _currentSwitchHistory.Remove(NodeToUpdate);
        }
        else
        {
            _history.Add(NodeToUpdate);
            _currentSwitchHistory.Add(NodeToUpdate);
            if(NodeToUpdate.InGameObject.IsNull())return;
            SelectedGOs.Add(NodeToUpdate.InGameObject);
        }
    }
    
    public void AddTrunk(Trunk trunk)
    {
        _currentTrunk = trunk;
        _onRootTrunk = RootTrunk == trunk;
        DoSetIsAtRoot?.Invoke(this);
        
        if(ActiveTrunks.Contains(trunk)) return;
        ActiveTrunks.Add(trunk);
        //ThisTrunk = trunk;
        //DoAddTrunk?.Invoke(this);
        
    }
    
    public void RemoveTrunk(Trunk trunk)
    {
        if (!ActiveTrunks.Contains(trunk)) return;
        ActiveTrunks.Remove(trunk);
       // ThisTrunk = trunk;
        //DoRemoveTrunk?.Invoke(this);
    }
    
    public void SaveState()
    {
        //TODO Add InMenu so can switch from game and back
        _wasAtRoot = _onRootTrunk;
        _lastSelectedBeforePause = _lastSelected;
        _lastActiveBranchBeforePause = _activeBranch;
        _lastTrunkBeforePause = _currentTrunk;
        _lastCurrentSwitchHistory = _currentSwitchHistory.ToList();
        _lastSwitcher = CurrentSwitcher;
    }

    public void RestoreState()
    {
        _onRootTrunk = _wasAtRoot;
        _lastSelected = _lastSelectedBeforePause;
        _activeBranch = _lastActiveBranchBeforePause;
        _currentTrunk = _lastTrunkBeforePause;
        SetSwitcher(_lastSwitcher);
        _currentSwitchHistory = _lastCurrentSwitchHistory;
    }
}
