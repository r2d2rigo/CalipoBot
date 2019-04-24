using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CalipoBot.Commands
{
    public interface IBotCommand
    {
        string Command { get; }
        AccessLevel AccessLevel { get; }

        Task ExecuteAsync(ITelegramBotClient botClient, Message message);
    }

    public interface IBotCommand2
    {
        Task ExecuteAsync(Message message, Chat chat, MessageEntity entity);
    }
}
