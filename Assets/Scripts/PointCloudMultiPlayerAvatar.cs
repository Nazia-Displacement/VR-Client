using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PointCloudMultiPlayerAvatar : MonoBehaviour
{
    public GameObject head;
    public GameObject[] maskGraphics;
    public string sid;
    public ParticleSystem myParticleSystem;
    public VisualEffect myVisualEffect;
    public Animator myAnimator;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private Quaternion targetMaskRot;
    private float duration = 0.15f; // Duration in seconds (150 milliseconds)
    private float timeElapsed;
    [ColorUsage(true,true)]
    private Color32 color = new();
    private Material lightMat;
    private int currentMask = 0;

    void Start()
    {
        ResetStarts();
        targetPos = transform.position;
        targetRot = transform.rotation;
        targetMaskRot = head.transform.localRotation;
        lightMat = myParticleSystem.GetComponent<ParticleSystemRenderer>().material;
        myParticleSystem.Clear();
        myParticleSystem.Play();
    }

    void Update()
    {
        //if(!ConnectionManager.instance.avatars.ContainsKey(sid)) { Destroy(this); }

        if (timeElapsed <= duration && 
            (Vector3.Distance(targetPos, transform.position) > 0.0001f ||
            Vector3.Distance(targetRot.eulerAngles, transform.rotation.eulerAngles) > 0.0001f ||
            Vector3.Distance(targetMaskRot.eulerAngles, head.transform.localEulerAngles) > 0.0001f))
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;

            // Lerp position and rotation
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, t);
            head.transform.localRotation = Quaternion.Lerp(head.transform.localRotation, targetMaskRot, t);

            if(Vector3.Distance(targetPos, transform.position) <= 0.0001f) SetMovementAnimation(-1);
        }
        else
        {
            // Snap to the target if the duration has passed or we've reached all targets
            transform.position = targetPos;
            transform.rotation = targetRot;
            head.transform.localRotation = targetMaskRot;
            SetMovementAnimation(-1);
        }

        ParticleSystem.MainModule m = myParticleSystem.main;
        m.startColor = new ParticleSystem.MinMaxGradient(color);
        lightMat.SetColor("_EmissionColor", color);
        myVisualEffect.SetVector4("ParticleColor", new Vector4(color.r / 255f, color.g / 255f, color.b / 255f, 1.0f));
    }

    private void SetMovementAnimation(float deg)
    {
        for (int i = 0; i < myAnimator.parameterCount; i++)
            myAnimator.SetBool(myAnimator.parameters[i].name, false);
             
        if (deg >= 0 && (deg <= 67.5  && deg >  22.5)) myAnimator.SetBool("LForward", true);
        else if (deg >= 0 && (deg <= 112.5 && deg >  67.5)) myAnimator.SetBool("Left", true);
        else if (deg >= 0 && (deg <= 157.5 && deg > 112.5)) myAnimator.SetBool("LBackward", true);
        else if (deg >= 0 && (deg <= 202.5 && deg > 157.5)) myAnimator.SetBool("Backward", true);
        else if (deg >= 0 && (deg <= 247.5 && deg > 202.5)) myAnimator.SetBool("RBackward", true);
        else if (deg >= 0 && (deg <= 292.5 && deg > 247.5)) myAnimator.SetBool("Right", true);
        else if (deg >= 0 && (deg <= 337.5 && deg > 292.5)) myAnimator.SetBool("RForward", true);
        else if (deg >= 0 && (deg <= 22.5 || deg > 337.5)) myAnimator.SetBool("Forward", true);
        else myAnimator.SetBool("Idle", true);
    }

    public void SetTargets(Vector3 targetPos, float maskRot, float targetRot)
    {
        ResetStarts();
        this.targetPos = targetPos;
        this.targetRot = Quaternion.Euler(0, targetRot, 0);
        maskRot = (maskRot > 100) ? maskRot - 360 : maskRot;
        targetMaskRot = Quaternion.Euler(Mathf.Clamp(maskRot, -30, 30), 0, 0);
        if(targetPos != transform.position)
            SetMovementAnimation((Vector3.SignedAngle(transform.forward, (targetPos - transform.position).normalized, Vector3.up) + 360) % 360);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
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
            for (int i = 0; i < maskGraphics.Length; i++)
            {
                maskGraphics[i].SetActive(false);
            }
            //maskGraphics[maskIndex].SetActive(true);
        }
    }

    private void ResetStarts()
    {
        timeElapsed = 0f;
    }
}
