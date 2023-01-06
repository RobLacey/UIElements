using System;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace UIElements
{
    [Serializable]
    public class PauseAndEscapeHandler : IMonoEnable, IMonoStart, IMonoDisable, IServiceUser, IEZEventDispatcher, IPausePressed, IEZEventUser
    {
        [SerializeField] private Trunk _pauseMenu;
        [SerializeField] private Trunk _escapeMenu;
        [SerializeField] 
        [Space(10f)] [HorizontalLine(1f, EColor.Blue)] [Header("User Events")]
        private GameIsPaused _gameIsPaused;

        //Variables
        private IDataHub _myDataHub;
        private IHistoryTrack _historyTracker;
        private PauseOrEscape _pauseOrEscape = PauseOrEscape.NotSet;

        //Events
        private Action<IPausePressed> OnPausedPressed { get; set; }
        [Serializable] public class GameIsPaused : UnityEvent<bool> { }
        
        //Enums
        private enum PauseOrEscape { Pause, Escape, NotSet }

        //Properties
        public bool ClearScreen { get; private set; }


        //Main
        public void OnEnable()
        {
            ObserveEvents();
            FetchEvents();
            UseEZServiceLocator();
        }

        public void FetchEvents() => OnPausedPressed = InputEvents.Do.Fetch<IPausePressed>();

        public void UseEZServiceLocator()
        {
            _myDataHub = EZService.Locator.Get<IDataHub>(this);
            _historyTracker = EZService.Locator.Get<IHistoryTrack>(this);
        }
        
        public void ObserveEvents() => InputEvents.Do.Subscribe<ICancelPause>(DoPauseOrEscapeProcess);

        public void OnDisable() => UnObserveEvents();

        public void UnObserveEvents() => InputEvents.Do.Unsubscribe<ICancelPause>(DoPauseOrEscapeProcess);


        public void OnStart()
        {
            _myDataHub.SetPausedTrunk(_pauseMenu);
            _myDataHub.SetEscapeTrunk(_escapeMenu);
        }

        public bool CanPause()
        {
            if (!_pauseMenu || _pauseOrEscape == PauseOrEscape.Escape) return false;
            
            _pauseOrEscape = PauseOrEscape.Pause;
            return true;
        }
        public bool CanEscape()
        {
            var atRootWithNoHistory = _myDataHub.IsAtRoot && _myDataHub.NoHistory && _myDataHub.NoPopUps;
            
            if (!_escapeMenu || _myDataHub.MultiSelectActive || _pauseOrEscape == PauseOrEscape.Pause) return false;
            
            if (_pauseOrEscape == PauseOrEscape.NotSet)
            {
                if (!atRootWithNoHistory) return false;
                _pauseOrEscape = PauseOrEscape.Escape;
                return true;
            }
            return false;
        }

        
        public void DoPauseOrEscapeProcess(ICancelPause args = null)
        {
            var pauseOrEscape = _pauseOrEscape == PauseOrEscape.Pause ? _pauseMenu : _escapeMenu;
            ClearScreen = pauseOrEscape.ScreenType == ScreenType.FullScreen;
            
            if (_myDataHub.GamePaused)
            {
                ExitPause(pauseOrEscape);
                _pauseOrEscape = PauseOrEscape.NotSet;
            }
            else
            {
                EnterPause(pauseOrEscape);
            }
            
            _gameIsPaused?.Invoke(_myDataHub.GamePaused);
        }

        private void ExitPause(Trunk pauseType)
        {
            _historyTracker.ExitPause();
            _myDataHub.SetIfGamePaused(false);
            OnPausedPressed?.Invoke(this);
            pauseType.SetCurrentMoveTypeToMoveToBack();
            pauseType.OnExitTrunk(End);

            void End()
            {
                _myDataHub.RestoreState();
                _historyTracker.MoveToLastBranchInHistory();
            }
        }

        private void EnterPause(Trunk pauseType)
        {
            _myDataHub.SaveState();
            _myDataHub.SetIfGamePaused(true);
            OnPausedPressed?.Invoke(this);
            pauseType.OnStartTrunk();
        }

    }
}