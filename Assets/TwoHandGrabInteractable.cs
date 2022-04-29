using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

//From https://www.youtube.com/watch?v=Ie0-oKN3Lq0&list=PL-87DucZp74RdpYapFNGVh9GCBSxkwXXW&index=9

public class TwoHandGrabInteractable : XRGrabInteractable
{
    public List<XRSimpleInteractable> secondHandGrabPoints = new List<XRSimpleInteractable>();
    private XRBaseInteractor secondInteractor;
    private Quaternion attachInitialRotation;
    public enum TwoHandRotationType { None, First, Second};
    public TwoHandRotationType twoHandRotationType;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in secondHandGrabPoints)
        {
            Debug.Log("Adding Grab: " + item.name);
            item.selectEntered.AddListener(OnSecondHandGrab);
            item.selectExited.AddListener(OnSecondHandRelease);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) 
    {
        if (selectingInteractor && secondInteractor && !IsSocketInteractorSelecting())
        {
            //selectingInteractor.attachTransform.rotation = GetTwoHandRotation();
            selectingInteractor.attachTransform.rotation = GetTwoHandRotation();
            //selectingInteractor.transform.rotation = GetTwoHandRotation();
            Debug.Log(selectingInteractor.name + " " + secondInteractor.name);
            //selectingInteractor.attachTransform.rotation = Quaternion.LookRotation(selectingInteractor.attachTransform.position - secondInteractor.attachTransform.position);
        }

        base.ProcessInteractable(updatePhase);
    }

    private Quaternion GetTwoHandRotation() 
    {
        Quaternion targetRotation;

        // WHY DOES THIS NOT WORK??
        Vector3 targetPosition = secondInteractor.attachTransform.position - selectingInteractor.attachTransform.position;

        //Vector3 targetPosition = secondInteractor.transform.position - selectingInteractor.attachTransform.position;
        //Vector3 targetPosition = secondInteractor.transform.position - selectingInteractor.transform.position;
        //Vector3 targetPosition = selectingInteractor.transform.position - secondInteractor.transform.position;
        //Vector3 targetPosition = secondInteractor.attachTransform.localPosition - selectingInteractor.attachTransform.localPosition;

        //Vector3 targetPosition = secondHandGrabPoints[0].transform.position - selectingInteractor.attachTransform.position;
        if (twoHandRotationType == TwoHandRotationType.None)
        {
            targetRotation = Quaternion.LookRotation(targetPosition);
        }
        else if (twoHandRotationType == TwoHandRotationType.First)
        {
            targetRotation = Quaternion.LookRotation(targetPosition, selectingInteractor.transform.up);
        }
        else 
        {
            targetRotation = Quaternion.LookRotation(targetPosition, secondInteractor.attachTransform.up);
        }

        //Debug.Log(targetRotation.ToString());

        return targetRotation;
    }

    public void OnSecondHandGrab(SelectEnterEventArgs args) 
    {
        Debug.Log("Second Hand Grab");
        secondInteractor = args.interactor;
    }

    public void OnSecondHandRelease(SelectExitEventArgs args)
    {
        Debug.Log("Second Hand Release");
        secondInteractor = null;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args) 
    {
        Debug.Log("First Hand Grab");
        base.OnSelectEntered(args);
        attachInitialRotation = args.interactor.attachTransform.localRotation;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log("First Hand Grab");
        base.OnSelectExited(args);
        secondInteractor = null;
        args.interactor.attachTransform.localRotation = attachInitialRotation;
    }


    public override bool IsSelectableBy(XRBaseInteractor interactor) 
    {
        bool isSocket = IsSocketInteractorSelecting();

        bool alreadygrabbed = selectingInteractor && !interactor.Equals(selectingInteractor) && !isSocket;
        //bool alreadygrabbed = selectingInteractor && !interactor.Equals(selectingInteractor) && selectingInteractor.interactionLayerMask.value != 64;

        return base.IsSelectableBy(interactor) && !alreadygrabbed;
    }

    private bool IsSocketInteractorSelecting()
    {
        bool isSocket;
        try
        {
            XRSocketInteractor selector = (XRSocketInteractor)selectingInteractor;
            isSocket = true;
        }
        catch (System.InvalidCastException)
        {
            isSocket = false;
        }

        return isSocket;
    }
}


