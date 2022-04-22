using System;
using System.Linq;
using System.Runtime.InteropServices;
using EZ.Events;
using EZ.Service;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]


public partial class Branch : MonoBehaviour, IEZEventUser,/*, IActiveBranch,*/ IBranch, IEZEventDispatcher, ICloseBranch, IServiceUser
{
    [Header("Branch Main Settings")] [HorizontalLine(2f, EColor.Blue, order = 1)]
    [SerializeField]
    private BranchType _branchType = BranchType.Standard;

    [SerializeField]
    [ShowIf(StandardBranch)] [Label("Is Control Bar")]
    private IsActive _controlBar = IsActive.No;
    
    [SerializeField]
    [Label("Start On (Optional)")] 
    private Node _startOnThisNode;
    
    // [SerializeField]
    // [Label("Preset Parent (Optional)")] 
    // private Branch _presetParent;

    [SerializeField] 
    [ShowIf(EConditionOperator.Or, TimedBranch, OptionalBranch)] [Range(0f,20f)] 
    private float _timer = 0f;

    [SerializeField]
    [Space(10f)]
    [ShowIf(EConditionOperator.Or, OptionalBranch , ResolveBranch, TimedBranch/*, OnlyAllowOnHomeScreen*/)]
    [Label("Pop-Up Allowed When...")]
    private WhenAllowed _whenAllowed;

    [Header("Canvas Order Settings")] [HorizontalLine(2f, EColor.Blue, order = 1)]
    [SerializeField] 
    [ShowIf(StandardBranch)] 
    private OrderInCanvas _canvasOrderSetting = OrderInCanvas.Default;
    
    [SerializeField]
    private IsActive _applyFocus = IsActive.No;
    
    [SerializeField] 
    [Range(1,40)] private int _whenFocusedSortingOrder = 2;

    [SerializeField] 
    [ShowIf(ShowManualOrder)] 
    [OnValueChanged(SetUpCanvasOrder)] 
    private int _orderInCanvas;
    
    [FormerlySerializedAs("_moveType")]
    [Header("Navigation Settings")] [HorizontalLine(2f, EColor.Blue, order = 1)]

    [SerializeField]
    [Label("Move To Child When...")] [HideIf(InGamUIBranch)] 
    private WhenToMove _moveToChild = WhenToMove.Immediately;
    
    [FormerlySerializedAs("_moveBackType")]
    [SerializeField]
    [Label("Move Back When...")] [HideIf(InGamUIBranch)] 
    private WhenToMove _moveBackWhen = WhenToMove.Immediately;

    [SerializeField] 
    [HideIf(EConditionOperator.Or, AnyPopUpBranch, ControlBarBranch, InGamUIBranch)]
    [Label("Visible When Child Active")]
    private IsActive _stayVisible = IsActive.No;

   [SerializeField] 
   [Label("When Active Do This...")] [ValidateInput(SetForResolve)]
   private WhenActiveDo _whenActiveDo = WhenActiveDo.Nothing;

    [SerializeField] 
    private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;

    [SerializeField] 
    [Label("Save Last Highlighted")] [HideIf(EConditionOperator.Or,AnyPopUpBranch, InGamUIBranch)] 
    private IsActive _saveExitSelection = IsActive.Yes;
    
    [SerializeField] 
    [ShowIf(CanAutoOpenClose)] 
    [Label("Auto Close Branch")]
    private IsActive _autoClose = IsActive.No;

    [SerializeField]
    [ShowIf(CanAutoOpenClose)] 
    [Label("Auto Close Delay")]
    [Range(0f, 1f)]private float _autoCloseDelay = 0.25f;


    [Header("Events & Create New Buttons", order = 2)][HorizontalLine(2f, EColor.Blue, order = 3)] 
    [Space(20, order = 1)]
    [SerializeField] 
    private BranchEvents _branchEvents;
    
    
    //Buttons
    [Button("Create Node")]
    private void CreateNode() => new CreateNewObjects().CreateNode(transform);

    [Button("Create Branch")]
    private void CreateBranch() => new CreateNewObjects().CreateBranch(transform.parent)
                                                         .CreateNode();
    
