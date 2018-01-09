using UnityEngine;
using UnityEngine.PostProcessing;

//copied from here https://github.com/Unity-Technologies/PostProcessing/wiki/(v1)-Runtime-post-processing-modification

[RequireComponent(typeof(PostProcessingBehaviour))]
public class BrightnessChange : MonoBehaviour
{
	PostProcessingProfile m_Profile;

	void OnEnable()
	{
		var behaviour = GetComponent<PostProcessingBehaviour>();

		if (behaviour.profile == null)
		{
			enabled = false;
			return;
		}

		m_Profile = Instantiate(behaviour.profile);
		behaviour.profile = m_Profile;
	}

	void Update()
	{
		//var vignette = m_Profile.vignette.settings;
		//vignette.smoothness = Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup) * 0.99f) + 0.01f;
		//m_Profile.vignette.settings = vignette;

		var colorGrading = m_Profile.colorGrading.settings;
		colorGrading.basic.postExposure = 0.4f;
		m_Profile.colorGrading.settings = colorGrading;
	}
}