using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "UIElements Schemes / New Input Scheme - Old", fileName = "Scheme - Old")]
public class OldSystem : InputScheme
{
    [Space(10f, order = 1)]
    [Header("Input Settings", order = 2)] [HorizontalLine(1, color: EColor.Blue, order = 3)]
    
    [Header("Main Controls", order = 4)]
    [SerializeField] [Label("Pause / Option Button")] [InputAxis]
    private string _pauseOptionButton = default;
    
    [SerializeField] 
    [InputAxis] private string _cancelButton = default;
    
    [SerializeField] [InputAxis] 
    private string _selectButton = default;
    
    [SerializeField] [InputAxis] 
    private string _multiSelectButton = default;

    [SerializeField] [InputAxis] 
    private string _upAndDownNavigate;
    
    [SerializeField] [InputAxis] 
    private string _leftAndRightNavigate;

    [Header("Switch Controls")]
    [SerializeField]
    [InputAxis] [Label("Next UI Group Object")]
    private string _posSwitchButton = default;
    [SerializeField] 
    [InputAxis] [Label("Previous UI Group Object")]
    private string _negSwitchButton = default;
    [SerializeField] 
    [InputAxis] [Label("Next GOUI Object")]
    private string _posNextGOUIButton = default;
    [SerializeField] 
    [InputAxis] [Label("Previous GOUI Object")]
    private string _negNextGOUIButton = default;
    [SerializeField] 
    [Label("Switch To/From Game Menus")] [InputAxis] 
    private string _switchToMenusButton = default;
    [SerializeField] 
    
    [Header("Virtual Cursor Controls")]
    [Label("Switch To Virtual Cursor")] [InputAxis] 
    private string _switchToVC = default;
    [SerializeField] 
    [Label("Virtual Cursor Horizontal")] [InputAxis] 
    private string _vCursorHorizontal = default;
    [SerializeField] 
    [Label("Virtual Cursor Vertical")] [InputAxis] 
    private string _vCursorVertical = default;

    [Space(10f, order = 1)]
    [Header("HotKey Settings", order = 2)] [HorizontalLine(1, color: EColor.Blue, order = 3)]

    [SerializeField] 
    [InputAxis] private string _hotKey1 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey2 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey3 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey4 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey5 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey6 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey7 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey8 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey9 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey0 = default;


    //Properties and Setter/Getters
    protected override float MouseXAxis => CheckInput.GetAxis("Mouse X");
    protected override float MouseYAxis => CheckInput.GetAxis("Mouse Y");
    public override bool AnyMouseClicked => LeftMouseClicked || RightMouseClicked;
    public override bool LeftMouseClicked => CheckInput.MouseButton(0);
    public override bool RightMouseClicked => CheckInput.MouseButton(1);
    public override bool CanSwitchToKeysOrController(bool allowKeys)
    {
        if (ControlType == ControlMethod.MouseOnly) return false;
        
        if (CanUseVirtualCursor)
        {
            return (SwitchToVCPressed() || SwitchKeyPressed()) && !allowKeys;
        }
        return NavigationKeyPressed() || SwitchKeyPressed() && !allowKeys;
    }

    //TODO ADD to parent class
    public override bool SwitchKeyPressed() => PressedNegativeSwitch() || PressedPositiveSwitch() 
                                                               || PressedNegativeGOUISwitch() 
                                                               || PressedPositiveGOUISwitch();

    //TODO ADD to parent class
    private bool NavigationKeyPressed() => HorizontalNavPressed() || VerticalNavPressed() || PressSelect();

    public override bool MenuNavigationPressed(bool allowKeys) => /*allowKeys &&*/ NavigationKeyPressed();

    public override AxisEventData DoMenuNavigation()
    {
        int upDownInput = CheckInput.GetAxisRaw(_upAndDownNavigate);
        int leftRightInput = CheckInput.GetAxisRaw(_leftAndRightNavigate);
        return CheckInput.MenuNavCalc(upDownInput, leftRightInput);
    }

    public override bool CanSwitchToMouseOrVC(bool allowKeys)
    {
        if (ControlType == ControlMethod.KeysOrControllerOnly) return false;

        if (CanUseVirtualCursor)
        {
            return SwitchToVCPressed() && allowKeys;
        }
        
        return MouseXAxis != 0 || MouseYAxis != 0;
    }

    public override Vector3 GetMouseOrVcPosition() 
        => CanUseVirtualCursor ? GetVirtualCursorPosition() : Input.mousePosition;
    
    public override void SetVirtualCursorPosition(Vector3 pos) => _virtualCursorPosition = pos;
    private protected override Vector3 GetVirtualCursorPosition() => _virtualCursorPosition;


    //Main
    public override bool HorizontalNavPressed() => CheckInput.Pressed(_upAndDownNavigate);

    public override bool VerticalNavPressed() => CheckInput.Pressed(_leftAndRightNavigate);

    public override bool PressPause() => CheckInput.Pressed(_pauseOptionButton);
    public override bool PressedMenuToGameSwitch() 
        => InGameMenuSystem == InGameSystem.On &&CheckInput.Pressed(_switchToMenusButton);
    public override bool PressedCancel() => CheckInput.Pressed(_cancelButton);
    public override bool PressedPositiveSwitch() => CheckInput.Pressed(_posSwitchButton);
    public override bool PressedNegativeSwitch() => CheckInput.Pressed(_negSwitchButton);
    public override bool PressedPositiveGOUISwitch() => CheckInput.Pressed(_posNextGOUIButton);
    public override bool PressedNegativeGOUISwitch()=> CheckInput.Pressed(_negNextGOUIButton);
    public override bool VcHorizontalPressed() => CheckInput.Pressed(_vCursorHorizontal);
    
    //TODO Check VC still works with these settings 
    public override bool VcVerticalPressed() =>  CheckInput.Pressed(_vCursorVertical);
    
    //TODO Check MultiSelect Still works
    public override bool MultiSelectPressed() => CheckInput.Held(_multiSelectButton);
    public override float VcHorizontal() => CheckInput.GetAxis(_vCursorHorizontal);
    public override float VcVertical() =>  CheckInput.GetAxis((_vCursorVertical));
    public override bool SwitchToVCPressed() => CheckInput.Pressed(_switchToVC); 
    public override bool PressSelect() =>  CheckInput.Pressed(_selectButton);

    public override bool HotKeyChecker(HotKey hotKey)
    {
        switch (hotKey)    
        {
            case HotKey.HotKey1:
                return CheckInput.Pressed(_hotKey1);
            case HotKey.HotKey2:
                return  CheckInput.Pressed(_hotKey2);
            case HotKey.HotKey3:
                return  CheckInput.Pressed(_hotKey3);
            case HotKey.HotKey4:
                return  CheckInput.Pressed(_hotKey4);
            case HotKey.HotKey5:
                return  CheckInput.Pressed(_hotKey5);
            case HotKey.HotKey6:
                return  CheckInput.Pressed(_hotKey6);
            case HotKey.HotKey7:
                return  CheckInput.Pressed(_hotKey7);
            case HotKey.HotKey8:
                return  CheckInput.Pressed(_hotKey8);
            case HotKey.HotKey9:
                return  CheckInput.Pressed(_hotKey9);
            case HotKey.HotKey0:
                return  CheckInput.Pressed(_hotKey0);
            default:
                throw new ArgumentOutOfRangeException(nameof(hotKey), hotKey, null);
        }
    }
}