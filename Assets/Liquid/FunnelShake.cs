using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunnelShake : MonoBehaviour
{
    Renderer rend;
    float target_y = 0.07f;
    float y_step;
    float last_known_y_offset;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        y_step = target_y / 90f;
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 rotation = transform.parent.eulerAngles;

        // TODO:: May need to change x and z offsets according to the y rotation
        // For positive y, increase z offset + decrease x offset
        // For negative y, vice versa
        // 30 degrees = -0.0975 x, -0.025 z
        // -30 degrees = -0.025 x, -0.0975 z
        // 90 degrees = -0.07 x, 0.07 z
        // - 90 degrees = 0.07 x, -0.07 z
        // 120 degrees = -0.025 x, 0.0975 z
        float angle = Vector3.Angle(Vector3.up, transform.up);
        float max_rot;
        if (Mathf.Abs(rotation.x) > Mathf.Abs(rotation.z))
        {
            max_rot = rotation.x;
        }
        else 
        {
            max_rot = rotation.z;
        }

        float new_step = y_step;
        if (max_rot > 180.1f)
        {
            /*
            if (last_known_y_offset <= 0f)
            {
                new_step *= -1f;
            }
            */

            new_step *= -1f;
        }

        float max_rot_abs = Mathf.Abs(max_rot);
        if (angle > 90f) 
        {
            //max_rot_abs -= 90f;
            angle = 180f - angle;
            //new_step *= -1f;
        }

        float y_offset = new_step * angle;
        last_known_y_offset = y_offset;
        rend.material.SetVector("_ObjectOffset", new Vector3(-0.07f, y_offset, -0.07f));

        //Debug.Log("Offset: " + rend.material.GetVector("_ObjectOffset").ToString());
        //Debug.Log("Rotation: " + max_rot);
        //Debug.Log("Angle: " + angle);
        //Debug.Log("Rotation: " + max_rot);
        //Debug.Log("Offset: " + rend.material.GetVector("_ObjectOffset").ToString());
        
    }
}
