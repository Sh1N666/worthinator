namespace ApiInator.Model;

public class SteamInfo
{
    public Guid Id { get; set; }
    public string SteamAppId { get; set; }
    public string Name { get; set; }
    public Price InitialPrice { get; set; }
    public string TinyImageUrl { get; set; }
}