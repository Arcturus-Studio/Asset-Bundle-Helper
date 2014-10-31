using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
/*	Utility functions for BundleTag and BundleTagGroup */
public static class BundleTagUtils {
	//Constructs all combinations of tags present in given list, starting from the given index in the list
	public static IEnumerable<List<BundleTag>> TagCombinations(List<BundleTagGroup> list, int index){
		//End condition 1: reached list end, yield list for each tag
		if(index == list.Count-1){
			foreach(BundleTag tag in list[index].tags){
				var newList = new List<BundleTag>(1);
				newList.Add(tag);
				yield return newList;
			}
		}
		//End condition 2: Past end of list (most likely due to list being empty): Return list containing "No Tag"
		else if (index >= list.Count){
			var newList = new List<BundleTag>();
			newList.Add(BundleTag.NoTag);
			yield return newList;
		}
		//Recurse: Yield each tag followed by all combinations of later tags
		else{
			foreach(List<BundleTag> suffix in TagCombinations(list, index+1)){
				foreach(BundleTag tag in list[index].tags){
					var newList = new List<BundleTag>(suffix.Count + 1);
					newList.Add(tag);
					newList.AddRange(suffix);
					yield return newList;
				}
			}
		}
	}
	
	//Returns the combination of the first tag in each tag group in the list, starting from the given index in the list
	public static List<BundleTag> DefaultTagCombination(List<BundleTagGroup> list, int index){
		return list.Select(x => x.tags[0]).Skip(index).ToList();
	}
	
	//Builds a period-delimited tag string from an enumerable of BundleTags
	public static string BuildTagString(IEnumerable<BundleTag> tags){		
		if(tags != null){
			StringBuilder result = new StringBuilder();
			foreach(BundleTag tag in tags){
				if(tag == BundleTag.NoTag){
					continue;
				}
				if(result.Length > 0){
					result.Append(".");
				}
				result.Append(tag.name);
			}
			return result.ToString();
		}
		else{
			return "";
		}
	}
	
	//Builds a period-delimited tag string from an enumerable of strings
	public static string BuildTagString(IEnumerable<string> tags){		
		if(tags != null){
			StringBuilder result = new StringBuilder();
			foreach(string tag in tags){
				if(string.IsNullOrEmpty(tag)){
					continue;
				}
				if(result.Length > 0){
					result.Append(".");
				}
				result.Append(tag);
			}
			return result.ToString();
		}
		else{
			return "";
		}
	}
}
