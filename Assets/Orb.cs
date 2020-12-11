using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Orb : MonoBehaviour
{
    public Hand leftHand;
    public Hand rightHand;
    public ParticleSystem hitFX;
    public SteamVR_Action_Vibration haptic;

    private Vector3 origin;
    private Mesh mesh;

    private float sparkIntensity = 0.0f;
    private float[] cooldownTime = {0.0f, 0.0f};
    private bool[] collideFlag = {false, false};

    // Start is called before the first frame update
    void Start() {
        origin = gameObject.transform.position;
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void SetVertexColor(Color col) {
        Vector3[] vertices = mesh.vertices;

        // create new colors array where the colors will be created.
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = col;

        // assign the array of colors to the Mesh.
        mesh.colors = colors;
    }

    void Vibrate(float strength, SteamVR_Input_Sources source) {
        haptic.Execute(0.0f, 0.05f, 100f, strength, source);
    }

    // Update is called once per frame
    void Update() {
        Hand [] hands = { leftHand, rightHand }; 
        SteamVR_Input_Sources[] sources = { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };

        Vector3 velocity;
        Vector3 angularVelocity;

        for (int i = 0; i < 2; ++i) {
            float distance = Vector3.Distance(hands[i].transform.position, transform.position);

            if (!collideFlag[i]) {
                // collide check
                float r = transform.localScale.x * 0.5f;
                if (distance < r && cooldownTime[i] < Time.time) {

                    hands[i].GetEstimatedPeakVelocities(out velocity, out angularVelocity);
                    float mag = velocity.magnitude;
                    float strength = Mathf.Clamp( (mag * mag + mag) * 0.3f, 0.0f, 1.0f );
                    cooldownTime[i] = Time.time + (0.6f - strength * 0.3f);

                    sparkIntensity = strength * 1.0f;
                    collideFlag[i] = true;
                    Vibrate(strength, sources[i]);
                    hitFX.startSpeed = strength;
                    hitFX.Emit( (int)(strength * 30.0f) );

                    Vector3 move = new Vector3(
                        0.1f  * velocity.x, 
                        0.03f * velocity.y,  // don't move horizontal so much
                        0.1f  * velocity.z
                        ); 
                    
                    transform.position += move;
                }
            } else {
                // un-collide check
                float r = transform.localScale.x * 0.5f;
                if (distance > r) {
                    collideFlag[i] = false;
                 }
            }
        }

        float si = sparkIntensity * 0.9f + 0.1f;  
        si *= si;
        Color col = new Color(si,si,si,1.0f);
        SetVertexColor(col);

        sparkIntensity = Mathf.Clamp(sparkIntensity - 1.5f * Time.deltaTime, 0.0f, 2.0f );

        Vector3 posDelta = origin - transform.position;
        transform.position += posDelta * 2.5f * Time.deltaTime;
    }
    
}
