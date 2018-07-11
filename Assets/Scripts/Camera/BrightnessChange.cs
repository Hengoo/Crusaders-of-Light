
using UnityEngine;
using UnityEngine.PostProcessing;

//copied and modified from here https://github.com/Unity-Technologies/PostProcessing/wiki/(v1)-Runtime-post-processing-modification

public class BrightnessChange : MonoBehaviour
{
	void Start()
	{
		//PS: when changing this remember to also change it in MenuLivePreview

		//ambient light version mainly brightens dark parts of the screen.  probably need the post exposure if it looks dull on dark monitors 
		RenderSettings.ambientSkyColor = Color.Lerp(Color.black, Color.white , GameController.Instance.Brightness);

		//var behaviour = GetComponent<PostProcessingBehaviour>();

		//if (behaviour.profile == null)
		//{
		//	enabled = false;
		//	return;
		//}
		//float bright = 0;
		//if (Application.isPlaying)
		//{
		//	bright = GameController.Instance.Brightness;
		//}
		//
		//m_Profile = Instantiate(behaviour.profile);
		//behaviour.profile = m_Profile;
		//
		//var colorGrading = m_Profile.colorGrading.settings;
		//colorGrading.basic.postExposure = bright*3;
		//m_Profile.colorGrading.settings = colorGrading;

	}
}