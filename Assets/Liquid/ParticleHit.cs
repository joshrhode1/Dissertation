using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleHit : MonoBehaviour
{
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;
    public GameObject canvas_ui;
    public ParticleHit other_particle_hit;
    public bool beaker_fully_poured = false;

    float fill_increase_per_particle = 0.001f;
    List<Material> pouring_liquid_materials;
    float sufficient_beaker_emptiness = 0.07f;

    // Start is called before the first frame update
    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        pouring_liquid_materials = new List<Material>();

        GameObject object_parent = transform.parent.gameObject;
        GameObject beaker_parent = object_parent.transform.parent.gameObject;
        GameObject beaker_LOD = GetChildWithName(beaker_parent, "BeakerLiquid_LOD0");
        GameObject beaker_LOD2 = GetChildWithName(beaker_parent, "BeakerLiquid_LOD1");
        GameObject beaker_LOD3 = GetChildWithName(beaker_parent, "BeakerLiquid_LOD2");
        GameObject beaker_LOD4 = GetChildWithName(beaker_parent, "BeakerLiquid_LOD3");

        Renderer liquid_rend = beaker_LOD.GetComponent<Renderer>();
        Renderer liquid_rend2 = beaker_LOD2.GetComponent<Renderer>();
        Renderer liquid_rend3 = beaker_LOD3.GetComponent<Renderer>();
        Renderer liquid_rend4 = beaker_LOD4.GetComponent<Renderer>();
        pouring_liquid_materials.Add(liquid_rend.material);
        pouring_liquid_materials.Add(liquid_rend2.material);
        pouring_liquid_materials.Add(liquid_rend3.material);
        pouring_liquid_materials.Add(liquid_rend4.material);

    }

    // Update is called once per frame
    void Update()
    {
    }

    // When particle hits 
    void OnParticleCollision(GameObject other)
    {
        if (other.name != "SeparatingFunnel" && other.name != "SecondGrabPoint") 
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

        GameObject second_grab_object = GetChildWithName(other, "SecondGrabPoint");
        BoxCollider grab_collider = second_grab_object.GetComponent<BoxCollider>();

        int grab_id = grab_collider.GetInstanceID();


        int count = 0;
        while (count < numCollisionEvents)
        {
            Component collider = collisionEvents[count].colliderComponent;

            // If we have hit the bottom of the funnel then fill it up while reducing fill of pouring beaker
            if (collider.GetInstanceID() == bottom_id || collider.GetInstanceID() == grab_id)
            {

                // THESE ARE OBJECTS RELATED TO RECEIVING FUNNEL
                GameObject funnel_object = GetChildWithName(other, "FunnelBase");
                GameObject funnel_liquid = GetChildWithName(funnel_object, "FunnelBase");
                Renderer liquid_rend = funnel_liquid.GetComponent<Renderer>();
                Material liquid_mat = liquid_rend.material;
                float current_fill = liquid_mat.GetFloat("_Fill");
                float second_fill = liquid_mat.GetFloat("_Fill2");
                var settings = part.main;
                var particle_colour = settings.startColor.color;
                Color liquid_colour = liquid_mat.GetColor("_LiquidColour");
                Color second_liquid_colour = liquid_mat.GetColor("_LiquidColour2");

                // Make sure that we haven't already filled the funnel
                if (current_fill + second_fill < 1f) 
                {
                    bool filling = false;
                    if (current_fill <= 0 || particle_colour == liquid_colour)
                    {
                        liquid_mat.SetColor("_LiquidColour", particle_colour);
                        liquid_mat.SetColor("_SurfacColour", particle_colour);
                        liquid_mat.SetFloat("_Fill", current_fill + fill_increase_per_particle);

                        filling = true;
                    }
                    else if (second_fill <= 0 || particle_colour == second_liquid_colour)
                    {
                        liquid_mat.SetColor("_LiquidColour2", particle_colour);
                        liquid_mat.SetColor("_SurfaceColour2", particle_colour);
                        liquid_mat.SetFloat("_Fill2", second_fill + fill_increase_per_particle);

                        filling = true;
                    }

                    if (filling) 
                    {
                        foreach (var mat in pouring_liquid_materials) 
                        {
                            float pouring_fill = mat.GetFloat("_Fill");
                            mat.SetFloat("_Fill", pouring_fill - fill_increase_per_particle);


                            GameObject welcome = GetChildWithName(canvas_ui, "WelcomeBackground");

                            // If we have poured enough of the beaker into the funnel then allow us to move to next step
                            if (pouring_fill <= sufficient_beaker_emptiness && other_particle_hit.beaker_fully_poured)
                            {

                                beaker_fully_poured = true;
                            }

                            if (pouring_fill <= sufficient_beaker_emptiness && !other_particle_hit.beaker_fully_poured)
                            {
                                beaker_fully_poured = true;
                            }

                        }
                    }
                }


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
        foreach (Transform child in canvas_ui.transform)
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
