using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;

public class UITweener : MonoBehaviour, IEndTween, IEZEventDispatcher, IServiceUser
{
    [SerializeField] 
    [BoxGroup("Tween Settings")] [HorizontalLine(1, EColor.Blue , order = 1)]
    private TweenScheme _scheme;

    [SerializeField] 
    [Label(BuildListName)] [BoxGroup("Build Sequence")] [HorizontalLine(1, EColor.Blue , order = 2)]
    private List<BuildTweenData> _buildObjectsList = new List<BuildTweenData>();

    [SerializeField]
    [BoxGroup("Events")] [HorizontalLine(1, EColor.Blue , order = 3)]
    private TweenEvents _tweenEvents;
    
    //Variables
    private int _counter, _effectCounter;
    private List<ITweenBase> _activeTweens;
    private readonly ITweenInspector _tweenInspector = EZInject.Class.NoParams<ITweenInspector>();
    private IDataHub _myDataHub;

    //Properties
    public RectTransform EndTweenRect { get; private set; }
    public bool HasBuildList => _buildObjectsList.Count > 0 && _buildObjectsList[0].Element.IsNotNull();
    public TweenScheme Scheme => _scheme;

    //Delegates
    private Action UserTweenEndCallback{ get; set; }
    private Action<IEndTween> EndTweenEffect { get; set; }
    private TweenTrigger TweenTypeEndEvent{ get; set; }
    //public bool HasInAndOutTween() => !(_scheme is null) && _scheme.InAndOutTween();
    
    //Editor
    private const string BuildListName = "Order To Tween (Reorderable)";

    //Main
    public void Awake()
    {
        if(_buildObjectsList.Count == 0 || _scheme is null) return;
        _activeTweens = TweenFactory.CreateTypes(_scheme);
        FetchEvents();
        
        foreach (var activeTween in _activeTweens)
        {
            _counter++;
            activeTween.SetUpTweens(_buildObjectsList, _scheme, PunchOrShakeEndEffect);
        }
    }
    
    public void FetchEvents() => EndTweenEffect = BranchEvent.Do.Fetch<IEndTween>();

    private void OnEnable()
    {
        UseEZServiceLocator();
        if (_activeTweens is null) return;
        foreach (var activeTween in _activeTweens)
        {
            activeTween.ObserveEvents();
        }
    }
    
    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    private void OnDisable()
    {
        if (_activeTweens is null) return;
        foreach (var activeTween in _activeTweens)
        {
            activeTween.UnObserveEvents();
        }
    }

    private void OnValidate() =>  _tweenInspector.CurrentScheme(_scheme)
                                                 .CurrentBuildList(_buildObjectsList)
                                                 .UpdateInspector();

    public void StartInTweens(Action callBack)
    {
        _myDataHub.AddPlayingTween();
        _tweenEvents.InTweenEventStart?.Invoke();
        StartProcessingTweens(TweenType.In, callBack, _tweenEvents.InTweenEventEnd);
    }
    public void StartOutTweens(Action callBack)
    {
        _myDataHub.AddPlayingTween();
        _tweenEvents.OutTweenEventStart?.Invoke();
        StartProcessingTweens(TweenType.Out, callBack, _tweenEvents.OutTweenEventEnd);
    }
    private void StartProcessingTweens(TweenType tweenType, Action callBack, TweenTrigger tweenTypeEvent)
    {
        UserTweenEndCallback = callBack;
        TweenTypeEndEvent = tweenTypeEvent;

        if (IfTweenCounterIsZero_In()) return;

        _effectCounter = _counter;
        DoTweens(tweenType, EndOfTween);
    }

    private bool IfTweenCounterIsZero_In()
    {
        if (_counter > 0 && _scheme) return false;
        EndOfTween();
        return true;
    }

    private void DoTweens(TweenType tweenType, TweenCallback endOfTweenActions)
    {
        foreach (var activeTween in _activeTweens)
        {
            activeTween.StartTween(tweenType, endOfTweenActions);
        }
    }
    
    private void EndOfTween()
    {
        _myDataHub.RemovePlayingTween();
        _effectCounter--;
        if (_effectCounter > 0) return;
        TweenTypeEndEvent?.Invoke();
        UserTweenEndCallback?.Invoke();
    }
    
    private void PunchOrShakeEndEffect(BuildTweenData item)
    {
        EndTweenRect = item.Element;
        EndTweenEffect?.Invoke(this);
    }

}

