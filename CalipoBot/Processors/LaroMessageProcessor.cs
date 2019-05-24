using CalipoBot.Commands;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CalipoBot.Processors
{
    public class LaroUrlEntity : TableEntity
    {
        public string Url { get; set; }

        public int MessageId { get; set; }
    }

    public class LaroRankingEntity : TableEntity
    {
        public int UserId { get; set; }

        public string Url { get; set; }
    }

    public class LaroMessageProcessor
    {
        public LaroMessageProcessor()
        {
        }

        public async Task ProcessMessageAsync(ITelegramBotClient botClient, Message message)
        {
            switch (message.Type)
            {
                case MessageType.Text:
                    {
                        var chat = message.Chat;

                        if (chat == null)
                        {
                            return;
                        }

                        if (message.Entities == null)
                        {
                            return;
                        }

                        var tableClient = new CloudTableClient(
                            new Uri(Environment.GetEnvironmentVariable("STORAGE_URL")),
                            new StorageCredentials(Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME"), Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY")));

                        var urlsTable = tableClient.GetTableReference("LaroUrls");
                        await urlsTable.CreateIfNotExistsAsync();

                        var laroRankingTable = tableClient.GetTableReference("LaroRanking");
                        await laroRankingTable.CreateIfNotExistsAsync();

                        var entityIndex = 0;

                        foreach (var messageEntity in message.Entities)
                        {
                            switch (messageEntity.Type)
                            {
                                case MessageEntityType.Url:
                                    {
                                        var urlString = message.EntityValues.Skip(entityIndex).First().ToLowerInvariant();

                                        var query = new TableQuery<GroupSubscriptionEntity>().
                                            Where(
                                                TableQuery.CombineFilters
                                                (
                                                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, message.Chat.Id.ToString()),
                                                    TableOperators.And,
                                                    TableQuery.GenerateFilterCondition("Url", QueryComparisons.Equal, urlString)
                                                )
                                            );

                                        var existingUrl = await urlsTable.ExecuteQuerySegmentedAsync(query, null);

                                        if (existingUrl.FirstOrDefault() != null)
                                        {
                                            await laroRankingTable.ExecuteAsync(TableOperation.Insert(new LaroRankingEntity()
                                            {
                                                PartitionKey = message.Chat.Id.ToString(),
                                                RowKey = Guid.NewGuid().ToString(),
                                                Url = urlString,
                                                UserId = message.From.Id,
                                                Timestamp = DateTimeOffset.UtcNow,
                                            }));

                                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Laro, laro, esto es un repost bien claro", replyToMessageId: message.MessageId);

                                            return;
                                        }

                                        await urlsTable.ExecuteAsync(TableOperation.Insert(new LaroUrlEntity()
                                        {
                                            PartitionKey = message.Chat.Id.ToString(),
                                            RowKey = Guid.NewGuid().ToString(),
                                            MessageId = message.MessageId,
                                            Url = urlString,
                                            Timestamp = DateTimeOffset.UtcNow,
                                        }));
                                    }
                                    break;
                            }

                            entityIndex++;
                        }
                    }
                    break;
            }
        }
    }
}