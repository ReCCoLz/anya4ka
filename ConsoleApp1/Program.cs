using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types.ReplyMarkups;

TelegramBotClient botClient = new("{token}");
        
using CancellationTokenSource cts = new ();

ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = { }
};

botClient.StartReceiving(HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();


Console.WriteLine($"работаем @{me.Username}");
Console.ReadLine();


cts.Cancel();



async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.Message && update?.Message?.Text != null)
    {
        await HandleMessage(botClient, update.Message);
        return;
    }

    if (update.Type == UpdateType.CallbackQuery)
    {
        await HandleCallbackQuery(botClient, update.CallbackQuery);
        return;
    }

}


static string GetPrices(string name)
{
    string sqlExp = $"SELECT price FROM animals WHERE name = '{name}'";
    using (var connection = new SqliteConnection("Data Source=animals.db"))
    {
        connection.Open();

        SqliteCommand command = new(sqlExp, connection);
        using (SqliteDataReader reader = command.ExecuteReader())

        if (reader.HasRows) // если есть данные
        {
            while (reader.Read())   // построчно считываем данные
            {
                    var price = reader.GetValue(0).ToString();
                    return price;
            }
        }
    }
    return "Error";
}

async Task HandleMessage(ITelegramBotClient botClient, Message message)
{
    if (message.Text == "/start")
    {
        ReplyKeyboardMarkup replyMarkup_start = new(new[]
            {
                new KeyboardButton[] {"Домашние питомцы🐽" },
                new KeyboardButton[] { "Support🆘" },
            })
        {
            ResizeKeyboard = true
        };
        await botClient.SendStickerAsync(message.Chat.Id, "CAACAgIAAxkBAAEIeRtkLi86Acr52-t20hhXraFBIw3zygACbwADmL-ADeFz2TIaBadILwQ");
        await botClient.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать в бота \"zooHUB\"!🤜🏻🤛🏿", replyMarkup: replyMarkup_start);
        return;
    }

    if (message.Text == "Домашние питомцы🐽")
    {
        InlineKeyboardMarkup replyMarkupL = new(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Собака🐩", "собака"),
                        InlineKeyboardButton.WithCallbackData("Кошка🐈", "кот"),
                    }, new []
                    {
                        InlineKeyboardButton.WithCallbackData("Папуга🦜", "папуга"),
                        InlineKeyboardButton.WithCallbackData("Рыбка🐠", "карась"),
                    },
                    new [] {
                        InlineKeyboardButton.WithCallbackData("Мышка🐹", "крыса"),
                        InlineKeyboardButton.WithCallbackData("Черепаха🐢", "суп"),
                    },
                });

        await botClient.SendTextMessageAsync(message.Chat.Id, $"Выберете подходящего вам животного!", replyMarkup : replyMarkupL);
        return;
    }


    if (message.Text == "Support🆘")
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "@wideRexx - Тех. Поддержка👌🏿");
        return;
    }
}

async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
{
    InlineKeyboardMarkup oplata = new(new[]
    {
        InlineKeyboardButton.WithUrl("Оплата QIWI🥝", "https://qiwi.com"),
    });

    switch (callbackQuery.Data)
    {
        case "собака":
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Собака🐩 стоит {GetPrices("собака")}$", replyMarkup : oplata);
            break;
        case "кот":
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Кошка🐈 стоит {GetPrices("кошка")}$", replyMarkup: oplata);
            break;
        case "папуга":
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Попугай🦜 стоит {GetPrices("попугай")}$", replyMarkup: oplata);
            break;
        case "карась":
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Рыбка🐠 стоит {GetPrices("рыбка")}$", replyMarkup: oplata);
            break;
        case "суп":
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Черепаха🐢 стоит {GetPrices("черепаха")}$", replyMarkup: oplata);
            break;
        case "крыса":
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Мышка🐹 стоит {GetPrices("мышка")}$", replyMarkup: oplata);
            break;
    }

    Random rnd = new Random();
    int time = rnd.Next(25000, 35000);
    Console.WriteLine(time);
    await Task.Delay(time);

    botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Спасибо за покупку❤️");
    botClient.SendStickerAsync(callbackQuery.Message.Chat.Id, "CAACAgIAAxkBAAEIeTJkLkQBLXOpegsKC_G9flXFUmhDqgACKQIAAkf7CQx4yOhHtlL_DC8E");

    return;
}


Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
{
    var Error = exception switch 
    {
        ApiRequestException apiRequestException  =>
        $"Ошибка на стороне TG\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(Error);
    return Task.CompletedTask;
}
