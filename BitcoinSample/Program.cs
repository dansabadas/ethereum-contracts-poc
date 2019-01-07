using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Newtonsoft.Json.Linq;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace BitcoinSample
{
    class Program
    {
        static async Task Main()
        {
            var privateKey = new Key(); // generate a random private key
            BitcoinAddresSamples(privateKey);

            PrivateKey(privateKey);

            await Transactions();

            await SpendCoins();
        }

        private static void PrivateKey(Key privateKey)
        {
            // generate our Bitcoin secret(also known as Wallet Import Format or simply WIF) from our private key for the mainnet
            BitcoinSecret mainNetPrivateKey = privateKey.GetBitcoinSecret(Network.Main);

            // generate our Bitcoin secret(also known as Wallet Import Format or simply WIF) from our private key for the testnet
            BitcoinSecret testNetPrivateKey = privateKey.GetBitcoinSecret(Network.TestNet); 
            Console.WriteLine($"mainNetPrivateKey={mainNetPrivateKey}"); 
            Console.WriteLine($"testNetPrivateKey={testNetPrivateKey}");

            bool wifIsBitcoinSecret = mainNetPrivateKey == privateKey.GetWif(Network.Main);
            Console.WriteLine($"WifIsBitcoinSecret={wifIsBitcoinSecret}");

            BitcoinSecret bitcoinSecret = privateKey.GetWif(Network.Main);
            Key samePrivateKey = bitcoinSecret.PrivateKey;
            Console.WriteLine(samePrivateKey == privateKey);
            PubKey publicKey = privateKey.PubKey;
            BitcoinPubKeyAddress bitcoinPublicKey = publicKey.GetAddress(Network.Main);
            Console.WriteLine($"publicKey={publicKey}");
            Console.WriteLine($"bitcoinPublicKey={bitcoinPublicKey}");

            Console.WriteLine($"mainnet bitcoinSecret={bitcoinSecret}");
        }

        private static void BitcoinAddresSamples(Key privateKey)
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

        private static async Task Transactions()
        {
            QBitNinjaClient client = new QBitNinjaClient(Network.Main);
            // Parse trn id to NBitcoin.uint256 so the qbitNinjaClient can eat it
            var transactionId = uint256.Parse("f13dc48fb035bbf0a6e989a26b3ecb57b84f85e0836e777d6edf60d87a4a2d94");

            // Query the trn
            GetTransactionResponse transactionResponse = await client.GetTransaction(transactionId);
            Transaction transaction = transactionResponse.Transaction;

            Console.WriteLine($"transactionResponse.TransactionId={transactionResponse.TransactionId}");
            Console.WriteLine($"trn.GetHash()={transaction.GetHash()}");

            Console.WriteLine(transactionResponse.TransactionId == transactionId);

            List<ICoin> receivedCoins = transactionResponse.ReceivedCoins;
            foreach (var coin in receivedCoins)
            {
                Money amount = (Money)coin.Amount;
                Console.WriteLine($"BTC amount={amount.ToDecimal(MoneyUnit.BTC)}");
                var paymentScript = coin.TxOut.ScriptPubKey;
                Console.WriteLine($"paymentScript={paymentScript}");
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine($"destination address = {address}");
                Console.WriteLine();
            }

            List<ICoin> spentCoins = transactionResponse.SpentCoins;
            foreach (var coin in spentCoins)
            {
                Money amount = (Money)coin.Amount;
                Console.WriteLine($"BTC amount={amount.ToDecimal(MoneyUnit.BTC)}");
                var paymentScript = coin.TxOut.ScriptPubKey;
                Console.WriteLine($"paymentScript={paymentScript}");
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine($"destination address = {address}");
                Console.WriteLine();
            }

            foreach (TxOut output in transaction.Outputs)
            {
                Money amount = output.Value;
                Console.WriteLine($"BTC amount={amount.ToDecimal(MoneyUnit.BTC)}");
                var paymentScript = output.ScriptPubKey;
                Console.WriteLine($"paymentScript={paymentScript}");
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine($"destination address = {address}");
                Console.WriteLine();
            }

            foreach (TxIn input in transaction.Inputs)
            {
                OutPoint previousOutpoint = input.PrevOut;
                Console.WriteLine($"previousOutpoint.Hash={previousOutpoint.Hash}"); // hash of prev tx
                Console.WriteLine($"previousOutpoint.N={previousOutpoint.N}"); // idx of out from prev tx, that has been spent in the current tx
                Console.WriteLine();
            }

            Money twentyOneBtc = new Money(21, MoneyUnit.BTC);
            var scriptPubKey = transaction.Outputs.First().ScriptPubKey;
            TxOut txOut = new TxOut(twentyOneBtc, scriptPubKey);
            Console.WriteLine(txOut.Value);

            async Task FindOriginatorCoinbaseTransaction()
            {
                OutPoint firstPreviousOutPoint = transaction.Inputs.First().PrevOut;
                var firstPreviousTransactionResponse = await client.GetTransaction(firstPreviousOutPoint.Hash);
                var firstPreviousTransaction = firstPreviousTransactionResponse.Transaction;
                var iteration = 0;
                Console.WriteLine($"{iteration}: total out={firstPreviousTransaction.TotalOut}, isCoinbase={firstPreviousTransaction.IsCoinBase}");
                while (!firstPreviousTransaction.IsCoinBase)
                {
                    firstPreviousTransactionResponse = await client.GetTransaction(firstPreviousTransaction.Inputs.First().PrevOut.Hash);
                    firstPreviousTransaction = firstPreviousTransactionResponse.Transaction;
                    Console.WriteLine($"{++iteration}: total out={firstPreviousTransaction.TotalOut}, isCoinbase={firstPreviousTransaction.IsCoinBase}");
                }
            }

            //await FindOriginatorCoinbaseTransaction();

            Money spentAmount = Money.Zero;
            foreach (var spentCoin in spentCoins)
            {
                spentAmount = (Money)spentCoin.Amount.Add(spentAmount);
            }

            Console.WriteLine($"spent amount={spentAmount.ToDecimal(MoneyUnit.BTC)}");

            Money receivedAmount = Money.Zero;
            foreach (var receivedCoin in transactionResponse.SpentCoins)
            {
                receivedAmount = (Money)receivedCoin.Amount.Add(spentAmount);
            }

            Console.WriteLine($"received amount={receivedAmount.ToDecimal(MoneyUnit.BTC)}");

            var fee = transactionResponse.Transaction.GetFee(spentCoins.ToArray());
            Console.WriteLine($"fee={fee}");

            // 7ed5cdde4b00520c59a9ec8e77e49177660669a7365af4cfb0aa3eecdd2331bd 187d345f355f13978d3cc9969ca7e78dc8f253d702931e3b999bdc12e64536da
            var trnId = "7ed5cdde4b00520c59a9ec8e77e49177660669a7365af4cfb0aa3eecdd2331bd";
            await ObtainQBitNinjaTransactionInfo(trnId);
            var customMessage = await ObtainBitcoinTransactionCustomMessage(trnId);

            // obtain trn info via QBitNinjaClient
            Console.WriteLine($"trnId={trnId} => customMessage={(string)customMessage}");
        }

        private static async Task SpendCoins()
        {
            // Replace this with Network.Main to do this on Bitcoin MainNet
            var network = Network.TestNet;

            var privateKey = new Key();
            var bitcoinPrivateKey = privateKey.GetWif(network);
            var address = bitcoinPrivateKey.GetAddress();

            Console.WriteLine($"bitcoinPrivateKey={bitcoinPrivateKey}");
            Console.WriteLine($"address={address}");

            // with the lines above we generated our private key and address to be used in subsequent examples
            // bitcoinPrivateKey=cT6DiZUoC61LqBsm5fTYW6BNkU9QwU4pJBdj84wmhi1daMECuneX
            // address=mtrQCDZenXXa1oWMhypzgvBinfrZYYveRC (the secondary testnet address)

            bitcoinPrivateKey = new BitcoinSecret("cT6DiZUoC61LqBsm5fTYW6BNkU9QwU4pJBdj84wmhi1daMECuneX");
            network = bitcoinPrivateKey.Network;
            address = bitcoinPrivateKey.GetAddress();

            Console.WriteLine($"hardcoded bitcoinPrivateKey={bitcoinPrivateKey}");
            Console.WriteLine($"hardcoded address={address}");
            Console.WriteLine($"network={network}");

            // get faucets from https://coinfaucet.eu/en/btc-testnet/
            // primary testnet address: https://live.blockcypher.com/btc-testnet/address/mtjeFt6dMKqvQmYKcBAkSX9AmX8qdynVKN/
            // with corresponding secret key cVX7SpYc8yjNW8WzPpiGTqyWD4eM4BBnfqEm9nwGqJb2QiX9hhdf

            // the main sample: create a trn and broadcast it!
            var transaction = Transaction.Create(network);

            string srcAddr = "mtrQCDZenXXa1oWMhypzgvBinfrZYYveRC";  //secondary
            string destAddr = "mtjeFt6dMKqvQmYKcBAkSX9AmX8qdynVKN"; //primary
            //var inputTranList = await GetUnspentTransactions(srcAddr);
            //transaction.Inputs.Add(inputTranList[1]);   //destAddr srcAddr
            Money amountToSpend = new Money(.004m, MoneyUnit.BTC);
            var minerFee = new Money(0.000007m, MoneyUnit.BTC);
            var trnId = await GetLatestSpentTransactionId(srcAddr);
            var customMessage = await ObtainBitcoinTransactionCustomMessage(trnId.ToString());

            transaction.Inputs.Add(await GetSpendableTransaction(srcAddr, amountToSpend + minerFee));  // we find a pre-existing input with an amount bigger than the desired spending

            TxOut destinationFourThousandthsTxOut = customMessage.N % 2 == 1 
                ? new TxOut
                    {
                        Value = amountToSpend,
                        ScriptPubKey = new BitcoinPubKeyAddress(destAddr).ScriptPubKey
                    } 
                : null; // every two transactions we just burn the money for the message fee only
            var unspentReceivedCoins = await GetUnspentBalance(srcAddr, amountToSpend);
            decimal srcAddrBalance = unspentReceivedCoins?.Amount.ToDecimal(MoneyUnit.BTC) ?? 0;
            Console.WriteLine($"unspentCoins={srcAddrBalance}");

            var changeBackAmount = srcAddrBalance 
                                   - (destinationFourThousandthsTxOut?.Value.ToDecimal(MoneyUnit.BTC) ?? 0m)
                                   - minerFee.ToDecimal(MoneyUnit.BTC);

            TxOut changeBackTxOut = new TxOut
            {
                Value = new Money(changeBackAmount, MoneyUnit.BTC),
                ScriptPubKey = new BitcoinPubKeyAddress(srcAddr).ScriptPubKey
            };

            if (destinationFourThousandthsTxOut != null)
            {
                transaction.Outputs.Add(destinationFourThousandthsTxOut);
            }

            transaction.Outputs.Add(changeBackTxOut);

            TranslationContract tc = new TranslationContract
            {
                E = "dan.sabadas@gmail.com",
                N = customMessage.N + 1,    // we increase the number of translated words of the latest contract! :)
                S = "ro",
                D = new[] {"en", "fr"}
            };
            string serializedMsg = (string)tc;
            transaction.Outputs.Add(new TxOut
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(Encoding.UTF8.GetBytes(serializedMsg))
            });

            //signing
            // Get it from the public address
            transaction.Inputs[0].ScriptSig = BitcoinAddress
                .Create(srcAddr, Network.TestNet)
                .ScriptPubKey;
            // OR we can also use the private key 
            transaction.Inputs.ForEach(input => input.ScriptSig = bitcoinPrivateKey.ScriptPubKey);
            Console.WriteLine($"transaction: {transaction}");

            transaction.Sign(bitcoinPrivateKey, new [] {unspentReceivedCoins});

            var client = new QBitNinjaClient(network);
            BroadcastResponse broadcastResponse = await client.Broadcast(transaction);

            if (!broadcastResponse.Success)
            {
                Console.Error.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
                Console.Error.WriteLine("Error message: " + broadcastResponse.Error.Reason);
            }
            else
            {
                Console.WriteLine("Success! You can check out the hash of the transaction in any block explorer:");
                Console.WriteLine(transaction.GetHash());
            }
        }

        private static async Task ObtainQBitNinjaTransactionInfo(string transactionId)
        {
            var client = new QBitNinjaClient(Network.TestNet);

            var transactionResponse = await client.GetTransaction(uint256.Parse(transactionId));
            Console.WriteLine(
                $"tr-id: {transactionResponse.TransactionId} => confirmations={transactionResponse.Block?.Confirmations ?? 0}");

            var bitcoinPrivateKey = new BitcoinSecret("cT6DiZUoC61LqBsm5fTYW6BNkU9QwU4pJBdj84wmhi1daMECuneX");

            OutPoint outPointToSpend = null;
            foreach (var receivedCoin in transactionResponse.ReceivedCoins)
            {
                if (receivedCoin.TxOut.ScriptPubKey == bitcoinPrivateKey.ScriptPubKey)
                {
                    outPointToSpend = receivedCoin.Outpoint;
                }
            }

            if (outPointToSpend == null)
                throw new Exception("TxOut doesn't contain our ScriptPubKey");
            Console.WriteLine($"We wanted to spend outpoint outPointToSpend.N + 1: {outPointToSpend.N + 1}.");
        }

        private static async Task<TranslationContract> ObtainBitcoinTransactionCustomMessage(string transactionId)
        {
            var url = $"https://api.blockcypher.com/v1/btc/test3/txs/{transactionId}";
            var request = WebRequest.Create(url);
            string text;

            var response = (HttpWebResponse) await request.GetResponseAsync(); //request.GetResponse()

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            dynamic json = JObject.Parse(text);
            JArray outputs = json.outputs;
            var message = outputs.Where(output =>
                {
                    dynamic dynOutput = output; // we return only the tx-out element with type null-data that has a custom string message
                    return dynOutput.script_type == "null-data" && !string.IsNullOrEmpty((string) dynOutput.data_string);
                })
                .Select(output => (string)((dynamic)output).data_string)
                .FirstOrDefault();

            return message;
        }

        static async Task<decimal> GetUnspentBalance(string publicAddress)
        {
            var network = Network.TestNet;
            var client = new QBitNinjaClient(network);
            var balanceModel = await client.GetBalance(new BitcoinPubKeyAddress(publicAddress));

            var unspentCoins = new List<Coin>();
            foreach (var operation in balanceModel.Operations)
            {
                unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
            }

            var balance = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));

            return balance;
        }

        static async Task<Coin> GetUnspentBalance(string publicAddress, Money amount)
        {
            var network = Network.TestNet;
            var client = new QBitNinjaClient(network);
            var balanceModel = await client.GetBalance(new BitcoinPubKeyAddress(publicAddress), unspentOnly: true);

            var unspentReceivedCoins = (Coin)balanceModel
                .Operations
                .SelectMany(operation => operation.ReceivedCoins)
                .FirstOrDefault(c => c is Coin coin && coin.Amount > amount);

            return unspentReceivedCoins;
        }

        static async Task<TxIn> GetSpendableTransaction(string publicAddress, Money amount)
        {
            var network = Network.TestNet;
            var client = new QBitNinjaClient(network);
            var balanceModel = await client.GetBalance(new BitcoinPubKeyAddress(publicAddress), unspentOnly: true);

            List<TxIn> unspentCoinsTransactions = balanceModel
                .Operations
                .SelectMany(operation => operation.ReceivedCoins)
                .Where(c => c is Coin coin && coin.Amount > amount)
                .Select(coin => new TxIn { PrevOut = coin.Outpoint })
                .ToList();

            return unspentCoinsTransactions.FirstOrDefault();
        }

        static async Task<uint256> GetLatestSpentTransactionId(string publicAddress)
        {
            var network = Network.TestNet;
            var client = new QBitNinjaClient(network);
            var balanceModel = await client.GetBalance(new BitcoinPubKeyAddress(publicAddress));

            var operation = balanceModel
                .Operations
                .Where(op => op.Amount < 0)
                .OrderByDescending(op => op.Height)
                .First();

            return operation.TransactionId;
        }

        static async Task<List<TxIn>> GetUnspentTransactions(string publicAddress)
        {
            var network = Network.TestNet;
            var client = new QBitNinjaClient(network);
            var balanceModel = await client.GetBalance(new BitcoinPubKeyAddress(publicAddress));

            List<TxIn> unspentCoinsTransactions = balanceModel
                .Operations
                .SelectMany(operation => operation.ReceivedCoins)
                .Select(coin => new TxIn { PrevOut = coin.Outpoint })
                .ToList();

            return unspentCoinsTransactions;
        }
    }
}
