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

     //       if (linkedObject != null)
     //       {
     //           linkedObject.InteractableObjectUsed += InteractableObjectUsed;
     //           linkedObject.InteractableObjectUnused += InteractableObjectUnused;
     //       }

        }

    //    protected virtual void OnDisable()
    //    {
    //        if (linkedObject != null)
    //        {
    //            Debug.Log("i'm enabled");
    //            linkedObject.InteractableObjectUsed -= InteractableObjectUsed;
    //            linkedObject.InteractableObjectUnused -= InteractableObjectUnused;
    //        }
    //    }

        protected virtual void Update()
        {
            if (linkedObject.IsGrabbed() || linkedObject.IsUsing() == true || linkedObject.usingState == 1)
                activate = true;
            else
                activate = false;
            cube.SetActive(activate);
            //else
            //      {
            //           cube.SetActive(false);
            //       }
        }
        public virtual void InteractUse()
        {
            Debug.Log("HEY!!!!");
            activate = !activate;
      //      cube.SetActive(activate);
        }
//        protected virtual void InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
//      {
//          Debug.Log("i'm used");
//         activate = true;
//    }

        //     protected virtual void InteractableObjectUnused(object sender, InteractableObjectEventArgs e)
        //    {
        //         activate = false;
        //     }
    }
}




