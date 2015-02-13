using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BundleTagSelection {

	int mask;
	public int Mask{
		get{
			return mask;
		}
	}
	
	private List<BundleTag> selection;
	
	private BundleTagSelection(){
		mask = 0;
		selection = new List<BundleTag>();
	}
	
	public BundleTagSelection(int mask, List<BundleTag> selection){
		this.mask = mask;
		this.selection = new List<BundleTag>(selection);
	}
	
	public override string ToString(){
		return BundleTagUtils.BuildTagString(selection);
	}
	
	//Returns a copy of the selection further masked by the given mask
	public BundleTagSelection GetMasked(int newMask){
		BundleTagSelection result = new BundleTagSelection();
		result.mask = mask & newMask;
		int listIndex = 0;
		//Iterate over bitmask
		for(int i = 0; i < 32; i++){
			//If this tag included in this selection
			if(this.MaskContains(i)){
				//If this tag included in result selection
				if(result.MaskContains(i)){
					//Add this tag to result
					result.selection.Add(selection[listIndex]);
				}
				//Advance along list of tags in this selection
				listIndex++;
			}
		}
		return result;
	}
	
	//Returns a copy of the selection unioned with another selection.
	//If both selections have tags defined for a given tag group,
	//The calling selection's value takes priority over the parameter selection's value.
	public BundleTagSelection GetUnion(BundleTagSelection other){
		BundleTagSelection result = new BundleTagSelection();
		result.mask = this.mask | other.mask;		
		int thisListIndex = 0, otherListIndex = 0;
		//Iterate over bitmask
		for(int i = 0; i < 32; i++){
			if(this.MaskContains(i) || other.MaskContains(i)){
				if(this.MaskContains(i)){
					result.selection.Add(this.selection[thisListIndex]);
					thisListIndex++;
					if(other.MaskContains(i)){
						otherListIndex++;
					}
				}
				else{
					result.selection.Add(other.selection[otherListIndex]);
					otherListIndex++;
				}
			}
		}
		return result;
	}
	
	private bool MaskContains(int bit){
		return (mask & (1 << bit)) != 0;
	}
}
