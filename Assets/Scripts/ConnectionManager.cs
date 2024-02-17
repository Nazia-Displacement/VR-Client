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
    public DoorwayLight doorway;

    SocketIOUnity socket;
    // Start is called before the first frame update
    void Start()
    {
        socket = new SocketIOUnity("https://displacementserver.isaachisey.com/", new SocketIOOptions
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

        socket.Connect();
        
        Console.WriteLine("Manager Started - Con");
        Debug.Log("Manager Started - Deb");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
