using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassScoreTrigger : MonoBehaviour {
	public ScoreBoard scoreBoard;
	void OnTriggerEnter(Collider col){	
		switch(col.tag){
			case "GlassTrash":
				{
                    Destroy(col.gameObject);
                    scoreBoard.handleScored();
                    break;
                }
			case "PlasticTrash":
				scoreBoard.handleWrongScored();
				break;
			case "PaperTrash":
				scoreBoard.handleWrongScored();
				break;
			case "MetalTrash":
				scoreBoard.handleWrongScored();
				break;
			case "Waste":
				scoreBoard.handleWrongScored();
				break;
		}	
			
	}
}
