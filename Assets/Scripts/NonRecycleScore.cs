using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonRecycleScore : MonoBehaviour {
	public ScoreBoard scoreBoard;
	void OnTriggerEnter(Collider col){	
		switch(col.tag){
			case "MetalTrash":
                    scoreBoard.handleWrongScored();
                    break;
			case "PlasticTrash":
				scoreBoard.handleWrongScored();
				break;
			case "PaperTrash":
				scoreBoard.handleWrongScored();
				break;
			case "GlassTrash":
				scoreBoard.handleWrongScored();
				break;
			case "OrganicTrash":
				scoreBoard.handleWrongScored();
				break;
			case "Waste":
			{
				scoreBoard.handleScored();
				Destroy(col.gameObject);
				break;
			}
		}	
			
	}
}
