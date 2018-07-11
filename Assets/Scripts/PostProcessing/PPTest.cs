using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(GrayscaleRenderer), PostProcessEvent.AfterStack, "Custom/Grayscale")]
public sealed class PPTest : PostProcessEffectSettings
{
	[Range(0f, 50f), Tooltip("Grayscale effect intensity.")]
	public FloatParameter blend = new FloatParameter { value = 0.5f };
	[Range(-5f, 5f), Tooltip("Darkness of the black and white effect.")]
	public FloatParameter black = new FloatParameter { value = 0f };
	[Range(-1f, 1f), Tooltip("Effect strength depending on distance object to camera.")]
	public FloatParameter dist = new FloatParameter { value = 1f };
	[Range(0, 5f), Tooltip("size factor")]
	public FloatParameter dist2 = new FloatParameter { value = 1f };
	[Range(0, 50), Tooltip("a")]
	public FloatParameter a = new FloatParameter { value = 2.7f };
	[Range(-10f, 10), Tooltip("b")]
	public FloatParameter b = new FloatParameter { value = 1f };
	[Range(-1f, 1), Tooltip("c")]
	public FloatParameter c = new FloatParameter { value = 1f };
}

public sealed class GrayscaleRenderer : PostProcessEffectRenderer<PPTest>
{


	public override void Render(PostProcessRenderContext context)
	{
		//check if my ressources (the list) is there, else do nothing
		//also do nothing for scene view because the transformation matrices are for the camera -> Screen space Positions are wrong
		if (PPHelper.Instance == null || context.isSceneView)
		{
			context.command.BlitFullscreenTriangle(context.source, context.destination);
			return;
		}


		var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Grayscale"));
		sheet.properties.SetFloat("_Factor", settings.blend);
		sheet.properties.SetFloat("_Black", settings.black);
		sheet.properties.SetFloat("_DistFactor", settings.dist);
		sheet.properties.SetFloat("_DistFactor2", settings.dist2);
		sheet.properties.SetFloat("_a", settings.a);
		sheet.properties.SetFloat("_b", settings.b);
		sheet.properties.SetFloat("_c", settings.c);
		//enemyPos[] test= new enemyPos[5];
		//PPHelper.T.buffer.GetData(test);
		//Debug.Log(test[0].factor);
		sheet.properties.SetBuffer("buffer", PPHelper.Instance.buffer);
		context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);





		//compute shader version:
		//can use groupshared memory for reading of enemy position -> should be an immense performance boost
		//DOES NOT WORK

		/*

		//find compute shader: -> need to put thin in PPHelper so its only called once
		//ComputeShader computeShader = (ComputeShader)Resources.Load("PPEffect");
		ComputeShader computeShader = PPHelper.T.computeShader;



		//Kernelid:
		var cmd = context.command;
		int kernel = computeShader.FindKernel("Effect");

		//set variables
		//computeShader.SetFloat("_Factor", settings.blend);
		//computeShader.SetFloat("_Black", settings.black);
		//computeShader.SetFloat("_DistFactor", settings.dist);
		//computeShader.SetFloat("_DistFactor2", settings.dist2);
		//computeShader.SetFloat("_a", settings.a);
		//computeShader.SetFloat("_b", settings.b);
		//computeShader.SetFloat("_c", settings.c);
		//
		//computeShader.SetBuffer(kernel, "buffer", PPHelper.T.buffer);

		cmd.SetComputeVectorParam(computeShader, "_FactorBlackDistDist", new Vector4(settings.blend, settings.black, settings.dist, settings.dist2));
		cmd.SetComputeVectorParam(computeShader, "_abc", new Vector4(settings.a, settings.b, settings.c, 0));

		cmd.SetComputeBufferParam(computeShader, kernel, "buffer", PPHelper.T.buffer);

		//set textures:
		cmd.SetComputeTextureParam(computeShader, kernel, "_Source", context.source);
		cmd.SetComputeTextureParam(computeShader, kernel, "_Destination", context.destination);

		//dispatch

		// Pixel dimensions of logical screen size
		//context.screenHeight;
		//context.screenWidth;
		cmd.DispatchCompute(computeShader, kernel, 20, 20, 1);

		//context.destination =

	*/
	}
}