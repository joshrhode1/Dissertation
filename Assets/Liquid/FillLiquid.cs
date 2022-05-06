using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillLiquid : MonoBehaviour
{
    Material liquid_mat;

    // Start is called before the first frame update
    void Start()
    {
        GameObject beaker_LOD = GetChildWithName(gameObject, "BeakerLiquid_LOD0");
        Renderer liquid_rend = beaker_LOD.GetComponent<Renderer>();
        liquid_mat = liquid_rend.material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // If particle collides with our object it calls this function
    void OnParticleCollision(GameObject other)
    {
        ParticleSystem particle_system = other.GetComponent<ParticleSystem>();
        var settings = particle_system.main;

        float current_fill = liquid_mat.GetFloat("_Fill");
        liquid_mat.SetFloat("_Fill", current_fill + 0.001f);
        liquid_mat.SetColor("_LiquidColour", settings.startColor.color);

        Debug.Log("Hit Fill");
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
