using UnityEditor;
using UnityEngine;
using System.Xml;

public class TextureAssetSettings {
	public string path = string.Empty;
	public int maxSize = 1024;
	public TextureImporterFormat format = TextureImporterFormat.AutomaticTruecolor;
	public TextureImporterNPOTScale npotScale = TextureImporterNPOTScale.None;
	public bool mipmaps = false;
	
	public TextureAssetSettings (XmlTextReader reader) {
		if(reader.MoveToAttribute("path"))
			path = reader.Value;
		while(reader.Read()) {
			if(reader.NodeType == XmlNodeType.EndElement && reader.Name == "TextureAsset") {
				reader.ReadEndElement();
				break;
			}
			if(reader.NodeType == XmlNodeType.Element && reader.Name == "maxSize") {
				maxSize = int.Parse(reader.ReadInnerXml());
			} else if(reader.NodeType == XmlNodeType.Element && reader.Name == "format") {
				format = (TextureImporterFormat) System.Enum.Parse(typeof(TextureImporterFormat), reader.ReadInnerXml());
			} else if(reader.NodeType == XmlNodeType.Element && reader.Name == "mipmaps") {
				mipmaps = bool.Parse(reader.ReadInnerXml());
			} else if(reader.NodeType == XmlNodeType.Element && reader.Name == "npotScale") {
				npotScale = (TextureImporterNPOTScale) System.Enum.Parse(typeof(TextureImporterNPOTScale), reader.ReadInnerXml());
			}
		}
	}
	
	public void ApplySettings() {
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
		if(importer == null) {
			Debug.Log("Importer for asset at path: " + path + " is null.");
			return;
		}
		
		bool differentSettings = false;
		
		TextureImporterSettings settings = new TextureImporterSettings();
		importer.ReadTextureSettings(settings);
		
		if(settings.mipmapEnabled != mipmaps) {
			settings.mipmapEnabled = mipmaps;
			differentSettings = true;
		}
		
		if(settings.maxTextureSize != maxSize) {
			settings.maxTextureSize = maxSize;
			differentSettings = true;
		}
		
		if(settings.textureFormat != format) {
			settings.textureFormat = format;
			differentSettings = true;
		}
		
		if(settings.npotScale != npotScale) {
			settings.npotScale = npotScale;
			differentSettings = true;
		}
		
		if(differentSettings) {
			importer.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate); //Re-import the asset
		}
	}
}
