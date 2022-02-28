using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UnityEngine;
using NaughtyAttributes;

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

        [Space(10f, order = 1)]
        [Header(StartOnTitle, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
        [SerializeField] private List<UIBranch> _homeBranches;
        
        //Variables
        private IDataHub _myDataHub;
        private IHistoryTrack _historyTrack;
        private ICancel _cancelHandler;
        private IHomeGroup _homeGroup;
        private const string StartOnTitle = "Set On Which Branch To Start On";

        //Events
        private Action<ISetUpStartBranches> SetUpBranchesAtStart { get; set; }

        
        //Properties & Getters/ Setters
        public List<IBranch> GroupsBranches => _homeBranches.ToList<IBranch>();

        //Main
        private void Awake()
        { 
            _historyTrack = EZInject.Class.NoParams<IHistoryTrack>();
            _cancelHandler = EZInject.Class.NoParams<ICancel>();
            _homeGroup = EZInject.Class.WithParams<IHomeGroup>(this);
        }

        private void OnEnable()
        {
            UseEZServiceLocator();
            FetchEvents();
            _historyTrack.OnEnable();
            _cancelHandler.OnEnable();
            _homeGroup.OnEnable();
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
            _homeGroup.OnDisable();
        }

        private void Start()
        {
            _myDataHub.ActiveTrunkGroup = GroupsBranches;
        }

        public void OnStart()
        {
            _myDataHub.CurrentSwitcher = _homeGroup;
            _myDataHub.ActiveTrunkGroup = GroupsBranches;
            _homeBranches.First().MoveToThisBranch();
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