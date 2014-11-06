using UnityEngine;
using System.Collections;

public class VerySimpleXml {

	public static string Indent(int num) {
		string t = "\t";
		for(int i=0; i<num-1; i++)
			t+=t;
		return t;
	}
	
	public static string StartNode(string nodeName) {
		return '<' + nodeName + '>';
	}
	
	public static string StartNode(string nodeName, int indent) {
		return Indent(indent) + StartNode(nodeName);
	}
	
	public static string EndNode(string nodeName) {
		return "</" + nodeName + '>';
	}
	
	public static string EndNode(string nodeName, int indent) {
		return Indent(indent) + EndNode(nodeName);
	}
	
	public static string NodeValue(string line, string nodeName) {
		string nodeStart = StartNode(nodeName);
		string nodeEnd = EndNode(nodeName);
		int startIdx = line.IndexOf(nodeStart) + nodeStart.Length;
		return line.Substring(startIdx, line.IndexOf(nodeEnd) - startIdx);
	}
}
