using System;
using System.Collections;
using System.Linq;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIElements
{
    public interface IHub : IParameters
{
    AudioSource UI_AudioSource { get; }
}

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]

    [RequireComponent(typeof(UIInput))]
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
        [SerializeField] private int _nextLevel;
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

        //Variables
        private bool _startingInGame, _inMenu;
        private IDataHub _myDataHub;
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
            
            Debug.Log("UpTo : Back from other trunk doesn't work yet.");
            Debug.Log("UpTo : Trunk needs Tween Module. Make Navigate to take branch or trunk");
            Debug.Log("UpTo : Try just storing the active branch maybe? Branch can handle what was last selected by being set from it's node");
            
            var uIInput = GetComponent<IInput>();
            AddService();

            _startingInGame = uIInput.StartInGame();
            _myDataHub = new DataHub(GetComponent<RectTransform>());
            _myDataHub.OnAwake();
            _myDataHub.RootTrunk = _rootTrunk;
            _audioService = EZInject.Class.WithParams<IAudioService>(this);
            _historyTrack = EZInject.Class.NoParams<IHistoryTrack>();
            _cancelHandler = EZInject.Class.NoParams<ICancel>();

        }

        private void OnEnable()
        {
            UseEZServiceLocator();
            ObserveEvents();
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
            _myDataHub.Highlighted = _rootTrunk.GroupsBranches.First().DefaultStartOnThisNode;
            _myDataHub.ActiveBranch = _rootTrunk.GroupsBranches.First();

            StartCoroutine(StartUIDelay());
        }

        
        private IEnumerator StartUIDelay()
        {
            yield return new WaitForEndOfFrame();
            if(_delayUIStart != 0)
                yield return new WaitForSeconds(_delayUIStart);
            CheckIfStartingInGame();
            _rootTrunk.SetStartPositionsAndSettings();
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

            _myDataHub.InMenu = _inMenu;
        }

        private IEnumerator EnableStartControls()
        {
            if(_controlActivateDelay != 0)
                yield return new WaitForSeconds(_controlActivateDelay);
            
            _myDataHub.SetStarted();
            _rootTrunk.StartRootTrunk();
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