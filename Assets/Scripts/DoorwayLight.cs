using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEngine.Rendering.DebugUI;

public class DoorwayLight : MonoBehaviour
{
    [SerializeField]
    public Light[] lights;

    private MeshRenderer m;


    // Start is called before the first frame update
    void Start()
    {
        m = GetComponent<MeshRenderer>();
        float a = 0;
        m.material.color = new(a, a, a, a);
        foreach (Light light in lights)
        {
            light.color = new(a, a, a, a);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateDoorway(int value)
    {
        float a = value / 127f;
        m.material.color = new(a, a, a, a);
        foreach (Light light in lights)
        {
            light.color = new(a, a, a, a);
        }
    }
}
