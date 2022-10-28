using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Market.Enum;

public class CryptoALGO : ICryptoALGO
{
    [JsonIgnore] public string Crypto { get; set; } = "ALGO-EUR";
    public int CountDecimalNumberInPosition { get; } = 4;
    public int CountDecimalNumberCryptoCurency { get; } = 0;
}

public class CryptoBTC : ICryptoBTC
{
    [JsonIgnore] public string Crypto { get; set; } = "BTC-EUR";
    public int CountDecimalNumberInPosition { get; } = 2;
    public int CountDecimalNumberCryptoCurency { get; }
}

public class CryptoETH : ICryptoETH
{
    [JsonIgnore] public string Crypto { get; set; } = "ETH-EUR";
    public int CountDecimalNumberInPosition { get; } = 2;
    public int CountDecimalNumberCryptoCurency { get; } = 6;
}

public class CryptoALCX : ICryptoALCX
{
    [JsonIgnore] public string Crypto { get; set; } = "ALCX-EUR";
    public int CountDecimalNumberInPosition { get; } = 4;
    public int CountDecimalNumberCryptoCurency { get; } = 6;
}

public class CryptoDOGE : ICryptoDOGE
{
    [JsonIgnore] public string Crypto { get; set; } = "DOGE-EUR";
    public int CountDecimalNumberInPosition { get; } = 4;
    public int CountDecimalNumberCryptoCurency { get; } = 6;
}

public interface ICryptoCurrency
{
    public string Crypto { get; set; }
    [JsonIgnore] public int CountDecimalNumberInPosition { get; }
    [JsonIgnore] public int CountDecimalNumberCryptoCurency { get; }
}

public class CryptoCurrency : ICryptoCurrency
{
    public bool Equals(CryptoCurrency other)
    {
        return Crypto == other.Crypto && CountDecimalNumberInPosition == other.CountDecimalNumberInPosition;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Crypto, CountDecimalNumberInPosition);
    }

    public CryptoCurrency(string crypto)
    {
        Crypto = crypto;
    }

    public string Crypto { get; set; }
    public int CountDecimalNumberInPosition { get; } = 2;
    public int CountDecimalNumberCryptoCurency { get; } = 6;

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