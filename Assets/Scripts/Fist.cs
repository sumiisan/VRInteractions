using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

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

        if ( grabPinchActive && upLength < maxLength) {
            upLength += morphSpeed; 
        } else if ( upLength > 0.0f) {
            upLength -= morphSpeed;
            if (upLength < 0.0f)
             upLength = 0.0f;
        }  

        if ( grabGripActive && downLength < maxLength) {
            downLength += morphSpeed;
        } else if ( downLength > 0.0f) {
            downLength -= morphSpeed;
            if (downLength < 0.0f)
             downLength = 0.0f;
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
        float thickness = defaultFistRadius * (1.0f - ratio * (1.0f - defaultFistRadius)));

        sphere.transform.localScale = new Vector3(thickness, thickness, len);
        sphere.transform.localPosition = new Vector3(0.0f, 0.0f, zOfs);
    }


}
