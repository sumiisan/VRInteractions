﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;


public enum HandShape {
    Fist,
    Sword,
    Staff,
    SwordStaff,
    Unknown,
}

public struct SmashInfo {
    public Fist fist;
    public bool smashing;
    public int handIndex;
    public HandShape shape;
    public Vector3 posiion;
    public Quaternion rotation;
    public Vector3 smashVelocity;
    public Vector3 sliceVelocity;
}

interface ISmashable {
    void SmashHit(SmashInfo info);
}

public class Fist : VertexColored {
    Hand hand;
    public int handIndex;
    public float moveSensitivity = 1.5f;

    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
    public SteamVR_Action_Boolean grabGripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
    public HandShape handShape = HandShape.Fist;

    private const float zOffset = -0.07f;
    private const float maxLength = 1.3f;
    private float downLength = 0.0f;
    private float upLength = 0.0f;

    private bool grabPinchActive = false;
    private bool grabGripActive = false;
    
    public float [] verticalPowerCharge  = { 0.0f, 0.0f };
    private Rigidbody body;

    public SphereCollider fistCollider;
    public BoxCollider swordCollider;
    public BoxCollider staffCollider;

    private SteamVR_Input_Sources inputSource = 0;

    const float defaultFistRadius = 0.2f;

    GameObject sphere = null;


    void OnDestroy() {
        grabPinchAction.RemoveOnStateDownListener(GrabPinchDown, inputSource);
        grabPinchAction.RemoveOnStateUpListener(GrabPinchUp, inputSource);
        grabGripAction.RemoveOnStateDownListener(GrabGripDown, inputSource);
        grabGripAction.RemoveOnStateUpListener(GrabGripUp, inputSource);
    }

