using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using LaunchBot.Helpers;
using LaunchBot.Models;
using LaunchBot.Models.EventArgs;
using StackExchange.Redis;

namespace LaunchBot
{
    public class Controller
    {
        protected MainForm Form;
        //protected readonly string Token = "457947516:AAFHOqCs0yU0jtWVNIch-YbaUWsAUU3nk84";
        protected readonly string ProgrammerId = "433094062";  // @Den2736
        protected readonly string LogId = "-1001363037234";  // BotLog
        protected readonly int[] OwnersId = { 433094062, 34970817 };
        protected string BotUsername;
        protected TelegramBotClient Bot;
        private StateMachine StateMachine;
        protected Texts Texts;

        public Controller()
        {
            Form = new MainForm();
            Form.OnLaunchButtonClick += Form_OnLaunchButtonClick;
            Form.OnTerminateButtonClick += Form_OnTerminateButtonClick;
            Form.SetLaunchButton(true);
            Form.SetTerminateButton(false);
            Form.SetNeutralStatus("Bot is offline.");
        }

        public void ShowView()
        {
            Form.ShowDialog();
        }

        private void Form_OnLaunchButtonClick(object sender, LaunchBotEventArgs e)
        {
            try
            {
                LoadTexts();
            }
            catch (Exception ex)
            {
                string errorMessage = "Ошибка загрузки текстов ";
                FatalError(errorMessage);
                Logger.Log(errorMessage + ex.ToString());
                return;
            }

            try
            {
                BotInitialize(e);
            }
            catch (ArgumentException)
            {
                FatalError("Error 404: bot not found.");
                return;
            }
            catch (Exception ex)
            {
                FatalError("Ошибка инициализации бота", ex);
                return;
            }

            try
            {
                CheckDB();
            }
            catch (Exception ex)
            {
                FatalError("Ошибка базы данных. См.логи");
                Logger.Log("Ошибка базы данных" + ex.ToString());
                return;
            }

            try
            {
                CheckRedis();
            }
            catch (Exception ex)
            {
                FatalError("Ошибка хэшированной базы данных. См.логи", ex);
                Logger.Log("Ошибка хэшированной базы данных" + Environment.NewLine + ex.ToString());
                return;
            }

            try
            {
                ActivateBot();
            }
            catch (Exception ex)
            {
                string errorMessage = "Ошибка активации бота. См.логи. ";
                FatalError(errorMessage);
                Logger.Log(errorMessage + ex.ToString());
                return;
            }
        }

        private void Form_OnTerminateButtonClick(object sender, EventArgs e)
        {
            Bot.StopReceiving();
            Form.SetTerminateButton(false);
            Form.SetLaunchButton(true);
            Form.SetNeutralStatus("Bot is offline.");
            Logger.Log("Bot was disabled by Terminate Button");
        }

        private void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;

            try
            {
                if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                {
                    if (message.Text == "/start")
                    {
                        DBHelper.AddUser(message.From);
                        StateMachine.Start(e);
                    }
                    else
                    {
                        StateMachine.Next(e);
                    }
                }
            }
            catch (Exception ex)
            {
                Form.SetDangerStatus($"Error with message #{message.MessageId}. Look logs for more details.");
                Logger.Log(ex.ToString());
            }
        }

