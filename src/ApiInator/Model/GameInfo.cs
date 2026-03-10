namespace ApiInator.Model;

public class GameInfo
{
    public Guid Id { get; set; }
    public double WorthFactor { get; set; }
    public SteamInfo SteamInfo { get; set; }
    public GgDealsInfo GgDeals { get; set; }
    public OpenCriticInfo OpenCritic { get; set; }
    public HowLongToBeatInfo HowLongToBeat { get; set; }
}