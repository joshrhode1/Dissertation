using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Class to make an object respawn if it is dropped somewhere random in the scene
public class Respawner : MonoBehaviour
{
    Vector3 original_position;
    Quaternion original_rotation;
    List<string> equipment_names = new List<string> { "SeparatingFunnel", "BeakerMedium", "ErlenmeyerMedium", "Stopper", "PouringBeaker2", "Table_Dining_Modern_Dark", "Top_Colliders", "Leg1_Collider", "Leg2_Collider", "Leg3_Collider", "Leg4_Collider", "Center_table", "SupportClamp", "SupportFrame" };

    // Start is called before the first frame update
    void Start()
    {
        original_position = transform.position;
        original_rotation = transform.rotation;

    }

    // Update is called once per frame
    void Update()
    {
    }

    // If it collides with anything other than the equipment or the table they are placed upon -> respawn in original position
    void OnCollisionEnter(Collision col)
    {

        if (equipment_names.Contains(col.gameObject.name)) 
        {
            return;
        }

        Debug.Log("Respawn Collision");

        
        Debug.Log("Name: " + gameObject.name + ", " + col.gameObject.name);
        
        transform.position = original_position;
        transform.rotation = original_rotation;
    }
}
