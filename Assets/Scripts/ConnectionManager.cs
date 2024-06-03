using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager instance;
    public DoorwayLight doorway;
    public Ghost ghost0;

    SocketIOUnity socket;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        socket = new SocketIOUnity("http://127.0.0.1:3001"/*"https://displacementserver.isaachisey.com/"*/, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "Nazia.Unity.Project" }
            },
            Transport = SocketIOClient.Transport.TransportProtocol.Polling
        });

        Debug.Log(socket);
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        ///// reserved socketio events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };
        socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"Reconnecting attempt");
        };
        ////

        socket.OnUnityThread("door-on", (res) =>
        {
            Debug.Log(res.GetValue<int>());
            doorway.UpdateDoorway(res.GetValue<int>());
        });

        socket.OnUnityThread("ukeyframes", (res) =>
        {
            List<GhostMovementData> myDeserializedClass = JsonConvert.DeserializeObject<List<GhostMovementData>>(res.GetValue<string>());
            if (myDeserializedClass.Count > 0)
            {
                List<double>[] targets = {
                    myDeserializedClass[0].Nose.Pos,
                    myDeserializedClass[0].LShoulder.Pos,
                    myDeserializedClass[0].RShoulder.Pos,
                    myDeserializedClass[0].LElbow.Pos,
                    myDeserializedClass[0].RElbow.Pos,
                    myDeserializedClass[0].LWrist.Pos,
                    myDeserializedClass[0].RWrist.Pos,
                    myDeserializedClass[0].LHip.Pos,
                    myDeserializedClass[0].RHip.Pos,
                    myDeserializedClass[0].LKnee.Pos,
                    myDeserializedClass[0].RKnee.Pos,
                    myDeserializedClass[0].LAnkle.Pos,
                    myDeserializedClass[0].RAnkle.Pos
                };
                List<int>[] colors = {
                    myDeserializedClass[0].Nose.Color,
                    myDeserializedClass[0].LShoulder.Color,
                    myDeserializedClass[0].RShoulder.Color,
                    myDeserializedClass[0].LElbow.Color,
                    myDeserializedClass[0].RElbow.Color,
                    myDeserializedClass[0].LWrist.Color,
                    myDeserializedClass[0].RWrist.Color,
                    myDeserializedClass[0].LHip.Color,
                    myDeserializedClass[0].RHip.Color,
                    myDeserializedClass[0].LKnee.Color,
                    myDeserializedClass[0].RKnee.Color,
                    myDeserializedClass[0].LAnkle.Color,
                    myDeserializedClass[0].RAnkle.Color
                };
                ghost0.UpdateKeypoints(targets, colors);
            }
        });

        socket.Connect();
        
        Debug.Log("Manager Started - Deb");
    }

    public void UpdatePanel(float value)
    {
        socket.Emit("unity-panel-update-request", value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
