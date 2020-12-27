using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using TeleSharp.TL;
using TLSharp.Core.Network.Exceptions;

namespace TelegramForwardMessage.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            

            Assert.Pass();
        }

        [Test]
        public async Task Test2()
        {
            List<string> channels = new List<string>();
            HttpClient client = new HttpClient();

            int offset = 0;
            do
            {
                string url = "https://tgstat.com/ru/channels/list";
                string parameters =
                    $"sort=&offset={offset}&sort_direction=&period=yesterday&category=adult&country=global&language=global&verified=1&price%5Bvp%5D=0&subscribers%5Bfrom%5D=&subscribers%5Bto%5D=&post_reach%5Bfrom%5D=&post_reach%5Bto%5D=&err%5Bfrom%5D=&err%5Bto%5D=&price%5Bfrom%5D=&price%5Bto%5D=&search=";
                var result = await client.PostAsync(url,
                    new StringContent(parameters, Encoding.UTF8, "application/x-www-form-urlencoded"));
                string response = await result.Content.ReadAsStringAsync();
                var des = JsonConvert.DeserializeObject<Tgstat.TgstatResponse>(response);
                var codes = des.Items.List.Select(x => x.ChannelIdCode).ToList();
                channels.AddRange(codes);
                offset += 100;
            } while (offset <= 700);

            File.WriteAllLines("channels_adult.txt", channels.
                Distinct()
                .Where(x=> x.StartsWith("@"))
                .Select(x=> x.Remove(0,1)));


        }
    }

    class Tgstat
    {
         public partial class TgstatResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("items")]
        public Items Items { get; set; }
    }

    public partial class Items
    {
        [JsonProperty("list")]
        public List<List> List { get; set; }

        [JsonProperty("total")]
        public Total Total { get; set; }
    }

    public partial class List
    {
        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("category_url")]
        public string CategoryUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("likes")]
        public long Likes { get; set; }

        [JsonProperty("adv_enabled")]
        public long? AdvEnabled { get; set; }

        [JsonProperty("adv_mutual_enabled")]
        public long? AdvMutualEnabled { get; set; }

        [JsonProperty("adv_text")]
        public string AdvText { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("badlisted")]
        public bool Badlisted { get; set; }

        [JsonProperty("redlabel")]
        public bool Redlabel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("channelIdCode")]
        public string ChannelIdCode { get; set; }

        [JsonProperty("members")]
        public long Members { get; set; }

        [JsonProperty("members_growth")]
        public MembersGrowth MembersGrowth { get; set; }

        // [JsonProperty("views")]
        // [JsonConverter(typeof(DecodingChoiceConverter))]
        // public long Views { get; set; }

        [JsonProperty("views_growth_percent")]
        public double ViewsGrowthPercent { get; set; }

        [JsonProperty("views_per_post")]
        public long ViewsPerPost { get; set; }

        [JsonProperty("err_percent")]
        public string ErrPercent { get; set; }

        [JsonProperty("err_percent_kilo")]
        public string ErrPercentKilo { get; set; }

        [JsonProperty("ic_count")]
        public double IcCount { get; set; }

        // [JsonProperty("ic_count_kilo")]
        // public IcCountKilo IcCountKilo { get; set; }

        [JsonProperty("members_kilo")]
        public string MembersKilo { get; set; }
        //
        // [JsonProperty("members_growth_kilo")]
        // public MembersGrowth MembersGrowthKilo { get; set; }
        //
        // [JsonProperty("views_kilo")]
        // public MembersGrowth ViewsKilo { get; set; }

        [JsonProperty("views_per_post_kilo")]
        public string ViewsPerPostKilo { get; set; }

        [JsonProperty("alert_subscription_url")]
        public object AlertSubscriptionUrl { get; set; }
    }

    public partial class Total
    {
        [JsonProperty("qty")]
        public long Qty { get; set; }

        [JsonProperty("members")]
        public long Members { get; set; }

        [JsonProperty("members_growth")]
        public long MembersGrowth { get; set; }

        [JsonProperty("views")]
        public long Views { get; set; }

        [JsonProperty("views_growth_percent")]
        public double ViewsGrowthPercent { get; set; }

        [JsonProperty("ic_count")]
        public double IcCount { get; set; }

        [JsonProperty("ic_count_kilo")]
        public string IcCountKilo { get; set; }

        [JsonProperty("members_kilo")]
        public string MembersKilo { get; set; }

        [JsonProperty("members_growth_kilo")]
        public string MembersGrowthKilo { get; set; }

        [JsonProperty("views_kilo")]
        public string ViewsKilo { get; set; }

        [JsonProperty("views_per_post")]
        public long ViewsPerPost { get; set; }

        [JsonProperty("views_per_post_kilo")]
        public long ViewsPerPostKilo { get; set; }

        [JsonProperty("likes")]
        public long Likes { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }
    }

    public partial struct IcCountKilo
    {
        public double? Double;
        public string String;

        public static implicit operator IcCountKilo(double Double) => new IcCountKilo { Double = Double };
        public static implicit operator IcCountKilo(string String) => new IcCountKilo { String = String };
    }

    public partial struct MembersGrowth
    {
        public long? Integer;
        public string String;

        public static implicit operator MembersGrowth(long Integer) => new MembersGrowth { Integer = Integer };
        public static implicit operator MembersGrowth(string String) => new MembersGrowth { String = String };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                IcCountKiloConverter.Singleton,
                MembersGrowthConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class IcCountKiloConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(IcCountKilo) || t == typeof(IcCountKilo?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new IcCountKilo { Double = doubleValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new IcCountKilo { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type IcCountKilo");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (IcCountKilo)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type IcCountKilo");
        }

        public static readonly IcCountKiloConverter Singleton = new IcCountKiloConverter();
    }

    internal class MembersGrowthConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(MembersGrowth) || t == typeof(MembersGrowth?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new MembersGrowth { Integer = integerValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new MembersGrowth { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type MembersGrowth");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (MembersGrowth)untypedValue;
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type MembersGrowth");
        }

        public static readonly MembersGrowthConverter Singleton = new MembersGrowthConverter();
    }

    internal class DecodingChoiceConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return integerValue;
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    long l;
                    if (Int64.TryParse(stringValue, out l))
                    {
                        return l;
                    }
                    break;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value);
            return;
        }

        public static readonly DecodingChoiceConverter Singleton = new DecodingChoiceConverter();
    }
    }
}