    void Start() {
        // fill references
        sphere = transform.GetChild(0).gameObject;      // TODO: make better reference
        InitMesh(sphere);
        hand = GetComponentInParent<Hand>();
        handIndex = hand.handType == SteamVR_Input_Sources.LeftHand ? 0 : 1; 
        SteamVR_Input_Sources [] sources = { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
        inputSource = sources[handIndex];

        GameObject steamVRObject = hand.transform.parent.gameObject;
        GameObject bodyObject = steamVRObject.transform.Find("BodyCollider").gameObject;
        body = bodyObject.GetComponent<Rigidbody>();

        // init action listeners
        grabPinchAction.AddOnStateDownListener(GrabPinchDown, inputSource);
        grabPinchAction.AddOnStateUpListener(GrabPinchUp, inputSource);
        grabGripAction.AddOnStateDownListener(GrabGripDown, inputSource);
        grabGripAction.AddOnStateUpListener(GrabGripUp, inputSource);

        switch(handIndex) {
        case 0:
            SetVertexColor(new Color(1.0f, 0.1f, 0.3f));
            break;
        case 1:
            SetVertexColor(new Color(0.1f, 0.3f, 1.0f));
            break;
        }
    }


    void Update() {
        Vector3 velocity = hand.GetTrackedObjectVelocity();
        float magnitude = velocity.magnitude;

        ModifyShape();
        ChargeVerticalPower(velocity, magnitude);
        ApplyMovingPower(velocity, magnitude);
    }

    private void GrabPinchDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        grabPinchActive = true;
    }
    private void GrabPinchUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        grabPinchActive = false;
    }
    private void GrabGripDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        grabGripActive = true;
    }
    private void GrabGripUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        grabGripActive = false;
    }

    public void ResetVerticalPowerCharge() {
        verticalPowerCharge[0] = 0.0f;
        verticalPowerCharge[1] = 0.0f;
    }

    void ChargeVerticalPower(Vector3 velocity, float magnitude) {
        float exponentialFadeFactor = Mathf.Clamp(1.0f - Time.deltaTime * 8.0f, 0.0f, 1.0f);
        verticalPowerCharge[0] *= exponentialFadeFactor;
        verticalPowerCharge[1] *= exponentialFadeFactor;

        if (!grabGripActive) 
            return;

        verticalPowerCharge[velocity.y < 0 ? 0 : 1] += (Mathf.Abs(velocity.y) + magnitude) * moveSensitivity * Time.deltaTime;
        if(handIndex == 0) {
            HUD.shared.SetProp("VPC", $"vel:{velocity.y:N2},mag:{magnitude:N2} \n({verticalPowerCharge[0]:N2},{verticalPowerCharge[1]:N2})");
        }
    }

    void ApplyMovingPower(Vector3 velocity, float magnitude) {
        if (!grabGripActive) 
            return;

        float vx = velocity.x;
        float sx = Mathf.Sign(vx);
        float vz = velocity.z;
        float sz = Mathf.Sign(vz);

//        Vector3 moveVector = new Vector3(vx, 0.0f, vz);
        Vector3 moveVector = new Vector3(
            vx * vx * sx * 0.05f,
            0.0f,
            vz * vz * sz * 0.05f
        );

        MoveIndicator.shared.Trigger(this, moveVector);
    }

    void ModifyShape() {
        float morphSpeed = 10.0f * Time.deltaTime;
        upLength   += grabPinchActive ? morphSpeed : -morphSpeed;     
        downLength += grabGripActive  ? morphSpeed : -morphSpeed;

        upLength   = Mathf.Clamp(upLength,   0.0f, maxLength); 
        downLength = Mathf.Clamp(downLength, 0.0f, maxLength); 

        HandShape lhs = handShape;

        if (upLength > 0.3f) {
            if (downLength > 0.3f) {
                handShape = HandShape.SwordStaff;
            } else {
                handShape = HandShape.Sword;
            } 
        } else {
            if (downLength > 0.3f) {
                handShape = HandShape.Staff;   
            } else {
                handShape = HandShape.Fist;
            }
        }

        if (lhs != handShape) {
            // hand shape has changed:
            fistCollider.enabled = handShape == HandShape.Fist;
            swordCollider.enabled = handShape == HandShape.Sword || handShape == HandShape.SwordStaff;
            staffCollider.enabled = handShape == HandShape.Staff || handShape == HandShape.SwordStaff;
        }

        SetDimension();
    }

    void SetDimension() {
        float zOfs = zOffset;
        float len = defaultFistRadius;  

        // add downward
        len += downLength;
        zOfs -= downLength / 2.0f;
    
        // add upward

        len += upLength;
        zOfs += upLength / 2.0f;

        float ratio = Mathf.Clamp((len - defaultFistRadius) / (maxLength * 1.0f), 0.0f, 1.0f);
        float thickness = defaultFistRadius * (1.0f - ratio * (1.0f - defaultFistRadius) );

        sphere.transform.localScale = new Vector3(thickness, thickness, len);
        sphere.transform.localPosition = new Vector3(0.0f, 0.0f, zOfs);
    }

    ISmashable GetFirstSmashableFromCollider(Collider coll) {
        MonoBehaviour[] list = coll.gameObject.GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour mb in list)
            if (mb is ISmashable) 
                return mb as ISmashable;
        return null;
    }

    void OnTriggerEnter(Collider triggerCollider) {
        ISmashable smashable = GetFirstSmashableFromCollider(triggerCollider);
        if (smashable == null) 
            return;

        Vector3 smashVelocity, sliceVelocity;
        hand.GetEstimatedPeakVelocities(out smashVelocity, out sliceVelocity);

        SmashInfo info = new SmashInfo {
            fist = this,
            smashing = true, 
            handIndex = handIndex,
            shape = handShape,
            posiion = transform.position,   // for fist smashing, use fist position
            rotation = transform.rotation,   // for staff slice, use rotation 
            smashVelocity = smashVelocity,
            sliceVelocity = sliceVelocity
        };

        smashable.SmashHit(info);
    }

    void OnTriggerExit(Collider triggerCollider) {
        ISmashable smashable = GetFirstSmashableFromCollider(triggerCollider);
        if (smashable == null) 
            return;

        SmashInfo info = new SmashInfo {
            fist = this,
            smashing = false, 
            handIndex = handIndex,
            shape = handShape,
            posiion = transform.position,   // for fist smashing, use fist position
            rotation = transform.rotation,   // for staff slice, use rotation 
            smashVelocity = Vector3.zero,
            sliceVelocity = Vector3.zero
        };

        smashable.SmashHit(info);
    }
}
