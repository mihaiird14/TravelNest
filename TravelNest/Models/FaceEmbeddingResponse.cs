using System.Text.Json.Serialization;
public class FaceEmbeddingResponse
{
    [JsonPropertyName("faces")]
    public List<List<double>> FacesEmb { get; set; }
}
