using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Orb : VertexColored, ISmashable 
{
    public Hand leftHand;
    public Hand rightHand;

    public Transform body;
    public ParticleSystem hitFX;
    public SteamVR_Action_Vibration haptic;

    private Vector3 origin;
    private float sparkIntensity = 0.0f;
    private float[] collideDist = {0.0f, 0.0f};
    private Quaternion[] collideRotation = {Quaternion.identity, Quaternion.identity};
    private bool[] collideFlag = {false, false};
    private bool colliding;
    private SmashInfo lastSmashInfo = new SmashInfo { shape = HandShape.Unknown };

    void Start() {
        InitMesh(gameObject);
        origin = transform.position;
    }

    void Vibrate(float strength, SteamVR_Input_Sources source) {
        haptic.Execute(0.0f, 0.05f, 100f, strength, source);
    }

    public void SmashHit(SmashInfo info) {
        colliding = info.smashing;
        SteamVR_Input_Sources[] sources = { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };

        Vector3 bodyCenter = new Vector3(
             body.position.x,
             body.position.y + 1.3f, // assumed shoulder height
             body.position.z); 

        float mag, strength;     

        if (colliding) {
            // collide 
            ParticleSystem.MainModule main = hitFX.main;
            collideFlag[info.handIndex] = true;

            switch (info.shape) {
            case HandShape.Fist:
                mag = info.smashVelocity.magnitude;
                strength = Mathf.Clamp( (mag * mag + mag) * 0.3f, 0.0f, 1.0f );
                collideDist[info.handIndex] = Vector3.Distance(bodyCenter, info.posiion);

                sparkIntensity = strength * 1.0f;
                Vibrate(strength, sources[info.handIndex]);
                main.startSpeed = strength;
                hitFX.Emit( (int)(strength * 30.0f) );

                Vector3 move = new Vector3(
                    0.1f  * info.smashVelocity.x, 
                    0.05f * info.smashVelocity.y,  // don't move horizontal so much
                    0.1f  * info.smashVelocity.z
                    ); 
                transform.position += move;

                break;

            case HandShape.Staff:
            case HandShape.Sword:
            case HandShape.SwordStaff:
                mag = info.sliceVelocity.magnitude * 0.5f;
                strength = Mathf.Clamp( (mag * mag + mag) * 0.3f, 0.0f, 1.0f );
                collideRotation[info.handIndex] = info.rotation;

                sparkIntensity = strength * 1.0f;
                Vibrate(strength, sources[info.handIndex]);
                main = hitFX.main;
                main.startSpeed = strength;
                hitFX.Emit( (int)(strength * 30.0f) );
                break;
            }
        } else {
            //  check un-collide

            bool uncollide = false; // (info.shape != lastSmashInfo.shape);

            switch(info.shape) {
            case HandShape.Fist:
                if (Vector3.Distance(bodyCenter, info.posiion) < collideDist[info.handIndex]) {
                    uncollide = true;
                }
                break;
            case HandShape.Staff:
            case HandShape.Sword:
            case HandShape.SwordStaff:
                if (Quaternion.Angle(info.rotation, collideRotation[info.handIndex]) > 45.0f) {
                    uncollide = true;
                }
                break;
            }

            if (uncollide) {
                collideFlag[info.handIndex] = true;
            }

        }

        lastSmashInfo = info;
    }

    void Update() {
        float si = sparkIntensity * 0.9f + 0.1f;  
        si *= si;
        Color col = new Color(si,si,si,1.0f);
        SetVertexColor(col);

        sparkIntensity = Mathf.Clamp(sparkIntensity - 1.5f * Time.deltaTime, 0.0f, 2.0f );

        Vector3 posDelta = origin - transform.position;
        transform.position += posDelta * 2.5f * Time.deltaTime;
    }
    
}
