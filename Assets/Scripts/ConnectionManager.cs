using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using Firesplash.GameDevAssets.SocketIO;
using Unity.VisualScripting;
using System.Text;
using Newtonsoft.Json;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager instance;
    public DoorwayLight doorway;
    public KinectManager kinectManager;

    SocketIOCommunicator socket;
    SIOAuthPayload p = new SIOAuthPayload();
    float reconnectTimerMax = 30f;
    float currentReconnectTimer = 0f;
    // Start is called before the first frame update

    public MultiPlayerAvatar avatarPrefab;

    public Dictionary<string, MultiPlayerAvatar> avatars = new();

    [Serializable]
    struct KinectData 
    {
        public string data;
    }
    void Start()
    {
        instance = this;
        socket = gameObject.AddComponent<SocketIOCommunicator>();
        p.AddElement("token", "Nazia.Unity.Project");

        ///// reserved socketio events
        socket.Instance.On("connect", (string data) =>
        {
        });
        socket.Instance.On("disconnect", (string data) =>
        {
        });
        ////

        socket.Instance.On("door-on", (string res) =>
        {
            Debug.Log(res);
        });

        socket.Instance.On("deletePlayer", (string id) =>
        {
            if (avatars.ContainsKey(id))
            {
                Destroy(avatars[id].gameObject);
                avatars.Remove(id);
            }
        });

        socket.Instance.On("kudata", (string res) =>
        {
            try
            {
                KinectData numbers = JsonUtility.FromJson<KinectData>(res);
                byte[] b = Convert.FromBase64String(numbers.data);
                // Use d.data as needed
                DecompressAndExtractFrames(b,
                    out ushort[] depthFrameData,
                    out byte[] bodyIndexFrameData,
                    out int depthWidth,
                    out int depthHeight,
                    out float focalLengthX,
                    out float focalLengthY,
                    out float principalPointX,
                    out float principalPointY,
                    out Vector4 floorClipPlan
                );
                kinectManager.RenderPointCloud(
                    depthFrameData,
                    bodyIndexFrameData,
                    depthWidth,
                    depthHeight,
                    focalLengthX,
                    focalLengthY,
                    principalPointX,
                    principalPointY,
                    floorClipPlan
                );
            }
            catch (Exception ex)
            {
                Debug.LogError("JSON parsing error: " + ex.Message);
            }
            
        });

        socket.Instance.On("updatePositions", (data) =>
        {
            ReceivePositions(data);
        });

        socket.Instance.Connect("https://displacementserver.isaachisey.com", true, p);
        //socket.Instance.Connect("http://127.0.0.1:3001", true, p);
    }

    public void UpdatePanel(float value)
    {
        socket.Instance.Emit("unity-panel-update-request", value.ToString(), true);
    }

    public void SendPosition(float x, float y, float z, float xRot, float yRot)
    {
        // Step 1: Convert each float to a byte array
        byte[] xBytes = BitConverter.GetBytes(x);
        byte[] yBytes = BitConverter.GetBytes(y);
        byte[] zBytes = BitConverter.GetBytes(z);
        byte[] xRotBytes = BitConverter.GetBytes(xRot);
        byte[] yRotBytes = BitConverter.GetBytes(yRot);

        // Step 2: Convert the SocketID string to a byte array
        string socketID = socket.Instance.SocketID; // Get the SocketID
        byte[] idBytes = Encoding.UTF8.GetBytes(socketID);

        // Step 3: Combine the id byte array with the other byte arrays into one larger array
        int totalLength = idBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length + xRotBytes.Length + yRotBytes.Length;
        byte[] allBytes = new byte[totalLength];

        Buffer.BlockCopy(idBytes, 0, allBytes, 0, idBytes.Length);
        Buffer.BlockCopy(xBytes, 0, allBytes, idBytes.Length, xBytes.Length);
        Buffer.BlockCopy(yBytes, 0, allBytes, idBytes.Length + xBytes.Length, yBytes.Length);
        Buffer.BlockCopy(zBytes, 0, allBytes, idBytes.Length + xBytes.Length + yBytes.Length, zBytes.Length);
        Buffer.BlockCopy(xRotBytes, 0, allBytes, idBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length, xRotBytes.Length);
        Buffer.BlockCopy(yRotBytes, 0, allBytes, idBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length + xRotBytes.Length, yRotBytes.Length);

        // Step 4: Compress the combined byte array
        byte[] compressedBytes;
        using (var outputStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(allBytes, 0, allBytes.Length);
            }
            compressedBytes = outputStream.ToArray();
        }

        // Step 5: Convert the compressed byte array to a base64 string
        string base64String = Convert.ToBase64String(compressedBytes);

        // Now you can send the base64String, for example, over a network or to another system.
        socket.Instance.Emit("playerPosUpdate", base64String, true);
    }

    public void ReceivePosition(string base64String)
    {
        // Step 1: Convert the base64 string back to a compressed byte array
        byte[] compressedBytes = Convert.FromBase64String(base64String);

        // Step 2: Decompress the byte array
        byte[] decompressedBytes;
        using (var inputStream = new MemoryStream(compressedBytes))
        {
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    gzipStream.CopyTo(outputStream);
                    decompressedBytes = outputStream.ToArray();
                }
            }
        }

        // Step 3: Extract the SocketID (20 characters)
        int idLength = 20; // SocketID length
        byte[] idBytes = new byte[idLength];
        Buffer.BlockCopy(decompressedBytes, 0, idBytes, 0, idLength);
        string socketID = Encoding.UTF8.GetString(idBytes);

        // Step 4: Extract the float values
        int floatSize = sizeof(float);
        float x = BitConverter.ToSingle(decompressedBytes, idLength);
        float y = BitConverter.ToSingle(decompressedBytes, idLength + floatSize);
        float z = BitConverter.ToSingle(decompressedBytes, idLength + 2 * floatSize);
        float xRot = BitConverter.ToSingle(decompressedBytes, idLength + 3 * floatSize);
        float yRot = BitConverter.ToSingle(decompressedBytes, idLength + 4 * floatSize);

        // Now you have the original values
    }

    public void ReceivePositions(string compressedData)
    {
        if (compressedData == null || compressedData == "") return;

        // Step 1: Convert the base64 string back to a compressed byte array
        byte[] compressedBytes = Convert.FromBase64String(compressedData);

        // Step 2: Decompress the byte array
        byte[] decompressedBytes;
        using (var inputStream = new MemoryStream(compressedBytes))
        {
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    gzipStream.CopyTo(outputStream);
                    decompressedBytes = outputStream.ToArray();
                }
            }
        }

        // Step 3: Convert the decompressed byte array back into a JSON string
        string jsonString = Encoding.UTF8.GetString(decompressedBytes);

        // Step 4: Deserialize the JSON string back into a dictionary of player positions
        var players = JsonConvert.DeserializeObject<Dictionary<string, PlayerPosition>>(jsonString);

        // Step 5: Update the game world with all the player positions
        foreach (var kvp in players)
        {
            if (kvp.Key == socket.Instance.SocketID) continue;

            string socketID = kvp.Key;
            PlayerPosition position = kvp.Value;

            if (avatars.ContainsKey(socketID))
            {
                avatars[socketID].SetTargets(new Vector3(position.x, position.y, position.z), position.xRot, position.yRot);
            }
            else
            {
                avatars.Add(socketID, Instantiate(avatarPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentReconnectTimer += Time.deltaTime;

        if (!socket.Instance.IsConnected() && currentReconnectTimer >= reconnectTimerMax)
            socket.Instance.Connect("http://127.0.0.1:3001", true, p);

        if (socket.Instance.IsConnected() || currentReconnectTimer >= reconnectTimerMax) 
            currentReconnectTimer = 0;
    }

    private static void DecompressAndExtractFrames(
    byte[] compressedData,
    out ushort[] depthFrameData,
    out byte[] bodyIndexFrameData,
    out int depthWidth,
    out int depthHeight,
    out float focalLengthX,
    out float focalLengthY,
    out float principalPointX,
    out float principalPointY,
    out Vector4 floorClipPlane)
    {
        byte[] decompressedData = DecompressData(compressedData);

        using (var memoryStream = new MemoryStream(decompressedData))
        using (var binaryReader = new BinaryReader(memoryStream))
        {
            // Read the lengths of each array
            int depthFrameBytesLength = binaryReader.ReadInt32();
            int bodyIndexFrameDataLength = binaryReader.ReadInt32();

            // Read the depth frame width and height
            depthWidth = binaryReader.ReadInt32();
            depthHeight = binaryReader.ReadInt32();

            // Read the intrinsic parameters
            focalLengthX = binaryReader.ReadSingle();
            focalLengthY = binaryReader.ReadSingle();
            principalPointX = binaryReader.ReadSingle();
            principalPointY = binaryReader.ReadSingle();

            // Read the floor clip plane
            float floorClipPlaneX = binaryReader.ReadSingle();
            float floorClipPlaneY = binaryReader.ReadSingle();
            float floorClipPlaneZ = binaryReader.ReadSingle();
            float floorClipPlaneW = binaryReader.ReadSingle();
            floorClipPlane = new Vector4(floorClipPlaneX, floorClipPlaneY, floorClipPlaneZ, floorClipPlaneW);

            // Extract the actual frame data
            depthFrameData = new ushort[depthFrameBytesLength / sizeof(ushort)];
            bodyIndexFrameData = new byte[bodyIndexFrameDataLength];

            // Extract depth frame data
            byte[] depthFrameBytes = binaryReader.ReadBytes(depthFrameBytesLength);
            depthFrameData = new ushort[depthFrameBytesLength / sizeof(ushort)];
            Buffer.BlockCopy(depthFrameBytes, 0, depthFrameData, 0, depthFrameBytesLength);

            // Extract body index frame data
            binaryReader.BaseStream.Read(bodyIndexFrameData, 0, bodyIndexFrameDataLength);
        }
    }

    private static byte[] DecompressData(byte[] compressedData)
    {
        using (var inputStream = new MemoryStream(compressedData))
        using (var decompressionStream = new DeflateStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new MemoryStream())
        {
            decompressionStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }

}

public class PlayerPosition
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public float xRot { get; set; }
    public float yRot { get; set; }
}
