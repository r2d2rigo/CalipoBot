using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CalipoBot.Commands
{
    public class ShrughCommand : IBotCommand
    {
        public string Command
        {
            get
            {
                return "/shrug";
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
            await botClient.SendTextMessageAsync(message.Chat.Id, @"¯\_(ツ)_/¯");
        }
    }
}