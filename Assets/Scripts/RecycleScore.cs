using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecycleScore : MonoBehaviour {
	public ScoreBoard scoreBoard;
	void OnTriggerEnter(Collider col){
		if (col.tag == "MetalTrash" || col.tag == "PlasticTrash" 
		|| col.tag == "PaperTrash" || col.tag == "GlassTrash")
		{
			scoreBoard.handleScored();
			Destroy(col.gameObject);
		}
		else
			scoreBoard.handleWrongScored();
		Trash trash = col.gameObject.GetComponent<Trash>();
		trash.setSpeedToZero();
	}
}
