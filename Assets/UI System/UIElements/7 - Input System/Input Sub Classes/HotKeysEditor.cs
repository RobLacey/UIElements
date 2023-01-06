﻿using System;
using UnityEngine;

public partial class HotKeys
{
    private Branch _oldBranchType;
    private const string Title = "Invalid Hot Key Type";
    private const string IsAllowed = nameof(IsAllowedType);
    private const string Check = nameof(CheckPreset);

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
                      "Only Standard or Pop Up branch Types allowed";
        }

        if (_myBranch.IsStandardBranch() ||_myBranch.IsAPopUpBranch() || !_myBranch.IsControlBar())
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

    private void CheckPreset()
    {
#if UNITY_EDITOR
        
        var message = String.Empty;

        if (_parentNode.IsNotNull() && _parentNode.HasChildBranch != (IBranch)_myBranch)
        {
            message = $"Can't have \"{_parentNode.name}\" as Preset. " +
                      $"{Environment.NewLine}"                          +
                      $"{Environment.NewLine}"                          +
                      $"Does not have \"{_myBranch}\" as a CHILD BRANCH so isn't a valid Parent.";
        
            UIEditorDialogue.WarningDialogue(Title, message, "OK");

            _parentNode = null;
            
#endif
        }
    }
}