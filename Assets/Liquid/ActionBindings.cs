using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActionBindings : MonoBehaviour
{
    public InputActionReference input_reference;
    GameObject last_open_ui = null;
    GameObject restart_ui;

    private void Awake() 
    {
        input_reference.action.started += OnSecondaryPress;

        restart_ui = GetChildWithName(gameObject, "ResetBackground");
        GameObject cancel_object = GetChildWithName(restart_ui, "CancelButton");
        Button cancel_button = cancel_object.GetComponent<Button>();

        cancel_button.onClick.AddListener(OnCancelPressed);
    }

    private void OnDestroy() 
    {
        input_reference.action.started -= OnSecondaryPress;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnSecondaryPress(InputAction.CallbackContext context) 
    {
        last_open_ui = null;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf) 
            {
                last_open_ui = child.gameObject;
                child.gameObject.SetActive(false);
                break;
            }
        }

        restart_ui.SetActive(true);
    }

    void OnCancelPressed() 
    {
        // If there was a previously open ui then bring it back
        restart_ui.SetActive(false);

        if (last_open_ui) 
        {
            last_open_ui.SetActive(true);
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
