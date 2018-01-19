using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIController : Singleton<GUIController> {

    [Header("GUI Controller:")]
    public Canvas Canvas;
    public RectTransform CanvasRectTrans;

    [Header("Character Following GUI")]
    public GUICharacterFollow CharacterFollowingGUIPrefab;

    public GUICharacterFollow GenerateGUICharacterFollow()
    {
        return Instantiate(CharacterFollowingGUIPrefab, this.transform);
    }

    public Canvas GetCanvas()
    {
        return Canvas;
    }

    public RectTransform GetCanvasRectTrans()
    {
        return CanvasRectTrans;
    }
}
