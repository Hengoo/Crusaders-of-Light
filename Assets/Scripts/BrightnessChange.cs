
using UnityEngine;
using UnityEngine.PostProcessing;

//copied and modified from here https://github.com/Unity-Technologies/PostProcessing/wiki/(v1)-Runtime-post-processing-modification

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
		float bright = 0;
		if (Application.isPlaying)
		{
			bright = GameController.Instance.Brightness;
		}

		m_Profile = Instantiate(behaviour.profile);
		behaviour.profile = m_Profile;

		var colorGrading = m_Profile.colorGrading.settings;
		colorGrading.basic.postExposure = bright;
		m_Profile.colorGrading.settings = colorGrading;

	}
}