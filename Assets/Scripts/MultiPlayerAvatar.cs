using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiPlayerAvatar : MonoBehaviour
{
    public GameObject mask;
    public string sid;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private Quaternion targetMaskRot;
    private float duration = 0.05f; // Duration in seconds (50 milliseconds)
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion startMaskRotation;
    private float timeElapsed;

    void Start()
    {
        ResetStarts();
        targetPos = transform.position;
        targetRot = transform.rotation;
        targetMaskRot = mask.transform.localRotation;
    }

    void Update()
    {
        //if(!ConnectionManager.instance.avatars.ContainsKey(sid)) { Destroy(this); }

        if (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;

            // Lerp position and rotation
            transform.position = Vector3.Lerp(startPosition, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRotation, targetRot, t);
            mask.transform.localRotation = Quaternion.Lerp(startMaskRotation, targetMaskRot, t);
        }
        else
        {
            // Snap to the target if the duration has passed
            transform.position = targetPos;
            transform.rotation = targetRot;
            mask.transform.localRotation = targetMaskRot;
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

    private void ResetStarts()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startMaskRotation = mask.transform.localRotation;
        timeElapsed = 0f;
    }
}
