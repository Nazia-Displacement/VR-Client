using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    // Start is called before the first frame update
    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
        
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "CC0 Wheat.mp4");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
