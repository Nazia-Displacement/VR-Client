using UnityEngine;
using Windows.Kinect;

//fileIO
using System.IO;
using System.Collections.Generic;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

public class KinectManager : MonoBehaviour
{
    public Color color = Color.green;
    public bool cameraColor = true;
    public float scale = 10f;
    public float max_size = 0.05f;

    [Range(0f, 1f)]
    public float mixFactor = 0.5f;

    float max_depth = 3f;

    private Vector3 initialRotation;

    private UnityEngine.Vector4 floorClipPlane;

    private ParticleSystem _particleSystem;
    private int maxParticles = 217088;
    private int particleCount = 0;
    private ParticleSystem.Particle[] particles_a;

    // Use this for initialization
    void Start()
    {
        initialRotation = transform.rotation.eulerAngles;
        _particleSystem = gameObject.GetComponent<ParticleSystem>();
        particles_a = new ParticleSystem.Particle[maxParticles];
    }

    // Update is called once per frame
    void Update()
    {
        AlignWithFloorPlane();
    }

    public void RenderPointCloud(
    ushort[] depthFrameData,
    byte[] bodyIndexFrameData,
    int depthWidth,
    int depthHeight,
    float focalLengthX,
    float focalLengthY,
    float principalPointX,
    float principalPointY,
    UnityEngine.Vector4 floorClipPlane)
    {
        particleCount = 0;
        this.floorClipPlane = floorClipPlane;

        for (int y = 0; y < depthHeight; y += 2)
        {
            for (int x = 0; x < depthWidth; x += 2)
            {
                if (particleCount >= maxParticles)
                    break;

                int depthIndex = (y * depthWidth) + x;
                byte bodyIndex = bodyIndexFrameData[depthIndex];

                if (bodyIndex != 255)
                {
                    // Reconstruct the 3D point from depth
                    float z = depthFrameData[depthIndex] * 0.001f;  // Convert to meters
                    if (z > 0) // Check for non-zero depth
                    {
                        float xPoint = (x - principalPointX) * z / focalLengthX;
                        float yPoint = (y - principalPointY) * z / focalLengthY;

                        if (!float.IsInfinity(xPoint) && !float.IsInfinity(yPoint) && !float.IsInfinity(z))
                        {
                            if (z > max_depth)
                                max_depth = z;

                            // Set particle position (or point cloud data)
                            particles_a[particleCount++] = new ParticleSystem.Particle()
                            {
                                position = new Vector3(xPoint, -yPoint, z),
                                startColor = Color.white,
                                startSize = max_size * (z / max_depth)
                            };
                        }
                    }
                }
            }
        }
        _particleSystem.SetParticles(particles_a, particleCount);
    }

    void AlignWithFloorPlane()
    {
        if (floorClipPlane != UnityEngine.Vector4.zero)
        {
            Vector3 kinectFloorNormal = new(floorClipPlane.x, floorClipPlane.y, floorClipPlane.z);
            Quaternion rotationToAlign = Quaternion.FromToRotation(kinectFloorNormal, Vector3.up);
            transform.rotation = rotationToAlign;
            transform.eulerAngles += initialRotation;

            float floorHeight = floorClipPlane.w;
            transform.position = new Vector3(transform.position.x, floorHeight * scale, transform.position.z);
        }
    }
}