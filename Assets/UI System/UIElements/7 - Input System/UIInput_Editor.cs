using System.Linq;

public partial class Input
{
    private const string Settings = "Other Settings ";
    private bool HasScheme(InputScheme scheme) => scheme != null;
    private const string InfoBox = "Must Assign an Input Scheme";
    private const string CheckForScheme = nameof(HasScheme);
    private int _lastIndex;
    private const string CheckForNewHotKey = nameof(CheckForNew);

    private const string HotKeyText =
        "All Hot Keys, except Pop Ups, must have a parent node to return to. " +
        "This node MUST have the hotkey as a child branch too. "               +
        "This means when the hot key is activated it will always return to this node";

    private void CheckForNew()
    {
        if (_lastIndex < _hotKeySettings.Count)
        {
            _lastIndex = _hotKeySettings.Count;
            _hotKeySettings.Last().ResetOnNewHotKey();
        }

        if (_lastIndex > _hotKeySettings.Count)
            _lastIndex = _hotKeySettings.Count;
    }
}