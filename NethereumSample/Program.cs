using System;
using System.Threading.Tasks;

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
            await AccountBalance.GetAccountBalance();
            Console.ReadLine();
        }
    }
}
