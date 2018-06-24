using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalActivateTrigger : MonoBehaviour
{
    public GameObject Portal;
    public readonly float OffIntensity = -2;
    public readonly float OnIntensity = 16;

    private Material _material;

    void Awake()
    {
        _material = Portal.transform.GetChild(1).gameObject.GetComponent<Renderer>().material;
        SetPortalOff();
    }

    void OnDestroy()
    {
        SetPortalOn();
    }

    private void SetPortalOff()
    {
        Color emission = Color.white * Mathf.LinearToGammaSpace(OffIntensity);
        _material.SetColor("_EmissionColor", emission);
        Portal.transform.GetChild(2).gameObject.SetActive(false);
    }

    private void SetPortalOn()
    {
        Color emission = Color.white * Mathf.LinearToGammaSpace(OnIntensity);
        _material.SetColor("_EmissionColor", emission);
        Portal.transform.GetChild(2).gameObject.SetActive(true);
    }
}
