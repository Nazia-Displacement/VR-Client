using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiPlayerAvatar : MonoBehaviour
{
    public GameObject mask;
    public GameObject[] maskGraphics;
    public GameObject textCube;
    public string sid;
    public ParticleSystem myParticleSystem;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private Quaternion targetMaskRot;
    private float duration = 0.05f; // Duration in seconds (50 milliseconds)
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion startMaskRotation;
    private float timeElapsed;
    private Color32 color = new();
    private Material lightMat;
    private Renderer textCub_r;
    private Renderer mask_r;

    private int currentMask = 0;

    void Start()
    {
        ResetStarts();
        targetPos = transform.position;
        targetRot = transform.rotation;
        targetMaskRot = mask.transform.localRotation;
        lightMat = myParticleSystem.GetComponent<ParticleSystemRenderer>().material;
        myParticleSystem.Clear();
        myParticleSystem.Play();
    }

    void Update()
    {
        //if(!ConnectionManager.instance.avatars.ContainsKey(sid)) { Destroy(this); }

        if (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;

            // Lerp position and rotation
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, t);
            mask.transform.localRotation = Quaternion.Lerp(mask.transform.localRotation, targetMaskRot, t);
        }
        else
        {
            // Snap to the target if the duration has passed
            transform.position = targetPos;
            transform.rotation = targetRot;
            mask.transform.localRotation = targetMaskRot;
        }

        ParticleSystem.MainModule m = myParticleSystem.main;
        m.startColor = new ParticleSystem.MinMaxGradient(color);
        lightMat.SetColor("_EmissionColor", color);

        if (mask_r != null && mask_r.material != null)
        {
            // Change the Face Color
            mask_r.material.SetColor("_BaseColor", color);
        }

        if (textCub_r != null && textCub_r.material != null)
        {
            // Change the Face Color
            textCub_r.material.SetColor("_FaceColor", color);
        }
    }

    public void SetTargets(Vector3 targetPos, float maskRot, float targetRot)
    {
        ResetStarts();
        this.targetPos = targetPos;
        this.targetRot = Quaternion.Euler(0, targetRot, 0);
        targetMaskRot = Quaternion.Euler(maskRot, 0, 0);
    }

    public void SetVisible(bool visible)
    {
        enabled = visible;
    }

    public void SetColor(byte r, byte g, byte b)
    {
        color.r = r; 
        color.g = g;
        color.b = b;
        color.a = 255;
    }

    public void SetMask(int maskIndex)
    {
        if (maskIndex < maskGraphics.Length && maskIndex >= 0 && currentMask != maskIndex)
        {
            for(int i = 0; i < maskGraphics.Length; i++)
            {
                maskGraphics[i].SetActive(false);
            }
            maskGraphics[maskIndex].SetActive(true);
            mask_r = maskGraphics[maskIndex].GetComponent<Renderer>();
            textCub_r = textCube.GetComponent<Renderer>();
        }
    }

    private void ResetStarts()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startMaskRotation = mask.transform.localRotation;
        timeElapsed = 0f;
    }
}