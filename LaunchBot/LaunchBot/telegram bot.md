Ресурсы
=======

-   [Создание интерактивного бота в бесконечном
    цикле](http://aftamat4ik.ru/pishem-bota-telegram-na-c/)

-   [Написание бота с полученим
    сертификата](https://habrahabr.ru/sandbox/103396/)

-   [Инфа по SQLite3 для
    C\#](https://msdn.microsoft.com/ru-ru/magazine/mt736454.aspx)

-   [Пример бота с
    событиями](https://github.com/TelegramBots/telegram.bot.examples/blob/master/Telegram.Bot.Examples.Echo/Program.cs)

-   [Рабочий пример
    клавиатуры](https://stackoverflow.com/questions/34899614/telegram-bot-custom-keyboard-in-С)

-   [Сериализация данных с JSON](http://котодомик.рф/json_csharp/)

-   \[XML with
    linq\][https://msdn.microsoft.com/ru-ru/library/system.xml.linq.xdocument(v=vs.110).aspx\#Примеры](https://msdn.microsoft.com/ru-ru/library/system.xml.linq.xdocument(v=vs.110).aspx#Примеры)

-   [State-machine](https://habrahabr.ru/post/160105/)

-   [Получение полей класса и их
    значений](https://ru.stackoverflow.com/questions/260530/%D0%9F%D0%BE%D0%BB%D1%83%D1%87%D0%B8%D1%82%D1%8C-%D1%81%D0%BF%D0%B8%D1%81%D0%BE%D0%BA-%D0%BF%D0%BE%D0%BB%D0%B5%D0%B9-%D1%81%D1%82%D1%80%D1%83%D0%BA%D1%82%D1%83%D1%80%D1%8B-%D0%B8-%D0%B8%D1%85-%D0%B7%D0%BD%D0%B0%D1%87%D0%B5%D0%BD%D0%B8%D0%B9)

-   [Redis
    cashe](http://www.c-sharpcorner.com/UploadFile/2cc834/using-redis-cache-with-C-Sharp/)

-   [SQLite-net](http://www.sergechel.info/ru/content/using-sqllite-with-c-sharp-part-4-sqlite-net)

-   [Smiles unicode](https://apps.timwhitlock.info/emoji/tables/unicode)

Access
======

Сервер доступен через внц 192.168.1.19 пароль 153426...пользователь user
пароль zflvby3737

REDIS
=====

Запуск сервера:

    redis-server.exe

STATE-MACHINE ~ SCHEME!

ReplyKeyboardMarkup
===================

Клавиатура под полем ввода.

    public static ReplyKeyboardMarkup GreetingKeyboard => new ReplyKeyboardMarkup
    {
      Keyboard = new KeyboardButton[][]
      {
        new KeyboardButton[] {new KeyboardButton(Texts.Lamagna.Button1) }
      }
    };
