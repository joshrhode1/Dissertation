using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidPouring : MonoBehaviour
{
    GameObject pouring;
    GameObject particle_module;
    ParticleSystem particle_system;
    Material liquid_material;
    Color liquid_colour;
    //float angle_step = 11f;
    float angle_step = 21f;
    float down_step = 2f;
    float fill_multiplier = 10f;
    //float starting_angle = 112f;
    float starting_angle = 73f;
    float step_factor = 2f;

    // Start is called before the first frame update
    void Start()
    {
        GameObject beaker_LOD = GetChildWithName(gameObject, "BeakerLiquid_LOD0");

        pouring = GetChildWithName(gameObject, "LiquidObject");
        particle_module = GetChildWithName(pouring, "Liquid Pour");
        particle_system = particle_module.GetComponent<ParticleSystem>();

        Renderer liquid_rend = beaker_LOD.GetComponent<Renderer>();
        liquid_material = liquid_rend.material;

        liquid_colour = liquid_material.GetColor("_LiquidColour");
   
        // TODO: Set colour of particles to the colour of liquid being poured
        //Debug.Log("Colour: " + liquid_colour.ToString());
        //particle_settings.startColor = liquid_colour;
    }

    // Update is called once per frame
    void Update()
    {
        var particle_settings = particle_system.main;
        particle_settings.startColor = new Color(liquid_colour.r, liquid_colour.g, liquid_colour.b, 1f);

        float angle = Vector3.Angle(Vector3.down, transform.up);

        //TODO:: Change position of particle to pour from the side that is rotated on
        float fill = liquid_material.GetFloat("_Fill");
        //float fill_scaled = (fill - 0.5f) * fill_multiplier;
        float fill_scaled = (fill - 0.1f) * fill_multiplier;

        float down_scaled = (fill - 0.2f) * fill_multiplier;
        float subtract_step = down_scaled * down_step;
        /*
        float added_factor = 0f;
        if () 
        {
            added_factor = step_factor * Mathf.Pow(2, fill_scaled - 1);
        }
        */
        //float angle_threshold = starting_angle + (fill_scaled * angle_step);
        float angle_threshold = starting_angle + (fill_scaled * (angle_step - subtract_step));

        /*
        if (fill < 0f) 
        {
            fill = -0.6f;
            liquid_material.SetFloat("_Fill", fill);
        }
        */

        //Debug.Log("Angle Thres: " + angle_threshold);

        if (fill > 0 && angle <= angle_threshold)
        {
            if (pouring) 
            {
                // Make sure the particles are pouring
                pouring.SetActive(true);
                //liquid_material.SetFloat("_Fill", fill - 0.001f);
            }
        }
        else
        {
            if (pouring)
            {
                // Stop particles from pouring
                pouring.SetActive(false);
            }
        }
        
        //Debug.Log("Angle: " + angle);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pouring)
        {
            // Make sure the particles are pouring
            pouring.SetActive(true);
        }

        Debug.Log("Trigger Enter");
    }

    void OnTriggerStay(Collider other)
    {
        if (pouring)
        {
            // Make sure the particles are pouring
            pouring.SetActive(true);
        }

        //Debug.Log("Trigger Stay: " + other.name);
    }

    void OnTriggerExit(Collider other)
    {
        if (pouring)
        {
            // Stop particles from pouring
            pouring.SetActive(false);
        }

        Debug.Log("Trigger Exit");
    }


    GameObject GetChildWithName(GameObject obj, string name)
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null)
        {
            return childTrans.gameObject;
        }
        else
        {
            return null;
        }
    }
}
