//using Accord.WindowsForms;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
//using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using System.Drawing;
using Accord.IO;
using AForge.WindowsForms;
using Telegram.Bot.Types.InputFiles;
using System.Drawing.Imaging;

namespace NeuralNetwork1
{
    class TLGBotik
    {
        public TelegramBotClient botik = null;

        private MagicEye magicEye = null;

        private string[] classLabels;

        public delegate string TalkToAIML(string phrase, Telegram.Bot.Types.User tlgUsr);
        private TalkToAIML talk;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public TLGBotik(Settings settings, TalkToAIML talkTo)
        {
            // Initialize components
            magicEye = new MagicEye
            {
                settings = settings
            };
            magicEye.settings.processImg = true;
            magicEye.settings.border = 165;

            // Read bot token
            var key = System.IO.File.ReadAllText("..\\..\\botKey.txt");
            botik = new TelegramBotClient(key);

            // Assign AIML delegate
            talk = new TalkToAIML(talkTo);
        }

        public void StartReceiving()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message } // Only listen for messages
            };

            Console.WriteLine("Bot started receiving updates...");
            botik.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            Console.WriteLine(message.Text);

            if (message.Text == "/start")
            {
                var result = talk("СТАРТ", message.From);
                botClient.SendTextMessageAsync(message.Chat.Id, result, cancellationToken: cancellationToken);
            }
            else if (message.Type == MessageType.Photo)
            {
                var fileId = message.Photo[message.Photo.Length - 1].FileId;
                Telegram.Bot.Types.File fileInfo = botik.GetFileAsync(fileId).Result;
                var stream = new MemoryStream();

                await botClient.DownloadFileAsync(fileInfo.FilePath, stream, cancellationToken);
                var img = System.Drawing.Image.FromStream(stream);

                var bitmap_img = new System.Drawing.Bitmap(img);

                string autoName = magicEye.Predict(bitmap_img, false);

                var answer = talk(autoName, message.From);
                botClient.SendTextMessageAsync(message.Chat.Id, answer, cancellationToken: cancellationToken);
            }
            else if (message.Type == MessageType.Text)
            {
                var result = talk(message.Text, message.From);
                var filePath = "BotSave\\" + message.From;
                System.IO.Directory.CreateDirectory(filePath);
                var fileName = message.From + ".txt";
                var time = message.Date.ToString();
                var path = Path.Combine(filePath, fileName);

                using (StreamWriter sw = System.IO.File.AppendText(path))
                {
                    sw.WriteLine(time);
                    sw.WriteLine(message.From + ":");
                    sw.WriteLine(message.Text);
                    sw.WriteLine("Bot:");
                    sw.WriteLine(result + "\n");
                }

                botClient.SendTextMessageAsync(message.Chat.Id, result, cancellationToken: cancellationToken);
            }
            else
            {
                var result = talk("СТРАННОЕ", message.From);
                botClient.SendTextMessageAsync(message.Chat.Id, result, cancellationToken: cancellationToken);
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
