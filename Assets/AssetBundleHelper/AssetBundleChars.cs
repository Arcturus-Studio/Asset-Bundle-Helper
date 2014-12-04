using UnityEngine;
using System.Collections;

public static class AssetBundleChars {

	public const char TagSeparator = '.'; //Separates tags in tag set
	public const char BundleSeparator = '_'; //Separates bundle name from tag set
	
	//Returns array of all special characters
	public static char[] GetSpecialChars(){
		return new char[]{
			TagSeparator,
			BundleSeparator			
		};
	}

}
