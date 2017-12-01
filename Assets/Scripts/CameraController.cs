using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singleton<CameraController> {

    public Camera Camera;

    public Camera GetCamera()
    {
        return Camera;
    }

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
