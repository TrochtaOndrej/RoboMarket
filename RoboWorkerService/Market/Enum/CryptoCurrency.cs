using Newtonsoft.Json;
using RoboWorkerService.JsonMapping;

namespace RoboWorkerService.Market.Enum;


public interface ICryptoALGO : ICryptoCurrency
{
}

public class CryptoALGO : ICryptoALGO
{
    [JsonIgnore]
    public string Crypto { get; set; } = "ALGO-EUR";
}


public interface ICryptoBTC : ICryptoCurrency
{
}

public class CryptoBTC : ICryptoBTC
{
    [JsonIgnore]
    public string Crypto { get; set; } = "BTC-EUR";
}

public interface ICryptoETH : ICryptoCurrency
{
}

public class CryptoETH : ICryptoETH
{
    [JsonIgnore]
    public string Crypto { get; set; } = "ETH-EUR";
}

public interface ICryptoDOGE : ICryptoCurrency { }

public class CryptoDOGE : ICryptoDOGE
{
    [JsonIgnore]
    public string Crypto { get; set; } = "DOGE-EUR";
}


public interface ICryptoCurrency
{
    public string Crypto { get; set; }
}


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

    public static bool operator !=(CryptoCurrency a, CryptoCurrency b)
    {
        return !a.Equals(b);
    }

    public static bool operator ==(CryptoCurrency a, CryptoCurrency b)
    {
        return a.Equals(b);
    }
}