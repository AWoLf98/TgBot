using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Dialogflow.V2;
using Telegram.Bot;
using Telegram.Bot.Types;
//using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    //клас для роботи з dialogflow
    class DialogFlowSession
    {
        private const string projectId = "billagent-gnul";
        private static string languageCode = "uk";
        public static async void GetDialogFlowAnswer(TelegramBotClient Bot, Message message)
        {
            SessionsClient client = SessionsClient.Create();
            long id = message.From.Id;
            string text = message.Text;
            string sessionId = id.ToString();

            DetectIntentResponse response = client.DetectIntent(
                session: SessionName.FromProjectSession(projectId, sessionId),
                queryInput: new QueryInput()
                {
                    Text = new TextInput()
                    {
                        Text = text,
                        LanguageCode = languageCode
                    }
                }
                );
            QueryResult queryResult = response.QueryResult;
            MongoDB.WriteUserBotMessages(queryResult, message);
            await Bot.SendTextMessageAsync(id, queryResult.FulfillmentText);
        }
        public static void CreateNewIntent(string messageText, string trainingPhrasesPart)
        {
            IntentsClient client = IntentsClient.Create();

            Intent.Types.Message.Types.Text text = new Intent.Types.Message.Types.Text();
            text.Text_.Add(messageText);

            Intent.Types.Message message = new Intent.Types.Message()
            {
                Text = text
            };

            List<Intent.Types.TrainingPhrase.Types.Part> phraseParts = new List<Intent.Types.TrainingPhrase.Types.Part>();
            phraseParts.Add(new Intent.Types.TrainingPhrase.Types.Part()
            {
                Text = trainingPhrasesPart
            });

            Intent.Types.TrainingPhrase trainingPhrase = new Intent.Types.TrainingPhrase();
            trainingPhrase.Parts.AddRange(phraseParts);

            Intent intent = new Intent();
            intent.DisplayName = trainingPhrasesPart;
            intent.Messages.Add(message);
            intent.TrainingPhrases.Add(trainingPhrase);

            Intent newIntent = client.CreateIntent(
                parent: new AgentName(projectId),
                intent: intent
            );

            Console.WriteLine($"Created Intent: {newIntent.Name}");
        }
    }
}

