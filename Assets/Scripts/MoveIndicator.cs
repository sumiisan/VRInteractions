using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class MoveIndicator : MonoBehaviour {
    public static MoveIndicator shared;
    private MeshRenderer indicatorRenderer;
    private AudioSource audioSource;
    private Vector3 moveVelocity; 
    private Vector3 [] moveForce = {new Vector3(), new Vector3()};
    private Fist [] fists = { null, null };
  
    // Start is called before the first frame update
    void Start() {
        shared = this;
        indicatorRenderer = GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        indicatorRenderer.enabled = false;
    }

    private float ProductVertPC() {
        if (fists[0] == null)
            return 0.0f;
        if (fists[1] == null)
            return 0.0f;

        float vpc0 = fists[0].verticalPowerCharge[0] * fists[1].verticalPowerCharge[0];
        float vpc1 = fists[0].verticalPowerCharge[1] * fists[1].verticalPowerCharge[1];

        return vpc0 > vpc1 ? vpc0 : vpc1 * -1.0f;
    }

    public void Trigger(Fist fist, Vector3 force) {
        fists[fist.handIndex] = fist;

        float pvpc = ProductVertPC();
        HUD.shared.SetProp("VertPC", pvpc.ToString());

        if (indicatorRenderer.enabled)
            return;

        moveForce[fist.handIndex] = force;

        bool moveToHeadDirection = true;

        if (Mathf.Abs(pvpc) > 1.0f) {
            //triggered

            indicatorRenderer.enabled = true;

            fists[0].ResetVerticalPowerCharge();
            fists[1].ResetVerticalPowerCharge();

//            float sign = Mathf.Sign(pvpc);

            moveVelocity = new Vector3(
                (moveForce[0].x + moveForce[1].x) * pvpc * 2.0f,
                moveForce[0].y + moveForce[1].y,
                (moveForce[0].z + moveForce[1].z) * pvpc * 2.0f
            );

            if (moveToHeadDirection) {
                moveVelocity = Player.instance.hmdTransforms[0].forward * moveVelocity.magnitude * pvpc * 2.0f;
                moveVelocity.y = 0.0f;
            } 

            transform.position = Player.instance.hmdTransforms[0].position + new Vector3(0.0f, -0.3f, 0.0f);
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    // Update is called once per frame
    void Update() {
        transform.position += moveVelocity;

        float exponentialFadeFactor = Mathf.Clamp(1.0f - Time.deltaTime * 8.0f, 0.0f, 1.0f); 
        moveVelocity *= exponentialFadeFactor;

        if (moveVelocity.magnitude < 0.01f) {
            // position determined
            indicatorRenderer.enabled = false;

            // do actual avatar move here:
        }
    }
}
