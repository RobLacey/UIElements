using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor.Experimental;

namespace UIElements
{
    /// <summary>
    /// Trunk is the base module of each new screen that has a connected behaviour 
    /// </summary>

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(UITweener))]
    public class Trunk : MonoBehaviour, IEZEventDispatcher, IServiceUser, IIsAtRootTrunk, IClearScreen
    {
        [SerializeField] [Label("Overlay Or Fullscreen")]
        private ScreenType _screenType = ScreenType.Overlay;
        
        [SerializeField] [Space(10f)] private WhenToMove _moveToNextTrunk = WhenToMove.Immediately;
        [SerializeField] private WhenToMove _moveBackFromTrunk = WhenToMove.Immediately;
        [SerializeField] private OnGoingToFullScreen _goingToFullscreen = OnGoingToFullScreen.UseBranchDefaults;
        [SerializeField] private IsActive _loopAroundSwitcher = IsActive.Yes;
        
        [Space(10f, order = 1)]
        [ValidateInput(HasBranches, "Must have at least one branch assigned")]
        [Label(BranchListTitle)]
        [SerializeField] private List<Branch> _branches;

       //Variables
        private IDataHub _myDataHub;
        private ISwitchTrunkGroup _switcher;
        private const string BranchListTitle = "Set Main Branch/es For This Trunk. Multiple Branches Activate Switcher";
        private Canvas _myCanvas;
        private UITweener _myTweener;
        private WhenToMove _currentMoveType;

        //Editor
        private const String HasBranches = nameof(HasTrunkBranches);
        private bool HasTrunkBranches(List<Branch> list) => list.IsNotEmpty() && list.First().IsNotNull();

        //Events
        private Action<IClearScreen> DoClearScreen { get; set; }
        
        //Enums
        private enum OnGoingToFullScreen { TweenAllOpenBranches, UseBranchDefaults }
        
        //Properties & Getters/ Setters
        public List<IBranch> GroupsBranches => _branches.ToList<IBranch>();
        public ScreenType ScreenType => _screenType;
        private bool TweenToNextImmediately => _currentMoveType == WhenToMove.Immediately;
        private bool HasValidTweener => _myTweener.Scheme && _myTweener.HasBuildList;
        public bool CanvasIsActive => _myCanvas.enabled == true;
        public IBranch ActiveBranch => _switcher.CurrentBranch;
        public List<Node> SwitcherHistory => _switcher.SwitchHistory;
        public bool DontLoopSwitcher => _loopAroundSwitcher == IsActive.No;
        public bool ForceClear => _goingToFullscreen == OnGoingToFullScreen.TweenAllOpenBranches;
        public void SetCurrentMoveTypeToMoveToNext() => _currentMoveType = _moveToNextTrunk;
        public void SetCurrentMoveTypeToMoveToBack() => _currentMoveType = _moveBackFromTrunk;
        public void SetNewSwitcherIndex(INode newNode) => _switcher.SetNewIndex(newNode);
        
        /// <summary>
        /// Method can be used to reactive the default trunk switcher if tab groups are present
        /// </summary>

        //Main
        private void Awake()
        { 
            _switcher = EZInject.Class.NoParams<ISwitchTrunkGroup>();
            _switcher.ThisTrunk = this;
            _myCanvas = GetComponent<Canvas>();
            _myTweener = GetComponent<UITweener>();
            SetUpTrunkGroup();
            _myCanvas.enabled = false;
        }

        private void SetUpTrunkGroup()
        {
            var temp = new List<Branch>();
            
            if(_branches.IsEmpty())
                throw new Exception($"No Branches Set on : {this}");
            
            foreach (var uiBranch in _branches)
            {
                if (CheckForEmptyElements(uiBranch)) continue;
                temp.Add(uiBranch);
                uiBranch.ParentTrunk = this;
                foreach (var branch in GetComponentsInChildren<Branch>())
                {
                    branch.ParentTrunk = this;
                }
            }

            _branches.Clear();
            _branches = temp;
        }

        private bool CheckForEmptyElements(Branch branch)
        {
            if (branch.IsNull())
            {
                Debug.Log($"You Have empty elements in {this} Trunk");
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            UseEZServiceLocator();
            FetchEvents();
             _switcher.OnEnable();
        }
        
        public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

        public void FetchEvents()
        {
            DoClearScreen = BranchEvent.Do.Fetch<IClearScreen>();
        }

        private void OnDisable() => _switcher.OnDisable();

        private void Start()
        {
            _switcher.ThisGroup = GroupsBranches;
        }

        public void OnStartTrunk(IBranch newParent = null)
        {
           // Debug.Log($"Start Trunk : {this}");

            void ActivateCurrentBranch() => _switcher.ActivateCurrentBranch();
            
            _switcher.UpdateSwitchHistory();
            
             var counter = GroupsBranches.Count + SwitcherHistory.Count + 1;

            
            if(_myDataHub.CurrentTrunk == this) return;
            
            if(ScreenType == ScreenType.FullScreen)
                 DoClearScreen?.Invoke(this);
            
            _myDataHub.AddTrunk(this);
            _myDataHub.SetSwitcher(_switcher);
            OnEnable();

            if (!_myCanvas.enabled)
            {
                _myCanvas.enabled = true;
                _myTweener.StartInTweens(EndAction);
                _switcher.OpenAllBranches(newParent, HasValidTweener, EndAction);
            }
            else
            {
                ActivateCurrentBranch();
            }

            void EndAction()
            {
                counter--;
                if (counter > 0) return;
                 
                ActivateCurrentBranch();
            }
        }


        public void OnExitTrunk(Action endOfMoveAction, bool removeFromHistory = true)
        {
            Debug.Log($"Exit Trunk : {this}");
            if (!_myCanvas.enabled)
            {
                endOfMoveAction?.Invoke();
                return;
            }
            
            var counter = GroupsBranches.Count + SwitcherHistory.Count + 1;

            if(removeFromHistory)
                _myDataHub.RemoveTrunk(this);
            
            if (TweenToNextImmediately)
            {
                CloseBranches();
                endOfMoveAction?.Invoke();
            }
            else
            {
                CloseBranches();
            }
        
            void CloseBranches()
            {
                _myTweener.StartOutTweens(EndAction);
                _switcher.CloseAllBranches(EndAction, HasValidTweener);
            }  
            
             void EndAction()
             {
                 counter--;
                 if (counter > 0) return;
                 
                 _myCanvas.enabled = false;
                 endOfMoveAction?.Invoke();
             }
        }
        
        [Button("Add a New Tree Structure")]
        private void MakeTreeFolders()
        {
            new CreateNewObjects().CreateMainFolder(transform)
                                  .CreateBranch()
                                  .CreateNode();
        }
    }
}