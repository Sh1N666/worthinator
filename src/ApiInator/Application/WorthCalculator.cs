using ApiInator.Model;

namespace ApiInator.Application;

public static class WorthCalculator
{
    public static int CalculateWorthFactor(GameData game)
    {
        double L = (game.Hltb.MainStory + game.Hltb.MainExtra + game.Hltb.Completionist) / 3.0;

        double totalR = 0;
        int count = 0;
        if (game.Metacritic?.Score > 0) 
        { 
            totalR += (game.Metacritic.Score / 10.0); 
            count++; 
        }
    
        if (game.Hltb.ReviewScore > 0) 
        { 
            totalR += (game.Hltb.ReviewScore / 10.0); 
            count++; 
        }
    
        double R = count > 0 ? totalR / count : 0;

        double pAkt = Math.Min(game.GgDeals.Prices.CurrentKeyshops, game.GgDeals.Prices.CurrentRetail); 
        double pHist = Math.Max(Math.Min(game.GgDeals.Prices.HistoricalKeyshops, game.GgDeals.Prices.HistoricalRetail), 0.01);
        double pRegular = (game.SteamPrice.Initial) / 100.0;

        if (pAkt <= 0) pAkt = 0.01;

        double numerator = ((L + 50) * Math.Pow(R, 4)) / Math.Sqrt(pAkt);
        double priceRatioHist = Math.Sqrt(pHist / pAkt);
        double priceRatioReg = Math.Pow(pRegular / pAkt, 0.25); 

        int rawWorth = (int)Math.Floor(1 + 99 * (numerator * priceRatioHist * priceRatioReg / 175000.0));

        return Math.Min(100, rawWorth);
    }
}