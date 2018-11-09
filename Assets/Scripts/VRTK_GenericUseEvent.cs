using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRTK;

public class VRTK_GenericUseEvent : VRTK_InteractableObject {
	public UnityEvent whatToDo;
	public override void OnInteractableObjectUsed (InteractableObjectEventArgs e){
		base.OnInteractableObjectUsed(e);
		whatToDo.Invoke();
	}
}
