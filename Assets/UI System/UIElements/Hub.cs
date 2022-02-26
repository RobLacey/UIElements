using System;
using System.Collections;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UIElements
{
    [RequireComponent(typeof(UIInput))]

    public class Hub : MonoBehaviour, IServiceUser, IEZEventUser, ISceneIsChanging, IOnStart
    {
        [SerializeField] private Trunk _rootMenu;
        [SerializeField] private int _nextLevel;
        [Header("Start Delay")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    
        [SerializeField] 
        [Label("Delay UI Start By then..")] [Range(0, 10)]
        protected int _delayUIStart;
    
        [SerializeField] 
        [Label("..Enable Controls After..")] [Range(0, 10)] 
        protected int _controlActivateDelay;

        
        //Variables
        private INode _lastHighlighted;
        private bool _startingInGame, _inMenu;
        private IDataHub _myDataHub;

        //Events
        private Action<IOnStart> OnStart { get; set; } 
        private Action<ISceneIsChanging> SceneChanging { get; set; }

        private void Awake()
        {
            var uIInput = GetComponent<IInput>();

            _startingInGame = uIInput.StartInGame();
            _myDataHub = new DataHub(_rootMenu.MainCanvasRect);
            _myDataHub.OnAwake();
        }

        private void OnEnable()
        {
            UseEZServiceLocator();
            ObserveEvents();
            _myDataHub.OnEnable();
        }

        private void OnDisable()
        {
            UnObserveEvents();
        }

        public void UseEZServiceLocator()
        {
            OnStart = HistoryEvents.Do.Fetch<IOnStart>();
            SceneChanging = HistoryEvents.Do.Fetch<ISceneIsChanging>();
            HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenu);

        }
        
        public void ObserveEvents()
        {
           // InputEvents.Do.Subscribe<IAllowKeys>(SwitchedToKeys);
            HistoryEvents.Do.Subscribe<IHighlightedNode>(SetLastHighlighted);

        }

        public void UnObserveEvents()
        {
            
        }


        private void Start() => StartCoroutine(StartUIDelay());

        
        private IEnumerator StartUIDelay()
        {
            yield return new WaitForEndOfFrame();
            if(_delayUIStart != 0)
                yield return new WaitForSeconds(_delayUIStart);
            CheckIfStartingInGame();
            SetStartPositionsAndSettings();
            StartCoroutine(EnableStartControls());
        }

        private void CheckIfStartingInGame()
        {
            if (_startingInGame)
            {
                // OnStart?.Invoke(this);
                // _myDataHub.SetStarted();
                _inMenu = false;
            }
            else
            {
              //  SetEventSystem(GetFirstHighlightedNodeInHomeGroup());
                _inMenu = true;
            }

            _myDataHub.InMenu = _inMenu;
        }

        private void SetStartPositionsAndSettings() => _rootMenu.SetStartPositionsAndSettings();

        private IEnumerator EnableStartControls()
        {
            if(_controlActivateDelay != 0)
                yield return new WaitForSeconds(_controlActivateDelay);
            
            // if(!_startingInGame)
            // {
                 _myDataHub.SetStarted();
                OnStart?.Invoke(this);
            //}            
         //   SetEventSystem(GetFirstHighlightedNodeInHomeGroup());
        }

        private void SaveInMenu(IInMenu args)
        {
            _inMenu = args.InTheMenu;
           // if(!_inMenu) SetEventSystem(null);
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

        private void SetLastHighlighted(IHighlightedNode args)
        {
            _lastHighlighted = args.Highlighted;
            // if(_inMenu) 
            //     SetEventSystem(_lastHighlighted.ReturnGameObject);
        }

       // private void SwitchedToKeys(IAllowKeys args) => SetEventSystem(GetCorrectLastHighlighted());

        // private static void SetEventSystem(GameObject newGameObject) 
        //     => EventSystem.current.SetSelectedGameObject(newGameObject);
        //
        // private GameObject GetCorrectLastHighlighted()
        // {
        //     return _lastHighlighted is null ? GetFirstHighlightedNodeInHomeGroup() : 
        //                _lastHighlighted.ReturnGameObject;
        // }
        //
        // private GameObject GetFirstHighlightedNodeInHomeGroup()
        // {
        //     return _rootMenu.StartBranch.DefaultStartOnThisNode.ReturnGameObject;
        // }


    }
}