using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This script allows a user to change animation elements of an object to suit
// The situation

public class Sc_Cont_Anim : MonoBehaviour
{
    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    private RL_Clock rL_Clock;
    private Sc_SortInput sI;

    /////////////////////
    // SELF REFERENCES //
    /////////////////////

    public Coroutine cr;
    private Animator anim;

    //////////////////////
    // USER PREFERENCES //
    //////////////////////

    // If the device is animating
    public bool isOn = true;
    // If the animation is tied directly to the real world clock yet
    public bool isLocked = false;
    // Select the sound effect
    [FMODUnity.EventRef]
    public string fmodEvent_String;
    FMOD.Studio.EventInstance fmodEvent;
    [Tooltip ("3D audio requires a rigidbody attached to it in order to work (But I can just disable it too")]
    [FMODUnity.ParamRef]
    public string paramRef;
    public bool is3DAudio = false;

    public Transform animPlat_Target;   // The target transform that will be animated
    public Transform sound_Target;       // That target that will output sound, if sound is 3D

    public enum AnimMovement {Path, PathRealTime, Orbit, Rotate, Debug, Toggle, Floaty, Input, Anim, Path_Lift, Toggle_Lift};
    public enum AnimFunct {Sigmoid, Sine, Linear, Ticking, Lift};
    public AnimMovement animMovement;
    public AnimFunct animFunct;

    // Time offset
    [Range (0f, 1f)]
    public float offset = 0f;

    // Animation speed
    public float animSpeed = 1f;
    private float sigmoidMultiplier = 7f;

    // The start point for oscillations are where the target starts
    private Vector3 startPoint = Vector3.zero;

    // Users can set a series of points for an object to move through
    public List<Vector3> listOf_Points;
    private Vector3[] arrayOf_Points;

    // Rotation
    public Vector3 rotationAxis = new Vector3(1f, 0f, 0f);
    public float rotationIndex = 1f;

    // Orbit
    public float radius_Orbit = 3f;
    // How to pick orbit plane?

    // Floaty angle
    [Tooltip ("Max angle of floaty bob effect")]
    public float maxAngle;

    // t_Anim
    private float t_Anim = 0f;
    private float t_Anim2 = 0f;

    public float test_Move = 8f;
    public float test_Move2 = 8f;
    public float fixingRatio = 0.95f;
    public float lockingSpeed = 0f;

    void Start()
    {
        // World References
        rL_Clock = GM.Instance.GetComponentInChildren<RL_Clock>();
        sI = GM.Instance.GetComponent<Sc_SortInput>();

        // Build the SFX reference, if there is one
        if (fmodEvent_String != "")
        {
            fmodEvent = FMODUnity.RuntimeManager.CreateInstance(fmodEvent_String);

            if (is3DAudio)
            {
                // 3D sounds must have their sounds attached to a gameObject
                // Create a rigidbody for the transform's 3D sound
                Rigidbody tempRB = sound_Target.gameObject.AddComponent<Rigidbody>();
                tempRB.isKinematic = true;
                tempRB.useGravity = false;
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(fmodEvent, sound_Target, tempRB);
                // There is a bug with FMod that requires a 3D sound to start playing in the same instance that it is attached to a gameobject
                // In this case, play 3D just after attaching and do not stop or change it, just keep it playing.
                // Could also reattach each time, but that sounds like overkill.
                fmodEvent.start();
            }
        }

        // Start point
        if (animPlat_Target != null)
        {
            startPoint = animPlat_Target.localPosition;
        }

        // Point list
        // Add an initial point to the first and last positions in the list
        listOf_Points.Insert(0, Vector3.zero);
        listOf_Points.Add(Vector3.zero);
        // Convert the list to an array
        arrayOf_Points = listOf_Points.ToArray();
        // Shift the elements of the array by the startPoint offset
        for (int i = 0; i < arrayOf_Points.Length; i++)
        {
            arrayOf_Points[i] = arrayOf_Points[i] + startPoint;
        }

        Switch(isOn);
    }

