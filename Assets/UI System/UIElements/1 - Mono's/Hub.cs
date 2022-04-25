using System;
using System.Collections;
using DG.Tweening;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIElements
{
    public interface IHub : IParameters
{
    AudioSource UI_AudioSource { get; }
}
    
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(Input))]
    [RequireComponent(typeof(AudioSource))]

    public partial class Hub : MonoBehaviour, IHub, IServiceUser, IEZEventUser, ISceneIsChanging, IOnStart, IIsAService
    {
        
        public Hub()
        {
            PopUpEvents.Do.Initialise(new PopUpBindings());
            HistoryEvents.Do.Initialise(new HistoryBindings());
            InputEvents.Do.Initialise(new InputBindings());
            GOUIEvents.Do.Initialise(new GOUIEventsBindings());
            BranchEvent.Do.Initialise(new BranchBindings());
            CancelEvents.Do.Initialise(new CancelBindings());
            EZService.Locator.Initialise();
        }
        
        [Required("Must have a start point")]
        [SerializeField] private Trunk _rootTrunk;

        [SerializeField] 
        private int _nextLevel;
        [SerializeField] private AudioSource _uiAudioSource;
        
        [Header("Start Delay")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    
        [SerializeField] 
        [Label("Delay UI Start By then..")] [Range(0, 10)]
        protected int _delayUIStart;
    
        [SerializeField] 
        [Label("..Enable Controls After..")] [Range(0, 10)] 
        protected int _controlActivateDelay;
        
        [Space(10f, order = 1)] 
        [Header(CanvasOrderTitle, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
        
        [SerializeField] 
        private CanvasOrderData _canvasSortingOrderSettings;

        [SerializeField] private DataHub _myDataHub;

        //Variables
        private bool _startingInGame, _inMenu;
        //private IDataHub _myDataHub;
        private IAudioService _audioService;
        private IHistoryTrack _historyTrack;
        private ICancel _cancelHandler;


        private const string CanvasOrderTitle = "Canvas Sorting Order Setting for Branch Types";


        public AudioSource UI_AudioSource => _uiAudioSource;


        //Events
        private Action<IOnStart> OnStart { get; set; } 
        private Action<ISceneIsChanging> SceneChanging { get; set; }

        private void Awake()
        {
            Debug.Log("Upto : Back To home Doesn't clear switch history's. Check how this process clears history ");
            Debug.Log( "To Fix : Add a target trunk option for going back (Already in use I think in HistoryData. Allow current trunk to close first." +
                       " Stop trunks being close multiple times (use _tweening same as Branch). Use same process as moving to new Trunk!!");
            Debug.Log("Have clear current Trunk cancel option (uses currentSwitcher list) ");
            Debug.Log("Upto : Rework Canvas Order Calculator to reset on Trunk exit as if it has no tween it doesnt tween Branches ");

            var uIInput = GetComponent<IInput>();
            AddService();
            _startingInGame = uIInput.StartInGame();
            _myDataHub.OnAwake();
            _myDataHub.SetMasterRectTransform(GetComponent<RectTransform>());
            _myDataHub.SetRootTrunk(_rootTrunk);
            _audioService = EZInject.Class.WithParams<IAudioService>(this);
            _historyTrack = EZInject.Class.NoParams<IHistoryTrack>();
            _cancelHandler = EZInject.Class.NoParams<ICancel>();
        }

        private void OnEnable()
        {
            UseEZServiceLocator();
            ObserveEvents();
            //_myDataHub.OnEnable();
            _myDataHub.OnEnable();
            _audioService.OnEnable();
            _canvasSortingOrderSettings.OnEnable();
            _historyTrack.OnEnable();
            _cancelHandler.OnEnable();
        }

        private void OnDisable()
        {
            UnObserveEvents();
            _audioService.OnDisable();
            _cancelHandler.OnDisable();
            DOTween.KillAll();
        }

        public void UseEZServiceLocator()
        {
            OnStart = HistoryEvents.Do.Fetch<IOnStart>();
            SceneChanging = HistoryEvents.Do.Fetch<ISceneIsChanging>();
            HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenu);

        }
        
        public void ObserveEvents()
        {
            
        }

        public void UnObserveEvents()
        {
            
        }
        
        public void AddService()
        {
            EZService.Locator.AddNew<IHub>(this);
        }

        public void OnRemoveService()
        {
            
        }

        private void Start()
        {
            // _myDataHub.SetHighLighted(_rootTrunk.GroupsBranches.First().DefaultStartOnThisNode);
            // _myDataHub.SetActiveBranch(_rootTrunk.GroupsBranches.First());
            _myDataHub.OnStart();
            StartCoroutine(StartUIDelay());
        }

        
        private IEnumerator StartUIDelay()
        {
            yield return new WaitForEndOfFrame();
            if(_delayUIStart != 0)
                yield return new WaitForSeconds(_delayUIStart);
            CheckIfStartingInGame();
            _rootTrunk.OnStartTrunk();
            StartCoroutine(EnableStartControls());
        }

        private void CheckIfStartingInGame()
        {
            if (_startingInGame)
            {
                _inMenu = false;
            }
            else
            {
                _inMenu = true;
            }
        
            _myDataHub.SetInMenu(_inMenu);
        }

        private IEnumerator EnableStartControls()
        {
            if(_controlActivateDelay != 0)
                yield return new WaitForSeconds(_controlActivateDelay);
            
            _myDataHub.SetStarted();
            OnStart?.Invoke(this);
        }

        private void SaveInMenu(IInMenu args)
        {
            _inMenu = args.InTheMenu;
        }


        public void LoadNextLevel()
        {
            SceneChanging?.Invoke(this);
            CloseServices();
            StartCoroutine(Load());
        }
        
        private IEnumerator Load()
        {
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(_nextLevel);
        }

        private void OnApplicationQuit()
        {
            SceneChanging?.Invoke(this);
            CloseServices();
        }
        
        private void CloseServices()
        {
            BranchEvent.Do.Purge();
            GOUIEvents.Do.Purge();
            PopUpEvents.Do.Purge();
            HistoryEvents.Do.Purge();
            InputEvents.Do.Purge();
            CancelEvents.Do.Purge();
            EZService.Locator.Purge();
        }

    }
}