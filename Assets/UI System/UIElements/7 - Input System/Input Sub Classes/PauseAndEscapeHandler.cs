using System;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace UIElements
{
    [Serializable]
    public class PauseAndEscapeHandler : IMonoEnable, IMonoStart, IServiceUser, IEZEventDispatcher, IPausePressed
    {
        [SerializeField] private Trunk _pauseMenu;
        [SerializeField] private Trunk _escapeMenu;
        [SerializeField] [AllowNesting]
        [Label("Nothing to Cancel Action")] 
        private PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
        [SerializeField]
        private PauseFunction _globalEscapeFunction;
        [SerializeField] [Space(10f)] [HorizontalLine(1f, EColor.Blue)] [Header("User Events")]
        private GameIsPaused _gameIsPaused;

        //Events
        [Serializable] public class GameIsPaused : UnityEvent<bool> { }
        private enum PauseOptionsOnEscape { DoNothing, EnterPauseMenu, EnterEscapeMenu }
        private enum PauseFunction { DoNothing, BackOneLevel, BackToHome }

        private IDataHub _myDataHub;
        private IHistoryTrack _historyTracker;
        private Action<IPausePressed> OnPausedPressed { get; set; }
        public bool ClearScreen => _pauseMenu.ScreenType == ScreenType.FullScreen;

        public bool CanPause() => _pauseMenu;
        // private bool CanEnterPauseWithNothingSelected() =>
        //     (NoActivePopUps && !GameIsPaused && _myDataHub.NoHistory)
        //     && NothingSelectedAction;



        public void OnEnable()
        {
            FetchEvents();
            UseEZServiceLocator();
            Debug.Log("Add Escape menu & Add block pressed paused when in escape menu");
        }

        public void FetchEvents()
        {
            OnPausedPressed = InputEvents.Do.Fetch<IPausePressed>();
        }

        public void UseEZServiceLocator()
        {
            _myDataHub = EZService.Locator.Get<IDataHub>(this);
            _historyTracker = EZService.Locator.Get<IHistoryTrack>(this);
        }
        
        public void OnStart()
        {
            _myDataHub.SetGlobalEscapeSetting(SetGlobalEscapeFunction());
            _myDataHub.SetPausedTrunk(_pauseMenu);
        }
        
        public void PausePressed()
        {
            if (_myDataHub.GamePaused)
            {
                _historyTracker.ExitPause();
                _myDataHub.SetIfGamePaused(false);
                OnPausedPressed?.Invoke(this);
                _pauseMenu.OnExitTrunk(End);

                void End()
                {
                    _myDataHub.RestoreState();
                    _historyTracker.MoveToLastBranchInHistory();
                }
            }
            else
            {
                _myDataHub.SaveState();
                _myDataHub.SetIfGamePaused(true);
                OnPausedPressed?.Invoke(this);
                _pauseMenu.OnStartTrunk();
            }
            
            _gameIsPaused?.Invoke(_myDataHub.GamePaused);
        }

        public bool HandlePauseOrEscapeMenu()
        {
            return false;
        }
        
        private EscapeKey SetGlobalEscapeFunction()
        {
            switch (_globalEscapeFunction)
            {
                case PauseFunction.DoNothing:
                    return EscapeKey.None;
                case PauseFunction.BackOneLevel:
                    return EscapeKey.BackOneLevel;
                case PauseFunction.BackToHome:
                    return EscapeKey.BackToHome;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


    }
}