using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// changes intensity and position of light (color?) to create an effect like a fire
/// Is supposed to be on the point light
/// inspired by https://answers.unity.com/questions/742466/camp-fire-light-flicker-control.html
/// </summary>
public class LightFlicker : MonoBehaviour
{
	public float minInten;
	public float maxInten;
	public float maxPosChange;
	/// <summary>
	/// time in sec between target intent changes. value is randomized between *0.8 and *1.2
	/// </summary>
	public float rate;
	/// <summary>
	/// color of low intesity
	/// </summary>
	public Color color1;
	/// <summary>
	/// color of high intensity
	/// </summary>
	public Color color2;

	private Vector3 startPosition;
	private Light lightSource;
	private float lastFlicker;
	private float lastIntent;
	private float targetIntent;
	private float realRate;

	private Vector3 targetPos;
	private Vector3 lastPos;

	// Use this for initialization
	void Start()
	{
		startPosition = transform.localPosition;
		lightSource = GetComponent<Light>();
		lastIntent = lightSource.intensity;
		targetIntent = Random.Range(minInten, maxInten);
		lastPos = startPosition;
		Vector3 tmp = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
		tmp.Normalize();
		targetPos = startPosition + tmp * Random.Range(0, maxPosChange);
	}

	void FixedUpdate()
	{
		if (Time.time == 0)
		{
			return;
		}
		if (Time.time - lastFlicker > realRate)
		{
			lastIntent = targetIntent;
			targetIntent = Random.Range(minInten, maxInten);
			lastFlicker = Time.time;
			realRate = rate * Random.Range(0.8f, 1.2f);

			lastPos = targetPos;
			Vector3 tmp = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
			tmp.Normalize();
			targetPos = startPosition + tmp * Random.Range(0, maxPosChange);
		}
		//lightSource.intensity = Mathf.Lerp(lightSource.intensity, targetIntent, rate / Time.fixedDeltaTime);
		lightSource.intensity = Mathf.Lerp(lastIntent, targetIntent, (Time.time - lastFlicker) / realRate);
		lightSource.color = Color.Lerp(color1, color2, (lightSource.intensity - minInten) / (maxInten - minInten));
		transform.localPosition = Vector3.Lerp(lastPos , targetPos , (Time.time - lastFlicker) / realRate);

		//print((lightSource.intensity - minInten) / (maxInten - minInten));

	}
}