    public void Switch_State(int state = 0)
    {
        // Reverse the switch state, if no coroutine is currently running
        if (cr == null)
        {
            if (state == -1)
            {
                Switch(!isOn);
            }
            else if (state == 0)
            {
                Switch(false);
            }
            else if (state == 1)
            {
                Switch(true);
            }
        }
    }

    public void Switch(bool pIsOn)
    {
        // Special case for Toggle, we want to check that the animation state has switched
        if (animMovement == AnimMovement.Toggle)
        {
            if (isOn != pIsOn)
            {
                isOn = pIsOn;
                if (cr != null)
                {
                    StopCoroutine(cr);
                }
                cr = StartCoroutine(Animate_Toggle());
            }
        }
        else if (animMovement == AnimMovement.Toggle_Lift)
        {
            if (isOn != pIsOn)
            {
                isOn = pIsOn;
                if (cr != null)
                {
                    StopCoroutine(cr);
                }
                cr = StartCoroutine(Animate_Path_Lift_Toggle());
            }
        }

        // Set the animation state
        isOn = pIsOn;

        // Cases for toggling devices
        if (animMovement == AnimMovement.Anim)
        {
            cr = StartCoroutine(Animate_Anim(isOn));
        }

        // If we're turning on the device
        // And there isn't a coroutine already playing
        if (isOn && cr == null)
        {
            // Delock it
            isLocked = false;

            // Play sound, if viable
            if (fmodEvent_String != "" && !is3DAudio)
            {
                fmodEvent.start();
            }            

            // Pick animation
            if (animMovement == AnimMovement.Path)
            {
                cr = StartCoroutine(Animate_Path());
            }
            else if (animMovement == AnimMovement.PathRealTime)
            {
                cr = StartCoroutine(Animate_Path_RealTime());
            }
            else if (animMovement == AnimMovement.Orbit)
            {
                cr = StartCoroutine(Animate_Orbit());
            }
            else if (animMovement == AnimMovement.Rotate)
            {
                cr = StartCoroutine(Animate_Rotate());
            }
            else if (animMovement == AnimMovement.Debug)
            {
                cr = StartCoroutine(Animate_Debug());
            }
            else if (animMovement == AnimMovement.Floaty)
            {
                cr = StartCoroutine(Animate_Floaty());
            }
            else if (animMovement == AnimMovement.Input)
            {
                cr = StartCoroutine(Animate_Input());
            }
            else if (animMovement == AnimMovement.Path_Lift)
            {
                cr = StartCoroutine(Animate_Path_Lift());
            }
        }

        if (!isOn)
        {
            // Switch off sound
            if (fmodEvent_String != "" && !is3DAudio)
            {
                fmodEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    IEnumerator Animate_Path_RealTime()
    {
        // Animation total time
        float totalAnimationTime = (1f/animSpeed) * (arrayOf_Points.Length - 1);

        while (isOn)
        {
            // Get the real world time that we'd like the objects to be positioned at
            float t_rw = (float)((rL_Clock.rW_Sec_D*animSpeed + totalAnimationTime*offset) % ((float)arrayOf_Points.Length - 1f));

            Lerp_To_TAnim(t_rw);

            yield return null;
        }

        // Run a coroutine that reduces the t_Anim value back to zero
        while (t_Anim > 0f && !isOn)
        {
            Lerp_To_TAnim(0f);

            yield return null;
        }

        // Reset the main coroutine
        cr = null;

        yield return null;
    }

    IEnumerator Animate_Path()
    {
        // Animation total time
        float totalAnimationTime = (1f/animSpeed) * (arrayOf_Points.Length - 1);

        // First, we perform a delay based on the offset time
        //yield return new WaitForSeconds(totalAnimationTime * offset);

        // Wait until the external clock says you can turn on
        // Modulo the unix time and ensure it's on beat with the clock syncopation?

        while (isOn)
        {
            // Go through each point in the point list
            for (int i = 0; i < arrayOf_Points.Length - 1; i++)
            {
                // Lerp towards the next destination in the list based on the real world time?
                for (float t = 0; t < 1f; t += Time.deltaTime/animSpeed)
                {
                    // Calculate the rate of change
                    Update_SFX(f_Sin_Norm(t));

                    animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[i], arrayOf_Points[i + 1], Pick_F(t));
                    yield return null;
                }

                // Manually position object after animation
                animPlat_Target.localPosition = arrayOf_Points[i + 1];

                /*
                while ( !
                        (   (rL_Clock.rW_Sec_D % totalAnimationTime < totalAnimationTime*offset + 0.5f)
                            &&
                            (rL_Clock.rW_Sec_D % totalAnimationTime > totalAnimationTime*offset)))
                {
                    yield return null;
                }*/
            }
        }

        // Reset the main coroutine
        cr = null;

        yield return null;
    }

    IEnumerator Animate_Path_Lift()
    {
        // Animation total time
        float totalAnimationTime = (1f/animSpeed) * (arrayOf_Points.Length - 1);

        float percentageStep = 0.4f;    // Percentage gap change in stages

        // First, we perform a delay based on the offset time
        //yield return new WaitForSeconds(totalAnimationTime * offset);

        // Wait until the external clock says you can turn on
        // Modulo the unix time and ensure it's on beat with the clock syncopation?

        while (isOn)
        {
            // Go through each point in the point list
            for (int i = 0; i < arrayOf_Points.Length - 1; i++)
            {
                // Move one unit, get up to speed
                for (float t = 0; t < 1f; t += 9f * Time.deltaTime/animSpeed)
                {
                    // Calculate the rate of change
                    Update_SFX(f_Sin_Norm(t));

                    // We want to move from the start and move 1 unit increment in the right direction
                    animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[i], arrayOf_Points[i] + arrayOf_Points[i + 1].normalized, f_Sigmoid_Half(t));
                    yield return null;
                }

                // Move linearly
                for (float t = 0f; t < 1f; t += Time.deltaTime/animSpeed)
                {
                    // Calculate the rate of change
                    Update_SFX(f_Sin_Norm(t));

                    // Here we move a from start + 1 to the end - 1, linearly
                    animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[i] + arrayOf_Points[i + 1].normalized, arrayOf_Points[i + 1] - arrayOf_Points[i + 1].normalized, t);
                    yield return null;
                }

                // Move one unit, slow to stop
                for (float t = 0f; t < 1f; t += 9f * Time.deltaTime/animSpeed)
                {
                    // Calculate the rate of change
                    Update_SFX(f_Sin_Norm(t));

                    // We want to move from the start and move 1 unit increment in the right direction
                    animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[i + 1] - arrayOf_Points[i + 1].normalized, arrayOf_Points[i + 1], f_Sigmoid_Half_Other(t));
                    yield return null;
                }

                // Manually position object after animation
                animPlat_Target.localPosition = arrayOf_Points[i + 1];

                /*
                while ( !
                        (   (rL_Clock.rW_Sec_D % totalAnimationTime < totalAnimationTime*offset + 0.5f)
                            &&
                            (rL_Clock.rW_Sec_D % totalAnimationTime > totalAnimationTime*offset)))
                {
                    yield return null;
                }*/
            }
        }

