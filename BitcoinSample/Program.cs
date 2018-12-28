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

            PrivateKey(privateKey);
        }

        private static void PrivateKey(Key privateKey)
        {
            // generate our Bitcoin secret(also known as Wallet Import Format or simply WIF) from our private key for the mainnet
            BitcoinSecret mainNetPrivateKey = privateKey.GetBitcoinSecret(Network.Main);

            // generate our Bitcoin secret(also known as Wallet Import Format or simply WIF) from our private key for the testnet
            BitcoinSecret testNetPrivateKey = privateKey.GetBitcoinSecret(Network.TestNet); 
            Console.WriteLine($"mainNetPrivateKey={mainNetPrivateKey}"); 
            Console.WriteLine($"testNetPrivateKey={testNetPrivateKey}");

            bool WifIsBitcoinSecret = mainNetPrivateKey == privateKey.GetWif(Network.Main);
            Console.WriteLine($"WifIsBitcoinSecret={WifIsBitcoinSecret}");
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

            var paymentScript = publicKeyHash.ScriptPubKey;
            var sameMainNetAddress = paymentScript.GetDestinationAddress(Network.Main);
            Console.WriteLine($"mainNetAddress == sameMainNetAddress:{mainNetAddress == sameMainNetAddress}");

            var samePublicKeyHash = (KeyId)paymentScript.GetDestination();
            Console.WriteLine(publicKeyHash == samePublicKeyHash);
            var sameMainNetAddress2 = new BitcoinPubKeyAddress(samePublicKeyHash, Network.Main);
            Console.WriteLine(mainNetAddress == sameMainNetAddress2);
        }
    }
}
