using Newtonsoft.Json;
using System.Collections.Generic;

// List<GhostMovementData> myDeserializedClass = JsonConvert.DeserializeObject<List<GhostMovementData>>(myJsonResponse);
public class LAnkle
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class LEar
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class LElbow
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class LEye
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class LHip
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class LKnee
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class LShoulder
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class LWrist
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class Nose
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class RAnkle
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class REar
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class RElbow
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class REye
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class RHip
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class RKnee
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class GhostMovementData
{
    [JsonProperty("Nose")]
    public Nose Nose { get; set; }

    [JsonProperty("LEye")]
    public LEye LEye { get; set; }

    [JsonProperty("REye")]
    public REye REye { get; set; }

    [JsonProperty("LEar")]
    public LEar LEar { get; set; }

    [JsonProperty("REar")]
    public REar REar { get; set; }

    [JsonProperty("LShoulder")]
    public LShoulder LShoulder { get; set; }

    [JsonProperty("RShoulder")]
    public RShoulder RShoulder { get; set; }

    [JsonProperty("LElbow")]
    public LElbow LElbow { get; set; }

    [JsonProperty("RElbow")]
    public RElbow RElbow { get; set; }

    [JsonProperty("LWrist")]
    public LWrist LWrist { get; set; }

    [JsonProperty("RWrist")]
    public RWrist RWrist { get; set; }

    [JsonProperty("LHip")]
    public LHip LHip { get; set; }

    [JsonProperty("RHip")]
    public RHip RHip { get; set; }

    [JsonProperty("LKnee")]
    public LKnee LKnee { get; set; }

    [JsonProperty("RKnee")]
    public RKnee RKnee { get; set; }

    [JsonProperty("LAnkle")]
    public LAnkle LAnkle { get; set; }

    [JsonProperty("RAnkle")]
    public RAnkle RAnkle { get; set; }
}

public class RShoulder
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}

public class RWrist
{
    [JsonProperty("pos")]
    public List<double> Pos { get; set; }

    [JsonProperty("color")]
    public List<int> Color { get; set; }
}
