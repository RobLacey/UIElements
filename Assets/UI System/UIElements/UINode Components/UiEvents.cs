using System;

public interface IUiEvents
{
    int MasterNodeID { get; }
    UINode ReturnMasterNode { get; }
    event Action<bool> WhenPointerOver;
    event Action<bool> IsSelected;
    event Action IsPressed;
    event Action<IDisableData> IsDisabled;
    event Action MuteAudio;
    void DoWhenPointerOver(bool pointer);
    void DoIsSelected(bool selected);
    void DoIsPressed();
    void DoIsDisabled(IDisableData disabled);
    void DoMuteAudio();
}

public class UiEvents : IUiEvents
{
    public UiEvents(int instanceId, UINode node)
    {
        MasterNodeID = instanceId;
        ReturnMasterNode = node;
    }

    public int MasterNodeID { get; }
    public UINode ReturnMasterNode { get; }
    
    //Events
    public event Action<bool> WhenPointerOver;
    public event Action<bool> IsSelected;
    public event Action IsPressed;
    public event Action<IDisableData> IsDisabled;
    public event Action MuteAudio;

    public void DoWhenPointerOver(bool pointer) => WhenPointerOver?.Invoke(pointer);
    public void DoIsSelected(bool selected) => IsSelected?.Invoke(selected);
    public void DoIsPressed() => IsPressed?.Invoke();
    public void DoIsDisabled(IDisableData disabled) => IsDisabled?.Invoke(disabled);
    public void DoMuteAudio() => MuteAudio?.Invoke();
}
