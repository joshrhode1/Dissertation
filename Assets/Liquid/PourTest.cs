using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PourTest : MonoBehaviour
{
    GameObject pouring;
    GameObject particle_module;
    ParticleSystem particle_system;
    Color liquid_colour;

    // Start is called before the first frame update
    void Start()
    {
        GameObject beaker_liquid = GetChildWithName(gameObject, "BeakerLiquid");
        GameObject beaker_LOD = GetChildWithName(beaker_liquid, "BeakerLiquid_LOD0");

        pouring = GetChildWithName(beaker_liquid, "LiquidObject");
        particle_module = GetChildWithName(pouring, "Liquid Pour");
        particle_system = particle_module.GetComponent<ParticleSystem>();

        Renderer liquid_rend = beaker_LOD.GetComponent<Renderer>();
        Material liquid_material = liquid_rend.material;

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

        /*
        if (angle <= 135f)
        {
            if (pouring) 
            {
                // Make sure the particles are pouring
                pouring.SetActive(true);
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
        */
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pouring)
        {
            // Make sure the particles are pouring
            pouring.SetActive(true);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (pouring)
        {
            // Make sure the particles are pouring
            pouring.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (pouring)
        {
            // Stop particles from pouring
            pouring.SetActive(false);
        }
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
