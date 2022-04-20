using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;


[Serializable]
public class DataHub:  IIsAService, IDataHub, IEZEventDispatcher, IServiceUser
{
    [SerializeField] private Node _lastHighlighted = default;
    [SerializeField] private GameObject _lastHighlightedGo = default;
    [SerializeField] private Node _lastSelected = default;
    [SerializeField] private GameObject _lastSelectedGo = default;
    [SerializeField] private Branch _activeBranch = default;
    [SerializeField] private Trunk _currentTrunk = default;
    [SerializeField] private Trunk _pausedTrunk = default;
    [SerializeField] private Trunk _escapeTrunk = default;
    [SerializeField] [ReadOnly] private Trunk _rootTrunk;
    [SerializeField] [ReadOnly] private bool _sceneStarted;
    [SerializeField] [ReadOnly] private bool _onRootTrunk = true;
    [SerializeField] [ReadOnly] private bool _controllingWithKeys = default;
    [SerializeField] [ReadOnly] private bool _inMenu;
    [SerializeField] [ReadOnly] private bool _gameIsPaused;
    [SerializeField] [ReadOnly] private bool _multiSelectActive;
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
    private IHistoryTrack _historyTracker;

    //Events
    private Action<IAddOptionalPopUp> DoAddOptionalPopUp { get; set; }
    private Action<IRemoveOptionalPopUp> DoRemoveOptionalPopUp { get; set; }
    private Action<IAddResolvePopUp> DoAddResolvePopUp { get; set; }
    private Action<IRemoveResolvePopUp> DoRemoveResolvePopUps { get; set; }
    private Action<IHighlightedNode> DoHighlighted { get; set; } 
    private Action<ISelectedNode> DoSelected { get; set; }
    private Action<IIsAtRootTrunk> DoSetIsAtRoot { get; set; }

    //Properties, Getters / Setters
    public RectTransform MainCanvasRect { get; private set; }
    public void SetMasterRectTransform(RectTransform mainRect) => MainCanvasRect = mainRect;
    public bool SceneStarted => _sceneStarted;
    public void SetStarted() => _sceneStarted = true;
    public bool InMenu => _inMenu;
    public void SetInMenu(bool inMenu) => _inMenu = inMenu;
    public bool PausedOrEscapeTrunk(Trunk compare) => compare == _pausedTrunk || compare == _escapeTrunk;
    public void SetPausedTrunk(Trunk pausedTrunk) => _pausedTrunk = pausedTrunk;
    public void SetEscapeTrunk(Trunk escapeTrunk) => _escapeTrunk = escapeTrunk;
    public bool GamePaused => _gameIsPaused;
    public void SetIfGamePaused(bool paused) => _gameIsPaused = paused;
    public void SetGlobalEscapeSetting(EscapeKey setting) => _globalEscapeKey = setting;
    public EscapeKey GlobalEscapeSetting => _globalEscapeKey;
    public bool AllowKeys => _controllingWithKeys;
    public void SetAllowKeys(bool newAllowKeysSetting) => _controllingWithKeys = newAllowKeysSetting;
    public bool NoPopUps  => _activeOptionalPopUps.IsEmpty() & _activeResolvePopUps.IsEmpty();
    public bool HasPopUps  => _activeOptionalPopUps.IsNotEmpty() || _activeResolvePopUps.IsNotEmpty();
    public bool NoResolvePopUp => _activeResolvePopUps.IsEmpty();
    public bool HasResolvePopUp => _activeResolvePopUps.IsNotEmpty();
    public List<IBranch> ActiveResolvePopUps => _activeResolvePopUps.ToList<IBranch>();
    public List<IBranch> ActiveOptionalPopUps => _activeOptionalPopUps.ToList<IBranch>();
    public bool NoHistory => History.IsEmpty();
    public IBranch ActiveBranch => _activeBranch;
    public void SetActiveBranch(Branch activeBranch) => _activeBranch = activeBranch;

