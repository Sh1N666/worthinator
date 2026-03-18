using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiInator.Model;

public class GameData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public int SteamAppId { get; set; }
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string TinyImage { get; set; }
    public string ReleaseDate { get; set; }
    
    public List<string> Developers { get; set; } = new();
    public List<string> Genres { get; set; } = new();
    
    public GamePlatforms Platforms { get; set; }
    public MetacriticData? Metacritic { get; set; }
    
    public SteamPriceData? SteamPrice { get; set; }
    public HltbData Hltb { get; set; }
    public GgDealsData? GgDeals { get; set; }
}