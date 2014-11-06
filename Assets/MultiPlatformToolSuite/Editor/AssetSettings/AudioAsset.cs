using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class AudioAsset : Asset {
	
	public const string nodeName = "AudioAsset";
	
	public int compressionBitrate;
	public bool forceToMono;
	public AudioImporterFormat format;
	public bool hardware;
	public AudioImporterLoadType loadType;
	public bool loopable;
	public bool threeD;
	
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
			if(line.Contains(VerySimpleXml.StartNode("compressionBitrate")))
				compressionBitrate = int.Parse(VerySimpleXml.NodeValue(line, "compressionBitrate"));
			
			if(line.Contains(VerySimpleXml.StartNode("forceToMono")))
				forceToMono = bool.Parse(VerySimpleXml.NodeValue(line, "forceToMono"));
			
			if(line.Contains(VerySimpleXml.StartNode("format")))
				format = (AudioImporterFormat) System.Enum.Parse(typeof(AudioImporterFormat), VerySimpleXml.NodeValue(line, "format"));
			
			if(line.Contains(VerySimpleXml.StartNode("hardware")))
				hardware = bool.Parse(VerySimpleXml.NodeValue(line, "hardware"));
			
			if(line.Contains(VerySimpleXml.StartNode("loadType")))
				loadType = (AudioImporterLoadType) System.Enum.Parse(typeof(AudioImporterLoadType), VerySimpleXml.NodeValue(line, "loadType"));
			
			if(line.Contains(VerySimpleXml.StartNode("loopable")))
				loopable = bool.Parse(VerySimpleXml.NodeValue(line, "loopable"));
			
			if(line.Contains(VerySimpleXml.StartNode("threeD")))
				threeD = bool.Parse(VerySimpleXml.NodeValue(line, "threeD"));
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
			audioImporter.compressionBitrate = compressionBitrate;
			audioImporter.forceToMono = forceToMono;
			audioImporter.format = format;
		    audioImporter.hardware = hardware;
		    audioImporter.loadType = loadType;
		    audioImporter.loopable = loopable;
		    audioImporter.threeD = threeD;
			
			AssetDatabase.ImportAsset(path);
		}
	}
	
	public override bool DoesImporterMatchSettings(AssetImporter importer) {
		AudioImporter audioImporter = importer as AudioImporter;
		
		return audioImporter.compressionBitrate == compressionBitrate &&
				audioImporter.forceToMono == forceToMono &&
				audioImporter.format == format &&
			    audioImporter.hardware == hardware &&
			    audioImporter.loadType == loadType &&
			    audioImporter.loopable == loopable &&
			    audioImporter.threeD == threeD;
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
		
		compressionBitrate = audioImporter.compressionBitrate;
		forceToMono = audioImporter.forceToMono;
		format = audioImporter.format;
	    hardware = audioImporter.hardware;
	    loadType = audioImporter.loadType;
	    loopable = audioImporter.loopable;
	    threeD = audioImporter.threeD;
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
			
			//Compression bitrate
			GUILayout.Label("Compression Bitrate: " + compressionBitrate.ToString(), audioImporter.compressionBitrate != compressionBitrate ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Force to mono
			GUILayout.Label("Force to mono: " + forceToMono.ToString(), audioImporter.forceToMono != forceToMono ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Format
			GUILayout.Label("Format: " + format.ToString(), audioImporter.format != format ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Hardware
			GUILayout.Label("Hardware: " + hardware.ToString(), audioImporter.hardware != hardware ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Load type
			GUILayout.Label("Load type: " + loadType.ToString(), audioImporter.loadType != loadType ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//Loopable
			GUILayout.Label("Loopable: " + loopable.ToString(), audioImporter.loopable != loopable ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
			//3D
			GUILayout.Label("3D: " + threeD.ToString(), audioImporter.threeD != threeD ? AssetSettingsWindow.singleton.redTextStyle : EditorStyles.label);
		}
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
		writer.WriteLine(VerySimpleXml.StartNode("compressionBitrate", 3) + compressionBitrate.ToString() + VerySimpleXml.EndNode("compressionBitrate"));
		writer.WriteLine(VerySimpleXml.StartNode("forceToMono", 3) + forceToMono.ToString() + VerySimpleXml.EndNode("forceToMono"));
		writer.WriteLine(VerySimpleXml.StartNode("format", 3) + format.ToString() + VerySimpleXml.EndNode("format"));
		writer.WriteLine(VerySimpleXml.StartNode("hardware", 3) + hardware.ToString() + VerySimpleXml.EndNode("hardware"));
		writer.WriteLine(VerySimpleXml.StartNode("loadType", 3) + loadType.ToString() + VerySimpleXml.EndNode("loadType"));
		writer.WriteLine(VerySimpleXml.StartNode("loopable", 3) + loopable.ToString() + VerySimpleXml.EndNode("loopable"));
		writer.WriteLine(VerySimpleXml.StartNode("threeD", 3) + threeD.ToString() + VerySimpleXml.EndNode("threeD"));
		
		//End
		writer.WriteLine(VerySimpleXml.EndNode(nodeName, 2));
	}
	
	public override bool Equals(Asset asset) {
		if(asset is AudioAsset) {
			AudioAsset otherAsset = asset as AudioAsset;
			return this.name == otherAsset.name &&
					this.guid == otherAsset.guid &&
					this.compressionBitrate == otherAsset.compressionBitrate &&
					this.forceToMono == otherAsset.forceToMono &&
					this.format == otherAsset.format &&
					this.hardware == otherAsset.hardware &&
					this.loadType == otherAsset.loadType &&
					this.loopable == otherAsset.loopable &&
					this.threeD == otherAsset.threeD;
		} else {
			return false;
		}
	}
	
	public override string[] GetAllPropertyNames() {
		return new string[] {"compressionBitrate", "forceToMono", "format", "hardware", "loadType", "loopable", "threeD"};
	}
	
	public override object[] GetAllPropertyValues() {
		return new object[] {compressionBitrate, forceToMono, format, hardware, loadType, loopable, threeD};
	}
	
	public override void ApplyCopiedValues(string[] properties, object[] values) {
		for(int i=0; i<properties.Length; i++) {
			string property = properties[i];
			
			if(property == "compressionBitrate") { compressionBitrate = (int) values[i]; }
			if(property == "forceToMono") { forceToMono = (bool) values[i]; }
			if(property == "format") { format = (AudioImporterFormat) values[i]; }
			if(property == "hardware") { hardware = (bool) values[i]; }
			if(property == "loadType") { loadType = (AudioImporterLoadType) values[i]; }
			if(property == "loopable") { loopable = (bool) values[i]; };
			if(property == "threeD") { threeD = (bool) values[i]; }
		}
	}
}
