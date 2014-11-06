using UnityEngine;
using System.Collections;

public enum AspectRatio
{
	Aspect4by3 = 43,
	Aspect5by4 = 54,
	Aspect5by3 = 53,
	Aspect16by9 = 169,
	Aspect16by10 = 1610,
	Aspect3by2 = 32,
	Aspect25by15 = 2515,
	AspectCustom1024x600 = 1024600,
	AspectCustom800x480 = 800480,
	AspectOthers = 0
}

public class AspectRatios : MonoBehaviour
{
	//Tolerance of calculated ratio to exact ratio
	const float ratioTolerance = 0.03f;
	
	const float aspect4By3Ratio = 4f / 3f;
	const float aspect5By4Ratio = 5f / 4f;
	const float aspect5By3Ratio = 5f / 3f;
	const float aspect16By9Ratio = 16f / 9f;
	const float aspect16By10Ratio = 16f / 10f;
	const float aspect3By2Ratio = 3f / 2f;
	const float aspect25By15Ratio = 25f / 15f;
	
	//These are currently the custom Unity Android resolutions that don't fit into a standard aspect ratio category
	const float aspectCustom1024x600 = 1024f / 600f;
	const float AspectCustom800x480 = 800f / 480f;
	
	#if UNITY_EDITOR
	// NOTE: this is a hack.  When "Screen.width/height" is called from an editor script, it returns the wrong size, so we call this instead.
	internal static void getScreenSizeHack(out float width, out float height)
	{
	    System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
	    System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
	    System.Object rez = GetSizeOfMainGameView.Invoke(null,null);
	    var rezVec = (Vector2)rez;
	    width = rezVec.x;
	    height = rezVec.y;
	}
	
	public static bool UseEditorResolutionHack;
	#endif

	private static bool statsPrinted;
	public static AspectRatio GetAspectRatio ()
	{
		float currentWidth = Screen.width;
		float currentHeight = Screen.height;
		#if UNITY_EDITOR
		if (UseEditorResolutionHack) getScreenSizeHack(out currentWidth, out currentHeight);
		#endif
		
		//Calculate aspect ratio as a float
		float calculatedAspectRatio = currentWidth / currentHeight;
		
		//check for custom resolutions (usually Android) that don't fit a standard aspect ratio category
		AspectRatio ratio = AspectRatio.AspectOthers;
		if(currentWidth == 1024 && currentHeight == 600)
			ratio = AspectRatio.AspectCustom1024x600;
		else if (currentWidth == 800 && currentHeight == 480)
			ratio = AspectRatio.AspectCustom800x480;
		
		//check for the resular aspect ratios
		else if (Mathf.Abs (calculatedAspectRatio - aspect4By3Ratio) < ratioTolerance)
			ratio = AspectRatio.Aspect4by3;
		else if (Mathf.Abs (calculatedAspectRatio - aspect5By4Ratio) < ratioTolerance)
			ratio = AspectRatio.Aspect5by4;
		else if (Mathf.Abs (calculatedAspectRatio - aspect5By3Ratio) < ratioTolerance)
			ratio = AspectRatio.Aspect5by3;
		else if (Mathf.Abs (calculatedAspectRatio - aspect16By9Ratio) < ratioTolerance)
			ratio = AspectRatio.Aspect16by9;
		else if (Mathf.Abs (calculatedAspectRatio - aspect16By10Ratio) < ratioTolerance)
			ratio = AspectRatio.Aspect16by10;
		else if (Mathf.Abs (calculatedAspectRatio - aspect3By2Ratio) < ratioTolerance)
			ratio = AspectRatio.Aspect3by2;
		else if (Mathf.Abs (calculatedAspectRatio - aspect25By15Ratio) < ratioTolerance)
			ratio = AspectRatio.Aspect25by15;
		
		//we haven't matched an exact aspect ratio so lets find the closest one!
		else
			ratio = FindNearestAspectRatio (calculatedAspectRatio);
			
		if (!statsPrinted)
		{
			statsPrinted = true;
			Debug.Log(string.Format("Unity/Device Resolution: {0} - {1}", currentWidth, currentHeight));
			Debug.Log("Ratio value: " + calculatedAspectRatio);
			Debug.Log("AspectRatio: " + ratio);

			const string label = "AspectRatio value: ";
			switch (ratio)
			{
				case AspectRatio.Aspect4by3: Debug.Log(label + aspect4By3Ratio); break;
				case AspectRatio.Aspect5by4: Debug.Log(label + aspect5By4Ratio); break;
				case AspectRatio.Aspect5by3: Debug.Log(label + aspect5By3Ratio); break;
				case AspectRatio.Aspect16by9: Debug.Log(label + aspect16By9Ratio); break;
				case AspectRatio.Aspect16by10: Debug.Log(label + aspect16By10Ratio); break;
				case AspectRatio.Aspect3by2: Debug.Log(label + aspect3By2Ratio); break;
				case AspectRatio.Aspect25by15: Debug.Log(label + aspect25By15Ratio); break;
				case AspectRatio.AspectCustom1024x600: Debug.Log(label + aspectCustom1024x600); break;
				case AspectRatio.AspectCustom800x480: Debug.Log(label + AspectCustom800x480); break;
				case AspectRatio.AspectOthers: Debug.Log(label + ratio); break;
			}
		}
		
		return ratio;
	}

	static AspectRatio FindNearestAspectRatio (float calculatedAspectRatio)
	{	
		float nearestRatio = float.MinValue;
		float closestFoundSoFar = float.MaxValue;
		float[] ratios = {aspect4By3Ratio, aspect5By4Ratio, aspect5By3Ratio, aspect16By9Ratio, aspect16By10Ratio, aspect3By2Ratio, aspect25By15Ratio};
		
		for (int i = 0; i < ratios.Length; i++) {
			float dist = Mathf.Abs(calculatedAspectRatio - ratios[i]);
			if (dist < closestFoundSoFar){
				nearestRatio = ratios[i];
				closestFoundSoFar = dist;
			}
		}
		
		//return the closest aspect ratio
		if(nearestRatio == aspect4By3Ratio) 
			return AspectRatio.Aspect4by3;
		else if(nearestRatio == aspect5By4Ratio) 
			return AspectRatio.Aspect5by4;
		else if(nearestRatio == aspect5By3Ratio) 
			return AspectRatio.Aspect5by3;
		else if(nearestRatio == aspect16By9Ratio) 
			return AspectRatio.Aspect16by9;
		else if(nearestRatio == aspect16By10Ratio) 
			return AspectRatio.Aspect16by10;
		else if(nearestRatio == aspect3By2Ratio)
			return AspectRatio.Aspect3by2;
		else if(nearestRatio == aspect25By15Ratio)
			return AspectRatio.Aspect25by15;
		else 
			return AspectRatio.AspectOthers;
	}
}