    public ISwitch CurrentSwitcher { get; private set; }
    public void SetSwitcher(ISwitch newSwitcher)
    {
        if(newSwitcher != CurrentSwitcher)
        {
            _currentSwitchHistory = newSwitcher.SwitchHistory;
        }        
        CurrentSwitcher = newSwitcher;
    }

    public void SetRootTrunk(Trunk root) => _rootTrunk = root; 
    public bool IsAtRoot => _onRootTrunk;
    public List<Trunk> ActiveTrunks => _activeTrunks;
    public Trunk CurrentTrunk => _currentTrunk;
    public Trunk RootTrunk => _rootTrunk;
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

    public List<Node> CurrentSwitchHistory => _currentSwitchHistory;

    public List<GameObject> SelectedGOs => _selectedGOs;
    public INode Highlighted => _lastHighlighted;
    
    public void SetAsHighLighted(INode newNode)
    {
        UnhighlightLastNode(newNode);
        _lastHighlighted = (Node)newNode;
        IsAnInGameObject();
        DoHighlighted?.Invoke(this);
    }

    public bool CanSetAsHighlighted(INode newNode)
    {
        if (!SceneStarted/* || !AllowKeys*/) return false;
        
        return _historyTracker.ReturnNextBranch() == newNode.MyBranch;
    }

    private void UnhighlightLastNode(INode newNode)
    {
        if (Highlighted.IsNotNull() && newNode != Highlighted) 
            Highlighted.ThisNodeNotHighLighted();
    }

    private void IsAnInGameObject()
    {
        if (_lastHighlighted.IsNotNull() && _lastHighlighted.InGameObject.IsNotNull())
            _lastHighlightedGo = _lastHighlighted.InGameObject;
    }

    public INode SelectedNode => _lastSelected;
    
    public void SetSelected(INode newNode)
    {
        _lastSelected = (Node)newNode;
        
        if (_lastSelected.IsNotNull() && _lastSelected.InGameObject.IsNotNull())
            _lastSelectedGo = _lastSelected.InGameObject;

        DoSelected?.Invoke(this);
    }

    public bool MultiSelectActive => _multiSelectActive;
    public void SetMultiSelect(bool active) => _multiSelectActive = active;
    public Node NodeToUpdate { get; private set; }
    public IBranch ThisPopUp{ get;  private set; }
    public int PlayingTweens { get; private set; }

    public void AddPlayingTween()
    {
        PlayingTweens++;
    }
    public void RemovePlayingTween()
    {
        PlayingTweens--;
    }


    //Main
    public void OnAwake() => AddService();

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
    }

    public void FetchEvents()
    {
        DoAddOptionalPopUp = PopUpEvents.Do.Fetch<IAddOptionalPopUp>();
        DoRemoveOptionalPopUp = PopUpEvents.Do.Fetch<IRemoveOptionalPopUp>();
        DoAddResolvePopUp = PopUpEvents.Do.Fetch<IAddResolvePopUp>();
        DoRemoveResolvePopUps = PopUpEvents.Do.Fetch<IRemoveResolvePopUp>();
        DoHighlighted = HistoryEvents.Do.Fetch<IHighlightedNode>();
        DoSelected = HistoryEvents.Do.Fetch<ISelectedNode>();
        DoSetIsAtRoot = HistoryEvents.Do.Fetch<IIsAtRootTrunk>();
    }
    public void UseEZServiceLocator() => _historyTracker = EZService.Locator.Get<IHistoryTrack>(this);

    public void AddService() => EZService.Locator.AddNew<IDataHub>(this);

    public void OnRemoveService() { }

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
    
    public void ManageHistory(INode node)
    {
        NodeToUpdate = (Node)node;
        
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
    }
    
    public void RemoveTrunk(Trunk trunk)
    {
        if (!ActiveTrunks.Contains(trunk)) return;
        ActiveTrunks.Remove(trunk);
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
