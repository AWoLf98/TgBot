using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace TelegramBot
{
    //клас для роботи з функціоналом бота (меню/кнопки/опції), а також обробники подій
    class FunctionalTelegramBot
    {
        private static TelegramBotClient bot = new TelegramBotClient("1621728687:AAF8rgQzchqPQ2P9gRUYVh_cw6LNMLQ_blM");
        //тут потрібно вказати id адміністратора
        private static long adminId = 341059938;
        private static long userAnswer = -1;
        private static bool ignoreAnswer = false;

        Dictionary<long, bool> openChatAdminValue = new Dictionary<long, bool>();
        List<Message> messagesUser = new List<Message>();
        List<Message> deleteUsers = new List<Message>();
        public static TelegramBotClient Bot
        {
            get => bot;
        }

        //public static async void BotOnCallbackQueryReceive(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        //{
        //    string buttonText = e.CallbackQuery.Data;
        //    switch (buttonText)
        //    {
        //        case "Тех. Підтримка":
        //            comWithAdmin.StartChat();
        //            await bot.SendTextMessageAsync(e.CallbackQuery.Id, "Розпочато чат з адміністратором");
        //            break;
        //        case "Завершити":
        //            comWithAdmin.StopChat();
        //            await bot.SendTextMessageAsync(e.CallbackQuery.Id, "Завершити чат з адміністратором");
        //            //bot.
        //            //bot.StopReceiving();
        //            break;
        //    }
        //    //await bot.SendTextMessageAsync(e.CallbackQuery.Id, e.CallbackQuery.Data);
        //    //await bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, url: buttonText);
        //}

        public async void Bot_OnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Message message = e.Message;
            long userId = message.From.Id;

            string name = $"{message.From.FirstName} {message.From.LastName} {message.From.Username}";
            Console.WriteLine($"{name} send message: {message.Text}");

            if (message.Text == "/start" && !openChatAdminValue.ContainsKey(userId))
                openChatAdminValue.Add(userId, false);
            if (openChatAdminValue.ContainsKey(userId))
                if (!openChatAdminValue[userId])
                    if (message == null || message.Type != MessageType.Text)
                        DownloadMedia(message);
                    else
                        switch (message.Text)
                        {
                            case "/start":
                                StartBot(message);
                                break;
                            case "/resources":
                                ResourcesBot(message);
                                break;
                            case "/keyboard":
                                KeyboardBot(message);
                                break;
                            case "Тех. Підтримка":
                                openChatAdminValue[userId] = true;
                                await bot.SendTextMessageAsync(userId, "Розпочато чат з Тех. Підтримкою. Для завершення натисніть кнопку Завершити на клавіатурі.");
                                break;
                            case "Завершити":
                                await bot.SendTextMessageAsync(userId, "Ви не розпочинали чат з Тех. Підтримкою");
                                break;
                            default:
                                DialogFlowSession.GetDialogFlowAnswer(bot, message);
                                break;
                        }
                else
                {
                    if (message.Text == "Завершити")
                    {
                        openChatAdminValue[message.From.Id] = false;
                        await bot.SendTextMessageAsync(userId, "Завершення чату з Тех. Підтримкою");
                    }
                    else
                    {
                        if (message == null || message.Type != MessageType.Text)
                            return;
                        if (userId == adminId)
                        {
                            Regex rg = new Regex(@"/\d*$");
                            if (message.Text == "/scheduler")
                            {
                                string mesAdmin = "";
                                foreach (Message mes in messagesUser)
                                {
                                    mesAdmin += $"/{mes.From.Id} \n";
                                    mesAdmin += $"{MongoDB.ValueNull(mes.From.FirstName)} {MongoDB.ValueNull(mes.From.LastName)} \n";
                                    mesAdmin += $"{MongoDB.ValueNull(mes.From.Username)} \n";
                                    mesAdmin += $"{MongoDB.ValueNull(mes.Text)} \n";
                                }
                                await bot.SendTextMessageAsync(adminId, MongoDB.ValueNull(mesAdmin));
                            }
                            else if (message.Text == "/delete" && userAnswer != -1)
                            {
                                foreach (Message mes in messagesUser)
                                    if (mes.From.Id == userAnswer)
                                        deleteUsers.Add(mes);
                                foreach (Message mes in deleteUsers)
                                    messagesUser.Remove(mes);
                                await bot.SendTextMessageAsync(adminId, $"Видалено {userAnswer}");
                            }
                            else if (message.Text == "/ignore" && userAnswer != -1)
                            {
                                ignoreAnswer = true;
                            }
                            else if (rg.IsMatch(message.Text))
                            {
                                foreach (Message mes in messagesUser)
                                {
                                    string str = message.Text.TrimStart('/');
                                    if (mes.From.Id == int.Parse(str))
                                    {
                                        userAnswer = mes.From.Id;
                                        break;
                                    }
                                }
                            }
                            else if (userAnswer != -1)
                            {
                                await bot.ForwardMessageAsync(new ChatId(userAnswer), new ChatId(adminId), message.MessageId);
                                string answer = "";
                                Message messageAnswerInfo = null;
                                foreach (Message mes in messagesUser)
                                {
                                    if (mes.From.Id == userAnswer)
                                    {
                                        answer += mes.Text;
                                        deleteUsers.Add(mes);
                                        messageAnswerInfo = mes;
                                    }
                                }

                                foreach(Message mes in deleteUsers )
                                    messagesUser.Remove(mes);

                                if(message.Text != null)
                                {
                                    MongoDB.WriteAdminMessageInfo(messageAnswerInfo, message, answer);
                                    if(!ignoreAnswer)
                                        DialogFlowSession.CreateNewIntent(message.Text, answer);
                                }
                                ignoreAnswer = false;
                                userAnswer = -1;
                            }
                            else
                                await bot.SendTextMessageAsync(message.From.Id, "Невірна команда!!! Оберіть завдання /scheduler");
                        }
                        else
                        {
                            await Bot.ForwardMessageAsync(new ChatId(adminId), new ChatId(userId), message.MessageId);
                            messagesUser.Add(message);
                        }
                    }
                }
            else
                await bot.SendTextMessageAsync(userId, "Для початку роботи з ботом введіть /start");
        }
        private static async void StartBot(Message message)
        {
            string text = "Список команд доступних в чат боті: \n" +
                "**/start** - початок роботи з ботом та вивід доступних команд; \n" +
                "**/resources** - вивід всіх інформаційних ресурсів Bill \n" +
                "**/keyboard** - вивід клавіатури для спілкування з Тех. Підтримкою \n" +
                "`Важливо!!! Чат-бот спілкується винятково українською мовою. \n" +
                "Спілкування реалізоване у вигляді запитання-відповідь, \n" +
                "тому ваше питання має бути включене в одне повідомлення. \n" +
                "Якщо ви надсилаєте більше одного повідомлення, \n" +
                "то ризикуєте що вас не зрозуміють. \n" +
                "Спілкування з Тех. Підтримкою реалізоване аналогічним чином.`";
            await bot.SendTextMessageAsync(message.From.Id, text, ParseMode.Markdown);
            if(message.From.Id == adminId)
            {
                text = "Команди адміністратора: \n" +
                    "**/scheduler** - список задач \n" +
                    "**/delete** - видалити поточну задачу \n" +
                    "**/ignore** - не надсилати задачу для навчання моделі агенту \n";
                await bot.SendTextMessageAsync(message.From.Id, text, ParseMode.Markdown);
            }
        }
        private static async void ResourcesBot(Message message)
        {
            InlineKeyboardMarkup inlineKeyBoard = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Головний сайт", "http://bill.univ.kiev.ua/"),
                            InlineKeyboardButton.WithUrl("Канал Bill KNU", "https://t.me/BILL_KNU")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Форум", "http://talks.univ.kiev.ua/viewforum.php?f=9"),
                            InlineKeyboardButton.WithUrl("Веб-ресурс", "https://help.icc.knu.ua/uk")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Чат","https://t.me/floodBILLKNU")
                        }
                    });
            await bot.SendTextMessageAsync(message.From.Id, "Доступні ресурси: ", replyMarkup: inlineKeyBoard);
        }
        private static async void KeyboardBot(Message message)
        {
            ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new[] {
                        new[]
                        {
                            new KeyboardButton("Тех. Підтримка"),
                            new KeyboardButton("Завершити")
                        },
                        new[]
                        {
                            new KeyboardButton("Контакт") { RequestContact = true },
                            new KeyboardButton("Геолокація") { RequestLocation = true}
                        }
                    }); ;
            await bot.SendTextMessageAsync(message.From.Id, "Клавіатура для спілкування з Тех. Підтримкою", replyMarkup: replyKeyboard);
        }
        private static async void DownloadMedia(Message message)
        {
            string fileId = "";
            //тут потрібно вказати шлях в якому бажаєте зберігати файли
            string path = @"D:";
            if (message.Type == MessageType.Voice)
                fileId = message.Voice.FileId;
            else if (message.Type == MessageType.Video)
                fileId = message.Video.FileId;
            else if (message.Type == MessageType.Audio)
                fileId = message.Audio.FileId;
            else if (message.Type == MessageType.Document)
                fileId = message.Document.FileId;
            else if (message == null || message.Type != MessageType.Text)
                return;

            File fileInfo = await bot.GetFileAsync(fileId);
            path += $"\\{DateTime.Now.ToString("yyyy MM dd HH-mm-ss")}_{GetFileName(fileInfo.FilePath)}";
            // Скачуєте 
            using (var fileStream = System.IO.File.OpenWrite(path))
            {
                await bot.DownloadFileAsync(
                  filePath: fileInfo.FilePath,
                  destination: fileStream
                );
            }
            MongoDB.WriteFileFromMessage(message, path);
        }
        private static string GetFileName(string path)
        {
            int position = path.LastIndexOf("/");
            return path.Substring(position + 1);
        }
    }
}
