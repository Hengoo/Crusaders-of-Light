using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositioner : MonoBehaviour
{
    private GameObject[] cameraTargets;
	public float distanceMin;
	public float distanceMultiplier;
	public float distanceMax;

	// Use this for initialization
	void Start()
	{
	    cameraTargets = LevelController.Instance.GetActivePlayers();
	}

	// Update is called once per frame
	void FixedUpdate()
	{
        if (cameraTargets.Length <= 0)
        {
            return;
        }
		float distance = 0;
		Vector3 averagePos = Vector3.zero;
		foreach (GameObject target in cameraTargets)
		{
			averagePos += target.transform.position;
		}
		averagePos /= cameraTargets.Length;
		foreach (GameObject target in cameraTargets)
		{
			distance += (averagePos - target.transform.position).magnitude;
		}
		distance = distance * distanceMultiplier;
		distance = Mathf.Min(distanceMax, distance);
		distance = Mathf.Max(distanceMin, distance);
		this.transform.position = Vector3.Slerp(this.transform.position, averagePos + new Vector3(0, 1, -.8f).normalized * distance, 0.1f);
	}

    public void UpdateCameraTargetsOnPlayerDeath(GameObject DeadPlayer)
    {
        GameObject[] NewCameraTargets = new GameObject[cameraTargets.Length - 1];

        int counter = 0;

        for (int i = 0; i < cameraTargets.Length; i++)
        {
            if (cameraTargets[i] != DeadPlayer)
            {
                NewCameraTargets[counter] = cameraTargets[i];
                counter++;
            }
        }

        cameraTargets = NewCameraTargets;
    }

    public void UpdateCameraTargetsOnPlayerRespawn(GameObject RespawnedPlayer)
    {
        GameObject[] NewCameraTargets = new GameObject[cameraTargets.Length + 1];

        for (int i = 0; i < NewCameraTargets.Length - 1; i++)
        {
            NewCameraTargets[i] = cameraTargets[i];
        }

        NewCameraTargets[NewCameraTargets.Length - 1] = RespawnedPlayer;

        cameraTargets = NewCameraTargets;
    }
}
