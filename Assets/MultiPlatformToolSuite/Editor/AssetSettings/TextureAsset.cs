using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class TextureAsset : Asset {
	
	public const string nodeName = "TextureAsset";
	
	public int anisoLevel;
    public bool borderMipmap;
    public bool convertToNormalmap;
    public bool correctGamma;
    public bool fadeout;
    public FilterMode filterMode;
    public TextureImporterGenerateCubemap generateCubemap;
    public bool grayscaleToAlpha;
    public float heightmapScale;
    public bool isReadable;
    public bool lightmap;
    public int maxTextureSize;
    public float mipMapBias;
    public bool mipmapEnabled;
    public int mipmapFadeDistanceEnd;
    public int mipmapFadeDistanceStart;
    public TextureImporterMipFilter mipmapFilter;
    public bool normalmap;
    public TextureImporterNormalFilter normalmapFilter;
    public TextureImporterNPOTScale npotScale;
    public TextureImporterFormat textureFormat;
    public TextureImporterType textureType;
    public TextureWrapMode wrapMode;
	
	public TextureAsset() {}
	
	public TextureAsset(StreamReader reader) {
		string line = string.Empty;
		while((line = reader.ReadLine()) != null) {
			if(line.Contains(VerySimpleXml.EndNode(nodeName)))
				break;
			
			//Name
			if(line.Contains(VerySimpleXml.StartNode(nameNodeName)))
				name = VerySimpleXml.NodeValue(line, nameNodeName);
			
			//Path
			if(line.Contains(VerySimpleXml.StartNode(pathNodeName))) {
				guid = AssetDatabase.AssetPathToGUID(VerySimpleXml.NodeValue(line, pathNodeName));
			}
			
			if(line.Contains(VerySimpleXml.StartNode(guidNodeName)))
				guid = VerySimpleXml.NodeValue(line, guidNodeName);
			
			//IMPORT SETTINGS
			if(line.Contains(VerySimpleXml.StartNode("anisoLevel")))
				anisoLevel = int.Parse(VerySimpleXml.NodeValue(line, "anisoLevel"));
			
			if(line.Contains(VerySimpleXml.StartNode("borderMipmap")))
				borderMipmap = bool.Parse(VerySimpleXml.NodeValue(line, "borderMipmap"));
			
			if(line.Contains(VerySimpleXml.StartNode("convertToNormalmap")))
				convertToNormalmap = bool.Parse(VerySimpleXml.NodeValue(line, "convertToNormalmap"));
			
			if(line.Contains(VerySimpleXml.StartNode("correctGamma")))
				correctGamma = bool.Parse(VerySimpleXml.NodeValue(line, "correctGamma"));
			
			if(line.Contains(VerySimpleXml.StartNode("fadeout")))
				fadeout = bool.Parse(VerySimpleXml.NodeValue(line, "fadeout"));
			
			if(line.Contains(VerySimpleXml.StartNode("filterMode")))
				filterMode = (FilterMode) System.Enum.Parse(typeof(FilterMode), VerySimpleXml.NodeValue(line, "filterMode"));
			
			if(line.Contains(VerySimpleXml.StartNode("generateCubemap")))
				generateCubemap = (TextureImporterGenerateCubemap) System.Enum.Parse(typeof(TextureImporterGenerateCubemap), VerySimpleXml.NodeValue(line, "generateCubemap"));
			
			if(line.Contains(VerySimpleXml.StartNode("grayscaleToAlpha")))
				grayscaleToAlpha = bool.Parse(VerySimpleXml.NodeValue(line, "grayscaleToAlpha"));
			
			if(line.Contains(VerySimpleXml.StartNode("heightmapScale")))
				heightmapScale = float.Parse(VerySimpleXml.NodeValue(line, "heightmapScale"));
			
			if(line.Contains(VerySimpleXml.StartNode("isReadable")))
				isReadable = bool.Parse(VerySimpleXml.NodeValue(line, "isReadable"));
			
			if(line.Contains(VerySimpleXml.StartNode("lightmap")))
				lightmap = bool.Parse(VerySimpleXml.NodeValue(line, "lightmap"));
			
			if(line.Contains(VerySimpleXml.StartNode("maxTextureSize")))
				maxTextureSize = int.Parse(VerySimpleXml.NodeValue(line, "maxTextureSize"));
			
			if(line.Contains(VerySimpleXml.StartNode("mipMapBias")))
				mipMapBias = float.Parse(VerySimpleXml.NodeValue(line, "mipMapBias"));
			
			if(line.Contains(VerySimpleXml.StartNode("mipmapEnabled")))
				mipmapEnabled = bool.Parse(VerySimpleXml.NodeValue(line, "mipmapEnabled"));
			
			if(line.Contains(VerySimpleXml.StartNode("mipmapFadeDistanceEnd")))
				mipmapFadeDistanceEnd = int.Parse(VerySimpleXml.NodeValue(line, "mipmapFadeDistanceEnd"));
			
			if(line.Contains(VerySimpleXml.StartNode("mipmapFadeDistanceStart")))
				mipmapFadeDistanceStart = int.Parse(VerySimpleXml.NodeValue(line, "mipmapFadeDistanceStart"));
			
			if(line.Contains(VerySimpleXml.StartNode("mipmapFilter")))
				mipmapFilter = (TextureImporterMipFilter) System.Enum.Parse(typeof(TextureImporterMipFilter), VerySimpleXml.NodeValue(line, "mipmapFilter"));
			
			if(line.Contains(VerySimpleXml.StartNode("normalmap")))
				normalmap = bool.Parse(VerySimpleXml.NodeValue(line, "normalmap"));
			
			if(line.Contains(VerySimpleXml.StartNode("normalmapFilter")))
				normalmapFilter = (TextureImporterNormalFilter) System.Enum.Parse(typeof(TextureImporterNormalFilter), VerySimpleXml.NodeValue(line, "normalmapFilter"));
			
			if(line.Contains(VerySimpleXml.StartNode("npotScale")))
				npotScale = (TextureImporterNPOTScale) System.Enum.Parse(typeof(TextureImporterNPOTScale), VerySimpleXml.NodeValue(line, "npotScale"));
			
			if(line.Contains(VerySimpleXml.StartNode("textureFormat")))
				textureFormat = (TextureImporterFormat) System.Enum.Parse(typeof(TextureImporterFormat), VerySimpleXml.NodeValue(line, "textureFormat"));
			
			if(line.Contains(VerySimpleXml.StartNode("textureType")))
				textureType = (TextureImporterType) System.Enum.Parse(typeof(TextureImporterType), VerySimpleXml.NodeValue(line, "textureType"));
			
			if(line.Contains(VerySimpleXml.StartNode("wrapMode")))
				wrapMode = (TextureWrapMode) System.Enum.Parse(typeof(TextureWrapMode), VerySimpleXml.NodeValue(line, "wrapMode"));
		}
	}

	public override void ApplyImportSettings() {
		string path = AssetDatabase.GUIDToAssetPath(guid);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		
		if(textureImporter == null) {
			Debug.Log("Importer for texture could not be found at path: " + path);
			return;
		}
		
		//Do an import only if the importer's settings don't match our settings
		if(!DoesImporterMatchSettings(textureImporter)) {
			textureImporter.anisoLevel = anisoLevel;
		    textureImporter.borderMipmap = borderMipmap;
		    textureImporter.convertToNormalmap = convertToNormalmap;
		    textureImporter.fadeout = fadeout;
		    textureImporter.filterMode = filterMode;
		    textureImporter.generateCubemap = generateCubemap;
		    textureImporter.grayscaleToAlpha = grayscaleToAlpha;
		    textureImporter.heightmapScale = heightmapScale;
		    textureImporter.isReadable = isReadable;
		    textureImporter.lightmap = lightmap;
		    textureImporter.maxTextureSize = maxTextureSize;
		    textureImporter.mipMapBias = mipMapBias;
		    textureImporter.mipmapEnabled = mipmapEnabled;
		    textureImporter.mipmapFadeDistanceEnd = mipmapFadeDistanceEnd;
		    textureImporter.mipmapFadeDistanceStart = mipmapFadeDistanceStart;
		    textureImporter.mipmapFilter = mipmapFilter;
		    textureImporter.normalmap = normalmap;
		    textureImporter.normalmapFilter = normalmapFilter;
		    textureImporter.npotScale = npotScale;
		    textureImporter.textureFormat = textureFormat;
		    textureImporter.textureType = textureType;
		    textureImporter.wrapMode = wrapMode;
			
			AssetDatabase.ImportAsset(path);
		}
	}
	
	public override bool DoesImporterMatchSettings(AssetImporter importer) {
		TextureImporter textureImporter = importer as TextureImporter;
		
		return textureImporter.anisoLevel == anisoLevel &&
			    textureImporter.borderMipmap == borderMipmap &&
			    textureImporter.convertToNormalmap == convertToNormalmap &&
			    textureImporter.fadeout == fadeout &&
			    textureImporter.filterMode == filterMode &&
			    textureImporter.generateCubemap == generateCubemap &&
			    textureImporter.grayscaleToAlpha == grayscaleToAlpha &&
			    textureImporter.heightmapScale == heightmapScale &&
			    textureImporter.isReadable == isReadable &&
			    textureImporter.lightmap == lightmap &&
			    textureImporter.maxTextureSize == maxTextureSize &&
			    textureImporter.mipMapBias == mipMapBias &&
			    textureImporter.mipmapEnabled == mipmapEnabled &&
			    textureImporter.mipmapFadeDistanceEnd == mipmapFadeDistanceEnd &&
			    textureImporter.mipmapFadeDistanceStart == mipmapFadeDistanceStart &&
			    textureImporter.mipmapFilter == mipmapFilter &&
			    textureImporter.normalmap == normalmap &&
			    textureImporter.normalmapFilter == normalmapFilter &&
			    textureImporter.npotScale == npotScale &&
			    textureImporter.textureFormat == textureFormat &&
			    textureImporter.textureType == textureType &&
			    textureImporter.wrapMode == wrapMode;
	}
	
	public override void DrawImportSettings() {
		string path = AssetDatabase.GUIDToAssetPath(guid);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		
		//If importer is still null, asset can't be found at path
		if(textureImporter == null) {
			GUILayout.Label("Texture Asset with GUID: " + guid + " no longer exists.");
		} else {
			//Name
			GUILayout.Label("Name: " + name);
			//Path
			GUILayout.Label("Path: " + path);
			//GUID
			GUILayout.Label("GUID: " + guid);
			
			//Aniso level
			GUILayout.Label("Aniso level: " + anisoLevel.ToString(), textureImporter.anisoLevel != anisoLevel ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Border mipmap
			GUILayout.Label("Border mipmap: " + borderMipmap.ToString(), textureImporter.borderMipmap != borderMipmap ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Convert to normal map
			GUILayout.Label("Conver to normal map: " + convertToNormalmap.ToString(), textureImporter.convertToNormalmap != convertToNormalmap ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Fadeout
			GUILayout.Label("Fadeout: " + fadeout.ToString(), textureImporter.fadeout != fadeout ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Filter mode
			GUILayout.Label("Filter mode: " + filterMode.ToString(), textureImporter.filterMode != filterMode ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Generate cubemap
			GUILayout.Label("Generate cubemap: " + generateCubemap.ToString(), textureImporter.generateCubemap != generateCubemap ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Heightmap scale
			GUILayout.Label("Heightmap scale: " + heightmapScale.ToString(), textureImporter.heightmapScale != heightmapScale ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Is readable
			GUILayout.Label("Is readable: " + isReadable.ToString(), textureImporter.isReadable != isReadable ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Lightmap
			GUILayout.Label("Lightmap: " + lightmap.ToString(), textureImporter.lightmap != lightmap ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Max texture size
			GUILayout.Label("Max texture size: " + maxTextureSize.ToString(), textureImporter.maxTextureSize != maxTextureSize ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Mipmap bias
			GUILayout.Label("Mipmap bias: " + mipMapBias.ToString(), textureImporter.mipMapBias != mipMapBias ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Mipmap enabled
			GUILayout.Label("Mipmap enabled: " + mipmapEnabled.ToString(), textureImporter.mipmapEnabled != mipmapEnabled ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Mipmap fade distance start
			GUILayout.Label("Mipmap fade distance start: " + mipmapFadeDistanceStart.ToString(), textureImporter.mipmapFadeDistanceStart != mipmapFadeDistanceStart ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Mipmap fade distance end
			GUILayout.Label("Mipmap fade distance end: " + mipmapFadeDistanceEnd.ToString(), textureImporter.mipmapFadeDistanceEnd != mipmapFadeDistanceEnd ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Mipmap filter
			GUILayout.Label("Mipmap filter: " + mipmapFilter.ToString(), textureImporter.mipmapFilter != mipmapFilter ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Normalmap
			GUILayout.Label("Normalmap: " + normalmap.ToString(), textureImporter.normalmap != normalmap ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Normalmap filter
			GUILayout.Label("Normalmap filter: " + normalmapFilter.ToString(), textureImporter.normalmapFilter != normalmapFilter ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//NPOT scale
			GUILayout.Label("NPOT scale: " + npotScale.ToString(), textureImporter.npotScale != npotScale ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Texture format
			GUILayout.Label("Texture format: " + textureFormat.ToString(), textureImporter.textureFormat != textureFormat ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Texture type
			GUILayout.Label("Texture type: " + textureType.ToString(), textureImporter.textureType != textureType ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Wrap mode
			GUILayout.Label("Wrap mode: " + wrapMode.ToString(), textureImporter.wrapMode != wrapMode ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
		}
	}
	
	public override void ReadFromAsset(Object asset) {
		if(!(asset is Texture2D)) {
			Debug.Log("Asset isn't a Texture2D. Can't read from asset");
			return;
		}
		
		Texture2D texture = asset as Texture2D;
		
		name = texture.name;
		
		TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
		if(textureImporter == null) {
			Debug.Log("Could not get texture importer for asset at path: " + AssetDatabase.GetAssetPath(texture));
			return;
		}
		
		guid = AssetDatabase.AssetPathToGUID(textureImporter.assetPath);
		
		anisoLevel = textureImporter.anisoLevel;
	    borderMipmap = textureImporter.borderMipmap;
	    convertToNormalmap = textureImporter.convertToNormalmap;
	    fadeout = textureImporter.fadeout;
	    filterMode = textureImporter.filterMode;
	    generateCubemap = textureImporter.generateCubemap;
	    grayscaleToAlpha = textureImporter.grayscaleToAlpha;
	    heightmapScale = textureImporter.heightmapScale;
	    isReadable = textureImporter.isReadable;
	    lightmap = textureImporter.lightmap;
	    maxTextureSize = textureImporter.maxTextureSize;
	    mipMapBias = textureImporter.mipMapBias;
	    mipmapEnabled = textureImporter.mipmapEnabled;
	    mipmapFadeDistanceEnd = textureImporter.mipmapFadeDistanceEnd;
	    mipmapFadeDistanceStart = textureImporter.mipmapFadeDistanceStart;
	    mipmapFilter = textureImporter.mipmapFilter;
	    normalmap = textureImporter.normalmap;
	    normalmapFilter = textureImporter.normalmapFilter;
	    npotScale = textureImporter.npotScale;
	    textureFormat = textureImporter.textureFormat;
	    textureType = textureImporter.textureType;
	    wrapMode = textureImporter.wrapMode;
	}
	
	public override void WriteToWriter(StreamWriter writer) {
		//Start
		writer.WriteLine(VerySimpleXml.StartNode(nodeName, 2));
		
		//Name
		writer.WriteLine(VerySimpleXml.StartNode(nameNodeName, 3) + name + VerySimpleXml.EndNode(nameNodeName));
		
		//Path
//		writer.WriteLine(VerySimpleXml.StartNode(pathNodeName, 3) + path + VerySimpleXml.EndNode(pathNodeName));
		
		//GUID
		writer.WriteLine(VerySimpleXml.StartNode(guidNodeName, 3) + guid + VerySimpleXml.EndNode(guidNodeName));
		
		//IMPORT SETTINGS
		writer.WriteLine(VerySimpleXml.StartNode("anisoLevel", 3) + anisoLevel.ToString() + VerySimpleXml.EndNode("anisoLevel"));
		writer.WriteLine(VerySimpleXml.StartNode("borderMipmap", 3) + borderMipmap.ToString() + VerySimpleXml.EndNode("borderMipmap"));
		writer.WriteLine(VerySimpleXml.StartNode("convertToNormalmap", 3) + convertToNormalmap.ToString() + VerySimpleXml.EndNode("convertToNormalmap"));
		writer.WriteLine(VerySimpleXml.StartNode("correctGamma", 3) + correctGamma.ToString() + VerySimpleXml.EndNode("correctGamma"));
		writer.WriteLine(VerySimpleXml.StartNode("fadeout", 3) + fadeout.ToString() + VerySimpleXml.EndNode("fadeout"));
		writer.WriteLine(VerySimpleXml.StartNode("filterMode", 3) + filterMode.ToString() + VerySimpleXml.EndNode("filterMode"));
		writer.WriteLine(VerySimpleXml.StartNode("generateCubemap", 3) + generateCubemap.ToString() + VerySimpleXml.EndNode("generateCubemap"));
		writer.WriteLine(VerySimpleXml.StartNode("grayscaleToAlpha", 3) + grayscaleToAlpha.ToString() + VerySimpleXml.EndNode("grayscaleToAlpha"));
		writer.WriteLine(VerySimpleXml.StartNode("heightmapScale", 3) + heightmapScale.ToString() + VerySimpleXml.EndNode("heightmapScale"));
		writer.WriteLine(VerySimpleXml.StartNode("isReadable", 3) + isReadable.ToString() + VerySimpleXml.EndNode("isReadable"));
		writer.WriteLine(VerySimpleXml.StartNode("lightmap", 3) + lightmap.ToString() + VerySimpleXml.EndNode("lightmap"));
		writer.WriteLine(VerySimpleXml.StartNode("maxTextureSize", 3) + maxTextureSize.ToString() + VerySimpleXml.EndNode("maxTextureSize"));
		writer.WriteLine(VerySimpleXml.StartNode("mipMapBias", 3) + mipMapBias.ToString() + VerySimpleXml.EndNode("mipMapBias"));
		writer.WriteLine(VerySimpleXml.StartNode("mipmapEnabled", 3) + mipmapEnabled.ToString() + VerySimpleXml.EndNode("mipmapEnabled"));
		writer.WriteLine(VerySimpleXml.StartNode("mipmapFadeDistanceEnd", 3) + mipmapFadeDistanceEnd.ToString() + VerySimpleXml.EndNode("mipmapFadeDistanceEnd"));
		writer.WriteLine(VerySimpleXml.StartNode("mipmapFadeDistanceStart", 3) + mipmapFadeDistanceStart.ToString() + VerySimpleXml.EndNode("mipmapFadeDistanceStart"));
		writer.WriteLine(VerySimpleXml.StartNode("mipmapFilter", 3) + mipmapFilter.ToString() + VerySimpleXml.EndNode("mipmapFilter"));
		writer.WriteLine(VerySimpleXml.StartNode("normalmap", 3) + normalmap.ToString() + VerySimpleXml.EndNode("normalmap"));
		writer.WriteLine(VerySimpleXml.StartNode("normalmapFilter", 3) + normalmapFilter.ToString() + VerySimpleXml.EndNode("normalmapFilter"));
		writer.WriteLine(VerySimpleXml.StartNode("npotScale", 3) + npotScale.ToString() + VerySimpleXml.EndNode("npotScale"));
		writer.WriteLine(VerySimpleXml.StartNode("textureFormat", 3) + textureFormat.ToString() + VerySimpleXml.EndNode("textureFormat"));
		writer.WriteLine(VerySimpleXml.StartNode("textureType", 3) + textureType.ToString() + VerySimpleXml.EndNode("textureType"));
		writer.WriteLine(VerySimpleXml.StartNode("wrapMode", 3) + wrapMode.ToString() + VerySimpleXml.EndNode("wrapMode"));

		//End
		writer.WriteLine(VerySimpleXml.EndNode(nodeName, 2));
	}
	
	public override bool Equals(Asset asset) {
		if(asset is TextureAsset) {
			TextureAsset otherAsset = asset as TextureAsset;
			return this.name == otherAsset.name &&
					this.guid == otherAsset.guid &&
					this.anisoLevel == otherAsset.anisoLevel &&
					this.borderMipmap == otherAsset.borderMipmap &&
					this.convertToNormalmap == otherAsset.convertToNormalmap &&
					this.correctGamma == otherAsset.correctGamma &&
					this.fadeout == otherAsset.fadeout &&
					this.filterMode == otherAsset.filterMode &&
					this.grayscaleToAlpha == otherAsset.grayscaleToAlpha &&
					this.heightmapScale == otherAsset.heightmapScale &&
					this.isReadable == otherAsset.isReadable &&
					this.lightmap == otherAsset.lightmap &&
					this.maxTextureSize == otherAsset.maxTextureSize &&
					this.mipMapBias == otherAsset.mipMapBias &&
					this.mipmapEnabled == otherAsset.mipmapEnabled &&
					this.mipmapFadeDistanceEnd == otherAsset.mipmapFadeDistanceEnd &&
					this.mipmapFadeDistanceStart == otherAsset.mipmapFadeDistanceStart &&
					this.mipmapFilter == otherAsset.mipmapFilter &&
					this.normalmap == otherAsset.normalmap &&
					this.normalmapFilter == otherAsset.normalmapFilter &&
					this.npotScale == otherAsset.npotScale &&
					this.textureFormat == otherAsset.textureFormat &&
					this.textureType == otherAsset.textureType &&
					this.wrapMode == otherAsset.wrapMode &&
					this.generateCubemap == otherAsset.generateCubemap;
		} else {
			return false;
		}
	}
	
	public override string[] GetAllPropertyNames() {
		return new string[] {"anisoLevel", "borderMipmap", "convertToNormalmap", "correctGamma", "fadeout", "filterMode",
							"generateCubemap", "grayscaleToAlpha", "heightmapScale", "isReadable", "lightmap", "maxTextureSize", "mipMapBias",
							"mipmapEnabled", "mipmapFadeDistanceEnd", "mipmapFadeDistanceStart", "mipmapFilter", "normalmap", "normalmapFilter",
							"npotScale", "textureFormat", "textureType", "wrapMode"};
	}
	
	public override object[] GetAllPropertyValues() {
		return new object[] {anisoLevel, borderMipmap, convertToNormalmap, correctGamma, fadeout, filterMode,
							generateCubemap, grayscaleToAlpha, heightmapScale, isReadable, lightmap, maxTextureSize, mipMapBias,
							mipmapEnabled, mipmapFadeDistanceEnd, mipmapFadeDistanceStart, mipmapFilter, normalmap, normalmapFilter,
							npotScale, textureFormat, textureType, wrapMode};
	}
	
	public override void ApplyCopiedValues(string[] properties, object[] values) {
		for(int i=0; i<properties.Length; i++) {
			string property = properties[i];
			
			if(property == "anisoLevel") { anisoLevel = (int) values[i]; }
			if(property == "borderMipmap") { borderMipmap = (bool) values[i]; }
			if(property == "convertToNormalmap") { convertToNormalmap = (bool) values[i]; }
			if(property == "correctGamma") { correctGamma = (bool) values[i]; }
			if(property == "fadeout") { fadeout = (bool) values[i]; }
			if(property == "filterMode") { filterMode = (FilterMode) values[i]; };
			if(property == "generateCubemap") { generateCubemap = (TextureImporterGenerateCubemap) values[i]; }
			if(property == "grayscaleToAlpha") { grayscaleToAlpha = (bool) values[i]; }
			if(property == "heightmapScale") { heightmapScale = (float) values[i]; }
			if(property == "isReadable") { isReadable = (bool) values[i]; }
			if(property == "lightmap") { lightmap = (bool) values[i]; }
			if(property == "maxTextureSize") { maxTextureSize = (int) values[i]; }
			if(property == "mipMapBias") { mipMapBias = (float) values[i]; }
			if(property == "mipmapEnabled") { mipmapEnabled = (bool) values[i]; }
			if(property == "mipmapFadeDistanceEnd") { mipmapFadeDistanceEnd = (int) values[i]; }
			if(property == "mipmapFadeDistanceStart") { mipmapFadeDistanceStart = (int) values[i]; }
			if(property == "mipmapFilter") { mipmapFilter = (TextureImporterMipFilter) values[i]; }
			if(property == "normalmap") { normalmap = (bool) values[i]; }
			if(property == "normalmapFilter") { normalmapFilter = (TextureImporterNormalFilter) values[i]; }
			if(property == "npotScale") { npotScale = (TextureImporterNPOTScale) values[i]; }
			if(property == "textureFormat") { textureFormat = (TextureImporterFormat) values[i]; }
			if(property == "textureType") { textureType = (TextureImporterType) values[i]; }
			if(property == "wrapMode") { wrapMode = (TextureWrapMode) values[i]; }
		}
	}
}
