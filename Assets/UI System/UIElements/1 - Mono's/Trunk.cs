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
    public class Trunk : MonoBehaviour,/* ISetUpStartBranches,*/ IEZEventDispatcher, IServiceUser, IIsAtRootTrunk
                         , IClearScreen/*IAddTrunk, IRemoveTrunk*/
    {
        // [Header("Trunk Settings")] [HorizontalLine(1f, EColor.Blue, order = 1)]
        // [SerializeField]
        // private BranchType _allowedBranchType = BranchType.Standard;

        [SerializeField] 
        [Label("Overlay Or Fullscreen")]
        private ScreenType _screenType = ScreenType.Overlay;

        // [SerializeField] private IsActive _raycastToThisOnly = IsActive.No;

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

        //Editor
        private const String HasBranches = nameof(HasTrunkBranches);
        private bool HasTrunkBranches(List<Branch> list) => list.IsNotEmpty() && list.First().IsNotNull();


        //Events
        private Action<IClearScreen> DoClearScreen { get; set; }
        
        //Properties & Getters/ Setters
        public List<IBranch> GroupsBranches => _branches.ToList<IBranch>();
        public ScreenType ScreenType => _screenType;
        //public Trunk ThisTrunk => this;
        public IBranch ActiveBranch => _switcher.CurrentBranch;
        
        /// <summary>
        /// Method can be used to reactive the default trunk switcher if tab groups are present
        /// </summary>
        //public void ActivateTrunkSwitcher() => _myDataHub.SetSwitcher(_switcher);


        //Main
        private void Awake()
        { 
            _switcher = EZInject.Class.NoParams<ISwitchTrunkGroup>();
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
           // SetUpBranchesAtStart = BranchEvent.Do.Fetch<ISetUpStartBranches>();
            // SetIsAtRoot = HistoryEvents.Do.Fetch<IIsAtRootTrunk>();
            DoClearScreen = BranchEvent.Do.Fetch<IClearScreen>();
            // AddTrunk = HistoryEvents.Do.Fetch<IAddTrunk>();
            // RemoveTrunk = HistoryEvents.Do.Fetch<IRemoveTrunk>();
        }

        private void OnDisable() => _switcher.OnDisable();

        private void Start()
        {
            _switcher.ThisGroup = GroupsBranches;
           // _myDataHub.ActiveTrunkGroup = GroupsBranches;
        }

        public void SetStartPositionsAndSettings()
        {
            _myCanvas.enabled = true;
            OnStartTrunk();
            //SetUpBranchesAtStart?.Invoke(this);
        }

        // public void StartRootTrunk()
        // {
        //    // if(!_firstTimeStart)
        //        // OnStartTrunk();
        //     _switcher.CurrentBranch.MoveToThisBranch();
        //    // _firstTimeStart = true; //Stops OnStartTrunk being called twice at the scene start
        // }

        public void OnStartTrunk(IBranch newParent = null)
        {
            if(_myDataHub.CurrentTrunk == this) return;
            
           // RootTrunkActiveStatus();
            
            if(ScreenType == ScreenType.FullScreen)
            {
                 DoClearScreen?.Invoke(this);
            }            
            OnEnable();
            // AddTrunk?.Invoke(this);
            _myDataHub.AddTrunk(this);
            _myDataHub.SetSwitcher(_switcher);
            _myTweener.StartInTweens(null);
            //if(_myCanvas.enabled) return;
            _myCanvas.enabled = true;
            _switcher.OpenAllBranches(newParent);
         //   _myDataHub.ActiveTrunkGroup = GroupsBranches;

        }

        // private void RootTrunkActiveStatus()
        // {
        //     _myDataHub.SetIsAtRoot(_myDataHub.RootTrunk == this);
        //     SetIsAtRoot?.Invoke(this);
        // }

        public void OnMoveToNewTrunk(Action endOfMoveAction, ScreenType newTrunksScreenType)
        {
            if(newTrunksScreenType == ScreenType.FullScreen || _screenType == ScreenType.FullScreen)
            {
                _myDataHub.RemoveTrunk(this);
                _myTweener.StartOutTweens(EndActionNotVisible);
               // _switcher.CloseAllBranches(EndAction);
               // _myCanvas.enabled = false;
            }
            else
            {
                _myTweener.StartOutTweens(EndAction);
            }

            void EndActionNotVisible()
            {
                _switcher.CloseAllBranches(EndAction);
                //EndAction();
            }
            
            void EndAction()
            {
                OnDisable();
                _myCanvas.enabled = false;
                //RootTrunkActiveStatus();
                endOfMoveAction?.Invoke();
            }
        }
        
        public void OnExitTrunk(Action endOfMoveAction)
        {
            //OnDisable();
            _myDataHub.RemoveTrunk(this);
            
            _myTweener.StartOutTweens(CloseBranches);
        
            void CloseBranches()
            {
                _switcher.CloseAllBranches(EndAction);
            }  
            
             void EndAction()
             {
                 OnDisable();
                 _myCanvas.enabled = false;
                 //RootTrunkActiveStatus();
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