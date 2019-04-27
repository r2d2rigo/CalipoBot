using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CalipoBot.Commands
{
    public class TarotCommand : IBotCommand
    {
        private static readonly string[] MAJOR_ARCANA_CARDS =
        {
            "🧙 El Mago",
            "🧙‍♀️ La Sacerdotisa",
            "👸 La Emperatriz",
            "🤴 El Emperador",
            "⛪ El Hierofante",
            "💑 Los Enamorados",
            "🐎 El Carro",
            "👩‍⚖️ La Justicia",
            "🧔 El Ermitaño",
            "🎡 La Rueda de la Fortuna",
            "💪 La Fuerza",
            "🙃 El Colgado",
            "💀 La Muerte",
            "👼 La Templanza",
            "👿 El Diablo",
            "🗼 La Torre",
            "⭐ La Estrella",
            "🌚 La Luna",
            "🌞 El Sol",
            "⚖️ El Juicio",
            "🌍 El Mundo",
        };

        private static readonly Random RANDOM_GENERATOR = new Random();

        public string Command
        {
            get
            {
                return "/tarot";
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
            var cardsList = new List<string>();
            cardsList.AddRange(MAJOR_ARCANA_CARDS);

            var messageText = new StringBuilder();

            messageText.AppendLine("**PASADO**");
            var randomIndex = RANDOM_GENERATOR.Next(0, cardsList.Count);
            var selectedCard = cardsList[randomIndex];
            cardsList.RemoveAt(randomIndex);
            messageText.AppendLine(selectedCard);
            messageText.AppendLine();

            messageText.AppendLine("**PRESENTE**");
            randomIndex = RANDOM_GENERATOR.Next(0, cardsList.Count);
            selectedCard = cardsList[randomIndex];
            cardsList.RemoveAt(randomIndex);
            messageText.AppendLine(selectedCard);
            messageText.AppendLine();

            messageText.AppendLine("**FUTURO**");
            randomIndex = RANDOM_GENERATOR.Next(0, cardsList.Count);
            selectedCard = cardsList[randomIndex];
            cardsList.RemoveAt(randomIndex);
            messageText.AppendLine(selectedCard);

            await botClient.SendTextMessageAsync(message.Chat.Id, messageText.ToString(), Telegram.Bot.Types.Enums.ParseMode.Markdown, replyToMessageId: message.MessageId);
        }
    }
}