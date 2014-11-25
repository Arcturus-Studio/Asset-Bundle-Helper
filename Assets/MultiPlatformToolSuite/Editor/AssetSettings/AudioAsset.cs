using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class AudioAsset : Asset {
	
	public const string nodeName = "AudioAsset";
	
	public bool forceToMono;
	public AudioClipFormat format;
	public AudioClipLoadType loadType;
	public bool optimizeSampleRate;
	public bool overrideSampleRate;
	public bool preloadAudioData;
	public float quality; //0..1
	public float sampleRate;
	
	public AudioAsset() {}
	
	public AudioAsset(StreamReader reader) {
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
			if(line.Contains(VerySimpleXml.StartNode("forceToMono"))){
				forceToMono = bool.Parse(VerySimpleXml.NodeValue(line, "forceToMono"));
			}
			
			TryReadField(line, "forceToMono", bool.Parse, ref forceToMono);
			
			if(line.Contains(VerySimpleXml.StartNode("format"))){
				format = (AudioClipFormat) System.Enum.Parse(typeof(AudioClipFormat), VerySimpleXml.NodeValue(line, "format"));
			}
			
			if(line.Contains(VerySimpleXml.StartNode("loadType"))){
				loadType = (AudioClipLoadType) System.Enum.Parse(typeof(AudioClipLoadType), VerySimpleXml.NodeValue(line, "loadType"));
			}
			
			if(line.Contains(VerySimpleXml.StartNode("optimizeSampleRate"))){
				optimizeSampleRate = bool.Parse(VerySimpleXml.NodeValue(line, "optimizeSampleRate"));
			}
			
			if(line.Contains(VerySimpleXml.StartNode("overrideSampleRate"))){
				overrideSampleRate = bool.Parse(VerySimpleXml.NodeValue(line, "overrideSampleRate"));
			}
			
			if(line.Contains(VerySimpleXml.StartNode("preloadAudioData"))){
				preloadAudioData = bool.Parse(VerySimpleXml.NodeValue(line, "preloadAudioData"));
			}
			
			if(line.Contains(VerySimpleXml.StartNode("quality"))){
				quality = float.Parse(VerySimpleXml.NodeValue(line, "quality"));
			}
			
			if(line.Contains(VerySimpleXml.StartNode("sampleRate"))){
				sampleRate = float.Parse(VerySimpleXml.NodeValue(line, "sampleRate"));
			}
				
			//Legacy field warnings
			if(line.Contains(VerySimpleXml.StartNode("compressionBitrate"))){
				LogLegacyXMLWarning("compressionBitrate", line);
			}
			
			if(line.Contains(VerySimpleXml.StartNode("hardware"))){
				LogLegacyXMLWarning("hardware", line);
			}
			
			if(line.Contains(VerySimpleXml.StartNode("threeD"))){
				LogLegacyXMLWarning("threeD", line);
			}
				
			if(line.Contains(VerySimpleXml.StartNode("loopable"))){
				LogLegacyXMLWarning("loopable", line);
			}
		}
	}
	
	private void TryReadField<T>(string line, string fieldName, System.Func<string, T> parseFunc, ref T field){
		if(line.Contains(VerySimpleXml.StartNode(fieldName))){
			field = parseFunc(VerySimpleXml.NodeValue(line, fieldName));
		}
	}

	public override void ApplyImportSettings() {
		string path = AssetDatabase.GUIDToAssetPath(guid);
		AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
		
		if(audioImporter == null) {
			Debug.Log("Importer for audio clip could not be found at path: " + path);
			return;
		}
		
		//Do an import only if the importer's settings don't match our settings
		if(!DoesImporterMatchSettings(audioImporter)) {
			audioImporter.forceToMono = forceToMono;
			audioImporter.format = format;
		    audioImporter.loadType = loadType;
			audioImporter.optimizeSampleRate = optimizeSampleRate;
			audioImporter.overrideSampleRate = overrideSampleRate;
			audioImporter.quality = quality;
			audioImporter.sampleRate = sampleRate;
			audioImporter.preloadAudioData = preloadAudioData;
			
			AssetDatabase.ImportAsset(path);
		}
	}
	
	public override bool DoesImporterMatchSettings(AssetImporter importer) {
		AudioImporter audioImporter = importer as AudioImporter;
		
		return 	audioImporter.forceToMono == forceToMono &&
				audioImporter.format == format &&			   
			    audioImporter.loadType == loadType &&
			    audioImporter.optimizeSampleRate == optimizeSampleRate &&
				audioImporter.overrideSampleRate == overrideSampleRate &&
				audioImporter.quality == quality &&
				audioImporter.sampleRate == sampleRate &&
				audioImporter.preloadAudioData == preloadAudioData;
	}
	
	public override void ReadFromAsset(Object asset) {
		if(!(asset is AudioClip)) {
			Debug.Log("Asset isn't an AudioClip. Can't read from asset");
			return;
		}
		
		AudioClip clip = asset as AudioClip;
		
		name = clip.name;
		
		AudioImporter audioImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip)) as AudioImporter;
		if(audioImporter == null) {
			Debug.Log("Could not get audio importer for asset at path: " + AssetDatabase.GetAssetPath(clip));
			return;
		}
		
		guid = AssetDatabase.AssetPathToGUID(audioImporter.assetPath);
		
		forceToMono = audioImporter.forceToMono;
		format = audioImporter.format;
	    loadType = audioImporter.loadType;
		optimizeSampleRate = audioImporter.optimizeSampleRate;
		overrideSampleRate = audioImporter.overrideSampleRate;
		quality = audioImporter.quality;
		sampleRate = audioImporter.sampleRate;
		preloadAudioData = audioImporter.preloadAudioData;
	}
	
	public override void DrawImportSettings() {
		string path = AssetDatabase.GUIDToAssetPath(guid);
		AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
		
		//If importer is still null, asset can't be found at path
		if(audioImporter == null) {
			GUILayout.Label("Audio Asset with GUID: " + guid + " no longer exists.");
		} else {
			//Name
			GUILayout.Label("Name: " + name);
			//Path
			GUILayout.Label("Path: " + path);
			
			DrawDiffHighlightLabel("Quality", quality, audioImporter.quality);
			DrawDiffHighlightLabel("Force to Mono", forceToMono, audioImporter.forceToMono);
			DrawDiffHighlightLabel("Format", format, audioImporter.format);
			DrawDiffHighlightLabel("Load Type", loadType, audioImporter.loadType);
			DrawDiffHighlightLabel("Optimize Sample Rate", optimizeSampleRate, audioImporter.optimizeSampleRate);
			DrawDiffHighlightLabel("Override Sample Rate", overrideSampleRate, audioImporter.overrideSampleRate);
			DrawDiffHighlightLabel("Sample Rate", sampleRate, audioImporter.sampleRate);
			DrawDiffHighlightLabel("Preload Data", preloadAudioData, audioImporter.preloadAudioData);
		}
	}
	
	//Draws a label in a highlighted mode if the value differs from a reference value
	private void DrawDiffHighlightLabel<T>(string fieldName, T value, T referenceValue) {
		GUILayout.Label(fieldName + ": " + value.ToString(), !Equals(referenceValue, value) ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
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
		writer.WriteLine(VerySimpleXml.StartNode("forceToMono", 3) + forceToMono.ToString() + VerySimpleXml.EndNode("forceToMono"));
		writer.WriteLine(VerySimpleXml.StartNode("format", 3) + format.ToString() + VerySimpleXml.EndNode("format"));
		writer.WriteLine(VerySimpleXml.StartNode("loadType", 3) + loadType.ToString() + VerySimpleXml.EndNode("loadType"));
		writer.WriteLine(VerySimpleXml.StartNode("quality", 3) + quality.ToString() + VerySimpleXml.EndNode("quality"));
		writer.WriteLine(VerySimpleXml.StartNode("optimizeSampleRate", 3) + optimizeSampleRate.ToString() + VerySimpleXml.EndNode("optimizeSampleRate"));
		writer.WriteLine(VerySimpleXml.StartNode("overrideSampleRate", 3) + overrideSampleRate.ToString() + VerySimpleXml.EndNode("overrideSampleRate"));
		writer.WriteLine(VerySimpleXml.StartNode("sampleRate", 3) + sampleRate.ToString() + VerySimpleXml.EndNode("sampleRate"));
		writer.WriteLine(VerySimpleXml.StartNode("preloadAudioData", 3) + preloadAudioData.ToString() + VerySimpleXml.EndNode("preloadAudioData"));
				
		//End
		writer.WriteLine(VerySimpleXml.EndNode(nodeName, 2));
	}
	
	public override bool Equals(Asset asset) {
		if(asset is AudioAsset) {
			AudioAsset otherAsset = asset as AudioAsset;
			return this.name == otherAsset.name &&
					this.guid == otherAsset.guid &&
					this.forceToMono == otherAsset.forceToMono &&
					this.format == otherAsset.format &&
					this.loadType == otherAsset.loadType &&
					this.quality == otherAsset.quality &&
					this.optimizeSampleRate == otherAsset.optimizeSampleRate &&
					this.overrideSampleRate == otherAsset.overrideSampleRate &&
					this.sampleRate == otherAsset.sampleRate &&
					this.preloadAudioData == otherAsset.preloadAudioData;
		} else {
			return false;
		}
	}
	
	public override string[] GetAllPropertyNames() {
		return new string[] {"forceToMono", "format", "loadType", "quality", "optimizeSampleRate", "overrideSampleRate", "sampleRate", "preloadAudioData"};
	}
	
	public override object[] GetAllPropertyValues() {
		return new object[] {forceToMono, format, loadType, quality, optimizeSampleRate, overrideSampleRate, sampleRate, preloadAudioData};
	}
	
	public override void ApplyCopiedValues(string[] properties, object[] values) {
		for(int i=0; i<properties.Length; i++) {
			string property = properties[i];
			
			if(property == "forceToMono") { forceToMono = (bool) values[i]; }
			if(property == "format") { format = (AudioClipFormat) values[i]; }
			if(property == "loadType") { loadType = (AudioClipLoadType) values[i]; }
			if(property == "quality") { quality = (float) values[i]; }
			if(property == "optimizeSampleRate") { optimizeSampleRate = (bool) values[i]; }
			if(property == "overrideSampleRate") { overrideSampleRate = (bool) values[i]; }
			if(property == "sampleRate") { sampleRate = (float) values[i]; }
			if(property == "preloadAudioData") { preloadAudioData = (bool) values[i]; }
		}
	}
	
	private void LogLegacyXMLWarning(string fieldName, string line){
		Debug.LogWarning("Legacy field " + fieldName + " in XML for " + name + " import settings. Value was '" + VerySimpleXml.NodeValue(line, fieldName) + "'");
	}
}
