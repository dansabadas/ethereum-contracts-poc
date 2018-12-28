using System;
using NBitcoin;

namespace BitcoinSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var privateKey = new Key(); // generate a random private key
            BitcoinAddress(privateKey);
        }

        private static void BitcoinAddress(Key privateKey)
        {
            PubKey publicKey = privateKey.PubKey;
            Console.WriteLine($"publicKey={publicKey}");

            Console.WriteLine($"main addr={publicKey.GetAddress(Network.Main)}");
            Console.WriteLine($"test addr={publicKey.GetAddress(Network.TestNet)}");

            var publicKeyHash = publicKey.Hash;
            Console.WriteLine($"publicKeyHash={publicKeyHash}");
            var mainNetAddress = publicKeyHash.GetAddress(Network.Main);
            var testNetAddress = publicKeyHash.GetAddress(Network.TestNet);
            Console.WriteLine($"mainNetAddress==publicKey.GetAddress(Network.Main):{mainNetAddress == publicKey.GetAddress(Network.Main)}");
            Console.WriteLine($"testNetAddress={testNetAddress}");

            publicKeyHash = new KeyId("14836dbe7f38c5ac3d49e8d790af808a4ee9edcf");         
            testNetAddress = publicKeyHash.GetAddress(Network.TestNet);
            mainNetAddress = publicKeyHash.GetAddress(Network.Main);
            Console.WriteLine($"mainNetAddress.ScriptPubKey={mainNetAddress.ScriptPubKey}");
            Console.WriteLine($"mainNetAddress.ScriptPubKey==testNetAddress.ScriptPubKey:{mainNetAddress.ScriptPubKey==testNetAddress.ScriptPubKey}");
        }
    }
}
