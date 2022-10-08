using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interface;

public interface ICalculateCrypto
{
    /// <summary> Vypocita jestli se ma uskutecnit prodej nebo nakup. Pokud se nema nic provadet vraci NULL! </summary>
    /// <param name="profitEur"> Profit v EUR od ktereho se uskutecni proces</param>
    /// <returns>Pokud se vrati null nic se neprovadi. Neni zadny profit</returns>
    MarketProcessBuyOrSell? CalculateSellOrBuy(decimal profitEur, IActualMarketValue actualMarketValue,decimal addExtraMoneyToPrice =1);
}