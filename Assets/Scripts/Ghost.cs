using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public GameObject head;
    public GameObject lShlouder;
    public GameObject rShlouder;
    public GameObject lElbow;
    public GameObject rElbow;
    public GameObject lWrist;
    public GameObject rWrist;
    public GameObject lHip;
    public GameObject rHip;
    public GameObject lKnee;
    public GameObject rKnee;
    public GameObject lAnkle;
    public GameObject rAnkle;

    private float movementArea = 2f;

    public class MovementData
    {
        public Vector3 targetPosition;
        public Coroutine movement;
    }

    public Dictionary<GameObject, MovementData> keyPoints;

    // Start is called before the first frame update
    void Start()
    {
        keyPoints = new Dictionary<GameObject, MovementData>
        {
            { head, new MovementData { targetPosition = head.transform.position } },
            { lShlouder, new MovementData { targetPosition = lShlouder.transform.position } },
            { rShlouder, new MovementData { targetPosition = rShlouder.transform.position } },
            { lElbow, new MovementData { targetPosition = lElbow.transform.position } },
            { rElbow, new MovementData { targetPosition = rElbow.transform.position } },
            { lWrist, new MovementData { targetPosition = lWrist.transform.position } },
            { rWrist, new MovementData { targetPosition = rWrist.transform.position } },
            { lHip, new MovementData { targetPosition = lHip.transform.position } },
            { rHip, new MovementData { targetPosition = rHip.transform.position } },
            { lKnee, new MovementData { targetPosition = lKnee.transform.position } },
            { rKnee, new MovementData { targetPosition = rKnee.transform.position } },
            { lAnkle, new MovementData { targetPosition = lAnkle.transform.position } },
            { rAnkle, new MovementData { targetPosition = rAnkle.transform.position } }
        };
    }

    public IEnumerator LerpFromTo(Vector3 start, Vector3 end, float time, GameObject obj)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            obj.transform.position = Vector3.Lerp(start, end, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        obj.transform.position = end; // Ensure the final value is set to end
    }

    public void UpdateKeypoints(List<double>[] targets, List<int>[] colors)
    {
        int idx = 0;
        foreach (KeyValuePair<GameObject, MovementData> item in keyPoints)
        {
            if (item.Value.movement != null)
                StopCoroutine(item.Value.movement);
            if (targets[idx][0] != 0 || targets[idx][1] != 0) {
                item.Value.targetPosition = new Vector3(movementArea * ((float)targets[idx][0] - 0.5f) + transform.position.x, movementArea * (1 - (float)targets[idx][1]) + transform.position.y, transform.position.z);
                if (item.Key.activeInHierarchy == false)
                {
                    item.Key.SetActive(true);
                    item.Key.transform.position = item.Value.targetPosition;
                }
                item.Value.movement = StartCoroutine(LerpFromTo(item.Key.transform.position, item.Value.targetPosition, 1 / 3.5f, item.Key));
                item.Key.GetComponent<MeshRenderer>().material.color = new Color32((byte)colors[idx][0], (byte)colors[idx][1], (byte)colors[idx][2], 255);
            }
            else
            {
                item.Key.SetActive(false);
            }
            idx++;
        }
    }
}
