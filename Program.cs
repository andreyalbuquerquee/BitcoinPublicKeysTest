using BitcoinPublicKeysTest.Entities;
using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json.Linq;

class Program
{
    private const int AddressLimit = 10;
    private static readonly string Zpub = "zpub6qSbi54ZDPQ5EfLzsv4QYoaEdQs1TMUCv3W7HTLhu2DsuHDZpgS9ycqu9X6ALNcRPiixq1peayenAJXpxrFGKVh4LS5n2kgmFqyjNp9KjyX";

    static async Task Main(string[] args)
    {
        string xpub = ConvertZpubToXpub(Zpub);
        var extPubKey = ExtPubKey.Parse(xpub, Network.Main);

        var addressesList = GenerateAddresses(extPubKey, AddressLimit);
        var responseJson = await FetchAddressesData(addressesList);

        var addressesModelList = ParseAddresses(responseJson);

        addressesModelList.ForEach(a => Console.WriteLine(a.BitcoinAddress + "\n" 
            + "BTC: " + a.GetBalanceInBitcoin() + "\n"));
    }

    static string ConvertZpubToXpub(string zpub)
    {
        byte[] data = Encoders.Base58Check.DecodeData(zpub);
        data[0] = 0x04;
        data[1] = 0x88;
        data[2] = 0xB2;
        data[3] = 0x1E;

        return Encoders.Base58Check.EncodeData(data);
    }

    static string[] GenerateAddresses(ExtPubKey extPubKey, int limit)
    {
        var addresses = new string[limit];

        for (int i = 0; i < limit; i++)
        {
            var address = extPubKey.Derive(0).Derive((uint)i).PubKey.GetAddress(ScriptPubKeyType.Segwit, Network.Main);
            addresses[i] = address.ToString();
        }

        return addresses;
    }

    static async Task<string> FetchAddressesData(string[] addresses)
    {
        var addressesQueryParameter = string.Join("|", addresses);
        using HttpClient client = new()
        {
            BaseAddress = new Uri($"https://blockchain.info/multiaddr?active={addressesQueryParameter}")
        };

        var response = await client.GetAsync(string.Empty);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    static List<Address> ParseAddresses(string responseJson)
    {
        var responseObject = JObject.Parse(responseJson);
        var addressesObjectsList = responseObject["addresses"]?.Children().ToList();

        if (addressesObjectsList == null)
        {
            throw new Exception("Nenhum endereço encontrado!");
        }

        return addressesObjectsList
            .Select(addressObject => addressObject.ToObject<Address>())
            .ToList();
    }
}
