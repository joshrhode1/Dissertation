using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pour : MonoBehaviour
{
    ParticleSystem pouring_effect;

    // Start is called before the first frame update
    void Start()
    {
        pouring_effect = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        float angle = Vector3.Angle(Vector3.down, transform.forward);

        if (angle <= 135f)
        {
            Debug.Log("Angle: " + angle);
            Debug.Log("Forward: " + transform.forward.ToString());
            if (!pouring_effect.isPlaying)
                pouring_effect.Play();
        }
        else 
        {
            if (pouring_effect.isPlaying)
                pouring_effect.Stop();
        }
    }
}
