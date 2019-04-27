using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CalipoBot.Commands
{
    public class GroupSubscriptionEntity : TableEntity
    {
        public string UserId { get; set; }

        public string GroupName { get; set; }
    }

    public class JoinGroupCommand : IBotCommand
    {
        public string Command
        {
            get
            {
                return "/joingroup";
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
                        TableQuery.CombineFilters
                        (
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, message.Chat.Id.ToString()),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, message.From.Id.ToString())
                        ),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("GroupName", QueryComparisons.Equal, notificationGroupName)
                    )
                );

            var existingSubscriptions = await subscriptionsTable.ExecuteQuerySegmentedAsync(query, null);

            if (existingSubscriptions.FirstOrDefault() != null)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"You are already subscribed to the group \"{notificationGroupName}\"", replyToMessageId: message.MessageId);

                return;
            }

            await subscriptionsTable.ExecuteAsync(TableOperation.Insert(new GroupSubscriptionEntity()
            {
                PartitionKey = message.Chat.Id.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                UserId = message.From.Id.ToString(),
                GroupName = notificationGroupName,
            }));

            await botClient.SendTextMessageAsync(message.Chat.Id, $"You have been subscribed to the group \"{notificationGroupName}\"", replyToMessageId: message.MessageId);
        }
    }
}
