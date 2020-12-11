using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Orb : MonoBehaviour
{
    public GameObject[] colliders;
    public ParticleSystem hitFX;
    public SteamVR_Action_Vibration haptic;

    private Vector3 origin;
    private Mesh mesh;

    private float sparkIntensity = 0.0f;
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

        SteamVR_Input_Sources[] sources = { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
        for (int i = 0; i < 2; ++i) {
            GameObject o = colliders[i];
            float distance = Vector3.Distance(o.transform.position, gameObject.transform.position);
            if (!collideFlag[i]) {
                // collide check
                float r = gameObject.transform.localScale.x * 0.5f;
                if (distance < r) {
                    sparkIntensity = 1.0f;
                    collideFlag[i] = true;
                    Vibrate(0.7f, sources[i]);
                    hitFX.Emit(30);
                }
            } else {
                // un-collide check
                float r = gameObject.transform.localScale.x * 0.5f;
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

    }
    
}
