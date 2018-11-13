
using System.Collections;
using UnityEngine;

public class LoadOnClick : MonoBehaviour
{
    public void LoadScene(int level)
    {
        Application.LoadLevel(level);
    }

}