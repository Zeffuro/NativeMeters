using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.PartyList;

internal sealed partial class PartyListMeterConfigurationNode
{
    private void OpenTagPicker(LabeledTextInputNode targetInput)
    {
        currentTagInsertAction = tagString => InsertTag(targetInput, tagString);
        currentTagSelectionAction = tagInfo => InsertTag(targetInput, tagInfo.Tag);

        System.TagSearchAddon.OnInsertClicked = currentTagInsertAction;
        System.TagSearchAddon.SelectionResult = currentTagSelectionAction;
        System.TagSearchAddon.Open();
    }

    private void InsertTag(LabeledTextInputNode targetInput, string tagString)
    {
        if (isDisposed)
            return;

        targetInput.Text += tagString;
        targetInput.InnerInput.OnInputComplete?.Invoke(targetInput.Text);
    }

    protected override void Dispose(bool isNativeDestructor)
    {
        isDisposed = true;
        var canCloseSearchAddon = !isNativeDestructor;

        if (currentTagInsertAction != null
            && System.TagSearchAddon != null
            && System.TagSearchAddon.OnInsertClicked == currentTagInsertAction)
        {
            System.TagSearchAddon.OnInsertClicked = null;
            System.TagSearchAddon.SelectionResult = null;
            if (canCloseSearchAddon) System.TagSearchAddon.Close();
        }

        base.Dispose(isNativeDestructor);
    }
}
