using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UnityEngine;
using NaughtyAttributes;

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
        private bool _firstTimeStart = true;
        private WhenToMove _currentMoveType;

        //Editor
        private const String HasBranches = nameof(HasTrunkBranches);
        private bool HasTrunkBranches(List<Branch> list) => list.IsNotEmpty() && list.First().IsNotNull();

        //Events
        private Action<IClearScreen> DoClearScreen { get; set; }
        
        //Properties & Getters/ Setters
        public List<IBranch> GroupsBranches => _branches.ToList<IBranch>();
        public ScreenType ScreenType => _screenType;
        private bool TweenToNextImmediately => _currentMoveType == WhenToMove.Immediately;
        private bool HasNoValidTweener => _myTweener.Scheme.IsNull() || !_myTweener.HasBuildList;
        public bool CanvasIsActive => _myCanvas.enabled == true;
        public IBranch ActiveBranch => _switcher.CurrentBranch;
        
        /// <summary>
        /// Method can be used to reactive the default trunk switcher if tab groups are present
        /// </summary>

        //Main
        private void Awake()
        { 
            Debug.Log("Upto : Moving closing history to HistoryManagment from here. Make sure when moving to new switch it searches for  target from last Trunk correctly");
            Debug.Log("Upto : Make sure when moving to new switch it searches for  target from last Trunk correctly");
            Debug.Log("Upto : On move back after tween check why nodes are all cleared first");
            Debug.Log("Upto : On move to child after tween check why tooltips are still active until new node set");
            
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
            //Debug.Log($"Start This : {this}");
            if(_myDataHub.CurrentTrunk == this) return;

            if(ScreenType == ScreenType.FullScreen)
                 DoClearScreen?.Invoke(this);
            
            OnEnable();
            _myDataHub.AddTrunk(this);
            _myDataHub.SetSwitcher(_switcher);

            if (!_myCanvas.enabled)
            {
                _myCanvas.enabled = true;
                _switcher.OpenAllBranches(newParent, HasNoValidTweener);
                _myTweener.StartInTweens(ActivateCurrentBranch);
            }
            else
            {
                ActivateCurrentBranch();
            }
        }

        private void ActivateCurrentBranch()
            {
                if(_switcher.SwitchHistory.IsEmpty())
                {
                    ActiveBranch.OpenThisBranch();
                }
                else
                {
                    var last = _switcher.SwitchHistory.Last();
                    foreach (var node in _myDataHub.CurrentSwitchHistory)
                    {
                        if(_branches.Contains(node.MyBranch)) continue;
                        if(!node.MyBranch.StayVisibleMovingToChild() && node != last)
                            continue;
                        if(node != last)
                            node.MyBranch.DontSetAsActiveBranch();
                        node.MyBranch.OpenThisBranch();
                    }

                    _switcher.SwitchHistory.Remove(last);
                }
            }

        public void OnMoveToNewTrunk(Action endOfMoveAction, ScreenType newTrunksScreenType)
        {
            bool toFullScreenTrunk = newTrunksScreenType == ScreenType.FullScreen || _screenType == ScreenType.FullScreen;
            _currentMoveType = _moveToNextTrunk;

            if(toFullScreenTrunk)
            {
                CloseTrunkForFullScreen(endOfMoveAction);
            }
            else
            {
                endOfMoveAction?.Invoke();
            }
        }

        private void CloseTrunkForFullScreen(Action endOfMoveAction)
        {
            void CloseTrunk() => OnExitTrunk(endOfMoveAction, false);

            if (_myDataHub.CurrentSwitchHistory.IsNotEmpty())
            {
                CloseOpenBranchesInSwitchHistory(CloseTrunk);
            }
            else
            {
                CloseTrunk();
            }
        }

        private void CloseOpenBranchesInSwitchHistory(Action closeTrunk)
        {
            var last = _myDataHub.CurrentSwitchHistory.Last();
            foreach (var node in _myDataHub.CurrentSwitchHistory)
            {
                if (node == last) continue;
                if (_branches.Contains(node.MyBranch)) continue;

                node.MyBranch.ExitThisBranch(OutTweenType.MoveToChild);
            }

            last.MyBranch.ExitThisBranch(OutTweenType.MoveToChild, closeTrunk);
        }

        public void OnExitTrunk(Action endOfMoveAction, bool removeFromHistory = true)
        {
            Debug.Log($"Exit : {this}");

            if(removeFromHistory)
                _myDataHub.RemoveTrunk(this);
            
            if (TweenToNextImmediately)
            {
                _myTweener.StartOutTweens(CloseBranches);
                endOfMoveAction?.Invoke();
                endOfMoveAction = null;
            }
            else
            {
                _myTweener.StartOutTweens(CloseBranches);
            }
        
            void CloseBranches()
            {
                _switcher.CloseAllBranches(EndAction, HasNoValidTweener);
            }  
            
             void EndAction()
             {
                 _myCanvas.enabled = false;
                 endOfMoveAction?.Invoke();
             }

             _currentMoveType = _moveBackFromTrunk;
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