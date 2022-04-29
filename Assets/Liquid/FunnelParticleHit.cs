using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FunnelParticleHit : MonoBehaviour
{
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;
    FunnelFlowControl funnel_flow_control;
    //Material pouring_liquid_material;
    Material funnel_liquid_material;
    Material funnel_tip_material;
    float fill_increase_per_particle = 0.001f;

    // Start is called before the first frame update
    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();

        GameObject object_parent = transform.parent.gameObject;
        GameObject tip_object = object_parent.transform.parent.gameObject;
        //GameObject beaker_LOD = GetChildWithName(beaker_parent, "BeakerLiquid_LOD0");

        Renderer liquid_rend = tip_object.GetComponent<Renderer>();
        funnel_tip_material = liquid_rend.material;

        GameObject tip_container = tip_object.transform.parent.gameObject;
        GameObject funnel_tip_object = tip_container.transform.parent.gameObject;
        GameObject funnel_tip_parent = funnel_tip_object.transform.parent.gameObject;
        GameObject separating_funnel = funnel_tip_parent.transform.parent.gameObject;
        funnel_flow_control = separating_funnel.GetComponent<FunnelFlowControl>();
        GameObject funnel_liquid_parent = GetChildWithName(separating_funnel, "FunnelBase");
        GameObject funnel_liquid = GetChildWithName(funnel_liquid_parent, "FunnelBase");
        Renderer funnel_liquid_rend = funnel_liquid.GetComponent<Renderer>();
        funnel_liquid_material = funnel_liquid_rend.material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // TODO:: DO not allow pouring to occur too early
    // When particle hits 
    void OnParticleCollision(GameObject other)
    {
        if (other.name != "ErlenmeyerMedium") 
        {
            return;
        }
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        BoxCollider[] colliders = other.GetComponents<BoxCollider>();

        int bottom_id = 0;
        float lowest_y = 100000f;
        for (int i = 0; i < colliders.Length; i++)
        {
            float current_y = colliders[i].center.y;
            if (current_y < lowest_y)
            {
                lowest_y = current_y;
                bottom_id = colliders[i].GetInstanceID();
            }
        }

        /*
        GameObject second_grab_object = GetChildWithName(other, "SecondGrabPoint");
        BoxCollider grab_collider = second_grab_object.GetComponent<BoxCollider>();

        float grab_y = grab_collider.center.y;

        if (grab_y < lowest_y)
        {
            lowest_y = grab_y;
            bottom_id = grab_collider.GetInstanceID();
        }
        */

        int count = 0;
        while (count < numCollisionEvents)
        {
            Component collider = collisionEvents[count].colliderComponent;
            //Collider collider = (Collider)comp;

            if (collider.GetInstanceID() == bottom_id)
            {
                //TODO:: If the particle hits the bottom of the funnel then make sure it fills up
                //TODO:: If particle hits the bottom of the funnel then make sure the pouring beaker reduces fill

                if (!funnel_flow_control.stopper_added_correctly || !funnel_flow_control.funnel_clamped_correctly) 
                {
                    DisableAllUIExcept("EarlyStepFailure");
                }


                // THESE ARE OBJECTS RELATED TO RECEIVING FUNNEL
                GameObject flask_object = GetChildWithName(other, "FlaskLiquid");

                List<Material> liquid_mats = new List<Material>();
                GameObject flask_liquid = GetChildWithName(flask_object, "ErlenmeyerMedium_LOD0");
                GameObject flask_liquid2 = GetChildWithName(flask_object, "ErlenmeyerMedium_LOD1");
                GameObject flask_liquid3 = GetChildWithName(flask_object, "ErlenmeyerMedium_LOD2");
                GameObject flask_liquid4 = GetChildWithName(flask_object, "ErlenmeyerMedium_LOD3");
                Renderer liquid_rend = flask_liquid.GetComponent<Renderer>();
                Renderer liquid_rend2 = flask_liquid2.GetComponent<Renderer>();
                Renderer liquid_rend3 = flask_liquid3.GetComponent<Renderer>();
                Renderer liquid_rend4 = flask_liquid4.GetComponent<Renderer>();
                //Material liquid_mat = liquid_rend.material;
                liquid_mats.Add(liquid_rend.material);
                liquid_mats.Add(liquid_rend2.material);
                liquid_mats.Add(liquid_rend3.material);
                liquid_mats.Add(liquid_rend4.material);


                float current_fill1 = liquid_mats[0].GetFloat("_Fill");
                float second_fill1 = liquid_mats[0].GetFloat("_Fill2");
                var settings = part.main;
                var particle_colour = settings.startColor.color;
                Color liquid_colour = liquid_mats[0].GetColor("_LiquidColour");
                Color second_liquid_colour = liquid_mats[0].GetColor("_LiquidColour2");

                // THESE ARE OBJECTS RELATED TO POURING BEAKER
                float pouring_fill = funnel_liquid_material.GetFloat("_Fill");
                float second_pouring_fill = funnel_liquid_material.GetFloat("_Fill2");

                // Make sure that we haven't already filled the funnel
                if (current_fill1 + second_fill1 < 1f) 
                {
                    bool successful_fill = false;
                    if (current_fill1 <= 0 || particle_colour == liquid_colour)
                    {
                        foreach (var mat in liquid_mats)
                        {
                            float current_fill = mat.GetFloat("_Fill");
                            mat.SetColor("_LiquidColour", particle_colour);
                            //NOTE:: Surface colour should be slightly lighter than liquid but for now use the same colour
                            mat.SetColor("_SurfacColour", particle_colour);
                            mat.SetFloat("_Fill", current_fill + fill_increase_per_particle);
                        }

                        //liquid_mat.SetColor("_LiquidColour", particle_colour);
                        //liquid_mat.SetColor("_SurfacColour", particle_colour);
                        //liquid_mat.SetFloat("_Fill", current_fill + fill_increase_per_particle);
                        successful_fill = true;
                    }
                    else if (second_fill1 <= 0 || particle_colour == second_liquid_colour)
                    {
                        foreach (var mat in liquid_mats)
                        {
                            float second_fill = mat.GetFloat("_Fill2");
                            mat.SetColor("_LiquidColour2", particle_colour);
                            //NOTE:: Surface colour should be slightly lighter than liquid but for now use the same colour
                            mat.SetColor("_SurfaceColour2", particle_colour);
                            mat.SetFloat("_Fill2", second_fill + fill_increase_per_particle);
                        }

                        //liquid_mat.SetColor("_LiquidColour2", particle_colour);
                        //liquid_mat.SetColor("_SurfaceColour2", particle_colour);
                        //liquid_mat.SetFloat("_Fill2", second_fill + fill_increase_per_particle);
                        successful_fill = true;
                    }

                    if (successful_fill) 
                    {
                        if (particle_colour == funnel_liquid_material.GetColor("_LiquidColour"))
                        {
                            funnel_liquid_material.SetFloat("_Fill", pouring_fill - fill_increase_per_particle);
                        }
                        else if(particle_colour == funnel_liquid_material.GetColor("_LiquidColour2"))
                        {
                            funnel_liquid_material.SetFloat("_Fill2", second_pouring_fill - fill_increase_per_particle);
                        }
                    }
                }

                
                //liquid_mat.SetFloat("_Fill", current_fill + 0.001f);
                
                //liquid_mat.SetColor("_LiquidColour", particle_colour);

                Debug.Log("Hit Bottom");
            }
            else 
            {
                Debug.Log("Not Hit Bottom");
            }

            count++;
        } 
    }

    void DisableAllUIExcept(string name)
    {
        foreach (Transform child in funnel_flow_control.canvas_ui.transform)
        {

            if (child.name == name)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
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
