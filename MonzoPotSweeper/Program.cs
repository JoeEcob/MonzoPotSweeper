using System;
using Monzo;
using System.Linq;
using static System.Console;
using Microsoft.Extensions.Configuration;
using FileBasedMonzoProvider;

namespace MonzoPotSweeper
{
    class Program
    {
        static void Main(string[] args)
        {
            var configRoot = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var monzoProvider = new MonzoProvider(configRoot);

            var accessToken = monzoProvider.GetAccessToken().Result;

            var client = new MonzoClient(accessToken.Value);

            var accounts = client.GetAccountsAsync().Result;

            var currentAccount = accounts.Single(x => !x.Closed);

            var balance = client.GetBalanceAsync(currentAccount.Id).Result;

            var pots = client.GetPotsAsync().Result;

            var targetPot = pots.First(x => !x.Deleted); // TODO - support multiple pots

            var isLive = args.Length > 0 && args[0] == "live";

            var amountToDeposit = isLive ? balance.Value : 1;

            var random = new Random().Next();

            var deDupeId = $"{DateTime.UtcNow}-{random}";

            WriteLine($"Previous pot balance: {(decimal)targetPot.Balance / 100}{balance.Currency}");
            WriteLine($"Transferring {(decimal)amountToDeposit / 100}{balance.Currency} into the '{targetPot.Name}' pot");

            var response = client.DepositIntoPotAsync(targetPot.Id, currentAccount.Id, amountToDeposit, deDupeId).Result;

            WriteLine($"Success! New balance is {(decimal)response.Balance / 100}{response.Currency}");
        }
    }
}
