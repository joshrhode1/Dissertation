using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class FunnelFlowControl : MonoBehaviour
{
    public XRBaseInteractable stopperInteractable;
    public XRSimpleInteractable valve_interactable;
    public Material invisible_material;
    //public List<GameObject> user_interfaces;
    public GameObject canvas_ui;
    public GameObject finish_button;
    public GameObject flask_liquid;
    GameObject valve_pivot;
    Material funnel_liquid_material;
    Material funnel_tip_material;
    GameObject pouring_object;
    ParticleSystem particle_system;
    MeshRenderer stopper_top_mesh;
    MeshRenderer stopper_base_mesh;
    GameObject stopper_top;
    GameObject stopper_base;
    AudioSource hissing_audio;
    AudioSource positive_sound;
    AudioSource negative_sound;
    float pressure_countdown;
    float pressure_release_countdown = 20.0f;
    //bool countdown_active = false;
    int num_pressure_releases = 0;
    float countdown_lower = 7.0f;
    float countdown_upper = 13.0f;
    bool pressure_trapped = false;
    bool pressure_building = false;
    List<bool> shaking_verification = new List<bool>();
    public List<ParticleHit> particle_hit_scripts;
    public bool stopper_added_correctly = false;
    public bool funnel_clamped_correctly = false;
    bool step_4_done = false;
    enum CurrentStep { None, First, Second, Third, Fourth, Fifth, Sixth, Seventh };
    CurrentStep current_step = CurrentStep.First;
    VersionManager version_manager;
    bool last_known_guided;
    bool valve_events_added = false;


    private void Awake()
    {
        pressure_countdown = Random.Range(countdown_lower, countdown_upper);

        valve_pivot = GetChildWithName(gameObject, "ValvePivot");


        GameObject funnel_liquid_parent = GetChildWithName(gameObject, "FunnelBase");
        GameObject funnel_liquid = GetChildWithName(funnel_liquid_parent, "FunnelBase");
        Renderer liquid_rend = funnel_liquid.GetComponent<Renderer>();
        funnel_liquid_material = liquid_rend.material;


        GameObject funnel_tip_parent = GetChildWithName(gameObject, "FunnelTip");
        GameObject funnel_tip = GetChildWithName(funnel_tip_parent, "FunnelTip");
        GameObject funnel_tip_liquid_parent = GetChildWithName(funnel_tip, "TipLiquid");
        GameObject funnel_tip_liquid = GetChildWithName(funnel_tip_liquid_parent, "TipLiquid");

        pouring_object = GetChildWithName(funnel_tip_liquid, "LiquidObject");
        GameObject particle_module = GetChildWithName(pouring_object, "Liquid Pour");
        particle_system = particle_module.GetComponent<ParticleSystem>();

        /*
        if (!funnel_liquid)
        {
            Debug.Log("Funnel Liquid Null");
        }

        if (!funnel_tip_liquid) 
        {
            Debug.Log("Tip Liquid Null");
        }
        */

        Renderer tip_liquid_rend = funnel_tip_liquid.GetComponent<Renderer>();
        funnel_tip_material = tip_liquid_rend.material;

        stopperInteractable.selectEntered.AddListener(OnStopperSelected);
        stopperInteractable.selectExited.AddListener(OnStopperReleased);

        //stopperInteractable.hoverEntered.AddListener(OnSocketHoverEntered);
        //stopperInteractable.hoverExited.AddListener(OnSocketHoverExited);

        //Make child stopper inivisble to start with
        GameObject stopper_child = GetChildWithName(gameObject, "StopperStatic");
        stopper_top = GetChildWithName(stopper_child, "StopperTop");
        stopper_base = GetChildWithName(stopper_child, "StopperBase");

        stopper_top_mesh = stopper_top.GetComponent<MeshRenderer>();
        stopper_base_mesh = stopper_base.GetComponent<MeshRenderer>();

        stopper_top_mesh.enabled = false;
        stopper_base_mesh.enabled = false;

        valve_interactable.selectEntered.AddListener(OnValveSelected);
        //valve_interactable.activated.AddListener(OnValveActivated);

        hissing_audio = GetComponent<AudioSource>();

        GameObject positive_object = GetChildWithName(gameObject, "PositiveSound");
        positive_sound = positive_object.GetComponent<AudioSource>();

        GameObject negative_object = GetChildWithName(gameObject, "NegativeSound");
        negative_sound = negative_object.GetComponent<AudioSource>();

        XRGrabInteractable funnel_interactable = GetComponent<XRGrabInteractable>();
        funnel_interactable.selectEntered.AddListener(OnFunnelSelected);

        Button btn = finish_button.GetComponent<Button>();
        btn.onClick.AddListener(OnFinish);

        version_manager = GetComponent<VersionManager>();

        last_known_guided = version_manager.guided;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // TODO:: If bottom fill is greater than 0 then fill the tip with colour of bottom liquid colour
        // Else fill with top liquid (potential to fill tip partially with both colours)

        // TODO:: If Stopper is still in then do not pour
        // TODO:: If Stopper is not in, then pour is valve is open. Do not pour if it it is closed

        // TODO:: If tap is closed and there is still liquid in the tip, then keep the tip partially filled

        bool stopperSocket = IsStopperInSocket(stopperInteractable.selectingInteractor);


        //float valve_angle = Vector3.Angle(valve_pivot.transform.up, transform.up);
        //float valve_x_rotation = valve_pivot.transform.eulerAngles.x;

        float funnel_fill = funnel_liquid_material.GetFloat("_Fill");
        float top_funnel_fill = funnel_liquid_material.GetFloat("_Fill2");

        Color tip_colour = funnel_liquid_material.GetColor("_LiquidColour");

        bool liquid_present = true;
        if (funnel_fill > 0f)
        {
            tip_colour = funnel_liquid_material.GetColor("_LiquidColour");
        }
        else if (top_funnel_fill > 0f)
        {
            tip_colour = funnel_liquid_material.GetColor("_LiquidColour2");
        }
        else
        {
            liquid_present = false;
        }

        funnel_tip_material.SetColor("_LiquidColour", tip_colour);
        var particle_settings = particle_system.main;
        particle_settings.startColor = new Color(tip_colour.r, tip_colour.g, tip_colour.b, 1f);

        //float rotation_abs = Mathf.Abs(valve_x_rotation);

        // This means they are trying to release pressure from the funnel
        if (IsValveOpen() && stopperSocket)
        {
            /*
            Debug.Log("Releasing Pressure");
            // Send haptic for now. This will need to be hissing sound in the future
            try
            {
                XRBaseControllerInteractor hand = (XRBaseControllerInteractor)GetComponent<TwoHandGrabInteractable>().selectingInteractor;
                hand.SendHapticImpulse(0.4f, 0.5f);
            }
            catch (System.InvalidCastException)
            {
                Debug.Log("No Controller Holding");
            }

            if (pouring_object)
            {
                pouring_object.SetActive(false);
            }
            */
        }
        // Closed tap
        else if (!IsValveOpen() || !liquid_present || stopperSocket)
        {
            funnel_tip_material.SetFloat("_Fill", 0f);

            if (pouring_object)
            {
                pouring_object.SetActive(false);
            }
        }
        else // Opened tap
        {
            funnel_tip_material.SetFloat("_Fill", 1f);

            if (pouring_object)
            {
                // Make sure the particles are pouring
                pouring_object.SetActive(true);
            }
        }


        if (num_pressure_releases > 0 && pressure_countdown > 0f && pressure_building) 
        {
            pressure_countdown -= Time.deltaTime;
        }

        if (pressure_trapped && pressure_release_countdown > 0f) 
        {
            pressure_release_countdown -= Time.deltaTime;
        }

        if (pressure_trapped && pressure_release_countdown <= 0f)
        {
            // Experiment has failed. Pressure has built up too much
            // TODO :: Make stopper fly out of the funnel
            current_step = CurrentStep.None;
            DisableAllUIExcept("PressureFailure");

            /*
            foreach (GameObject ui in user_interfaces)
            {
                if (ui.name == "PressureFailure")
                {
                    ui.SetActive(true);
                }
                else
                {
                    ui.SetActive(false);
                }
            }
            */
        }

        if (num_pressure_releases > 0 && pressure_countdown <= 0f && pressure_building) 
        {
            //pressure_trapped = true;
            //pressure_release_countdown = 20.0f;
            //pressure_building = false;

            // Send haptic to controller holding the top of the funnel to indicate that pressure needs to be released
            try
            {
                XRBaseControllerInteractor hand = (XRBaseControllerInteractor)GetComponent<TwoHandGrabInteractable>().selectingInteractor;
                hand.SendHapticImpulse(0.8f, 1.0f);
                pressure_trapped = true;
                pressure_release_countdown = 20.0f;
                pressure_building = false;

                Debug.Log("Pressure is trapped");
            }
            catch (System.InvalidCastException)
            {
                Debug.Log("No Controller Holding");
            }
        }


        // Make sure they are shaking vigourously enough in between pressure releases
        if (num_pressure_releases > 0) 
        {
            // TODO:: Get wobble amount from wobble script
            float fill_1 = funnel_liquid_material.GetFloat("_Fill");
            float fill_2 = funnel_liquid_material.GetFloat("_Fill2");

            // Only check for the wobble if we are shaking two liquids
            if (fill_1 > 0 && fill_2 > 0) 
            {
                float wobble_x = funnel_liquid_material.GetFloat("_WobbleX");
                float wobble_z = funnel_liquid_material.GetFloat("_WobbleZ");

                float max_wobble = Mathf.Max(wobble_x, wobble_z);
                max_wobble = Mathf.Abs(max_wobble);
                //Debug.Log("Max wobble " + max_wobble);

                float wobble_threshold = funnel_liquid_material.GetFloat("_WobbleThreshold");
                //float wobble_threshold = 0.019f;

                if (max_wobble > wobble_threshold) 
                {
                    // They have shaken enough for this instance
                    shaking_verification[num_pressure_releases - 1] = true;

                    Debug.Log("Shaken enough for " + num_pressure_releases);
                }
            }
        }

        if (stopper_added_correctly && num_pressure_releases == 0 && !step_4_done) 
        {
            /*
            GameObject step_4 = GetChildWithName(canvas_ui, "Step4");
            GameObject step_5 = GetChildWithName(canvas_ui, "Step5");

            step_4.SetActive(false);
            step_5.SetActive(true);
            */
            current_step = CurrentStep.Fifth;

            if (version_manager.guided) 
            {
                DisableAllUIExcept("Step5");
            }
            
            step_4_done = true;
        }


        if (current_step == CurrentStep.First) 
        {
            foreach (ParticleHit particle_hit in particle_hit_scripts) 
            {
                if (particle_hit.beaker_fully_poured) 
                {
                    current_step = CurrentStep.Second;
                    if (version_manager.guided) 
                    {
                        DisableAllUIExcept("Step2");
                    }
                    break;
                }
            }
        }

        if (current_step == CurrentStep.Second) 
        {
            bool all_poured = true;
            foreach (ParticleHit particle_hit in particle_hit_scripts)
            {
                all_poured = all_poured && particle_hit.beaker_fully_poured;
            }

            if (all_poured) 
            {
                current_step = CurrentStep.Third;
                if (version_manager.guided)
                {
                    DisableAllUIExcept("Step3");
                }
            }
        }


        if (last_known_guided != version_manager.guided)
        {
            last_known_guided = version_manager.guided;
            OnGuidedUpdated();
        }

        /*
        if (!valve_events_added && valve_interactable.gameObject.activeSelf) 
        {
            valve_interactable.selectEntered.AddListener(OnValveSelected);
            valve_interactable.activated.AddListener(OnValveActivated);
            valve_events_added = true;
        }
        */

        //Debug.Log(valve_angle);
    }

    // TODO:: Make this validate support step & produce sound when funnel is placed in the clamp
    public void OnFunnelSelected(SelectEnterEventArgs args) 
    {
        bool funnel_in_socket = IsStopperInSocket(args.interactor);

        if (!funnel_in_socket)
        {
            bool stopperSocket = IsStopperInSocket(stopperInteractable.selectingInteractor);


            if (stopper_added_correctly && num_pressure_releases > 0 && stopperSocket && !pressure_trapped && !IsValveOpen()) 
            {
                pressure_building = true;
            }
            return;
        }

        if (stopper_added_correctly && num_pressure_releases > 0) 
        {
            current_step = CurrentStep.None;
            DisableAllUIExcept("PressureFailure");
        }

        if (stopper_added_correctly && num_pressure_releases == 0 && !funnel_clamped_correctly) 
        {
            /*
            GameObject step_5 = GetChildWithName(canvas_ui, "Step5");
            GameObject step_6 = GetChildWithName(canvas_ui, "Step6");

            step_5.SetActive(false);
            step_6.SetActive(true);
            */

            current_step = CurrentStep.Sixth;

            if (version_manager.guided) 
            {
                DisableAllUIExcept("Step6");
            }
            
            funnel_clamped_correctly = true;
        }
    }

    public void OnStopperSelected(SelectEnterEventArgs args)
    {
        //stopperInteractable.gameObject.transform.SetParent(gameObject.transform);

        bool stopperSocket = IsStopperInSocket(args.interactor);

        if (!stopperSocket)
        {
            return;
        }

        stopper_top_mesh.enabled = true;
        stopper_base_mesh.enabled = true;

        GameObject stopper_socket = stopperInteractable.gameObject;
        GameObject stopper_socket_top = GetChildWithName(stopper_socket, "StopperTop");
        GameObject stopper_socket_base = GetChildWithName(stopper_socket, "StopperBase");

        //stopper_socket_top.GetComponent<MeshRenderer>().enabled = false;
        //stopper_socket_base.GetComponent<MeshRenderer>().enabled = false;

        /*
        Material stopper_top_mat = stopper_socket_top.GetComponent<Material>();
        Material stopper_base_mat = stopper_socket_base.GetComponent<Material>();

        stopper_top_mat = invisible_material;
        stopper_base_mat = invisible_material;
        */

        stopper_socket_top.GetComponent<Renderer>().material = invisible_material;
        stopper_socket_base.GetComponent<Renderer>().material = invisible_material;


        if (current_step == CurrentStep.Seventh)
        {
            current_step = CurrentStep.Sixth;

            if (version_manager.guided)
            {
                DisableAllUIExcept("Step6");
            }

            return;
        }

        XRGrabInteractable funnel_interactable = GetComponent<XRGrabInteractable>();
        if (current_step == CurrentStep.Fourth && !IsValveOpen() && !IsStopperInSocket(funnel_interactable.selectingInteractor) && num_pressure_releases > 0) 
        {
            pressure_building = true;
        }

        if (current_step == CurrentStep.Fourth || current_step == CurrentStep.Fifth || current_step == CurrentStep.Sixth) 
        {
            return;
        }

        num_pressure_releases = Random.Range(1, 6);
        Debug.Log("Number pressures: " + num_pressure_releases);

        for (int i = 0; i < num_pressure_releases; i++) 
        {
            shaking_verification.Add(false);
        }

        if (!IsValveOpen() && !IsStopperInSocket(funnel_interactable.selectingInteractor)) 
        {
            // TODO :: This may only need to be set when the funnel starts to be shaken
            pressure_building = true;
        }

        //GameObject step_3 = GetChildWithName(canvas_ui, "Step3");
        //GameObject step_4 = GetChildWithName(canvas_ui, "Step4");

        bool beakers_empty = true;

        foreach (ParticleHit particle_hit in particle_hit_scripts) 
        {
            beakers_empty = beakers_empty && particle_hit.beaker_fully_poured;
        }

        if (beakers_empty && !step_4_done)
        {
            //step_3.SetActive(false);
            //step_4.SetActive(true);

            current_step = CurrentStep.Fourth;

            if (version_manager.guided)
            {
                DisableAllUIExcept("Step4");
            }

            stopper_added_correctly = true;
        }
        else
        {
            current_step = CurrentStep.None;
            DisableAllUIExcept("EarlyStepFailure");
        }

    }

    public void OnStopperReleased(SelectExitEventArgs args)
    {
        //stopperInteractable.gameObject.transform.SetParent(null);

        bool stopperSocket = IsStopperInSocket(args.interactor);

        if (!stopperSocket)
        {
            return;
        }

        stopper_top_mesh.enabled = false;
        stopper_base_mesh.enabled = false;

        GameObject stopper_socket = stopperInteractable.gameObject;
        GameObject stopper_socket_top = GetChildWithName(stopper_socket, "StopperTop");
        GameObject stopper_socket_base = GetChildWithName(stopper_socket, "StopperBase");

        //stopper_socket_top.GetComponent<MeshRenderer>().enabled = true;
        //stopper_socket_base.GetComponent<MeshRenderer>().enabled = true;

        /*
        Material stopper_top_mat = stopper_socket_top.GetComponent<Material>();
        Material stopper_base_mat = stopper_socket_base.GetComponent<Material>();

        stopper_top_mat = stopper_top.GetComponent<Material>();
        stopper_base_mat = stopper_base.GetComponent<Material>();
        */

        stopper_socket_top.GetComponent<Renderer>().material = stopper_top.GetComponent<Renderer>().material;
        stopper_socket_base.GetComponent<Renderer>().material = stopper_base.GetComponent<Renderer>().material;

        if (funnel_clamped_correctly)
        {
            /*
            GameObject step_6 = GetChildWithName(canvas_ui, "Step6");
            GameObject step_7 = GetChildWithName(canvas_ui, "Step7");

            step_6.SetActive(false);
            step_7.SetActive(true);
            */

            current_step = CurrentStep.Seventh;

            
            DisableAllUIExcept("Step7");
            
        }
        else if (num_pressure_releases > 0) 
        {
            current_step = CurrentStep.None;
            DisableAllUIExcept("PressureFailure");
        }
        else if (!funnel_clamped_correctly || !stopper_added_correctly)
        {
            current_step = CurrentStep.None;
            DisableAllUIExcept("EarlyStepFailure");
        }
    }

    public void OnSocketHoverEntered(HoverEnterEventArgs args) 
    {
        positive_sound.Play();
    }

    public void OnSocketHoverExited(HoverExitEventArgs args)
    {
        negative_sound.Play();
    }

    public void OnValveSelected(SelectEnterEventArgs args)
    {
        valve_pivot.transform.RotateAround(valve_pivot.transform.position, valve_pivot.transform.right, 90);

        //float valve_angle = Vector3.Angle(valve_pivot.transform.up, transform.up);
        bool stopperSocket = IsStopperInSocket(stopperInteractable.selectingInteractor);

        if (IsValveOpen() && stopperSocket)
        {
            Debug.Log("Releasing Pressure");

            if (num_pressure_releases > 0 && pressure_trapped)
            {
                hissing_audio.Play();

                // Set pressure releases down by 1
                num_pressure_releases--;
            }

            pressure_building = false;
            pressure_trapped = false;

            if (pouring_object)
            {
                pouring_object.SetActive(false);
            }
        }
        else if (!IsValveOpen() && stopperSocket)
        {
            if (num_pressure_releases > 0)
            {
                // Reset pressure countdown
                pressure_countdown = Random.Range(countdown_lower, countdown_upper);

                // TODO :: This may only need to be set when the funnel starts to be shaken
                pressure_building = true;
                Debug.Log("Pressure building again");
            }
        }
    }

    public void OnValveActivated(ActivateEventArgs args)
    {
        valve_pivot.transform.RotateAround(valve_pivot.transform.position, valve_pivot.transform.right, 90);
    }

    public bool IsStopperInSocket(XRBaseInteractor socket_interactor) 
    {
        bool stopperSocket = false;
        if (socket_interactor)
        {
            try
            {
                XRSocketInteractor selector = (XRSocketInteractor)socket_interactor;
                stopperSocket = true;
            }
            catch (System.InvalidCastException)
            {
                stopperSocket = false;
            }
        }

        return stopperSocket;
    }

    public bool IsValveOpen() 
    {
        float valve_angle = Vector3.Angle(valve_pivot.transform.up, transform.up);

        if ((valve_angle >= 179f && valve_angle <= 181f) || (valve_angle >= -1f && valve_angle <= 1f))
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    void OnFinish()
    {
        // Make sure they have shaken enough between pressure releases

        bool all_shaken = true;

        foreach(bool shaken in shaking_verification) 
        {
            all_shaken = shaken && all_shaken;
        }


        float funnel_fill = funnel_liquid_material.GetFloat("_Fill");

        GameObject liquid = GetChildWithName(flask_liquid, "ErlenmeyerMedium_LOD0");
        Renderer liquid_rend = liquid.GetComponent<Renderer>();
        Material liquid_mat = liquid_rend.material;

        float flask_upper_fill = liquid_mat.GetFloat("_Fill2");

        GameObject last_step = GetChildWithName(canvas_ui, "Step7");
        GameObject shake_failure = GetChildWithName(canvas_ui, "ShakeFailure");
        GameObject funnel_layer_failure = GetChildWithName(canvas_ui, "FunnelLayerFailure");
        GameObject flask_layer_failure = GetChildWithName(canvas_ui, "FlaskLayerFailure");
        GameObject success = GetChildWithName(canvas_ui, "Success");

        if (last_step.activeSelf) 
        {
            last_step.SetActive(false);
        }

        if (!all_shaken)
        {
            shake_failure.SetActive(true);
        }
        else if (funnel_fill >= 0.01) 
        {
            funnel_layer_failure.SetActive(true);
        }
        else if (flask_upper_fill >= 0.01)
        {
            flask_layer_failure.SetActive(true);
        }
        else
        {
            success.SetActive(true);
        }

        /*
        foreach (GameObject ui in user_interfaces) 
        {
            if (ui.name == "Step7" && ui.activeSelf) 
            {
                ui.SetActive(false);
            }

            if (!all_shaken && ui.name == "ShakeFailure")
            {
                ui.SetActive(true);
            }
            else if (all_shaken && funnel_fill >= 0.01 && ui.name == "FunnelLayerFailure")
            {
                ui.SetActive(true);
            }
            else if (all_shaken && funnel_fill < 0.01 && flask_upper_fill >= 0.01 && ui.name == "FlaskLayerFailure")
            {
                ui.SetActive(true);
            }
            else if (all_shaken && funnel_fill < 0.01 && flask_upper_fill < 0.01 && ui.name == "Success") 
            {
                ui.SetActive(true);
            }
            
        }
        */
    }

    void OnGuidedUpdated()
    {
        if (last_known_guided)
        {
            if (current_step == CurrentStep.First)
            {
                DisableAllUIExcept("Step1");
            }
            else if (current_step == CurrentStep.Second)
            {
                DisableAllUIExcept("Step2");
            }
            else if (current_step == CurrentStep.Third)
            {
                DisableAllUIExcept("Step3");
            }
            else if (current_step == CurrentStep.Fourth)
            {
                DisableAllUIExcept("Step4");
            }
            else if (current_step == CurrentStep.Fifth)
            {
                DisableAllUIExcept("Step5");
            }
            else if (current_step == CurrentStep.Sixth)
            {
                DisableAllUIExcept("Step6");
            }
            else if (current_step == CurrentStep.Seventh)
            {
                DisableAllUIExcept("Step7");
            }
        }
        else
        {
            DisableAllSteps();
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

    void DisableAllSteps()
    {
        foreach (Transform child in canvas_ui.transform)
        {

            if (child.name.Contains("Step") && !child.name.Contains("7"))
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