    //Variables
    private UITweener _uiTweener;
    private bool _tweenOnChange = true, _canActivateBranch = true;
    private bool _sceneIsChanging;
    private bool _tweening = true;
    private bool _overThisBranch;

    private IBranchBase _branchTypeBaseClass;
    private IDataHub _myDataHub;
    private INode _lastSelectedBeforeExit;
    

    //Delegates & Events
    private Action TweenFinishedCallBack { get; set; }
    private Action<ICloseBranch> CloseAndResetBranch { get; set; }
    public event Action OpenBranchStartEvent;
    public event Action OpenBranchEndEvent;
    public event Action ExitBranchStartEvent;
    public event Action ExitBranchEndEvent;
    public event Action OnMouseEnterEvent;
    public event Action OnMouseExitEvent;

    //Getters & Setters
    private void SceneIsChanging(ISceneIsChanging args) => _sceneIsChanging = true;
    public void SetNewSelected(INode newNode)
    {
        LastSelected = newNode;
        
        if (newNode.IsNotNull())
            _lastSelectedBeforeExit = newNode;
    }
    public void SetNewHighlighted(INode newNode) => LastHighlighted = newNode;

    //Main
    private void Awake()
    {
        // Debug.Log("Upto : When in full screen or another trunk pressing a hotkey that isn't in that trunk doesn't return to that branch." +
        //           "Also possiblily opens child to soon. Need to make sure that it doesn't do anything until back to correct trunk");
        
        ThisBranchesNodes = BranchChildNodeUtil.GetChildNodes(this);
        MyCanvasGroup = GetComponent<CanvasGroup>();
        MyCanvasGroup.blocksRaycasts = false;
        _uiTweener = GetComponent<UITweener>();
        MyCanvas = GetComponent<Canvas>();
        // if(IsInGameBranch()) 
        //     MyParentBranch = this;
        SetParentBranch(MyParentBranch);
        AutoOpenCloseClass = EZInject.Class.WithParams<IAutoOpenClose>(this); 
        _branchTypeBaseClass = BranchFactory.Factory.PassThisBranch(this).CreateType(_branchType);
        _branchTypeBaseClass.OnAwake();
        SetStartPositions();
    }

