using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class CursorSettings
{
    [SerializeField] 
    [ShowIf("CustomCursor")] 
    private Texture2D _cursor = default;

    [SerializeField] 
    [ShowIf("CustomCursor")] 
    private Vector2 _hotSpot =default;

    public Texture2D CursorTexture => _cursor;
    public Vector2 HotSpot => _hotSpot;
}