namespace VRTK.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SetActiveScript : MonoBehaviour
    {
        public GameObject cube;
        protected bool activate;
        public VRTK_InteractableObject linkedObject;


        protected virtual void OnEnable()
        {
            activate = false;
            linkedObject = (linkedObject == null ? GetComponent<VRTK_InteractableObject>() : linkedObject);

            if (linkedObject != null)
            {
                linkedObject.InteractableObjectUsed += InteractableObjectUsed;
                linkedObject.InteractableObjectUnused += InteractableObjectUnused;
            }

        }

        protected virtual void OnDisable()
        {
            if (linkedObject != null)
            {
                Debug.Log("i'm enabled");
                linkedObject.InteractableObjectUsed -= InteractableObjectUsed;
                linkedObject.InteractableObjectUnused -= InteractableObjectUnused;
            }
        }

        protected virtual void Update()
        {
            if (activate)
            {
                cube.SetActive(true);
            } else if (!activate)
            {
                cube.SetActive(false);
            }
        }

        protected virtual void InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
        {
            activate = true;
        }

        protected virtual void InteractableObjectUnused(object sender, InteractableObjectEventArgs e)
        {
            activate = false;
        }
    }
}




