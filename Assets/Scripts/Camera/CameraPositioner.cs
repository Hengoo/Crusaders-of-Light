using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositioner : MonoBehaviour
{
    public GameObject[] cameraTargets; // Changed to public for testing. Can be made private later again as it is set through code.
    public GameObject AudioListener;
    public float distanceMin;
	public float distanceMultiplier;
	public float distanceMax;

	// Use this for initialization
	void Start()
	{
	    if (LevelController.Instance)
	    {
	        cameraTargets = LevelController.Instance.GetActivePlayers();
	    }
        else if(TransitionLevelController.Instance)
	    {
	        cameraTargets = TransitionLevelController.Instance.GetActivePlayers();
	    }
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
	    this.transform.position = Vector3.Slerp(this.transform.position, averagePos + new Vector3(0, 1, -.1f).normalized * distance, 0.1f);
	    AudioListener.transform.position = Vector3.Slerp(AudioListener.transform.position, averagePos + Vector3.up * 7, 0.1f);
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
