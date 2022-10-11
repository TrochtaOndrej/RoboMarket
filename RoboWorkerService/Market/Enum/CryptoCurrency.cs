using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RoboWorkerService.Market.Enum;

public record CryptoDefinedList
{
    public static CryptoCurrency BTC_EUR = new CryptoCurrency("BTC-EUR");
    public static CryptoCurrency ETH_EUR = new CryptoCurrency("ETH-EUR");
}

public interface ICryptoBTC: ICryptoCurrency
{ }

public class CryptoBTC :  ICryptoBTC
{
    public string Crypto { get; set; } = "BTC-EUR";
}

public interface ICryptoETH: ICryptoCurrency
{ }

public class CryptoETH :  ICryptoETH
{
    public string Crypto { get; set; } = "ETH-EUR";
}




public interface ICryptoCurrency
{
    public string Crypto { get; set; }
}

[Serializable]
public class CryptoCurrency : ICryptoCurrency
{
    public string Crypto { get; set; }

    public CryptoCurrency(string crypto)
    {
        Crypto = crypto;
    }

    public override string ToString()
    {
        return Crypto;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is ICryptoCurrency || obj is CryptoCurrency)
        {
            return string.Equals(Crypto, ((ICryptoCurrency)obj).Crypto, StringComparison.InvariantCultureIgnoreCase);
        }

        return false;
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
