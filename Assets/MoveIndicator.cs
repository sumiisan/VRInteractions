using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class MoveIndicator : MonoBehaviour {
    public static MoveIndicator shared;
    private MeshRenderer indicatorRenderer;
    private Vector3 moveVelocity; 
    private Vector3 [] moveForce = {new Vector3(), new Vector3()};
    private Fist [] fists = { null, null };
  
    // Start is called before the first frame update
    void Start() {
        shared = this;
        indicatorRenderer = GetComponent<MeshRenderer>();
        indicatorRenderer.enabled = false;
    }

    private float ProductVertPC() {
        if (fists[0] == null)
            return 0.0f;
        if (fists[1] == null)
            return 0.0f;
        return fists[0].verticalPowerCharge * fists[1].verticalPowerCharge;
    }

    public void Trigger(Fist fist, Vector3 force) {
        fists[fist.handIndex] = fist;

        if (indicatorRenderer.enabled)
            return;

        if (force.magnitude == 0) {
//            HUD.shared.SetProp("s","NULL FORCE!");
        } else {
            moveForce[fist.handIndex] = force;
        }

//        HUD.shared.SetProp("F" + handIndex, $"({vx:N2},{vz:N2})");
/*
        HUD.shared.SetProp("F0", $"({moveForce[0].x:N2},{moveForce[0].z:N2}) {gameObject.GetInstanceID()}");
        HUD.shared.SetProp("F1", $"({moveForce[1].x:N2},{moveForce[1].z:N2}) {gameObject.GetInstanceID()}");
*/
        float pvpc = ProductVertPC();

        if (HUD.shared == null) {
            Debug.Log("HUD not inited!");
        } else {
            HUD.shared.SetProp("VertPC", pvpc.ToString());
        }

        bool moveToHeadDirection = true;

        if (pvpc > 1.0f) {
            indicatorRenderer.enabled = true;

            fists[0].verticalPowerCharge = 0.0f;
            fists[1].verticalPowerCharge = 0.0f;

            moveVelocity = new Vector3(
                moveForce[0].x + moveForce[1].x,
                moveForce[0].y + moveForce[1].y,
                moveForce[0].z + moveForce[1].z
            );

            if (moveToHeadDirection) {
                moveVelocity = Player.instance.hmdTransforms[0].forward * moveVelocity.magnitude;
                moveVelocity.y = 0.0f;
            } 

            transform.position = Player.instance.hmdTransforms[0].position + new Vector3(0.0f, -0.3f, 0.0f);
            HUD.shared.SetProp("MI", "TRIGGERED");
        } else {
            HUD.shared.SetProp("MI", "WEAK");
        }

    }

    // Update is called once per frame
    void Update() {
        transform.position += moveVelocity;

        float exponentialFadeFactor = Mathf.Clamp(1.0f - Time.deltaTime * 8.0f, 0.0f, 1.0f); 
        moveVelocity *= exponentialFadeFactor;

        if (moveVelocity.magnitude < 0.01f) {
            indicatorRenderer.enabled = false;
            HUD.shared.SetProp("MI", "-");
        } else if (indicatorRenderer.enabled) {
            HUD.shared.SetProp("MI", "MAG:" + moveVelocity.magnitude);
        }
    }
}
