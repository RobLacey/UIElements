using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class BuildTweenData
{
    [HideInInspector] public string _name = defaultName;
    [SerializeField] 
    [AllowNesting] [ValidateInput(SetClassName)] 
    private RectTransform _element;
    [SerializeField] private PositionSettings _positionSettings;
    [SerializeField] private ScaleSettings _scaleSettings;
    [SerializeField] private RotationSettings _rotationSettings;
    [SerializeField] private float _buildNextAfterDelay;
    [HideInInspector] public Vector3 _moveTo;
    [HideInInspector] public Vector3 _scaleTo;
    [HideInInspector] public Vector3 _targetRotation;
    [HideInInspector] public Vector3 _punchStartScale;
    [HideInInspector] public Vector3 _shakeStartScale;

    //Editor
    private const string SetClassName = nameof(SetName);
    private const string defaultName = "Set Me";
    
    private bool SetName()
    {
        if (_element != null)
        {
            _name = $"{_element.name} Tween";
        }
        else
        {
            _name = defaultName;
            ClearSettings(TweenStyle.NoTween);
        }

        return true;
    }
    
    public PositionSettings PositionSettings => _positionSettings;
    public ScaleSettings ScaleSettings => _scaleSettings;
    public RotationSettings RotationSettings => _rotationSettings;
    public RectTransform Element => _element;
    public CanvasGroup MyCanvasGroup { get; private set; }
    public float ToNextDelay => _buildNextAfterDelay;

    public void SetElement()
    {
        if(_element == null) return;
        MyCanvasGroup = SetCanvasGroup(_element);
        _positionSettings.SetRectTransform(_element);
        _scaleSettings.SetRectTransform(_element);
        _rotationSettings.SetRectTransform(_element);
        SetName();
    }

    public void ActivateTweenSettings(TweenScheme scheme)
    {
        _positionSettings.SetUpTween(scheme.PositionTween);
        _rotationSettings.SetUpTween(scheme.RotationTween);
        _scaleSettings.SetUpTween(scheme.ScaleTween);
    }
    
    public void ClearSettings(TweenStyle clear)
    {
        _positionSettings.SetUpTween(clear);
        _rotationSettings.SetUpTween(clear);
        _scaleSettings.SetUpTween(clear);
    }

    private CanvasGroup SetCanvasGroup(RectTransform element) => element.GetComponent<CanvasGroup>();
}