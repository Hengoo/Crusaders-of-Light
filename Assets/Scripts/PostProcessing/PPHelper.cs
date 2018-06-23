using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PPHelper : MonoBehaviour {

	private const int buffersize = 200;

	private Vector4[] vec4Array;

	public static PPHelper T;

	public ComputeBuffer buffer;

//	public ComputeShader computeShader;

	public Camera cam;

	public void Awake()
	{
		if (T != null)
			GameObject.Destroy(T);
		else
			T = this;

		//DontDestroyOnLoad(this);

		buffer = new ComputeBuffer(buffersize, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector4)), ComputeBufferType.Default);
		//enemyPos[] test = new enemyPos[] { new enemyPos { pos = new Vector3(0.5f, 0.5f, 0.5f), factor = 12} };
		//print(test[0].factor);
		//Vector4[] test = new Vector4[] { new Vector4 (0.5f,0.5f,0, ), new Vector4(0f, 0f, 0, 0) };

		vec4Array = new Vector4[buffersize];

		//computeShader = (ComputeShader)Resources.Load("PPEffect");
		
	}


	public void UpdateBuffer(List<GameObject> elements)
	{

		Vector3 tmp = Vector3.zero;
		int len = elements.Count;
		for(int i =0; i< buffersize; i++)
		{
			if(i < len)
			{
				tmp = cam.WorldToViewportPoint(elements[i].transform.position);
				vec4Array[i] = new Vector4(tmp.x, tmp.y, tmp.z, 0.1f);
			}else
			{
				vec4Array[i] = new Vector4(0, 0, 0, 0);
			}
			
		}
		buffer.SetData(vec4Array);
	}


	private void OnDisable()
	{
		buffer.Release();
	}
}
