using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class PlatformBuildSettings {

	public Platform platform;
	public List<TextureAssetSettings> textureSettings;
	
	public PlatformBuildSettings (XmlTextReader reader) {
		textureSettings = new List<TextureAssetSettings>(20);
		
		if(reader.MoveToAttribute("name"))
			platform = (Platform) System.Enum.Parse(typeof(Platform), reader.Value);
		while(reader.Read()) {
			if(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Platform") {
				reader.ReadEndElement();
				break;
			}
			if(reader.NodeType == XmlNodeType.Element && reader.Name == "TextureAsset") {
				textureSettings.Add(new TextureAssetSettings(reader));
			}
		}
	}
	
	public void ApplySettings() {
		foreach(TextureAssetSettings settings in textureSettings) {
			settings.ApplySettings();
		}
		
	}	
}
