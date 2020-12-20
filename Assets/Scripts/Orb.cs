using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Orb : VertexColored, ISmashable 
{
    public Hand leftHand;
    public Hand rightHand;

    public Transform body;
    public ParticleSystem hitFX;
    public SteamVR_Action_Vibration haptic;
    public AudioClip [] sfx; 

    private AudioSource audioSource;

    private Vector3 origin;
    private float sparkIntensity = 0.0f;
    private float[] collideDist = {0.0f, 0.0f};
    private Quaternion[] collideRotation = {Quaternion.identity, Quaternion.identity};
    private bool[] collideFlag = {false, false};
    private bool colliding;
    private SmashInfo[] lastSmashInfo = {
        new SmashInfo { shape = HandShape.Fist }, 
        new SmashInfo { shape = HandShape.Fist }
    };

    void Start() {
        InitMesh(gameObject);
        origin = transform.position;
        audioSource = GetComponent<AudioSource>();
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

        float mag, strength = 0.0f;  

        if (colliding && collideFlag[info.handIndex] == false) {
            // collide 
            ParticleSystem.MainModule main = hitFX.main;
            collideFlag[info.handIndex] = true;

            switch (info.shape) {
            case HandShape.Fist:
                mag = info.smashVelocity.magnitude * 0.3f;
                strength = Mathf.Clamp( (mag * mag + mag), 0.0f, 1.0f );

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
                collideDist[info.handIndex] = Vector3.Distance(bodyCenter, info.posiion - move);
                collideRotation[info.handIndex] = info.rotation;

                break;

            case HandShape.Staff:
            case HandShape.Sword:
            case HandShape.SwordStaff:
                mag = info.sliceVelocity.magnitude * 0.01f;
                strength = Mathf.Clamp( (mag * mag + mag), 0.0f, 1.0f );
                collideDist[info.handIndex] = Vector3.Distance(bodyCenter, info.posiion);
                collideRotation[info.handIndex] = info.rotation;

                sparkIntensity = strength * 1.0f;
                Vibrate(strength, sources[info.handIndex]);
                main = hitFX.main;
                main.startSpeed = strength;
                hitFX.Emit( (int)(strength * 30.0f) );
                break;
            }

            // sound:
            int sfxIndex = (int)((float)(sfx.Length - 1) * Mathf.Pow(strength, 2.0f));
            print(sfxIndex);
            AudioClip clip = sfx[sfxIndex];
            audioSource.PlayOneShot(clip);

        }

        lastSmashInfo[info.handIndex] = info;


    }

    int CheckUncollide (SmashInfo info) {
        // check if shape has changed
        if (info.fist.handShape != info.shape) 
            return 1;

        // check if fist position moved backwards
        Vector3 bodyCenter = new Vector3(
            body.position.x,
            body.position.y + 1.3f, // assumed shoulder height
            body.position.z); 
        if (Vector3.Distance(bodyCenter, info.fist.transform.position) < collideDist[info.handIndex]) 
            return 2;

        // check if rotation did change enough 
        if (Mathf.Abs(Quaternion.Angle(info.fist.transform.rotation, collideRotation[info.handIndex])) > 30.0f) 
            return 3;

        return 0;
    }

    void Update() {

        for (int handIndex = 0; handIndex < 2; ++handIndex) {
            if (collideFlag[handIndex]) {

                SmashInfo info = lastSmashInfo[handIndex];
                int uncollide = CheckUncollide(info);
                
                if (uncollide > 0) {
                    collideFlag[info.handIndex] = false;
                }
            }
        }

        Color col;
        bool collisionDebug = false;
        if (collisionDebug) {
            col = new Color(
                collideFlag[0] ? 1.0f : 0.0f,
                collideFlag[1] ? 1.0f : 0.0f,
                0.5f
            );
        } else {
            float si = sparkIntensity * 0.9f + 0.1f;  
            si *= si;
            col = new Color(si,si,si,1.0f);
        }

        SetVertexColor(col);

        sparkIntensity = Mathf.Clamp(sparkIntensity - 1.5f * Time.deltaTime, 0.0f, 2.0f );

        Vector3 posDelta = origin - transform.position;
        transform.position += posDelta * 2.5f * Time.deltaTime;
    }
    
}
