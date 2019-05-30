using CalipoBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CalipoBot.Processors
{
    public class BotCommandMessageProcessor
    {
        private List<IBotCommand> commandProcessors;

        public BotCommandMessageProcessor()
        {
            this.commandProcessors = new List<IBotCommand>();
            this.commandProcessors.Add(new EchoCommand());
            this.commandProcessors.Add(new ShrughCommand());
            this.commandProcessors.Add(new LennyCommand());
            this.commandProcessors.Add(new TarotCommand());
            this.commandProcessors.Add(new JoinGroupCommand());
            this.commandProcessors.Add(new LeaveGroupCommand());
            this.commandProcessors.Add(new NotifyGroupCommand());
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

                        if (message.Entities.Length >= 1)
                        {
                            var firstEntity = message.Entities[0];

                            if (firstEntity.Type == MessageEntityType.BotCommand && firstEntity.Offset == 0)
                            {
                                var botCommand = message.EntityValues.First().ToLower();
                                var botParts = botCommand.Split('@');
                                if (botParts.Length > 1)
                                {
                                    if (botParts[1] != (await botClient.GetMeAsync()).Username.ToLower())
                                    {
                                        return;
                                    }
                                }

                                botCommand = botParts[0];

                                var processor = this.commandProcessors.Where(p => p.Command == botCommand).FirstOrDefault();

                                if (processor == null)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Unknown command " + botCommand, replyToMessageId: message.MessageId);
                                    return;
                                }

                                if (processor.AccessLevel > AccessLevel.Public)
                                {

                                }                                

                                switch (processor.AccessLevel)
                                {
                                    case AccessLevel.Public:
                                        break;
                                    case AccessLevel.Administrator:
                                        {
                                            var isAdmin = await IsAdministratorAsync(botClient, message.Chat, message.From);
                                            var isOwner = await IsOwnerAsync(message.From);

                                            if (!isAdmin && !isOwner)
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, only admins can run that command.", replyToMessageId: message.MessageId);

                                                return;
                                            }
                                        }
                                        break;
                                    case AccessLevel.Owner:
                                        {
                                            var isOwner = await IsOwnerAsync(message.From);

                                            if (!isOwner)
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, only owners can run that command.", replyToMessageId: message.MessageId);

                                                return;
                                            }
                                        }
                                        break;
                                }

                                await processor.ExecuteAsync(botClient, message);
                            }
                        }
                    }
                    break;
            }
        }

        private static async Task<bool> IsAdministratorAsync(ITelegramBotClient botClient, Chat chat, User user)
        {
            var administrators = await botClient.GetChatAdministratorsAsync(chat.Id);

            if (!administrators.Any(a => a.User.Id == user.Id))
            {
                return true;
            }

            return false;
        }

        private static async Task<bool> IsOwnerAsync(User user)
        {
            var adminUserIds = Environment.GetEnvironmentVariable("BOT_ADMIN_USERIDS");
            var adminIds = adminUserIds.Split(';');

            foreach (var id in adminIds)
            {
                var idNumber = int.Parse(id);

                if (user.Id == idNumber)
                {
                    return true;
                }
            }

            return false;
        }
    }
}