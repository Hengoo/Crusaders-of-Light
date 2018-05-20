using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class MenuLivePreview : MonoBehaviour
{
	public GameObject mainCamera;
	public GameObject[] characters;

	public PostProcessingProfile m_Profile;

	void Start()
	{
		var behaviour = mainCamera.GetComponent<PostProcessingBehaviour>();
		//var behaviour = GetComponent<PostProcessingBehaviour>();

		if (behaviour.profile == null)
		{
			return;
		}

		m_Profile = Instantiate(behaviour.profile);
		behaviour.profile = m_Profile;
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
		if(m_Profile ==null)
		{
			//i believe this should never get here
			var behaviour = mainCamera.GetComponent<PostProcessingBehaviour>();
			//var behaviour = GetComponent<PostProcessingBehaviour>();
			m_Profile = Instantiate(behaviour.profile);
			behaviour.profile = m_Profile;
		}

		var colorGrading = m_Profile.colorGrading.settings;
		colorGrading.basic.postExposure = brightness * 3;
		m_Profile.colorGrading.settings = colorGrading;
	}

}
