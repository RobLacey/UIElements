using System;
using UnityEditor;

public class UIEditorDialogue
{
    //Convert into Builder pattern if expanded in future

    public static void WarningDialogue(string title, string message, string okButton)
    {
#if UNITY_EDITOR
        
        EditorUtility.DisplayDialog(title, message, okButton);
#endif
    }

    public static void DialogueWithCancelAction(string title, string message, string okButton, string cancel, Action cancelAction = null)
    {
#if UNITY_EDITOR
        
        if (!EditorUtility.DisplayDialog(title, message, okButton, cancel))
        {
            cancelAction?.Invoke();
        }
#endif
    }
}