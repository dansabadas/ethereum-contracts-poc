﻿using System;
using System.Threading.Tasks;
using Nethereum.Web3;

namespace NethereumSample
{
    class Program
    {
        //static void Main()
        //{
        //    GetAccountBalance().GetAwaiter().GetResult();
        //    Console.ReadLine();
        //}

        static async Task Main()
        {
            await GetAccountBalance();
            Console.ReadLine();
        }

        private static async Task GetAccountBalance()
        {
            var web3 = new Web3("https://mainnet.infura.io");

            var balance = await web3.Eth.GetBalance.SendRequestAsync("0xde0b295669a9fd93d5f28d9ec85e40f4cb697bae");
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in Ether: {etherAmount}");
        }
    }
}
