using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.JsonMapping;

//public class CryptoICurrencyJsonConvertor : JsonConverter<ICryptoCurrency>
//{
//    public override void WriteJson(JsonWriter writer, ICryptoCurrency? value, JsonSerializer serializer)
//    {
//        serializer.Serialize(writer, value);
//    }

//    public override ICryptoCurrency? ReadJson(JsonReader reader, Type objectType, ICryptoCurrency? existingValue, bool hasExistingValue,
//        JsonSerializer serializer)
//    {
//        var dd= serializer.Deserialize(reader, typeof(CryptoCurrency));
//        return (ICryptoCurrency)dd; 
//    }
//}

