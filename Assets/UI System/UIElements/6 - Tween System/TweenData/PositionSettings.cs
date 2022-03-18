using System;
using System.Diagnostics.CodeAnalysis;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class PositionSettings
{
    [SerializeField] 
    [AllowNesting] [ShowIf(OneWayTweenName)] [OnValueChanged("GrabStartPos")]
    private bool _grabStartPosition  = default;
    [SerializeField] 
    [AllowNesting] [ShowIf(InOutTweenName)] [OnValueChanged("GrabMidPos")]
    private bool _grabMidPosition  = default;
    [SerializeField] 
    [AllowNesting] [ShowIf(OneWayTweenName)] [OnValueChanged("GrabEndPos")]
    private bool _grabEndPosition  = default;
    
#pragma warning disable 414 
    [SerializeField] 
    [AllowNesting] [ShowIf(OneWayTweenName)] [OnValueChanged("GotToStart")]
    private bool _goToStartPosition  = default;
    [SerializeField] 
    [AllowNesting] [ShowIf(InOutTweenName)] [OnValueChanged("GotToMid")]
    private bool _goToMidPosition  = default;
    [SerializeField] 
    [AllowNesting] [ShowIf(OneWayTweenName)] [OnValueChanged("GotToEnd")]
    private bool _goToEndPosition  = default;
#pragma warning restore 414 

    [SerializeField] 
    [ShowIf(OneWayTweenName)] [Label("Start Position")] [ReadOnly]  
    private Vector3 _tweenStartPosition  = default;
    [SerializeField] 
    [ShowIf(InOutTweenName)] [Label("Mid Position")] [ReadOnly] 
    private Vector3 _tweenMiddlePosition  = default;
    [SerializeField] 
    [ShowIf(OneWayTweenName)] [Label("End Position")] [ReadOnly]  
    private Vector3 _tweenTargetPosition  = default;

    //Variables
    private RectTransform _element;
    private const string OneWayTweenName = nameof(OneWayTween);
    private const string InOutTweenName = nameof(InOutTween);

    
    //Properties
    public Vector3 StartPos => _tweenStartPosition;
    public Vector3 MidPos => _tweenMiddlePosition;
    public Vector3 EndPos => _tweenTargetPosition;
    
    public Vector3 TargetPosition { get; set; }

    
    //Editor
    private bool OneWayTween { get; set; }
    private bool InOutTween { get; set; }
    
    
    public void SetRectTransform(RectTransform rectTransform) 
        => _element = rectTransform;
    
    public void SetUpTween(TweenStyle tween)
    {
        OneWayTween = tween != TweenStyle.NoTween;
        InOutTween = tween  == TweenStyle.InAndOut;

        switch (tween)
        {
            case TweenStyle.NoTween:
                break;
            case TweenStyle.In:
                TargetPosition = EndPos;
                break;
            case TweenStyle.Out:
                TargetPosition = EndPos;
                break;
            case TweenStyle.InAndOut:
                TargetPosition = MidPos;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tween), tween, null);
        }
    }
    
    
    
    //Editor
    private void GrabStartPos() 
        => _tweenStartPosition = _grabStartPosition ? _element.anchoredPosition3D : Vector3.zero;
    
    private void GrabMidPos() 
        => _tweenMiddlePosition = _grabMidPosition ? _element.anchoredPosition3D : Vector3.zero;
    
    private void GrabEndPos() 
        => _tweenTargetPosition = _grabEndPosition ? _element.anchoredPosition3D : Vector3.zero;
    
    private void GotToStart()
    {
        _element.anchoredPosition3D = _tweenStartPosition;
        _goToStartPosition = false;
    }
    private void GotToMid()
    {
        _element.anchoredPosition3D = _tweenMiddlePosition;
        _goToMidPosition = false;
    }
    private void GotToEnd()
    {
        _element.anchoredPosition3D = _tweenTargetPosition;
        _goToEndPosition = false;
    }
}

