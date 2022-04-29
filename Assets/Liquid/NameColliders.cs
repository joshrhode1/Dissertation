using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameColliders : MonoBehaviour
{
    int bottom_id;

    // Start is called before the first frame update
    void Start()
    {
        BoxCollider[] colliders = GetComponents<BoxCollider>();

        int bottom_index = -1;
        float lowest_y = 100000f;
        for (int i = 0; i < colliders.Length; i++)
        {
            float current_y = colliders[i].center.y;
            if (current_y < lowest_y) 
            {
                bottom_index = i;
                lowest_y = current_y;
                bottom_id = colliders[i].GetInstanceID();
            }
        }

        //Debug.Log(colliders.Length);

        /*
        for (int i = 0; i < colliders.Length; i++)
        {
            if (bottom_index == i)
            {
                //colliders[i].tag = "Bottom";
                bottom_id = colliders[i].GetInstanceID();
            }
            else 
            {
                //colliders[i].tag = "Side " + i.ToString();
            }
        }
        */

        Debug.Log("Bottom Id: " + bottom_id.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        BoxCollider[] colliders = GetComponents<BoxCollider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].GetInstanceID() == bottom_id)
            {
                //Debug.Log("Bottom: " + colliders[i].center.y);
            }
            else 
            {
               // Debug.Log("Other: " + colliders[i].center.y);
            }
        }
    }
}
