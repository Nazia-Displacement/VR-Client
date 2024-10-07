using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderFromChildren : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
        Debug.Log(meshFilters.Length);
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].mesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
