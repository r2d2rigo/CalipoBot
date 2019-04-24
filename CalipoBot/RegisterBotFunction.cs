using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace CalipoBot
{
    public static class RegisterBotFunction
    {
        [FunctionName("RegisterBotFunction")]
        public static async Task Run([TimerTrigger("0 5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
            var webhookUrl = Environment.GetEnvironmentVariable("BOT_WEBHOOK_URL");

            var botClient = new TelegramBotClient(botToken);
            await botClient.SetWebhookAsync(webhookUrl);

            log.LogInformation($"Bot webhook registration executed successfully.");
        }
    }
}
