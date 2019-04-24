using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CalipoBot.Commands
{
    public class EchoCommand : IBotCommand
    {
        public string Command
        {
            get
            {
                return "/echo";
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
            await botClient.SendTextMessageAsync(message.Chat.Id, "Echo: " + message.Text, replyToMessageId: message.MessageId);
        }
    }
}