namespace VRTK.Examples
{
    using UnityEngine;

    public class SetActiveScript : MonoBehaviour
    {
        public VRTK_InteractableObject linkedObject;
        public GameObject text;

        protected static bool activate;

        protected virtual void OnEnable()
        {
            Debug.Log("I'm enabled");
            linkedObject = (linkedObject == null ? GetComponent<VRTK_InteractableObject>() : linkedObject);
            activate = false;
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
                linkedObject.InteractableObjectUsed -= InteractableObjectUsed;
                linkedObject.InteractableObjectUnused -= InteractableObjectUnused;
            }
        }

        protected virtual void FixedUpdate()
        {
            if (activate == true)
                text.SetActive(true);
        }

        protected virtual void InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
        {
            Debug.Log("ISUSED");
            activate = true;
        }

        protected virtual void InteractableObjectUnused(object sender, InteractableObjectEventArgs e)
        {
            Debug.Log("UNUSED");
            activate = false;
        }
    }
}