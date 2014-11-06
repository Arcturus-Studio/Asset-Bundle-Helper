using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class Asset {
	
	protected const string nameNodeName = "Name";
	protected const string pathNodeName = "Path";
	protected const string guidNodeName = "GUID";
	
	public string name;
	public string guid;
	
	public AssetImporter Importer {
		get { return AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));}
	}
	
	public abstract void ApplyImportSettings();
	public abstract void ReadFromAsset(Object asset);
	public abstract void DrawImportSettings();
	public abstract void WriteToWriter(StreamWriter writer);
	public abstract bool DoesImporterMatchSettings(AssetImporter importer);
	public abstract bool Equals(Asset asset);
	
	public abstract string[] GetAllPropertyNames();
	public abstract object[] GetAllPropertyValues();
	public abstract void ApplyCopiedValues(string[] properties, object[] values);
}