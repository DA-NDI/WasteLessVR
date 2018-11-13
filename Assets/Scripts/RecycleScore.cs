using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecycleScore : MonoBehaviour {
	public ScoreBoard scoreBoard;

	void OnTriggerEnter(Collider col){	
		switch(col.tag){
			case "MetalTrash":
				scoreBoard.handleScored();
				break;
			case "PlasticTrash":
				scoreBoard.handleScored();
				break;
			case "PaperTrash":
				scoreBoard.handleScored();
				break;
			case "GlassTrash":
				scoreBoard.handleScored();
				break;
			case "Waste":
				scoreBoard.handleWrongScored();
				break;
		}	
	
	}
}
