using NaughtyAttributes;

namespace UIElements
{
    public partial class Hub
    {

        [Button("Add a ToolTip Bin")]
        private void MakeTooltipFolder()
        {
            new CreateNewObjects().CreateToolTipFolder(transform);
        }

        [Button("Add a In Game Object UI Bin")]
        private void MakeInGameUiFolder()
        {
            new CreateNewObjects().MakeGOUIBin(transform);
        }

        [Button("Add a PopUp Bin")]
        private void MakePopupFolder()
        {
            new CreateNewObjects().MakePopUpBin(transform);
        }
    }
}