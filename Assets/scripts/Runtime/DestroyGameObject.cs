using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (!Application.isEditor) // if the game is not in the editor
        {
            Destroy(gameObject); // destroy the game object
        }
    }
}
