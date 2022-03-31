using System;
using System.Linq;
using System.Runtime.InteropServices;
using EZ.Events;
using EZ.Service;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.EventSystems;

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
    // [ShowIf(EConditionOperator.Or, TimedBranch, ResolveBranch, OptionalBranch)]
    // private IsActive _onlyAllowOnHomeScreen = IsActive.Yes;

    [SerializeField] 
    [ShowIf(EConditionOperator.Or, TimedBranch, OptionalBranch)] [Range(0f,20f)] 
    private float _timer = 0f;

    [SerializeField]
    [ShowIf(EConditionOperator.Or, OptionalBranch , ResolveBranch, TimedBranch/*, OnlyAllowOnHomeScreen*/)]
    [Label("When Allowed...")]
    private WhenAllowed _whenAllowed;
    // [SerializeField] 
    // [ShowIf(EConditionOperator.And, OptionalBranch/*, OnlyAllowOnHomeScreen*/)]
    // [Label("When Not On Home Screen")]
    // private StoreAndRestorePopUps _storeOrResetOptional = StoreAndRestorePopUps.DoNothing;

    // [SerializeField] 
    // [ShowIf(EConditionOperator.And, OptionalBranch)] 
    // [Label("Allow With Active Resolve PopUps")]
    // private IsActive _allowWithActiveResolve = IsActive.No;
    
    // [SerializeField] 
    // [ShowIf(OptionalBranch)]
    // [Label("Buffer Until Can Activate")]
    // private IsActive _canAddToHomeScreenBuffer = IsActive.No;
    

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
    
    [Header("Navigation Settings")] [HorizontalLine(2f, EColor.Blue, order = 1)]

    [SerializeField]
    [Label("Move To Next Branch...")] [HideIf(InGamUIBranch)] 
    private WhenToMove _moveType = WhenToMove.Immediately;

    [SerializeField] 
    [HideIf(EConditionOperator.Or, AnyPopUpBranch, ControlBarBranch, InGamUIBranch)]
   // [ValidateInput(ValidInAndOutTweens, MessageINAndOutTweens)]
    [Label("Visible When Child Active")]
    private IsActive _stayVisible = IsActive.No;

   [SerializeField] 
   [Label("Block Other Branches While Open")]
   private IsActive _blockRaycastToOpenBranches = IsActive.No;

    [SerializeField] 
    [ShowIf(EConditionOperator.Or, StandardBranch)]
    private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    // [SerializeField] 
    // [ShowIf(EConditionOperator.Or, NotControlBar, Stored)] 
    // [Label("Tween On Return")]
    // private DoTween _tweenOnReturn = DoTween.Tween;

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
    private bool _tweenOnChange = true, _canSetAsActivateBranch = true;
    private bool _sceneIsChanging;
    private bool _tweening = true;

    private IBranchBase _branchTypeBaseClass;
    private IDataHub _myDataHub;
    

    //Delegates & Events
    private Action TweenFinishedCallBack { get; set; }
    // private  Action<IActiveBranch> SetAsActiveBranch { get; set; }        
    private Action<ICloseBranch> CloseAndResetBranch { get; set; }
    public event Action EnterBranchEvent;
    public event Action ExitBranchEvent;

    //Getters & Setters
    private void SceneIsChanging(ISceneIsChanging args) => _sceneIsChanging = true;
    public void SetNewSelected(INode newNode) => LastSelected = newNode;
    public void SetNewHighlighted(INode newNode) => LastHighlighted = newNode;

    //Main
    private void Awake()
    {
        //CheckForValidSetUp();
        ThisBranchesNodes = BranchChildNodeUtil.GetChildNodes(this);
        MyCanvasGroup = GetComponent<CanvasGroup>();
        MyCanvasGroup.blocksRaycasts = false;
        _uiTweener = GetComponent<UITweener>();
        MyCanvas = GetComponent<Canvas>();
        if(IsHomeScreenBranch() || IsInGameBranch())
            MyParentBranch = this;
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

    // private void CheckForValidSetUp()
    // {
    //     if (AllowableInAndOutTweens(_stayVisible)) return;
    //     
    //     throw new Exception($"Can't have Stay Visible and also have IN AND OUT Tweens on : {this} " +
    //               $"{Environment.NewLine} OutTween NOT Allowed");
    // }

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
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

    public void FetchEvents()
    {
        CloseAndResetBranch = BranchEvent.Do.Fetch<ICloseBranch>();
        // SetAsActiveBranch = HistoryEvents.Do.Fetch<IActiveBranch>();
    }

    public void ObserveEvents() => HistoryEvents.Do.Subscribe<ISceneIsChanging>(SceneIsChanging);

    public void UnObserveEvents() => HistoryEvents.Do.Unsubscribe<ISceneIsChanging>(SceneIsChanging);

    public void OnDisable()
    {
        CloseAndResetBranch?.Invoke(this);
//        ExitBranchEvent?.Invoke();
        UnObserveEvents();
        AutoOpenCloseClass.OnDisable();
        _whenAllowed.OnDisable();
        
        if(_sceneIsChanging) return;
        _branchTypeBaseClass.OnDisable();
        //SetAsActiveBranch = null;
    }

    public void OnDestroy()
    {
        if(!_sceneIsChanging)
            MyCanvas.enabled = false;
        UnObserveEvents();
        _branchTypeBaseClass.OnDestroy();
        //SetAsActiveBranch = null;
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
        if(!_branchTypeBaseClass.CanStartBranch()) return;

        _branchTypeBaseClass.SetUpBranch(newParentBranch);
        
        SetBranchAsActive();

        if (_tweenOnChange)
        {
            _tweening = true;
            _uiTweener.StartInTweens(callBack: InTweenCallback);
        }
        else
        {
            InTweenCallback();
        }
        
        _tweenOnChange = true;
    }

    public void SetBranchAsActive()
    {
        _branchEvents.OnBranchEnter();
        EnterBranchEvent?.Invoke();
        if (!_canSetAsActivateBranch) return;
        _myDataHub.SetActiveBranch(this);
    }

    private void InTweenCallback()
    {
        _tweening = false;

        _branchTypeBaseClass.EndOfBranchStart();
        _branchEvents.OnBranchEnterEnd();
       
        SetHighlightedNode();
        
        _canSetAsActivateBranch = true;
    }

    private void SetHighlightedNode()
    {
        if(LastHighlighted.IsNull()) return;

        if (_canSetAsActivateBranch)
            LastHighlighted.SetNodeAsActive();
    }

    public void ExitThisBranch(OutTweenType outTweenType, Action endOfTweenCallback = null)
    {
        //Debug.Log(ThisBranch);
        if(!CanvasIsEnabled || DontExitBranch())
        {
            endOfTweenCallback?.Invoke();
            return;
        }
        
        if (WhenToMove == WhenToMove.AfterEndOfTween)
        {
            StartOutTween(endOfTweenCallback);
        }
        else
        {
            StartOutTween();
            endOfTweenCallback?.Invoke();
        }

        bool DontExitBranch() => _branchTypeBaseClass.DontExitBranch(outTweenType);
    }

    private void StartOutTween(Action endOfTweenCallback = null)
    {
        _tweening = true;
        
        TweenFinishedCallBack = endOfTweenCallback;
        _branchEvents.OnBranchExit();
        _branchTypeBaseClass.StartBranchExit();
        ExitBranchEvent?.Invoke();
        _uiTweener.StartOutTweens(OutTweenCallback);
        SetSaveLastSelected();
        
        void OutTweenCallback()
        {
            _tweening = false;
            _branchTypeBaseClass.EndOfBranchExit();
            TweenFinishedCallBack?.Invoke();
            _branchEvents.OnBranchExitEnd();
        }
    }

    private void SetSaveLastSelected()
    {
        if (_saveExitSelection == IsActive.No)
            LastHighlighted = ThisBranchesNodes[0];
    }

    public void SetCanvas(ActiveCanvas activeCanvas) => _branchTypeBaseClass.SetCanvas(activeCanvas);
    public void SetBlockRaycast(BlockRaycast blockRaycast) => _branchTypeBaseClass.SetBlockRaycast(blockRaycast);
    public void SetUpGOUIBranch(IGOUIModule module) => _branchTypeBaseClass.SetUpGOUIBranch(module);

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_applyFocus == IsActive.Yes)
            _branchTypeBaseClass.SetFocus(_whenFocusedSortingOrder);
        AutoOpenCloseClass.OnPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_applyFocus == IsActive.Yes)
            _branchTypeBaseClass.ResetFocus();

        AutoOpenCloseClass.OnPointerExit();
    }

    /// <summary>
    /// Method is used to close a branch from Inspector events or from external scripts
    /// </summary>
    public void ExitBranch_Runtime()
    {
        ExitThisBranch(OutTweenType.Cancel);
    }

}