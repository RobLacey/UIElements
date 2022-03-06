using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UnityEngine;
using NaughtyAttributes;

namespace UIElements
{
    public interface ITrunkData : IParameters
    {
        List<IBranch> GroupsBranches { get; }
    }

    /// <summary>
    /// Trunk is the base module of each new screen that has a connected behaviour 
    /// </summary>

    public class Trunk : MonoBehaviour, ITrunkData, ISetUpStartBranches, IEZEventDispatcher, IServiceUser, IOnHomeScreen
                         , IClearScreen, IAddTrunk
    {
        [SerializeField] 
        [Label("Overlay Or Fullscreen")]
        private ScreenType _screenType = ScreenType.Overlay;

        [Space(10f, order = 1)]
        [Header(SwitchTitle, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
        [SerializeField] private List<UIBranch> _branches;


        //Variables
        private IDataHub _myDataHub;
        private UIData _displayData;
        private ISwitchTrunkGroup _switcher;
        private const string SwitchTitle = "Set The Branches To Use With Switcher";

        //Events
        private Action<ISetUpStartBranches> SetUpBranchesAtStart { get; set; }
        private Action<IOnHomeScreen> SetIsOnHomeScreen { get; set; }
        private Action<IClearScreen> DoClearScreen { get; set; }
        private Action<IAddTrunk> AddTrunk { get; set; }


        
        //Properties & Getters/ Setters
        public List<IBranch> GroupsBranches => _branches.ToList<IBranch>();
        public ScreenType ScreenType => _screenType;
        public Trunk ThisTrunk => this;
        public IBranch ActiveBranch => _switcher.CurrentBranch;

        //Main
        private void Awake()
        { 
            // _historyTrack = EZInject.Class.NoParams<IHistoryTrack>();
            _switcher = EZInject.Class.WithParams<ISwitchTrunkGroup>(this);
            _displayData = FindObjectOfType<UIData>();

            var allChildBranches = GetComponentsInChildren<IBranch>();
            foreach (var uiBranch in allChildBranches)
            {
                uiBranch.ParentTrunk = this;
            }
        }

        private void OnEnable()
        {
            UseEZServiceLocator();
            FetchEvents();
           // _switcher.OnEnable();
        }
        
        public void UseEZServiceLocator()
        {
            _myDataHub = EZService.Locator.Get<IDataHub>(this);
        }

        public void FetchEvents()
        {
            SetUpBranchesAtStart = BranchEvent.Do.Fetch<ISetUpStartBranches>();
            SetIsOnHomeScreen = HistoryEvents.Do.Fetch<IOnHomeScreen>();
            DoClearScreen = BranchEvent.Do.Fetch<IClearScreen>();
            AddTrunk = HistoryEvents.Do.Fetch<IAddTrunk>();
        }

        private void OnDisable()
        {
            _switcher.OnDisable();
        }

        private void Start()
        {
             _myDataHub.ActiveTrunkGroup = GroupsBranches;
            // foreach (var uiBranch in _branches)
            // {
            //     uiBranch.ParentTrunk = this;
            // }
        }

        public void StartRootTrunk()
        {
            OnStartTrunk();
            _switcher.CurrentBranch.MoveToThisBranch();
        }

        // public void MoveBackTo()
        // {
        //     OnStartTrunk();
        //     _switcher.CurrentBranch.MoveToThisBranch();
        // }

        public void OnStartTrunk()
        {
            if (_myDataHub.RootTrunk == this)
            {
                _myDataHub.SetOnHomeScreen(true);
                SetIsOnHomeScreen?.Invoke(this);
            }
            
            if(ScreenType == ScreenType.FullScreen)
                DoClearScreen?.Invoke(this);
            OnEnable();
            _switcher.OnEnable();
            AddTrunk?.Invoke(this);
            //Debug.Log("Enter : " + name);
            //_myDataHub.se
            _myDataHub.CurrentSwitcher = _switcher;
            _myDataHub.ActiveTrunkGroup = GroupsBranches;
            _switcher.OpenAllBranches();
        }

        public void CancelPressed(Action endOfMoveAction, IBranch movingFrom)
        {
           // RemoveTrunks?.Invoke(this);
           // OnExitTrunk(endOfMoveAction);
        }
        
        public void OnExitTrunk(Action endOfMoveAction, ScreenType newTrunksScreenType)
        {
            _switcher.OnDisable();
            OnDisable();
            //IBranch lastBranch = null;
            
            if(newTrunksScreenType == ScreenType.FullScreen)
            {
                 _switcher.CloseAllBranches(EndOfClose);
            }
            else
            {
                 EndOfClose();
            }
            
            
            void EndOfClose()
            {
                //TODO Check This Functionality
                if (_myDataHub.RootTrunk == this)
                {
                    _myDataHub.SetOnHomeScreen(false);
                    SetIsOnHomeScreen?.Invoke(this);
                }

                endOfMoveAction?.Invoke();
            }
        }
        public void OnExitTrunk(Action endOfMoveAction)
        {
            _switcher.OnDisable();
            OnDisable();
            //IBranch lastBranch = null;
            
            _switcher.CloseAllBranches(EndOfClose);
            
            
            void EndOfClose()
            {
                //TODO Check This Functionality
                if (_myDataHub.RootTrunk == this)
                {
                    Debug.Log("Here");
                    _myDataHub.SetOnHomeScreen(false);
                    SetIsOnHomeScreen?.Invoke(this);
                }

                endOfMoveAction?.Invoke();
            }

        }

        public void SetStartPositionsAndSettings() => SetUpBranchesAtStart?.Invoke(this);
        
        [Button("Add a New Tree Structure")]
        private void MakeTreeFolders()
        {
            new CreateNewObjects().CreateMainFolder(transform)
                                  .CreateBranch()
                                  .CreateNode();
        }

        

    }
}