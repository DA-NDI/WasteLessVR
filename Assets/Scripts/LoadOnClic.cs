using System.Collections;
using UnityEngine;

public class LoadOnClic : MonoBehaviour
{
    public void LoadScene(int level)
    {
        Application.LoadLevel(level);
    }

}