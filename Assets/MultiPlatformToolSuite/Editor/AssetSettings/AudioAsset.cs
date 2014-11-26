using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class AudioAsset : Asset {
	
	public const string nodeName = "AudioAsset";
	
	public bool forceToMono;
	public bool preloadAudioData;
	public bool loadInBackground;
	public AudioCompressionFormat compressionFormat;
	public AudioClipLoadType loadType;
	public AudioSampleRateSetting sampleRateSetting;	
	public float quality; //0..1
	public uint sampleRateOverride;
	
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
			TryReadField(line, "forceToMono", bool.Parse, ref forceToMono);
			TryReadField(line, "compressionFormat", (x) => {return (AudioCompressionFormat)System.Enum.Parse(typeof(AudioCompressionFormat), x);}, ref compressionFormat);
			TryReadField(line, "loadType", (x) => {return (AudioClipLoadType)System.Enum.Parse(typeof(AudioClipLoadType), x);}, ref loadType);
			TryReadField(line, "sampleRateSetting", (x) => {return (AudioSampleRateSetting)System.Enum.Parse(typeof(AudioSampleRateSetting), x);}, ref sampleRateSetting);
			TryReadField(line, "preloadAudioData", bool.Parse, ref preloadAudioData);
			TryReadField(line, "loadInBackground", bool.Parse, ref loadInBackground);
			TryReadField(line, "quality", float.Parse, ref quality);
			TryReadField(line, "sampleRateOverride", uint.Parse, ref sampleRateOverride);
							
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
			
			if(line.Contains(VerySimpleXml.StartNode("format"))){
				LogLegacyXMLWarning("format", line);
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
			audioImporter.preloadAudioData = preloadAudioData;
			audioImporter.loadInBackground = loadInBackground;
			
			AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;
			sampleSettings.compressionFormat = compressionFormat;
			sampleSettings.loadType = loadType;
			sampleSettings.quality = quality;
			sampleSettings.sampleRateOverride = sampleRateOverride;
			sampleSettings.sampleRateSetting = sampleRateSetting;
			audioImporter.defaultSampleSettings = sampleSettings;			
			
			AssetDatabase.ImportAsset(path);
		}
	}
	
	public override bool DoesImporterMatchSettings(AssetImporter importer) {
		AudioImporter audioImporter = importer as AudioImporter;
		AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;
		
		return 	audioImporter.forceToMono == forceToMono &&
				audioImporter.loadInBackground == loadInBackground &&
				audioImporter.preloadAudioData == preloadAudioData &&
				sampleSettings.compressionFormat == compressionFormat &&
				sampleSettings.loadType == loadType &&
				sampleSettings.quality == quality &&
				sampleSettings.sampleRateOverride == sampleRateOverride &&
				sampleSettings.sampleRateSetting == sampleRateSetting;
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
		preloadAudioData = audioImporter.preloadAudioData;
		loadInBackground = audioImporter.loadInBackground;
		
		AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;
		compressionFormat = sampleSettings.compressionFormat;
		loadType = sampleSettings.loadType;
		quality = sampleSettings.quality;
		sampleRateOverride = sampleSettings.sampleRateOverride;
		sampleRateSetting = sampleSettings.sampleRateSetting;
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
			
			AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;			
			DrawDiffHighlightLabel("Force to Mono", forceToMono, audioImporter.forceToMono);
			DrawDiffHighlightLabel("Preload Data", preloadAudioData, audioImporter.preloadAudioData);
			DrawDiffHighlightLabel("Load in Background", loadInBackground, audioImporter.loadInBackground);
			DrawDiffHighlightLabel("Quality", quality, sampleSettings.quality);
			DrawDiffHighlightLabel("Compression Format", compressionFormat, sampleSettings.compressionFormat);
			DrawDiffHighlightLabel("Load Type", loadType, sampleSettings.loadType);
			DrawDiffHighlightLabel("Sample Rate Setting", sampleRateSetting, sampleSettings.sampleRateSetting);
			DrawDiffHighlightLabel("Sample Rate Override", sampleRateOverride, sampleSettings.sampleRateOverride);			
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
		WriteVerySimpleXmlNodeValue(writer, 3, "forceToMono", forceToMono.ToString());
		WriteVerySimpleXmlNodeValue(writer, 3, "compressionFormat", compressionFormat.ToString());
		WriteVerySimpleXmlNodeValue(writer, 3, "loadType", loadType.ToString());
		WriteVerySimpleXmlNodeValue(writer, 3, "quality", quality.ToString());
		WriteVerySimpleXmlNodeValue(writer, 3, "sampleRateSetting", sampleRateSetting.ToString());
		WriteVerySimpleXmlNodeValue(writer, 3, "sampleRateOverride", sampleRateOverride.ToString());
		WriteVerySimpleXmlNodeValue(writer, 3, "preloadAudioData", preloadAudioData.ToString());
		WriteVerySimpleXmlNodeValue(writer, 3, "loadInBackground", loadInBackground.ToString());
		//End
		writer.WriteLine(VerySimpleXml.EndNode(nodeName, 2));
	}
	
	private void WriteVerySimpleXmlNodeValue(StreamWriter writer, int indentLevel, string node, string value){
		writer.WriteLine(VerySimpleXml.StartNode(node, indentLevel) + value + VerySimpleXml.EndNode(node));
	}
	
	public override bool Equals(Asset asset) {
		if(asset is AudioAsset) {
			AudioAsset otherAsset = asset as AudioAsset;
			return this.name == otherAsset.name &&
					this.guid == otherAsset.guid &&
					this.forceToMono == otherAsset.forceToMono &&
					this.compressionFormat == otherAsset.compressionFormat &&
					this.loadType == otherAsset.loadType &&
					this.quality == otherAsset.quality &&
					this.sampleRateSetting == otherAsset.sampleRateSetting &&
					this.sampleRateOverride == otherAsset.sampleRateOverride &&
					this.preloadAudioData == otherAsset.preloadAudioData &&
					this.loadInBackground == otherAsset.loadInBackground;
		} else {
			return false;
		}
	}
	
	public override string[] GetAllPropertyNames() {
		return new string[] {"forceToMono", "compressionFormat", "loadType", "quality", "sampleRateSetting", "sampleRateOverride", "preloadAudioData", "loadInBackground"};
	}
	
	public override object[] GetAllPropertyValues() {
		return new object[] {forceToMono, compressionFormat, loadType, quality, sampleRateSetting, sampleRateOverride, preloadAudioData, loadInBackground};
	}
	
	public override void ApplyCopiedValues(string[] properties, object[] values) {
		for(int i=0; i<properties.Length; i++) {
			string property = properties[i];
			
			if(property == "forceToMono") { forceToMono = (bool) values[i]; }
			if(property == "compressionFormat") { compressionFormat = (AudioCompressionFormat) values[i]; }
			if(property == "loadType") { loadType = (AudioClipLoadType) values[i]; }
			if(property == "quality") { quality = (float) values[i]; }
			if(property == "sampleRateSetting") { sampleRateSetting = (AudioSampleRateSetting) values[i]; }
			if(property == "sampleRateOverride") { sampleRateOverride = (uint) values[i]; }
			if(property == "preloadAudioData") { preloadAudioData = (bool) values[i]; }
			if(property == "loadInBackground") { loadInBackground = (bool) values[i]; }
		}
	}
	
	private void LogLegacyXMLWarning(string fieldName, string line){
		Debug.LogWarning("Legacy field " + fieldName + " in XML for " + name + " import settings. Value was '" + VerySimpleXml.NodeValue(line, fieldName) + "'");
	}
}
