using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singleton<CameraController> {

    public Camera Camera;
    public CameraPositioner Positioner;

    public Camera GetCamera()
    {
        return Camera;
    }

    public CameraPositioner GetCameraPositioner()
    {
        return Positioner;
    }
}