    private void SetStartPositions()
    {
        SetDefaultStartPosition();
        LastHighlighted = DefaultStartOnThisNode;
        LastSelected = DefaultStartOnThisNode;
    }

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
        _branchEvents.SetUpClass(this);
        _branchTypeBaseClass.OnEnable();
        AutoOpenCloseClass.OnEnable();
        _whenAllowed.OnEnable();
    }

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    private void SetDefaultStartPosition()
    {
        if (_startOnThisNode)
        {
            DefaultStartOnThisNode = _startOnThisNode;
            return;
        }

        if (ThisBranchesNodes.Length == 0)
        {
            Debug.Log($"This Branch Has No Nodes : {this}");
            return;
        }
        DefaultStartOnThisNode = (Node) ThisBranchesNodes.First();
    }

    public void FetchEvents() => CloseAndResetBranch = BranchEvent.Do.Fetch<ICloseBranch>();

    public void ObserveEvents() => HistoryEvents.Do.Subscribe<ISceneIsChanging>(SceneIsChanging);

    public void UnObserveEvents() => HistoryEvents.Do.Unsubscribe<ISceneIsChanging>(SceneIsChanging);

    public void OnDisable()
    {
        CloseAndResetBranch?.Invoke(this);
        UnObserveEvents();
        AutoOpenCloseClass.OnDisable();
        _whenAllowed.OnDisable();
        
        if(_sceneIsChanging) return;
        _branchTypeBaseClass.OnDisable();
        _branchEvents.OnDisable();
    }

    public void OnDestroy()
    {
        if(!_sceneIsChanging)
            MyCanvas.enabled = false;
        UnObserveEvents();
        _branchTypeBaseClass.OnDestroy();
    }

    private void Start() => _branchTypeBaseClass.OnStart();

    public void StartPopUp_RunTimeCall(bool fromPool)
    {
        if(fromPool)
            OnEnable();
        OpenThisBranch();
    }

    public void OpenThisBranch(IBranch newParentBranch = null)
    {
        //Debug.Log(this);
        if(!_branchTypeBaseClass.CanStartBranch()) return;

        SetBranchAsActive(newParentBranch);

        if (_tweenOnChange)
        {
            _tweening = true;
            _myDataHub.AddPlayingTween();
            _uiTweener.StartInTweens(callBack: InTweenCallback);
        }
        else
        {
            InTweenCallback();
        }
        
        _tweenOnChange = true;
    }
    
    private void SetBranchAsActive(IBranch newParentBranch)
    {
        SetParentBranch(newParentBranch);
        OpenBranchStartEvent?.Invoke();
        if (!_canActivateBranch) return;
        _myDataHub.SetActiveBranch(this);
    }

    public void SetParentBranch(IBranch newParentBranch)
    {
        if (newParentBranch.IsNotNull())
            MyParentBranch = newParentBranch;
    }
    
    private void InTweenCallback()
    {
        _myDataHub.RemovePlayingTween();
        _tweening = false;
        OpenBranchEndEvent?.Invoke();
        SetHighlightedNode();
        _canActivateBranch = true;
    }

    private void SetHighlightedNode()
    {
        if (_saveExitSelection == IsActive.Yes && _lastSelectedBeforeExit.IsNotNull())
        {
            LastHighlighted = _lastSelectedBeforeExit;
            _lastSelectedBeforeExit = null;
        }
        
        if (_canActivateBranch && _myDataHub.CanSetAsHighlighted(LastHighlighted))
        {
            LastHighlighted.SetNodeAsActive();
        }
    }

    public void ExitThisBranch(OutTweenType outTweenType, Action endOfTweenCallback = null)
    {
        //Debug.Log(this);
        bool DontExitBranch() => _branchTypeBaseClass.DontExitBranch(outTweenType);
        var moveType = outTweenType == OutTweenType.MoveToChild ? _moveToChild : _moveBackWhen;
        
        if(!CanvasIsEnabled || DontExitBranch() || !_tweenOnChange)
        {
            endOfTweenCallback?.Invoke();
            _tweenOnChange = true;
            return;
        }

        if (moveType == WhenToMove.AfterEndOfTween)
        {
            StartOutTween(endOfTweenCallback);
        }
        else
        {
            StartOutTween();
            endOfTweenCallback?.Invoke();
        }
    }

    private void StartOutTween(Action endOfTweenCallback = null)
    {
        _tweening = true;
        TweenFinishedCallBack = endOfTweenCallback;
        ExitBranchStartEvent?.Invoke();
        _myDataHub.AddPlayingTween();
        _uiTweener.StartOutTweens(OutTweenCallback);
    }

    private void OutTweenCallback()
    {
        _myDataHub.RemovePlayingTween();
        _tweening = false;
        ExitBranchEndEvent?.Invoke();
        TweenFinishedCallBack?.Invoke();
        
        if (_saveExitSelection == IsActive.No)
            LastHighlighted = DefaultStartOnThisNode;
    }
    
    public void SetCanvas(ActiveCanvas activeCanvas) => _branchTypeBaseClass.SetCanvas(activeCanvas);
    public void SetBlockRaycast(BlockRaycast blockRaycast) => _branchTypeBaseClass.SetBlockRaycast(blockRaycast);
    public void SetUpGOUIBranch(IGOUIModule module) => _branchTypeBaseClass.SetUpGOUIBranch(module);

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_overThisBranch) return;
        OnMouseEnterEvent?.Invoke();
        _overThisBranch = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!_overThisBranch || eventData.IsNull() && ThisBranchesNodes.Contains(_myDataHub.Highlighted)) return;
        OnMouseExitEvent?.Invoke();
        _overThisBranch = false;
    }

    /// <summary>
    /// Method is used to close a branch from Inspector events or from external scripts
    /// </summary>
    public void ExitBranch_Runtime()
    {
        ExitThisBranch(OutTweenType.Cancel);
    }

    public void InTest() => Debug.Log($"Mouse over {this}");
    public void OutTest() => Debug.Log($"Mouse not over {this}");

}