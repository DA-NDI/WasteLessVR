using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassScoreTrigger : MonoBehaviour {
	public ScoreBoard scoreBoard;
	public GameObject conveyer;

	void Start(){
		conveyer = GameObject.FindWithTag("ConveyerSpawner");
	}
	void OnTriggerEnter(Collider col){	
		switch(col.tag){
			case "GlassTrash":
				{
                    Destroy(col.gameObject);
                    scoreBoard.handleScored();
					conveyer.GetComponent<Conveyer>().decreaseTrashNumber();
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
