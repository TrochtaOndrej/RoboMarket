using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RoboWorkerService.Market.Enum;

public record CryptoCurrencyDefinitionList
{
    public static CryptoCurrency BTC_EUR = new CryptoCurrency("BTC-EUR");
    public static CryptoCurrency ETH_EUR = new CryptoCurrency("ETH-EUR");
}


public readonly struct CryptoCurrency
{
    private readonly string _crypto;

    public CryptoCurrency(string crypto)
    {
        _crypto = crypto;
    }

    public override string ToString()
    {
        return _crypto;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CryptoCurrency) return false;
        return string.Equals(_crypto,((CryptoCurrency) obj)._crypto, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool operator !=(CryptoCurrency a,CryptoCurrency b)
    {
        return !a.Equals(b);
    }
    public static bool operator ==(CryptoCurrency a, CryptoCurrency b)
    {
        return a.Equals(b);
    }

}
