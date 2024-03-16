using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ProximityBeacon : MonoBehaviour
{
    public GameObject beaconGFX;
    public Light beaconLight;

    private Vector3 startPos;
    private float hoverOffset;
    private float inc = Mathf.PI * 3 / 1.99f;

    private float rateLimit = 0.2f;
    private float next = 1f;
    private float lastValue = 0;

    private List<GameObject> interactingControllers = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        startPos = beaconGFX.transform.localPosition;
        hoverOffset = 0.20f;
    }

    // Update is called once per frame
    void Update()
    {
        if (next <= rateLimit) next += rateLimit;

        float hoverPerc = (float)(Mathf.Sin(inc += Time.deltaTime) / 2.0 + 0.5);
        beaconGFX.transform.localPosition = startPos + Vector3.up * (hoverOffset * hoverPerc);
        if (inc > Mathf.PI*3) inc = Mathf.PI;

        if (interactingControllers.Count > 0 && next >= rateLimit)
        {
            float totalDist = 0;
            foreach (GameObject controller in interactingControllers)
            {
                totalDist += Vector3.Distance(beaconGFX.transform.position, controller.transform.position);
            }
            float avgDist = totalDist / interactingControllers.Count;
            float maxDist = 1.1f;
            float value = Mathf.Max(maxDist - avgDist, 0) / maxDist * 127;
            beaconLight.intensity = value;
            ConnectionManager.instance.UpdatePanel(value);
            lastValue = value;
            next = 0;
        } 
        else if (lastValue != 0)
        {
            beaconLight.intensity = 0;
            ConnectionManager.instance.UpdatePanel(0);
            lastValue = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter: " + other.tag);
        if (other.CompareTag("Player"))
            interactingControllers.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit");
        if (other.CompareTag("Player") && interactingControllers.Contains(other.gameObject))
            interactingControllers.Remove(other.gameObject);
    }

}