        private void Bot_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            try
            {
                StateMachine.Next(e);
            }
            catch (ArgumentNullException ex)
            {
                Logger.Log(ex.ToString());
                FatalError("Ошибка подключения к базе данных Redis");
            }
            catch (Exception ex)
            {
                Logger.Log("Ошибка:" + ex.ToString());
                ShowError("Ошибка. Подробности см. в логах.");
            }
        }

        protected void FatalError(string errorMessage, Exception e = null)
        {
            try
            {
                string message = $"Fatal error {Bot.GetMeAsync().Result.Username}: {Environment.NewLine} " +
                    $"{errorMessage} {Environment.NewLine}" +
                    $"{e.ToString()}";
                Bot.SendTextMessageAsync(LogId, message);
            }
            catch { }

            Bot.StopReceiving();
            Form.SetLaunchButton(false);
            Form.SetTerminateButton(false);
            Form.SetDangerStatus(errorMessage);
        }

        protected void ShowError(string errorMessage)
        {
            Form.SetDangerStatus(errorMessage);
        }

        private void LoadTexts()
        {
            Form.SetNeutralStatus("Загрузка текстов...");
            Texts = new Texts();
            Texts.Load();
            StateMachine = new StateMachine(Texts);
            Keyboards.SetTexts(Texts);
        }
        private void BotInitialize(LaunchBotEventArgs e)
        {
            Form.SetNeutralStatus("Инициализация бота...");
            Bot = new TelegramBotClient(e.Token);
        }
        private void CheckDB()
        {
            Form.SetNeutralStatus("Проверка базы данных...");
            DBHelper.CheckDB();
        }
        private void CheckRedis()
        {
            Form.SetNeutralStatus("Проверка хэшированной базы данных...");
            if (!RedisConnectorHelper.ConnectionIsWell())
            {
                FatalError("Ошибка хэшированной базы данных. См.логи");
                Logger.Log("Ошибка хэшированной базы данных");
            }
        }
        private void ActivateBot()
        {
            Form.SetNeutralStatus("Активация бота...");
            StateMachine.Bot = Bot;
            Bot.OnMessage += Bot_OnMessage;
            Bot.OnCallbackQuery += Bot_OnCallbackQuery;
            Bot.StartReceiving();
            Form.SetLaunchButton(false);
            Form.SetTerminateButton(true);
            Form.SetBotName(Bot.GetMeAsync().Result.Username);
            string message = "Bot is online.";
            Form.SetSuccessStatus(message);
            Logger.Log(message);
        }
    }

    public class StateMachine : Controller
    {
        public StateMachine(Texts texts)
        {
            Texts = texts;
        }

        public void Next(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            string message = e.CallbackQuery.Message.Text;
            string btnText = e.CallbackQuery.Data;
            try
            {
                // LAMAGNA
                if (message == Texts.Lamagna.Greeting)
                {
                    if (btnText == Texts.Lamagna.Button1)
                    {
                        SetLamagna1State(e);
                    }
                    else if (btnText == Texts.PersonalAccount.PersonalAccountButton)
                    {
                        SetChooseSettingsState(e);
                    }
                }
                else if (message == Texts.Lamagna.Text1)
                {
                    if (btnText == Texts.Other.Positive)
                    {
                        SetLamagna2State(e);
                    }
                    else if (btnText == Texts.Other.Negative)
                    {
                        SetLamagna3State(e);
                    }
                }
                else if (message == Texts.Lamagna.Text2)
                {
                    if (btnText == Texts.Other.Positive)
                    {
                        DBHelper.UserPassedTheBlock(userId, Block.Lamagna);
                        SetTrippier1State(e);
                    }
                    else if (btnText == Texts.Other.Negative)
                    {
                        SetLamagna4State(e);
                    }
                }
                else if (message == Texts.Lamagna.Text3)
                {
                    if (btnText == Texts.Lamagna.Button2)
                    {
                        SetLamagna2State(e);
                    }
                }
                else if (message == Texts.Lamagna.Text4)
                {
                    if (btnText == Texts.Lamagna.Button3)
                    {
                        DBHelper.UserPassedTheBlock(userId, Block.Lamagna);
                        SetTrippier1State(e);
                    }
                }

                // TRIPPIER
                else if (message == Texts.Trippier.Text1)
                {
                    if (btnText == Texts.Other.Positive)
                    {
                        DBHelper.UserPassedTheBlock(userId, Block.Trippier);
                        SetMainProductState(e);
                    }
                    else if (btnText == Texts.Other.Negative)
                    {
                        SetTrippier2State(e);
                    }
                }
                else if (message == Texts.Trippier.Text2)
                {
                    if (btnText == Texts.Other.Positive)
                    {
                        SetTrippier3State(e);
                    }
                    else if (btnText == Texts.Other.Negative)
                    {
                        SetTrippier1State(e);
                    }
                }
                else if (message == Texts.Trippier.Text3)
                {
                    if (btnText == Texts.Trippier.Button1)
                    {
                        SetContactsState(e);
                    }
                }

                // MAIN PRODUCT
                else if (message == Texts.MainProduct.Text1)
                {
                    if (btnText == Texts.MainProduct.Button1)
                    {
                        DBHelper.UserPassedTheBlock(userId, Block.MainProduct);
                        SetContactsState(e);
                    }
                }
                else if (message == Texts.Other.Contacts)
                {
                    if (btnText == Texts.MainProduct.Button2)
                    {
                        SetGreetingState(e.CallbackQuery.From.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Form.SetDangerStatus($"Ошибка. См. логи");
                Logger.Log(ex.ToString());
            }
        }

        public void Start(Telegram.Bot.Args.MessageEventArgs e)
        {
            SetGreetingState(e.Message.From.Id);
        }

        private State GetUserState(int userId)
        {
            State state = State.Greeting;
            try
            {
                var cache = RedisConnectorHelper.Connection.GetDatabase();
                string stateString = cache.StringGet($"Bot{BotUsername}User{userId}State");
                Enum.TryParse(stateString, out state);
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                string errorMessage = "Ошибка: сервер Redis недоступен. ";
                Logger.Log(errorMessage + ex.ToString());
                FatalError(errorMessage);
            }
            return state;
        }

        private void SetState(int userId, State state)
        {
            var cache = RedisConnectorHelper.Connection.GetDatabase();
            cache.StringSet($"Bot{BotUsername}User{userId}State", state.ToString());
        }

        private async void SetGreetingState(int userId)
        {
            var text = Texts.Lamagna.Greeting;
            var keyboard = (OwnersId.Contains(userId)) ? Keyboards.Lamagna.ExtendedGreetingKeyboard : Keyboards.Lamagna.GreetingKeyboard;
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Greeting);
        }

        private async void SetLamagna1State(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.Lamagna.Text1;
            var keyboard = Keyboards.Lamagna.Text1Keyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Lamagna1);
        }

        private async void SetLamagna2State(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.Lamagna.Text2;
            var keyboard = Keyboards.Lamagna.Text2Keyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Lamagna2);
        }

        private async void SetLamagna3State(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.Lamagna.Text3;
            var keyboard = Keyboards.Lamagna.Text3Keyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Lamagna3);
        }

        private async void SetLamagna4State(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.Lamagna.Text4;
            var keyboard = Keyboards.Lamagna.Text4Keyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Lamagna4);
        }

        private async void SetTrippier1State(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.Trippier.Text1;
            var keyboard = Keyboards.Trippier.Text1Keyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Trippier1);
        }

        private async void SetTrippier2State(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.Trippier.Text2;
            var keyboard = Keyboards.Trippier.Text2Keyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Trippier2);
        }

        private async void SetTrippier3State(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.Trippier.Text3;
            var keyboard = Keyboards.Trippier.Text3Keyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Trippier3);
        }

        private async void SetMainProductState(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.MainProduct.Text1;
            var keyboard = Keyboards.MainProduct.Text1Keyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.MainProduct);
        }

        private async void SetContactsState(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.Other.Contacts;
            var keyboard = Keyboards.MainProduct.ContactsKeyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.Contacts);
        }

        private async void SetChooseSettingsState(Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var userId = e.CallbackQuery.From.Id;
            var text = Texts.PersonalAccount.PersnonalAccountGreeting(e.CallbackQuery.From.FirstName);
            var keyboard = Keyboards.PersonalAccount.ChooseSettingKeyboard;
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "");
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.ChooseSettings);
        }

        private async void SetChooseSettingsState(Telegram.Bot.Args.MessageEventArgs e)
        {
            var userId = e.Message.From.Id;
            var text = Texts.PersonalAccount.PersnonalAccountGreeting(e.Message.From.FirstName);
            var keyboard = Keyboards.PersonalAccount.ChooseSettingKeyboard;
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.ChooseSettings);
        }


        public async void Next(Telegram.Bot.Args.MessageEventArgs e)
        {
            var userId = e.Message.From.Id;
            string btnText = e.Message.Text;
            var state = GetUserState(userId);

            try
            {
                if (btnText == Texts.ToStartButton)
                {
                    SetGreetingState(e.Message.From.Id);
                }
                else
                {
                    switch (state)
                    {
                        case State.ChooseSettings:
                            {
                                if (btnText == Texts.PersonalAccount.StatisticsButton)
                                {
                                    SetChooseStatisticsState(e);
                                }
                                else if (btnText == Texts.PersonalAccount.TextsEditingButton)
                                {
                                    SetChooseBlockState(e);
                                }
                                else if (btnText == Texts.BackButton)
                                {
                                    SetGreetingState(e.Message.From.Id);
                                }
                                break;
                            }
                        case State.ChooseStatistics:
                            {
                                if (btnText == Texts.PersonalAccount.AllUsersButton)
                                {
                                    ShowAllUsers(e);
                                }
                                else if (btnText == Texts.PersonalAccount.LamagnaPassedUsersButton)
                                {
                                    ShowLamagnaPassedUsers(e);
                                }
                                else if (btnText == Texts.PersonalAccount.TrippierPassedUsersButton)
                                {
                                    ShowTrippierPassedUsers(e);
                                }
                                else if (btnText == Texts.PersonalAccount.MainProductPassedUsersButton)
                                {
                                    ShowMainProductPassedUsers(e);
                                }
                                else if (btnText == Texts.BackButton)
                                {
                                    SetChooseSettingsState(e);
                                }
                                break;
                            }
                        case State.ChooseBlockToEdit:
                            {
                                if (btnText == Texts.BackButton)
                                {
                                    SetChooseSettingsState(e);
                                }
                                else if (Texts.ValidBlockName(e.Message.Text))
                                {
                                    SaveBlockChoice(userId, e.Message.Text);
                                    SetChooseTextState(e);
                                }
                                else
                                {
                                    await Bot.SendTextMessageAsync(userId, $"{Emoji.Error} Нет такого блока.");
                                    SetChooseBlockState(e);
                                }
                                break;
                            }
                        case State.ChooseTextToEdit:
                            {
                                if (btnText == Texts.BackButton)
                                {
                                    SetChooseBlockState(e);
                                }
                                else
                                {
                                    SaveTextChoice(userId, e.Message.Text);
                                    SetEnterNewTextState(e);
                                }
                                break;
                            }
                        case State.EnterNewText:
                            {
                                if (btnText == Texts.BackButton)
                                {
                                    SetChooseTextState(e);
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(btnText))
                                    {
                                        await Bot.SendTextMessageAsync(userId, "Молчание не привлечёт к вам клиентов!");
                                        SetChooseTextState(e);
                                    }
                                    UpdateText(e);
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Form.SetDangerStatus($"Ошибка. См. логи");
                Logger.Log(ex.ToString());
            }
        }

        private async void SetChooseStatisticsState(Telegram.Bot.Args.MessageEventArgs e)
        {
            var userId = e.Message.From.Id;
            var text = Texts.PersonalAccount.ChooseStatistics;
            var keyboard = Keyboards.PersonalAccount.ChooseStatisticsKeyboard;
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.ChooseStatistics);
        }

        private void ShowAllUsers(Telegram.Bot.Args.MessageEventArgs e)
        {
            var newMessage = $"Пользователи бота {Bot.GetMeAsync().Result.Username}:{Environment.NewLine}";

            using (var db = DBHelper.GetConnection())
            {
                var users = db.Table<User>();
                foreach (var user in users)
                {
                    newMessage += $"{Environment.NewLine}{user.ToString()}";
                }
            }
            Bot.SendTextMessageAsync(e.Message.From.Id, newMessage);
            SetChooseStatisticsState(e);
        }

        private void ShowLamagnaPassedUsers(Telegram.Bot.Args.MessageEventArgs e)
        {
            var newMessage = $"Пользователи бота {Bot.GetMeAsync().Result.Username}, прошедшие блок Лидмагнит:{Environment.NewLine}";

            using (var db = DBHelper.GetConnection())
            {
                var users = db.Table<User>().Where(u => u.LamagnaPassed);
                foreach (var user in users)
                {
                    newMessage += $"{Environment.NewLine}{user.ToString()}";
                }
            }
            Bot.SendTextMessageAsync(e.Message.From.Id, newMessage);
            SetChooseStatisticsState(e);
        }

        private void ShowTrippierPassedUsers(Telegram.Bot.Args.MessageEventArgs e)
        {
            var newMessage = $"Пользователи бота {Bot.GetMeAsync().Result.Username}, прошедшие блок Трипвайер:{Environment.NewLine}";

            using (var db = DBHelper.GetConnection())
            {
                var users = db.Table<User>().Where(u => u.TrippierPassed);
                foreach (var user in users)
                {
                    newMessage += $"{Environment.NewLine}{user.ToString()}";
                }
            }
            Bot.SendTextMessageAsync(e.Message.From.Id, newMessage);
            SetChooseStatisticsState(e);
        }

        private void ShowMainProductPassedUsers(Telegram.Bot.Args.MessageEventArgs e)
        {
            var newMessage = $"Пользователи бота {Bot.GetMeAsync().Result.Username}, прошедшие блок Главный продукт:{Environment.NewLine}";

            using (var db = DBHelper.GetConnection())
            {
                var users = db.Table<User>().Where(u => u.MainProductPassed);
                foreach (var user in users)
                {
                    newMessage += $"{Environment.NewLine}{user.ToString()}";
                }
            }
            Bot.SendTextMessageAsync(e.Message.From.Id, newMessage);
            SetChooseStatisticsState(e);
        }

        private async void SetChooseBlockState(Telegram.Bot.Args.MessageEventArgs e)
        {
            var userId = e.Message.From.Id;
            var text = Texts.PersonalAccount.ChooseBlock;
            var keyboard = Keyboards.PersonalAccount.ChooseBlockKeyboard;
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.ChooseBlockToEdit);
        }

        private async void SetChooseTextState(Telegram.Bot.Args.MessageEventArgs e)
        {
            var userId = e.Message.From.Id;
            var text = Texts.PersonalAccount.ChooseText;
            var blockChoice = GetBlockChoice(userId).ToString();
            var keyboard = Keyboards.PersonalAccount.ChooseTextKeyboard(blockChoice);
            await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
            SetState(userId, State.ChooseTextToEdit);
        }

        private async void SetEnterNewTextState(Telegram.Bot.Args.MessageEventArgs e)
        {
            var userId = e.Message.From.Id;
            var block = GetBlockChoice(userId);
            var text =
                $"{Texts.PersonalAccount.YouWantToChange} *{e.Message.Text}* {Environment.NewLine}";
            var currentText = Texts.GetText(block, e.Message.Text);
            if (string.IsNullOrEmpty(currentText))
            {
                text = $"{Emoji.Error} Такого текста не существует.";
                await Bot.SendTextMessageAsync(userId, text);
                SetChooseTextState(e);
            }
            else
            {
                await Bot.SendTextMessageAsync(userId, text, Telegram.Bot.Types.Enums.ParseMode.Markdown);
                text = Texts.PersonalAccount.EnterNewText;
                var keyboard = Keyboards.PersonalAccount.EnterNewTextKeyboard;
                await Bot.SendTextMessageAsync(userId, text, replyMarkup: keyboard);
                SetState(userId, State.EnterNewText);
            }
        }

        private async void UpdateText(Telegram.Bot.Args.MessageEventArgs e)
        {
            int userId = e.Message.From.Id;
            var block = GetBlockChoice(userId);
            var fieldName = GetTextChoice(userId);
            Texts.Update(block, fieldName, e.Message.Text);

            string text = Texts.PersonalAccount.NewTextSaved;
            await Bot.SendTextMessageAsync(userId, text);
            SetChooseTextState(e);
        }

        private void SaveBlockChoice(int userId, string block)
        {
            if (block == Texts.LamagnaBlock)
            {
                block = Block.Lamagna.ToString();
            }
            else if (block == Texts.TrippierBlock)
            {
                block = Block.Trippier.ToString();
            }
            else if (block == Texts.MainProductBlock)
            {
                block = Block.MainProduct.ToString();
            }
            else if (block == Texts.OtherBlock)
            {
                block = Block.Other.ToString();
            }

            var cache = RedisConnectorHelper.Connection.GetDatabase();
            cache.StringSet($"Bot{BotUsername}User{userId}BlockChoice", block);
        }
        private Block GetBlockChoice(int userId)
        {
            Block block;
            var cache = RedisConnectorHelper.Connection.GetDatabase();
            string blockString = cache.StringGet($"Bot{BotUsername}User{userId}BlockChoice");
            Enum.TryParse(blockString, out block);
            return block;
        }

        private void SaveTextChoice(int userId, string block)
        {
            var cache = RedisConnectorHelper.Connection.GetDatabase();
            cache.StringSet($"Bot{BotUsername}User{userId}TextChoice", block);
        }
        private string GetTextChoice(int userId)
        {
            var cache = RedisConnectorHelper.Connection.GetDatabase();
            return cache.StringGet($"Bot{BotUsername}User{userId}TextChoice");
        }
    }
}
