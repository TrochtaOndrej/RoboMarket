using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Market.Enum;

public class CryptoALGO : ICryptoALGO
{
    [JsonIgnore] public string Crypto { get; set; } = "ALGO-EUR";
}

public class CryptoBTC : ICryptoBTC
{
    [JsonIgnore] public string Crypto { get; set; } = "BTC-EUR";
}

public class CryptoETH : ICryptoETH
{
    [JsonIgnore] public string Crypto { get; set; } = "ETH-EUR";
}

public class CryptoDOGE : ICryptoDOGE
{
    [JsonIgnore] public string Crypto { get; set; } = "DOGE-EUR";
}

public interface ICryptoCurrency
{
    public string Crypto { get; set; }
}

public class CryptoCurrency : ICryptoCurrency
{
    public CryptoCurrency(string crypto)
    {
        Crypto = crypto;
    }

    public string Crypto { get; set; }

    public override string ToString()
    {
        return Crypto;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is ICryptoCurrency || obj is CryptoCurrency)
            return string.Equals(Crypto, ((ICryptoCurrency)obj).Crypto, StringComparison.InvariantCultureIgnoreCase);

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