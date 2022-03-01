using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

/// <summary>
/// UIHub is the core of the system and looks after starting the system Up and general state management 
/// </summary>

public interface IHomeGroupSettings : IParameters
{
    List<IBranch> GroupsBranches { get; }
}

namespace UIElements
{
    public class Trunk : MonoBehaviour, IHomeGroupSettings, ISetUpStartBranches, IEZEventDispatcher, IServiceUser
    {
        [SerializeField] 
        [Label("Overlay Or Fullscreen")]
        private ScreenType _screenType = ScreenType.Overlay;

        [Space(10f, order = 1)]
        [Header(StartOnTitle, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
        [SerializeField] private List<UIBranch> _branches;


        //Variables
        private IDataHub _myDataHub;
        private IHomeGroup _switcher;
        private const string StartOnTitle = "Set On Which Branch To Start On";

        //Events
        private Action<ISetUpStartBranches> SetUpBranchesAtStart { get; set; }

        
        //Properties & Getters/ Setters
        public List<IBranch> GroupsBranches => _branches.ToList<IBranch>();
        public ScreenType ScreenType => _screenType;

        //Main
        private void Awake()
        { 
            // _historyTrack = EZInject.Class.NoParams<IHistoryTrack>();
            _switcher = EZInject.Class.WithParams<IHomeGroup>(this);
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
        }

        private void OnDisable()
        {
            _switcher.OnDisable();
        }

        private void Start()
        {
            _myDataHub.ActiveTrunkGroup = GroupsBranches;
            foreach (var uiBranch in _branches)
            {
                uiBranch.ParentTrunk = this;
            }
        }

        public void OnStartTrunk()
        {
            OnEnable();
            _switcher.OnEnable();
            Debug.Log("Enter : " + name);
            _myDataHub.CurrentTrunk = this;
            _myDataHub.CurrentSwitcher = _switcher;
            _myDataHub.ActiveTrunkGroup = GroupsBranches;
            _switcher.ActivateHomeGroupBranch(null);
            _branches.First().MoveToThisBranch();
        }
        
        public void OnExitTrunk()
        {
            _switcher.OnDisable();
            OnDisable();
            Debug.Log("Exit : " + name);
            foreach (var uiBranch in _branches)
            {
                uiBranch.StartBranchExitProcess(OutTweenType.MoveToChild);
            }
        }

        public void SetStartPositionsAndSettings() => SetUpBranchesAtStart?.Invoke(this);
        
        
        
        //Editor
        [Button("Add a New Tree Structure")]
        private void MakeTreeFolders()
        {
            new CreateNewObjects().CreateMainFolder(transform)
                                  .CreateBranch()
                                  .CreateNode();
        }
        
        [Button("Add a ToolTip Bin")]
        private void MakeTooltipFolder()
        {
            new CreateNewObjects().CreateToolTipFolder(transform);
        }
        
        [Button("Add a In Game Object UI Bin")]
        private void MakeInGameUiFolder()
        {
            new CreateNewObjects().MakeGOUIBin(transform);
        }
        
        [Button("Add a PopUp Bin")]
        private void MakePopupFolder()
        {
            new CreateNewObjects().MakePopUpBin(transform);
        }

    }
}