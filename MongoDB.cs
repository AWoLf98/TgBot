using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using Google.Cloud.Dialogflow.V2;
using Telegram.Bot.Types;

namespace TelegramBot
{
    class MongoDB
    {
        readonly static MongoClient client = new MongoClient("mongodb+srv://admin:admin@cluster0.gt18j.mongodb.net/Cluster0?retryWrites=true&w=majority");
        readonly static IMongoDatabase database = client.GetDatabase("bill_user_info");
        public static async void WriteUserBotMessages(QueryResult result, Message message)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("user_messege_info");

            BsonDocument document = new BsonDocument { { "user_id", message.From.Id },
                { "user_full_name",ValueNull($"{message.From.FirstName} {message.From.LastName}") },
                { "username", ValueNull(message.From.Username) },
                { "data_time", DateTime.Now.ToString("ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’") },
                { "query_text", ValueNull(result.QueryText) },
                { "intent_detected", ValueNull(result.Intent.DisplayName) },
                { "intent_confidence", result.IntentDetectionConfidence },
                { "fulfillment_text", ValueNull(result.FulfillmentText) }
            };
            await collection.InsertOneAsync(document);
        }
        public static async void WriteFileFromMessage(Message message, string path)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("user_file_info");
            BsonDocument document = new BsonDocument { { "user_id", message.From.Id },
                { "user_full_name", ValueNull($"{message.From.FirstName} {message.From.LastName}") },
                { "username", ValueNull(message.From.Username) },
                { "data_time", DateTime.Now.ToString("ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’") },
                { "path", path }
            };
            await collection.InsertOneAsync(document);
        }
        public static async void WriteAdminMessageInfo(Message message, Message messageAdmin, string answer)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("user_admin_messege_info");
            BsonDocument document = new BsonDocument { { "user_id", message.From.Id },
                { "user_full_name", ValueNull($"{message.From.FirstName} {message.From.LastName}") },
                { "username", ValueNull(message.From.Username) },
                { "data_time", DateTime.Now.ToString("ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’") },
                { "user_question", ValueNull(answer) },
                { "admin_answer", ValueNull(messageAdmin.Text) }
            };
            await collection.InsertOneAsync(document);
        }
        public static string ValueNull(string str)
        {
            if (str == null || str =="")
                return "NULL";
            else
                return str;
        }
    }
}
