using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Azure.WebJobs.Host.Config;
using Telegram.Bot.Types.Enums;
using CalipoBot.Processors;

namespace CalipoBot
{
    public static class BotCallbackFunction
    {
        [FunctionName("BotCallbackFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

            var botClient = new TelegramBotClient(botToken);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var update = JsonConvert.DeserializeObject<Update>(requestBody);

            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var processor = new BotCommandMessageProcessor();
                            await processor.ProcessMessageAsync(botClient, update.Message);

                            var processor2 = new LaroMessageProcessor();
                            await processor2.ProcessMessageAsync(botClient, update.Message);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error processing update");
            }

            return new OkResult();
        }
    }
}
