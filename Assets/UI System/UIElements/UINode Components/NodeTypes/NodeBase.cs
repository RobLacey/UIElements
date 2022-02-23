using System;
using EZ.Events;
using EZ.Service;
using UIElements.Input_System;
using UnityEngine.EventSystems;

public abstract class NodeBase : INodeBase, IEZEventDispatcher, ISelectedNode, IHighlightedNode, IServiceUser
{
    protected NodeBase(INode node)
    {
        _uiNode = node;
        _uiFunctionEvents = _uiNode.UINodeEvents;
        MyBranch = _uiNode.MyBranch;
    }
    
    //Variables
    protected readonly INode _uiNode;
    private bool _hasFinishedSetUp;
    private IDataHub _myDataHub;
    private readonly IUiEvents _uiFunctionEvents;
    private bool _fromHotKey;
    private IHistoryTrack _historyTracker;

    //Events
    private Action<IHighlightedNode> DoHighlighted { get; set; } 

    private Action<ISelectedNode> DoSelected { get; set; }

    //Properties
    private INode LastHighlighted => _myDataHub.Highlighted;
    private bool InMenu => _myDataHub.InMenu;
    protected bool AllowKeys => _myDataHub.AllowKeys;
    protected bool PointerOverNode { get; private set; }
    public IBranch MyBranch { get; protected set; }
    public UINavigation Navigation { get; set; }
    protected bool IsSelected { get; private set; }
    public INode Highlighted => _uiNode;
    public INode SelectedNode { get; private set; }

    public void InMenuOrInGame()
    {
        if (HasNotFinishedSetUp()) return;
        
        if(!ReferenceEquals(LastHighlighted, _uiNode)) return;
        
        if (!InMenu)
        {
            OnExitingNode();
            return;
        }
        
        if (ReferenceEquals(LastHighlighted, _uiNode))
            SetNodeAsActive();
    }
    
    private bool HasNotFinishedSetUp()
    {
        if (_hasFinishedSetUp) return false;
        _hasFinishedSetUp = true;
        return true;
    }
    
    protected void DoPressOnNode() => _uiFunctionEvents.DoIsPressed();
    public virtual void ExitNodeByType() => OnExitingNode();

    
    //Main
    public void OnAwake() { }

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        LateStartSetUp();
    }

    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _historyTracker = EZService.Locator.Get<IHistoryTrack>(this);
    }

    private void LateStartSetUp()
    {
        if(_myDataHub.IsNull()) return;

        if (_myDataHub.SceneStarted)
        {
            SetNodeAsNotSelected_NoEffects();
            PointerOverNode = false;
        }
    }

    public virtual void FetchEvents()
    {
        DoHighlighted = HistoryEvents.Do.Fetch<IHighlightedNode>();
        DoSelected = HistoryEvents.Do.Fetch<ISelectedNode>();
    }
    
    public void OnDisable()
    {
        DoHighlighted = null;
        DoSelected = null;
        _myDataHub = null;
    }

    public void OnDestroy() => _myDataHub = null;

    public virtual void OnStart() { }

    public void EnableOrDisableNode(IDisableData isDisabled)
    {
        if(isDisabled.IsNodeDisabled())
            SetNodeAsNotSelected_NoEffects();
        _uiFunctionEvents.DoIsDisabled(isDisabled);
    }

    public virtual void SetNodeAsActive()
    {
        if (AllowKeys && InMenu)
        {
            OnEnteringNode();
        }
        else
        {
            if(PointerOverNode) return;
            OnExitingNode();
        }
    }

    private void ThisNodeIsSelected(INode node)
    {
        if(_uiNode.CanNotStoreNodeInHistory) return;
        SelectedNode = node;
        DoSelected?.Invoke(this);
    }

    protected void SetNodeAsSelected_NoEffects()
    {
        _uiFunctionEvents.DoMuteAudio();
        if (_fromHotKey)
        {
            Activate();
            _fromHotKey = false;
        }
        else
        {
            Activate(DoPressOnNode);
        }
    }

    public virtual void SetNodeAsNotSelected_NoEffects()
    {
        _uiFunctionEvents.DoMuteAudio();
        SetSelectedStatus(false, DoPressOnNode);
        OnExitingNode();
    }

   public virtual void OnEnteringNode() 
    {
        if(_myDataHub.Highlighted.IsNotNull())
            _myDataHub.Highlighted.ThisNodeNotHighLighted();
        DoHighlighted?.Invoke(this);

        PointerOverNode = true;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }

    public virtual void OnExitingNode()
    {
        PointerOverNode = false;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }

    public void DoMoveToNextNode(MoveDirection moveDirection) => Navigation.AxisMoveDirection(moveDirection);

    public void MenuNavigateToThisNode(MoveDirection moveDirection)
    {
        if(!MyBranch.CanvasIsEnabled)return;
        
        OnEnteringNode();
    }

    public virtual void NodeSelected()
    {
        if (IsSelected)
        {
            Deactivate();
        }
        else
        {
            Activate(DoPressOnNode);
        }
    }

    protected void Activate(Action endAction = null)
    {
        SetSelectedStatus(true, endAction);
        ThisNodeIsSelected(_uiNode);
    }

    private void Deactivate()
    {
        if(!_historyTracker.NodeNeededForMultiSelect(_uiNode))
            SetSelectedStatus(false, DoPressOnNode);
        ThisNodeIsSelected(_uiNode);
    }

    private void SetSelectedStatus(bool isSelected, Action endAction)
    {
        IsSelected = isSelected;
        _uiFunctionEvents.DoIsSelected(IsSelected);
        endAction?.Invoke();
    }

    public void HotKeyPressed(bool setAsActive)
    {
        _fromHotKey = true;
        OnEnteringNode();
        ThisNodeIsSelected(_uiNode);
        if(!setAsActive) return;
        SetNodeAsSelected_NoEffects();
    }
    
    public virtual void SetUpGOUIParent(IGOUIModule module) { }
}
