using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CryptoBotCore.API
{
    public interface ICoinmateAPI
    {
        string getSignature(Double time);
        string getSecuredHeaderPart();

        Task<decimal> getWithdrawalFeeAsync(decimal? amount = null, string? destinationAddress = null);
        Task<WithdrawalStateEnum> withdrawAsync(decimal amount, string destinationAddress);
        Task<decimal> getTakerFee();
        Task<List<WalletBalances>> getBalancesAsync();
        Task<string> buyOrderAsync(decimal amount, int price, string symbolMarket);
    }

    [Serializable]
    public class CoinmateAPI : ICoinmateAPI
    {
        private int clientId { get; set; }
        private string publicKey { get; set; }
        private string privateKey { get; set; }

        //private static long nonce { get; set; }
        static object nonceLock = new object();
        public ILogger Log { get; set; }

        public string pair_quote { get; set; }
        public string pair_base { get; set; }

        public CoinmateAPI()
        {
            this.Log = new LoggerFactory().CreateLogger("bla");
            clientId = 69105;
            publicKey = "6_kLajP-BHUxgrwP7TjdtKIuKaRUAjlFM7BL3aHmx3A";
            privateKey = "J-qU4SOnBUav3DntK3GGOCU_C_8o8bmoULfzPPqi-PM";
        }


        public string getSignature(Double time)
        {
            Encoding ascii = Encoding.ASCII;
            string message = time.ToString() + clientId.ToString() + publicKey;
            HMACSHA256 hmac = new HMACSHA256(ascii.GetBytes(privateKey));
            byte[] data = hmac.ComputeHash(ascii.GetBytes(message));
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }

        [Serializable]
        private class Nonce
        {
            public long Number { get; set; }

            public Nonce()
            {
                Number = (long)Utility.getUnixTimestamps(DateTime.UtcNow);
            }
        }

        private static long getNonce()
        {
            lock (nonceLock)
            {
                Nonce nonce = new Nonce();
                return nonce.Number;
            }
        }

        public string getSecuredHeaderPart()
        {
            Double nonce = getNonce();
            return "clientId=" + clientId + "&publicKey=" + publicKey + "&nonce=" + nonce + "&signature=" + getSignature(nonce);
        }


        public async Task<string> buyOrderAsync(decimal amount, int price, string symbolMarket)
        {
            int wait = 0;
            var attempt = 0;
            do
            {
                try
                {
                    WebClient client = new WebClient();
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                    //amount = Math.Floor(amount);
//amount=1&currencyPair=btc_eur&price=1&clientId=
                    string body = "total=" + amount + "&price=" + price + "&currencyPair=" + symbolMarket.ToLower() + "&" +
                                  getSecuredHeaderPart();
                    var response = await client.UploadStringTaskAsync(new Uri("https://coinmate.io/api/buyLimit"), body);

                    Response<string> result = JsonConvert.DeserializeObject<Response<string>>(response);

                    if (result.error)
                    {
                        Log.LogError(result.errorMessage?.ToString());
                        throw new Exception(result.errorMessage?.ToString());
                    }

                    return result.data;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt >= 3)
                    {
                        throw new Exception(JsonConvert.SerializeObject(ex));
                    }

                    Log.LogError(JsonConvert.SerializeObject(ex));
                    wait = (wait == 0) ? 200 : wait * 2;
                    await Task.Delay(wait);
                }
            } while (true);
        }

        public async Task<decimal> getWithdrawalFeeAsync(decimal? amount = null, string? destinationAddress = null)
        {
            if (this.pair_base == "LTC")
            {
                return 0.0004m; // HARDCODED
            }
            else if (this.pair_base == "ETH")
            {
                return 0.01m; // HARDCODED
            }
            else if (this.pair_base == "DSH")
            {
                return 0.00001m; // HARDCODED
            }

            int wait = 0;
            var attempt = 0;
            do
            {
                try
                {
                    WebClient client = new WebClient();
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    string body = getSecuredHeaderPart();
                    string response = await client.UploadStringTaskAsync("https://coinmate.io/api/bitcoinWithdrawalFees", body);
                    BTCWithdrawalFee_RootObject result = JsonConvert.DeserializeObject<BTCWithdrawalFee_RootObject>(response);

                    if (result.error)
                    {
                        throw new Exception(result.errorMessage?.ToString());
                    }

                    return result.data.low;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt >= 3)
                    {
                        throw new Exception(JsonConvert.SerializeObject(ex));
                    }

                    Log.LogError(JsonConvert.SerializeObject(ex));

                    wait = (wait == 0) ? 200 : wait * 2;
                    await Task.Delay(wait);
                }
            } while (true);
        }

        public async Task<WithdrawalStateEnum> withdrawAsync(decimal amount, string destinationAddress)
        {
            int wait = 0;
            var attempt = 0;
            do
            {
                try
                {
                    WebClient client = new WebClient();
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                    string? keypair = pair_base == "BTC" ? "bitcoinWithdrawal" :
                        pair_base == "LTC" ? "litecoinWithdrawal" :
                        pair_base == "ETH" ? "ethereumWithdrawal" :
                        pair_base == "DASH" ? "dashWithdrawal" :
                        null;

                    string fee_priority = pair_base == "BTC" ? "&feePriority=LOW" : "";

                    string body = "amount=" + amount + "&address=" + destinationAddress + fee_priority + "&" +
                                  getSecuredHeaderPart();
                    string response = await client.UploadStringTaskAsync("https://coinmate.io/api/" + keypair, body);
                    Response<string> result = JsonConvert.DeserializeObject<Response<string>>(response);

                    if (result.error)
                    {
                        throw new Exception(result.errorMessage?.ToString());
                    }

                    return WithdrawalStateEnum.OK;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Insufficient key privileges"))
                    {
                        return WithdrawalStateEnum.InsufficientKeyPrivileges;
                    }

                    Log.LogError(JsonConvert.SerializeObject(ex));
                    attempt++;

                    if (attempt >= 3)
                    {
                        throw new Exception(JsonConvert.SerializeObject(ex));
                    }

                    wait = (wait == 0) ? 200 : wait * 2;
                    await Task.Delay(wait);
                }
            } while (true);
        }

        public Task<decimal> getTakerFee()
        {
            // TODO: find an API call to get this value dynamically as one could have rebates due to its last 30 day trading volume
            return Task.FromResult(0.0035m); // HARDCODED: value is up to date on 24/11/2021 as a base default value
        }

        public async Task<List<WalletBalances>> getBalancesAsync()
        {
            int wait = 0;
            var attempt = 0;

            do
            {
                try
                {
                    WebClient client = new WebClient();
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    string body = getSecuredHeaderPart();
                    var response = await client.UploadStringTaskAsync("https://coinmate.io/api/balances", body);
                    Response<BalanceData> result = JsonConvert.DeserializeObject<Response<BalanceData>>(response);

                    if (result.error)
                    {
                        Log.LogError(result.errorMessage?.ToString());
                        throw new Exception(result.errorMessage?.ToString());
                    }

                    var wallets = new List<WalletBalances>();

                    if (result.data == null)
                    {
                        Log.LogError("No data found");
                        throw new Exception("No data found");
                    }

                    wallets.Add(new WalletBalances("CZK", result.data.CZK.available));
                    wallets.Add(new WalletBalances("EUR", result.data.EUR.available));
                    wallets.Add(new WalletBalances("BTC", result.data.BTC.available));
                    wallets.Add(new WalletBalances("LTC", result.data.LTC.available));
                    wallets.Add(new WalletBalances("ETH", result.data.ETH.available));
                    wallets.Add(new WalletBalances("DSH", result.data.DASH.available));

                    return wallets;
                }
                catch (Exception ex)
                {
                    Log.LogError(JsonConvert.SerializeObject(ex));
                    attempt++;

                    if (attempt >= 3)
                    {
                        throw new Exception(JsonConvert.SerializeObject(ex));
                    }

                    wait = (wait == 0) ? 200 : wait * 2;
                    await Task.Delay(wait);
                }
            } while (true);

            throw new Exception("Coinmate API is currently not available.");
        }

        private class Response<T>
        {
            public bool error { get; set; }
            public object? errorMessage { get; set; }
            public T? data { get; set; }
        }

        private class BTCWithdrawalFee_Data
        {
            public decimal low { get; set; }
            public decimal high { get; set; }
            public long timestamp { get; set; }
        }

        private class BTCWithdrawalFee_RootObject
        {
            public BTCWithdrawalFee_RootObject()
            {
                this.data = new BTCWithdrawalFee_Data();
            }

            public bool error { get; set; }
            public object? errorMessage { get; set; }
            public BTCWithdrawalFee_Data data { get; set; }
        }


        private class BalanceCurrency
        {
            public string? currency { get; set; }
            public decimal balance { get; set; }
            public decimal reserved { get; set; }
            public decimal available { get; set; }
        }

        private class BalanceData
        {
            public BalanceData()
            {
                this.EUR = new BalanceCurrency();
                this.CZK = new BalanceCurrency();
                this.BTC = new BalanceCurrency();
                this.LTC = new BalanceCurrency();
                this.ETH = new BalanceCurrency();
                this.DASH = new BalanceCurrency();
            }

            public BalanceCurrency EUR { get; set; }
            public BalanceCurrency CZK { get; set; }
            public BalanceCurrency BTC { get; set; }
            public BalanceCurrency LTC { get; set; }
            public BalanceCurrency ETH { get; set; }
            public BalanceCurrency DASH { get; set; }
        }
    }

    public static class Utility
    {
        public static Double timeDifference(Double dateFrom, Double dateTo)
        {
            return dateTo - dateFrom;
        }

        public static Double getUnixTimestamps(DateTime time)
        {
            return (time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc))).TotalMilliseconds;
        }

        public static DateTime getDateTimeFromUnixTimestamp(Double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            if (unixTimeStamp > 1000000000000)
                dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            else
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }

    public enum WithdrawalStateEnum
    {
        OK = 1,
        InsufficientKeyPrivileges = 2,
        UNKNOWN_FAIL = 3
    }

    [Serializable]
    public class WalletBalances
    {
        public WalletBalances(string currency, decimal available)
        {
            this.currency = currency;
            this.available = available;
        }

        public string currency { get; set; }

        /// <summary>
        /// How much X there is in this wallet that is available to trade
        /// </summary>
        public decimal available { get; set; }
    }
}