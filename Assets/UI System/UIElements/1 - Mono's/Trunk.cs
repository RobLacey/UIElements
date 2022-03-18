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
    public class Trunk : MonoBehaviour, ISetUpStartBranches, IEZEventDispatcher, IServiceUser, IOnHomeScreen
                         , IClearScreen, IAddTrunk, IRemoveTrunk
    {
        [SerializeField] 
        [Label("Overlay Or Fullscreen")]
        private ScreenType _screenType = ScreenType.Overlay;

        [Space(10f, order = 1)]
        [Header(SwitchTitle, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
        [SerializeField] private List<UIBranch> _branches;


        //Variables
        private IDataHub _myDataHub;
        private ISwitchTrunkGroup _switcher;
        private const string SwitchTitle = "Set The Branches To Use With Switcher";
        private Canvas _myCanvas;
        private UITweener _myTweener;
        private bool _firstTimeStart = true;

        //Events
        private Action<ISetUpStartBranches> SetUpBranchesAtStart { get; set; }
        private Action<IOnHomeScreen> SetIsOnHomeScreen { get; set; }
        private Action<IClearScreen> DoClearScreen { get; set; }
        private Action<IAddTrunk> AddTrunk { get; set; }
        private Action<IRemoveTrunk> RemoveTrunk { get; set; }
        
        //Properties & Getters/ Setters
        public List<IBranch> GroupsBranches => _branches.ToList<IBranch>();
        public ScreenType ScreenType => _screenType;
        public Trunk ThisTrunk => this;
        public IBranch ActiveBranch => _switcher.CurrentBranch;
        
        /// <summary>
        /// Method can be used to reactive the default trunk switcher if tab groups are present
        /// </summary>
        public void ActivateTrunkSwitcher() => _myDataHub.CurrentSwitcher = _switcher;


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
            var temp = new List<UIBranch>();
            foreach (var uiBranch in _branches)
            {
                if (CheckForEmptyElements(uiBranch)) continue;
                temp.Add(uiBranch);
                uiBranch.ParentTrunk = this;
            }

            _branches.Clear();
            _branches = temp;
        }

        private bool CheckForEmptyElements(UIBranch uiBranch)
        {
            if (uiBranch.IsNull())
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
            SetUpBranchesAtStart = BranchEvent.Do.Fetch<ISetUpStartBranches>();
            SetIsOnHomeScreen = HistoryEvents.Do.Fetch<IOnHomeScreen>();
            DoClearScreen = BranchEvent.Do.Fetch<IClearScreen>();
            AddTrunk = HistoryEvents.Do.Fetch<IAddTrunk>();
            RemoveTrunk = HistoryEvents.Do.Fetch<IRemoveTrunk>();
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
            SetUpBranchesAtStart?.Invoke(this);
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
            
            IfRootTrunkActiveStatus(true);
            
            if(ScreenType == ScreenType.FullScreen)
            {
                 DoClearScreen?.Invoke(this);
            }            
            OnEnable();
            AddTrunk?.Invoke(this);
            _myDataHub.CurrentSwitcher = _switcher;
            _myTweener.StartInTweens(null);
            //if(_myCanvas.enabled) return;
            _myCanvas.enabled = true;
            _switcher.OpenAllBranches(newParent);
         //   _myDataHub.ActiveTrunkGroup = GroupsBranches;
            
            // void End()
            // {
            // }
        }

        private void IfRootTrunkActiveStatus(bool isActive)
        {
            if (_myDataHub.RootTrunk == this)
            {
                _myDataHub.SetOnHomeScreen(isActive);
                SetIsOnHomeScreen?.Invoke(this);
            }
        }

        public void OnMoveToNewTrunk(Action endOfMoveAction, ScreenType newTrunksScreenType)
        {
            if(newTrunksScreenType == ScreenType.FullScreen || _screenType == ScreenType.FullScreen)
            {
                RemoveTrunk?.Invoke(this);
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
                _myCanvas.enabled = false;
                //EndAction();
            }
            
            void EndAction()
            {
                OnDisable();
                IfRootTrunkActiveStatus(false);
                endOfMoveAction?.Invoke();
            }
        }
        
        public void OnExitTrunk(Action endOfMoveAction)
        {
            //OnDisable();
            RemoveTrunk?.Invoke(this);
            
            _myTweener.StartOutTweens(CloseBranches);
        
            void CloseBranches()
            {
                _switcher.CloseAllBranches(EndAction);
            }  
            
             void EndAction()
             {
                 OnDisable();
                 _myCanvas.enabled = false;
                 IfRootTrunkActiveStatus(false);
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