using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasticScoreTrigger : MonoBehaviour {
	public ScoreBoard scoreBoard;
	public GameObject conveyer;

	void Start(){
		conveyer = GameObject.FindWithTag("ConveyerSpawner");
	}	void OnTriggerEnter(Collider col){	
		switch(col.tag){
			case "PlasticTrash":
				{
                    Destroy(col.gameObject);
                    scoreBoard.handleScored();
					conveyer.GetComponent<Conveyer>().decreaseTrashNumber();
                    break;
                }
			case "PaperTrash":
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
			
	}
}
