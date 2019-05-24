using CalipoBot.Processors;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CalipoBot.Commands
{
    public class LaroRankingCommand : IBotCommand
    {
        public string Command
        {
            get
            {
                return "/larorank";
            }
        }

        public AccessLevel AccessLevel
        {
            get
            {
                return AccessLevel.Administrator;
            }
        }


        public async Task ExecuteAsync(ITelegramBotClient botClient, Message message)
        {
            var tableClient = new CloudTableClient(
                new Uri(Environment.GetEnvironmentVariable("STORAGE_URL")),
                new StorageCredentials(Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME"),
                Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY")));

            var laroRankingTable = tableClient.GetTableReference("LaroRanking");
            await laroRankingTable.CreateIfNotExistsAsync();

            var query = new TableQuery<LaroRankingEntity>().
                Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, message.Chat.Id.ToString())
                );

            var rankingEntries = await laroRankingTable.ExecuteQuerySegmentedAsync(query, null);
            var groupedRankingEntries = rankingEntries.GroupBy(e => e.UserId).OrderByDescending(g => g.Count());

            foreach (var rankingUser in groupedRankingEntries)
            {
                var userInfo = await botClient.GetChatMemberAsync(message.Chat.Id, rankingUser.Key);

                await botClient.SendTextMessageAsync(message.Chat.Id, $"{userInfo.User.Username} : {rankingUser.Count()}");
            }
        }
    }
}
