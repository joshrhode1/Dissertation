using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ValveInteractor : XRSimpleInteractable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(transform.localPosition.ToString());
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        transform.RotateAround(transform.position, transform.right, 90);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        //transform.RotateAround(transform.localPosition, Vector3.right, 90);
        // transform.RotateAround(transform.localPosition, transform.right, 90);
        transform.RotateAround(transform.position, transform.right, 90);
        //Vector3 rotationToAdd = new Vector3(90, 0, 0);
        //transform.Rotate(Vector3.right, 90);
        //transform.Rotate(transform.right, 90);
        //transform.localRotation = Quaternion.Euler(90, 0, 0);
    }
}