        // Reset the main coroutine
        cr = null;

        yield return null;
    }

    IEnumerator Animate_Path_Lift_Toggle()
    {
        int i = 1;
        if (isOn)
        {
            i = 0;
        }


        // Animation total time
        float totalAnimationTime = (1f/animSpeed) * (arrayOf_Points.Length - 1);

        float percentageStep = 0.4f;    // Percentage gap change in stages


        // Move one unit, get up to speed
        for (float t = 0; t < 1f; t += 9f * Time.deltaTime/animSpeed)
        {
            // Calculate the rate of change
            Update_SFX(f_Sin_Norm(t));

            // We want to move from the start and move 1 unit increment in the right direction
            animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[i], arrayOf_Points[i] + arrayOf_Points[i + 1].normalized, f_Sigmoid_Half(t));
            yield return null;
        }

        // Move linearly
        for (float t = 0f; t < 1f; t += Time.deltaTime/animSpeed)
        {
            // Calculate the rate of change
            Update_SFX(f_Sin_Norm(t));

            // Here we move a from start + 1 to the end - 1, linearly
            animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[i] + arrayOf_Points[i + 1].normalized, arrayOf_Points[i + 1] - arrayOf_Points[i + 1].normalized, t);
            
            yield return null;
        }

        // Move one unit, slow to stop
        for (float t = 0f; t < 1f; t += 9f * Time.deltaTime/animSpeed)
        {
            // Calculate the rate of change
            Update_SFX(f_Sin_Norm(t));

            // We want to move from the start and move 1 unit increment in the right direction
            animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[i + 1] - arrayOf_Points[i + 1].normalized, arrayOf_Points[i + 1], f_Sigmoid_Half_Other(t));
            yield return null;
        }

        // Manually position object after animation
        animPlat_Target.localPosition = arrayOf_Points[i + 1];

        /*
        while ( !
                (   (rL_Clock.rW_Sec_D % totalAnimationTime < totalAnimationTime*offset + 0.5f)
                    &&
                    (rL_Clock.rW_Sec_D % totalAnimationTime > totalAnimationTime*offset)))
        {
            yield return null;
        }*/

        // Reset the main coroutine
        cr = null;

        yield return null;
    }

    // For animations that orbit with a radius
    IEnumerator Animate_Orbit()
    {
        // First, we perform a delay based on the offset time
        yield return new WaitForSeconds((1f/animSpeed) * offset);

        while (isOn)
        {
            // Orbits the object with a radius as dictated by the user

            // Let's start with just X and Z plane orbitting
            for (float t = 0; t < 1f; t += Time.deltaTime/animSpeed)
            {
                float xPos = Mathf.Lerp(startPoint.x, startPoint.x - radius_Orbit*2f, (Mathf.Sin(Mathf.PI*2f*t) + 1f)/2f);
                float zPos = Mathf.Lerp(startPoint.z, startPoint.z - radius_Orbit*2f, (Mathf.Sin(Mathf.PI*2f*t + Mathf.PI/2f) + 1f)/2f);
                animPlat_Target.localPosition = new Vector3(xPos, animPlat_Target.localPosition.y, zPos);
                //animPlat_Target.position = Vector3.Lerp(startPoint_Orbit, new Vector3(startPoint_Orbit.x, startPoint_Orbit.y, startPoint_Orbit.z - radius_Orbit*2f), Mathf.Sin(Mathf.PI*2f*t + Mathf.PI));
                yield return null;
            }
        }
        
        cr = null;
        yield return null;
    }

    // For rotating animations
    IEnumerator Animate_Rotate()
    {
        // First, we perform a delay based on the offset time
        yield return new WaitForSeconds((1f/animSpeed) * offset);

        while (isOn)
        {
            // Lerp rotate
            for (float t = 0; t < 1f; t += Time.deltaTime/animSpeed)
            {
                float rotationValue = (360f / rotationIndex) * Pick_F(t);
                Vector3 rotationAmount = new Vector3(rotationAxis.x * rotationValue, rotationAxis.y * rotationValue, rotationAxis.z * rotationValue);
                animPlat_Target.localRotation = Quaternion.Euler(rotationAmount);
                yield return null;
            }

            yield return null;
        }

        cr = null;
        yield return null;
    }

    IEnumerator Animate_Debug()
    {
        // Animation total time
        float totalAnimationTime = (1f/animSpeed) * (arrayOf_Points.Length - 1);

        while (isOn)
        {
            // Get the real world time that we'd like the objects to be positioned at
            float t_rw = (float)((rL_Clock.rW_Sec_D*animSpeed + totalAnimationTime*offset) % ((float)arrayOf_Points.Length - 1f));

            // For each leg of a path step, use a value between 0 and 1
            float t_Anim_1 = t_rw % 1f;

            // We select the leg of an animation using an integer of t (Floor)
            int t_Anim_2 = (int)Mathf.Floor(t_rw);

            // Position the animated object
            animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[t_Anim_2], arrayOf_Points[t_Anim_2 + 1], Pick_F(t_Anim_1));

            yield return null;
        }

        // Run a coroutine that reduces the t_Anim value back to zero
        while (t_Anim > 0f && !isOn)
        {
            Lerp_To_TAnim(0f);

            yield return null;
        }

        // Reset the main coroutine
        cr = null;

        yield return null;
    }

    // Just moves the pieces towards or away from it's state depending on where it's on or not
    IEnumerator Animate_Toggle()
    {
        int i = 1;
        if (isOn)
        {
            i = 0;
        }

        // Lerp towards the destination as dictate by it's on/off state
        for (float t = 0; t < 1f; t += Time.deltaTime/animSpeed)
        {
            animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[i], arrayOf_Points[i + 1], Pick_F(t));
            yield return null;
        }

        yield return null;

        // Check if we're in the off state and reset the object if needed
        if (i == 0 && !isOn)
        {
            cr = StartCoroutine(Animate_Toggle());
        }
    }

    IEnumerator Animate_Floaty()
    {
        // First, we perform a delay based on the offset time
        yield return new WaitForSeconds((1f/animSpeed) * offset);

        while (isOn)
        {
            // Lerp rotate
            for (float t = 0; t < 1f; t += Time.deltaTime/animSpeed)
            {
                float rotationValueX = maxAngle * Mathf.Sin(2f * Mathf.PI * Pick_F(t));
                float rotationValueY = maxAngle * Mathf.Sin(2f * Mathf.PI * Pick_F((t + 0.25f) % 1f));
                float rotationValueZ = maxAngle * Mathf.Sin(2f * Mathf.PI * Pick_F((t + 0.50f) % 1f));
                Vector3 rotationAmount = new Vector3(rotationAxis.x * rotationValueX, rotationAxis.y * rotationValueY, rotationAxis.z * rotationValueZ);
                animPlat_Target.localRotation = Quaternion.Euler(rotationAmount);
                yield return null;
            }

            yield return null;
        }

        yield return null;
    }

    // Just plays or resets an animation
    IEnumerator Animate_Anim(bool isOn)
    {
        anim = this.GetComponent<Animator>();

        if (isOn)
        {
            anim.SetBool("isOn", true);
        }
        else
        {
            anim.SetBool("isOn", false);
        }

        cr = null;
        yield return null;
    }

    IEnumerator Animate_Input()
    {
        // Here the object rotates based on user controller input
        // To begin with, grab the object's current orientation
        float lookup_RotationValue = 0f;
        if (rotationAxis.x != 0)
        {
            lookup_RotationValue = animPlat_Target.localEulerAngles.x;
        }
        else if (rotationAxis.y != 0)
        {
            lookup_RotationValue = animPlat_Target.localEulerAngles.y;
        }
        else if (rotationAxis.z != 0)
        {
            lookup_RotationValue = animPlat_Target.localEulerAngles.z;
        }

        float lookup_t = lookup_RotationValue * rotationIndex / 360f;

        float t = lookup_t;
        
        while (isOn)
        {
            t = (t + ((Time.deltaTime/animSpeed) * sI.cameraStick.x)) % 1f;
            float rotationValue = (360f / rotationIndex) * Pick_F(t);
            Vector3 rotationAmount = new Vector3(rotationValue * rotationAxis.x, rotationValue * rotationAxis.y, rotationValue * rotationAxis.z);
            animPlat_Target.localRotation = Quaternion.Euler(rotationAmount);
            yield return null;
        }

        cr = null;
        yield return null;
    }

    ///////////
    // AUDIO //
    ///////////

    void Update_SFX(float tA)
    {
        if (fmodEvent_String != "")
        {
            AC.Instance.Update_Parameter(fmodEvent, paramRef, tA);
        }
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    // This is the main function that moves t_Anim towards a suitable value
    // This also updates the animated platform position
    void Lerp_To_TAnim(float t_Temp)
    {
        // Take the normalized difference
        float difference = Mathf.Abs(t_Anim - t_Temp)/(arrayOf_Points.Length - 1);
        
        // Calculate a force value such that the largest force is applied when the difference is small
        // (Which includes differences close to 1 because those are technically small)
        float force = -((Mathf.Cos(difference*Mathf.PI*2f) - 1)/2f) * 0.99f;
        if (force > 0.5)
        {
            force = 1f - force;
        }

        // This is used to set how quickly we get to the right oscillation speed
        lockingSpeed = Mathf.Pow(Mathf.Exp(-difference), 4f)*50f;

        // I use the fixingRatio to fix that slight jump when the platform is very far from it's intended point
        if (difference < fixingRatio)
        {
            // Here is the main lerping equation
            // When the force is large the movement is slow
            // When the force is small, the movement rate is so high that we're technically just moving at the rate of t_Temp
            t_Anim = Mathf.Lerp(t_Anim, t_Temp, Mathf.Pow((1f - force), 3f) * lockingSpeed * Time.deltaTime);
        }
        else
        {
            t_Anim = Mathf.Lerp(t_Anim, t_Temp + (arrayOf_Points.Length - 1), Mathf.Pow((1f - force), 2f) * lockingSpeed * Time.deltaTime);
        }

        // Make sure the value doesn't explode
        t_Anim = (t_Anim + (arrayOf_Points.Length - 1)) % (arrayOf_Points.Length - 1);


        // Affix the values

        // For each leg of a path step, use a value between 0 and 1
        float t_Anim_1 = t_Anim % 1f;

        // We select the leg of an animation using an integer of t (Floor)
        int t_Anim_2 = (int)Mathf.Floor(t_Anim);

        // Position the animated object
        animPlat_Target.localPosition = Vector3.Lerp(arrayOf_Points[t_Anim_2], arrayOf_Points[t_Anim_2 + 1], Pick_F(t_Anim_1));
    }

    // Pass a value between 0 and 1 to moving between states based on chosen function
    float Pick_F(float t)
    {
        if (animFunct == AnimFunct.Sigmoid)
        {
            return f_Sigmoid(t);
        }
        else if (animFunct == AnimFunct.Sine)
        {
            return f_Sin(t);
        }
        else if (animFunct == AnimFunct.Linear)
        {
            return t;
        }
        else if (animFunct == AnimFunct.Ticking)
        {
            return f_Ticking(t);
        }
        else if (animFunct == AnimFunct.Lift)
        {
            return f_Lift(t);
        }

        return 0f;
    }

    float f_Sigmoid(float t)
    {
        float newT = (t * 2f - 1f) * sigmoidMultiplier;

        float val = 1f/(1 + (Mathf.Exp(-newT)));

        return val;
    }

    float f_Sigmoid_Half(float t)
    {
        float newT = (t - 1f) * sigmoidMultiplier;

        float val = 2f/(1 + (Mathf.Exp(-newT)));

        return val;
    }

    float f_Sigmoid_Half_Other(float t)
    {
        float newT = (t) * sigmoidMultiplier;

        float val = 2f/(1 + (Mathf.Exp(-newT))) - 1f;

        return val;
    }

    float f_Sigmoid_Norm(float t)
    {
        float newT = (t * 2f - 1f) * 1f;

        float val = 1f/(1 + (Mathf.Exp(-newT)));

        return val;
    }

    float f_Sin(float t)
    {
        float val = 1f - (Mathf.Cos(Mathf.PI * 2f * (t/2f)) + 1f)/2f;

        return val;
    }

    float f_Sin_Norm(float t)
    {
        float val = 1f - ((Mathf.Cos(Mathf.PI * 2f * t)) + 1f)/2f;

        return val;
    }

    float f_Ticking(float t)
    {
        float val = Mathf.Pow((Mathf.Cos(Mathf.PI * 2f * (t % 0.5f)) + 1f)/2f, 2f);

        return val;
    }

    // Used for lifts. Starts build up over 1 unit distance.
    // Then travels normally to it's destination -1.
    // Then repeat
    // This currently acts weird. Try again later
    // Maybe something like a hill function.
    float f_Lift(float t)
    {
        // 0 - full speed for the first 10%
        if (t >= 0f && t <= 0.1f)
        {
            return t * f_Sigmoid(t * 10f);
        }
        // Regular speed for the next 80%
        else if (t > 0.1f && t <= 0.9f)
        {
            return t;
        }
        // Sigmoid slow down for the last 10%
        else if (t > 0.9f && t <= 1f)
        {
            return t * f_Sigmoid((t * 10) - 9f);
        }

        return t;
    }
}
