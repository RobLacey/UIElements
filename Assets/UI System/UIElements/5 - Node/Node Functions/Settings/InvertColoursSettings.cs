using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public interface IInvertSettings : IComponentSettings
{
    ActivateWhen ActivateWhen { get; }
    Text Text { get; }
    Image Image { get; }
    Color Colour { get; }
}

[Serializable]
public class InvertColoursSettings : IInvertSettings
{
    [InfoBox("ONLY set to objects not effected by the Colour effects", EInfoBoxType.Warning)]
    
    [SerializeField]
    private ActivateWhen _activateWhen;

    [SerializeField] 
    [AllowNesting] [ShowIf(Active)] [DisableIf("ImageSet")] 
    private Text _text;
    
    [SerializeField] 
    [AllowNesting] [ShowIf(Active)] [DisableIf("TextSet")] 
    private Image _image;
    
    [SerializeField] [AllowNesting] [ShowIf(Active)]
    private Color _invertedColour = Color.white;

    // Editor Scripts
    private bool TextSet() => _text != null;
    private bool ImageSet() => _image != null;
    private bool IsActive() => _activateWhen != ActivateWhen.None;
    private const string Active = nameof(IsActive);

    //Properties, Setters & Getters
    public ActivateWhen ActivateWhen => _activateWhen;
    public Text Text => _text;
    public Image Image => _image;
    public Color Colour => _invertedColour;


    //Main
    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        CheckForSetUpError(functions, uiNodeEvents.ReturnMasterNode);
        
        if (CanCreate(functions))
        {
            return new UIInvertColours(this, uiNodeEvents);
        }
        return null;
    }

    private bool CanCreate(Setting functions) => (functions & Setting.InvertColourCorrection) != 0 && IsActive();

    private void CheckForSetUpError(Setting functions, Node parentNode) 
    {
        if(!CanCreate(functions)) return;
        
        if (_image.IsNull() && _text.IsNull() && _activateWhen != ActivateWhen.None)
            throw new Exception($"No Image or Text set in Invert Colour Correction settings for {parentNode}");
    }
}

