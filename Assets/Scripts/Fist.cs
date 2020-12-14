using System.Collections;
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
    int handIndex;

    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
    public SteamVR_Action_Boolean grabGripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");

    private const float zOffset = -0.07f;
    private const float maxLength = 1.3f;
    private float downLength = 0.0f;
    private float upLength = 0.0f;

    private bool grabPinchActive = false;
    private bool grabGripActive = false;

    public SphereCollider fistCollider;
    public BoxCollider swordCollider;
    public BoxCollider staffCollider;

    private SteamVR_Input_Sources inputSource = 0;
    private HandShape handShape = HandShape.Fist;

    const float defaultFistRadius = 0.2f;

    GameObject sphere = null;


    void OnDestroy() {
        grabPinchAction.RemoveOnStateDownListener(GrabPinchDown, inputSource);
        grabPinchAction.RemoveOnStateUpListener(GrabPinchUp, inputSource);
        grabGripAction.RemoveOnStateDownListener(GrabGripDown, inputSource);
        grabGripAction.RemoveOnStateUpListener(GrabGripUp, inputSource);
    }

    void Start() {
        sphere = transform.GetChild(0).gameObject;      // TODO: make better reference
        InitMesh(sphere);
        hand = GetComponentInParent<Hand>();
        handIndex = hand.handType == SteamVR_Input_Sources.LeftHand ? 0 : 1; 
        SteamVR_Input_Sources [] sources = { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
        inputSource = sources[handIndex];

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
