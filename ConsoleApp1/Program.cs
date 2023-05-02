using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types.ReplyMarkups;
//ВСЁ ЧТО ВЫШЕ, ИМПОРТИРОВАНИЕ БИБЛИОТЕК И КОНКРЕТНЫХ КЛАССОВ ИЗ НИХ

TelegramBotClient botClient = new("{token}");
//создаём экземпляр класса "TelegramBotClient" с нашим токеном, полученном из @BotFather
        
using CancellationTokenSource cts = new ();
//токен отмены операции, телеграмм его требует, но мы не используем его по факту, просто везде передаём как обязательный параметр

ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = { }
};
//здесь мы опять создаём экземпляр класса, который будет отвечать за то, будет ли бот обрабатывать сообщение, полученном, пока он был выключен

botClient.StartReceiving(HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

//здесь мы запускаем бот, через экземпляр класса используя метод "StartReceiving", передавая в него обработчик обновлений с сервера телеграмм, обработчик ошибок, получение обновлений, и токен отмены

var me = await botClient.GetMeAsync();
//для отладки, чтобы мы могли понять, что бот запущен. 

Console.WriteLine($"работаем @{me.Username}");
//просто вывод в консоль
Console.ReadLine();
//заставляем консоль быть открытой всё время

cts.Cancel();
//здесь мы отключаем бота


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
//создаём функцию обработки обновлений, передаём внего нашего бота, обновления, и токен отмены
//дальше мы просто проверям тип обновления, для того, чтобы задать конкретную реакцию на определённый тип
//мы обрабатываем два события на сервер, когда бот получает соообщение и когда получает сообщение типа CallBack, то есть нажание на кнопку в боте

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
//данная функция отвечает за работу с бд, мы передаём строчку, создайм SQL запрос, подключаемся к БД используя библиотеку для работы с ней.
//далее мы открываем подключение, создаём SQL команду, передавая в аругменты текстовый SQL запрос и подключение к БД
//дальше мы объявляем "читатель" из базы данных, который будет читать данные из БД, дальше идёт проверка, есть ли строки в БД
//начинаем цикл while пока reader может читать, внутри мы создаём переменную цену и возрващаем её, так как наша функция имеет тип string


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
//здесь мы объявляем функцию обработки сообщений, передаём в ней нашего бота и само сообщение, думаю понятно, что 
//происходит обычная реакция на сообщения, нет смысла писать что конкретно происхоидит
//внутри этой функции мы также создаём клавиатуру, которая будет вызываться, при отправки определённого сообщения

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
//здесь мы обрабатываем сообщения типа CallBack, всё абсолютно также, но также создаём экземпляр класса random 
//для создания случайно времени ответа на сообщение об оплате

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
//просто обработчик ошибок на серверах телеграмм, чтобы понимать, бот перестал работать по нашей вине или из-за телеграмма
