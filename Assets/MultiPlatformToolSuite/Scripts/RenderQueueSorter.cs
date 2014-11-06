/*
 * Render Queue Sorter by Owlchemy Labs
   * This tool allows you to set the render queue of multiple materials to setup custom layering of your transparent objects.
   * 
   * Quickstart:
   	 * 1) Attach this script to a game object to set up the queue groups and assign materials to override their render queue.
   	 * 2) Remember that the default render queue for Transparent objects is 3000. If you want something to draw on top, choose a higher queue number.
   * 
   * For support, please e-mail info@owlchemylabs.com.
   * Owlchemy Labs
 */

using UnityEngine;

public class RenderQueueSorter : MonoBehaviour {
	
	[System.Serializable]
	public class QueueGroup {
		public int queue;
		public Material[] materials;
	}
	
	public QueueGroup[] queueGroups;
	
	static bool queuesSet = false;

	void Awake() {
		if(queuesSet) {
			Destroy(this);
			return;
		}
		
		for(int i=0; i<queueGroups.Length; i++) {
			QueueGroup group = queueGroups[i];
			for(int m=0; m<group.materials.Length; m++) {
				if(group.materials[m] != null)
					group.materials[m].renderQueue = group.queue;
			}
		}
		
		Destroy(this);
	}
}
