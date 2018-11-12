using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperScoreTrigger : MonoBehaviour {
	public ScoreBoard scoreBoard;
	void OnTriggerEnter(Collider col){	
		switch(col.tag){
			case "PaperTrash":
				{
                    Destroy(col.gameObject);
                    scoreBoard.handleScored();
                    break;
                }
			case "PlasticTrash":
				scoreBoard.handleWrongScored();
				break;
			case "MetalTrash":
				scoreBoard.handleWrongScored();
				break;
			case "GlassTrash":
				scoreBoard.handleWrongScored();
				break;
			case "Waste":
				scoreBoard.handleWrongScored();
				break;
		}	
			Trash trash = col.gameObject.GetComponent<Trash>();
			trash.setSpeedToZero();
	}
}
