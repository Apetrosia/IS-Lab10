using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIMLbot;

namespace NeuralNetwork1
{
    class AIMLBotik
    {
        Bot myBot;
        Dictionary<string, User> tlgToAIML;
        public AIMLBotik()
        {
            tlgToAIML = new Dictionary<string, User>();
            myBot = new Bot();
            Console.WriteLine(myBot.PathToAIML);
            Console.WriteLine(myBot.PathToConfigFiles);
            myBot.loadSettings();
            myBot.isAcceptingUserInput = false;
            myBot.loadAIMLFromFiles();
            myBot.isAcceptingUserInput = true;
        }

        public string Talk(string phrase, Telegram.Bot.Types.User user)
        {
            if (!tlgToAIML.ContainsKey(user.Id.ToString()))
                tlgToAIML[user.Id.ToString()] = new User(user.Id.ToString(), myBot);
            Request r = new Request(phrase, tlgToAIML[user.Id.ToString()], myBot);
            Result res = myBot.Chat(r);
            return res.Output;
        }
    }
}
