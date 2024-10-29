using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UnityEngine;
using Firesplash.GameDevAssets.SocketIO;
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
    public Dictionary<string, Color> affectingLights = new();

    public Camera gameCam;
    public Camera kinectCam;

    public MyPlayerController player;

    private readonly bool live = true;
    private GameControls gameControls;

    [Serializable]
    struct KinectData 
    {
        public string data;
    }

    private struct KinectTransform {
        [JsonProperty]
        public Vector2 position;
        [JsonProperty]
        public Vector2 rotation;
    }
    void Start()
    {
        instance = this;
        socket = gameObject.AddComponent<SocketIOCommunicator>();
        gameControls = new GameControls();
        gameControls.Enable();

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
            if (affectingLights.ContainsKey(id))
            {
                affectingLights.Remove(id);
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

        socket.Instance.On("kinectTransform", data =>
        {
            data = data.Replace(@"\", "");
            KinectTransform kt = JsonConvert.DeserializeObject<KinectTransform>(data);
            Vector3 kpos = kinectManager.transform.position;
            kpos.x = kt.position.x;
            kpos.z = kt.position.y;
            kinectManager.transform.position = kpos;
            kinectManager.initialRotation = new Vector3(0, kt.rotation.x, 0);
        });

        if(live) socket.Instance.Connect("https://displacementserver.isaachisey.com", true, p);
        else socket.Instance.Connect("http://127.0.0.1:3001", true, p);
    }

    public void UpdatePanel(float value)
    {
        socket.Instance.Emit("unity-panel-update-request", value.ToString(), true);
    }

    public void SendPosition(float x, float y, float z, float xRot, float yRot, bool affectingLight)
    {
        // Step 1: Convert the SocketID string to a byte array
        string socketID = socket.Instance.SocketID; // Get the SocketID
        if (socketID == null) return;
        byte[] idBytes = Encoding.UTF8.GetBytes(socketID);

        // Step 2: Convert each float to a byte array
        byte[] xBytes = BitConverter.GetBytes(x);
        byte[] yBytes = BitConverter.GetBytes(y);
        byte[] zBytes = BitConverter.GetBytes(z);
        byte[] xRotBytes = BitConverter.GetBytes(xRot);
        byte[] yRotBytes = BitConverter.GetBytes(yRot);
        byte[] camMode = BitConverter.GetBytes(gameCam.enabled);
        byte[] isAffectingLights = BitConverter.GetBytes(affectingLight);

        // Step 3: Combine the id byte array with the other byte arrays into one larger array
        int totalLength = idBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length + xRotBytes.Length + yRotBytes.Length + camMode.Length + isAffectingLights.Length;
        byte[] allBytes = new byte[totalLength];

        Buffer.BlockCopy(idBytes, 0, allBytes, 0, idBytes.Length);
        Buffer.BlockCopy(xBytes, 0, allBytes, idBytes.Length, xBytes.Length);
        Buffer.BlockCopy(yBytes, 0, allBytes, idBytes.Length + xBytes.Length, yBytes.Length);
        Buffer.BlockCopy(zBytes, 0, allBytes, idBytes.Length + xBytes.Length + yBytes.Length, zBytes.Length);
        Buffer.BlockCopy(xRotBytes, 0, allBytes, idBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length, xRotBytes.Length);
        Buffer.BlockCopy(yRotBytes, 0, allBytes, idBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length + xRotBytes.Length, yRotBytes.Length);
        Buffer.BlockCopy(camMode, 0, allBytes, idBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length + xRotBytes.Length + yRotBytes.Length, camMode.Length);
        Buffer.BlockCopy(isAffectingLights, 0, allBytes, idBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length + xRotBytes.Length + yRotBytes.Length + camMode.Length, isAffectingLights.Length);

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
        var players = JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(jsonString);

        // Step 5: Update the game world with all the player positions
        foreach (var kvp in players)
        {
            string socketID = kvp.Key;
            PlayerData playerData = kvp.Value;

            if(playerData.affectingLights && !affectingLights.ContainsKey(socketID))
            {
                affectingLights.Add(socketID, new Color32(playerData.r, playerData.g, playerData.b, 255));
            }
            if (!playerData.affectingLights && affectingLights.ContainsKey(socketID))
            {
                affectingLights.Remove(socketID);
            }

            if (kvp.Key == socket.Instance.SocketID)
            {
                player.SetColor(playerData.r, playerData.g, playerData.b);
                continue;
            }

            if (avatars.ContainsKey(socketID))
            {
                avatars[socketID].SetTargets(new Vector3(playerData.x, playerData.y, playerData.z), playerData.xRot, playerData.yRot);
                avatars[socketID].SetVisible(playerData.display);
                avatars[socketID].SetColor(playerData.r, playerData.g, playerData.b);
            }
            else
            {
                avatars.Add(socketID, Instantiate(avatarPrefab, new Vector3(playerData.x, playerData.y, playerData.z), Quaternion.identity));
                avatars[socketID].SetColor(playerData.r, playerData.g, playerData.b);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentReconnectTimer += Time.deltaTime;

        if (!socket.Instance.IsConnected() && currentReconnectTimer >= reconnectTimerMax)
            if(live) socket.Instance.Connect("https://displacementserver.isaachisey.com", true, p);
            else socket.Instance.Connect("http://127.0.0.1:3001", true, p);

        if (socket.Instance.IsConnected() || currentReconnectTimer >= reconnectTimerMax) 
            currentReconnectTimer = 0;

        if(gameControls.SwitchCam.Switch.WasPerformedThisFrame())
        {
            gameCam.gameObject.SetActive(!gameCam.gameObject.activeInHierarchy);
            kinectCam.gameObject.SetActive(!gameCam.gameObject.activeInHierarchy);
        }
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

public class PlayerData
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public float xRot { get; set; }
    public float yRot { get; set; }
    public bool display { get; set; }
    public byte r { get; set; }
    public byte g { get; set; }
    public byte b { get; set; }
    public bool affectingLights { get; set; }
}
