using System.IO;
using System.Collections.Generic;

public class AssetConfiguration {
	
	public const string nodeName = "Configuration";
	const string nameNodeName = "Name";
	
	public string name;
	public List<Asset> assets;
	
	public AssetConfiguration(string name) {
		this.name = name;
		assets = new List<Asset>();
	}
	
	public AssetConfiguration(StreamReader reader) {
		assets = new List<Asset>();
		
		string line = string.Empty;
		while((line = reader.ReadLine()) != null) {
			if(line.Contains(VerySimpleXml.EndNode(nodeName)))
				break;
			
			//Name
			if(line.Contains(VerySimpleXml.StartNode(nameNodeName)))
				name = VerySimpleXml.NodeValue(line, nameNodeName);
			
			//////////
			//ASSETS//
			
			//Audio asset
			if(line.Contains(VerySimpleXml.StartNode(AudioAsset.nodeName)))
				assets.Add(new AudioAsset(reader));
			
			//Texture asset
			if(line.Contains(VerySimpleXml.StartNode(TextureAsset.nodeName)))
				assets.Add(new TextureAsset(reader));
		}
	}
	
	public void WriteToWriter(StreamWriter writer) {
		writer.WriteLine(VerySimpleXml.StartNode(nodeName, 1));
			//Name
			writer.WriteLine(VerySimpleXml.StartNode(nameNodeName, 2) + name + VerySimpleXml.EndNode(nameNodeName));
			
			//Assets
			for(int i=0; i<assets.Count; i++) {
				assets[i].WriteToWriter(writer);
			}
		writer.WriteLine(VerySimpleXml.EndNode(nodeName, 1));
	}
}