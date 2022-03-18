using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class ScaleSettings
{

    [SerializeField] [AllowNesting] [ShowIf(InTweenName)]
    private Vector3 _startScale;

    [SerializeField] [AllowNesting] [ShowIf(OutTweenName)] [Label("End Scale")]
    private Vector3 _endScale;

    //variables
    private RectTransform _element;
    private const string OutTweenName = nameof(OutTween);
    private const string InTweenName = nameof(InTween);
    
    //Properties
    public bool InTween { get; set; }
    public bool OutTween { get; set; }
    public Vector3 StartScale => _startScale;
    public Vector3 PresetScale { get; set; }
    public Vector3 EndScale => _endScale;

    public void SetRectTransform(RectTransform rectTransform) => _element = rectTransform;

    public void SetUpTween(TweenStyle scaleTween)
    {
        if(_element is null) return;

        PresetScale = _element.localScale;
        
        switch (scaleTween)
        {
            case TweenStyle.NoTween:
                InTween = false;
                OutTween = false;
                break;
            case TweenStyle.In:
                InTween = true;
                OutTween = false;
                _startScale = Vector3.zero;
                _endScale = PresetScale;
                break;
            case TweenStyle.Out:
                InTween = false;
                OutTween = true;
                _startScale = PresetScale;
                _endScale = Vector3.zero;
                break;
            case TweenStyle.InAndOut:
                InTween = true;
                OutTween = true;
                _startScale = Vector3.zero;
                _endScale = Vector3.zero;
                break;
        }
    }
}