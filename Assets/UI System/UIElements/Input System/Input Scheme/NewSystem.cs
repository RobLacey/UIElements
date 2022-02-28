using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "UIElements Schemes / New Input Scheme - New", fileName = "Scheme - New")]
public class NewSystem : InputScheme
{
    protected override float MouseXAxis { get; } = 0;
    protected override float MouseYAxis { get; } = 0;
    public override bool AnyMouseClicked { get; } = false;
    public override bool LeftMouseClicked { get; } = false;
    public override bool RightMouseClicked { get; } = false;

    public override bool SwitchKeyPressed()
    {
        return false;
    }

    public override bool MenuNavigationPressed(bool allowKeys)
    {
        return false;
    }

    public override AxisEventData DoMenuNavigation()
    {
        return new AxisEventData(EventSystem.current);
    }

    public override bool CanSwitchToMouseOrVC(bool allowKeys)
    {
        return false;
    }
    public override bool CanSwitchToKeysOrController(bool allowKeys)
    {
        return false;
    }

    public override Vector3 GetMouseOrVcPosition()
    {
        return Vector3.zero;
    }

    public override void SetVirtualCursorPosition(Vector3 pos)
    {
        Debug.Log("Set VC Position");
    }

    private protected override Vector3 GetVirtualCursorPosition()
    {
        return Vector3.zero;
    }

    public override bool HorizontalNavPressed()
    {
        return false;
    }

    public override bool VerticalNavPressed()
    {
        return false;
    }

    public override bool PressPause()
    {
        return false;
    }

    public override bool PressedMenuToGameSwitch()
    {
        return false;
    }

    public override bool PressedCancel()
    {
        return false;
    }

    public override bool PressedPositiveSwitch()
    {
        return false;
    }

    public override bool PressedNegativeSwitch()
    {
       return false;
    }

    public override bool PressedPositiveGOUISwitch()
    {
        return false;
    }

    public override bool PressedNegativeGOUISwitch()
    {
        return false;
    }

    public override float VcHorizontal()
    {
        return 0;
    }

    public override float VcVertical()
    {
        return 0;
    }

    public override bool VcHorizontalPressed()
    {
        return false;
    }

    public override bool VcVerticalPressed()
    {
        return false;
    }

    public override bool MultiSelectPressed()
    {
        return false;
    }

    public override bool SwitchToVCPressed()
    {
        return false;
    }

    public override bool PressSelect()
    {
        return false;
    }

    public override bool HotKeyChecker(HotKey hotKey)
    {
        return false;
    }

}