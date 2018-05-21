using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class MenuLivePreview : MonoBehaviour
{
	public GameObject mainCamera;
	public GameObject[] characters;

	void Start()
	{

	}


	public void UpdatePlayerCount(string count)
	{
		int intCount = int.Parse(count);
		for (int i = 0; i < 4; i++)
		{
			characters[i].SetActive(i < intCount);
		}
	}

	public void UpdateBrightness(float brightness)
	{
		RenderSettings.ambientSkyColor = Color.Lerp(Color.black, Color.white, brightness);
		print("test");
	}

}
