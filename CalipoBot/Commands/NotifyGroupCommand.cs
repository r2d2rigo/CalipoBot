using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CalipoBot.Commands
{
    public class NotifyGroupCommand : IBotCommand
    {
        public string Command
        {
            get
            {
                return "/notifygroup";
            }
        }

        public AccessLevel AccessLevel
        {
            get
            {
                return AccessLevel.Public;
            }
        }


        public async Task ExecuteAsync(ITelegramBotClient botClient, Message message)
        {
            var messageParts = message.Text.Split(' ');

            if (messageParts.Length < 2)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Please specify a group name", replyToMessageId: message.MessageId);

                return;
            }

            var notificationGroupName = messageParts[1].ToLowerInvariant();

            var tableClient = new CloudTableClient(
                new Uri(Environment.GetEnvironmentVariable("STORAGE_URL")),
                new StorageCredentials(Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME"), Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY")));

            var subscriptionsTable = tableClient.GetTableReference("NotificationGroups");
            await subscriptionsTable.CreateIfNotExistsAsync();

            var query = new TableQuery<GroupSubscriptionEntity>().
                Where(
                    TableQuery.CombineFilters
                    (
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, message.Chat.Id.ToString()),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("GroupName", QueryComparisons.Equal, notificationGroupName)
                    )
                );

            var existingSubscriptions = await subscriptionsTable.ExecuteQuerySegmentedAsync(query, null);

            if (existingSubscriptions.Count() == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"The group \"{notificationGroupName}\" does not exist", replyToMessageId: message.MessageId);

                return;
            }

            var messageText = new StringBuilder();

            foreach (var result in existingSubscriptions)
            {
                var user = await botClient.GetChatMemberAsync(message.Chat.Id, int.Parse(result.UserId));

                messageText.Append($"@{user.User.Username} ");
            }

            messageText.Append($"{notificationGroupName}");

            await botClient.SendTextMessageAsync(message.Chat.Id, messageText.ToString());
        }
    }
}
