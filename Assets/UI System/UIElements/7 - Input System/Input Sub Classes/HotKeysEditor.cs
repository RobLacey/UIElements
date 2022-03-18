﻿using System;

public partial class HotKeys
{
    private UIBranch _oldBranchType;
    private const string Title = "Invalid Hot Key Type";
    private const string IsAllowed = nameof(IsAllowedType);

    private void IsAllowedType()
    {
        if (_myBranch.IsNull())
        {
            _name = SetName;
            _oldBranchType = null;
            return;
        }

        var message = String.Empty;

        if (_myBranch.IsNotNull())
        {
            message = $"Can't have \"{_myBranch.name}\" as a Hot Key. " +
                      $"{Environment.NewLine}" +
                      $"{Environment.NewLine}" +
                      "Only Standard or HomeScreen branch Types allowed";
        }

        if (_myBranch.ReturnBranchType == BranchType.Standard
            || _myBranch.ReturnBranchType == BranchType.HomeScreenObsolete)
        {
            _name = _myBranch.name;
            _oldBranchType = _myBranch;
            return;
        }
#if UNITY_EDITOR
        
        UIEditorDialogue.WarningDialogue(Title, message, "OK");
#endif
        _myBranch = _oldBranchType;
    }
}