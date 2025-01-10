using Newtonsoft.Json;

namespace BitcoinPublicKeysTest.Entities
{
    public class Address
    {
        [JsonProperty("address")]
        public string BitcoinAddress { get; set; } = "";

        [JsonProperty("final_balance")]
        public long FinalBalance { get; set; }

        public decimal GetBalanceInBitcoin()
        {
            return FinalBalance / 100_000_000m;
        }
    }
}
