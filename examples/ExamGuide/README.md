# Информационные системы — WPF, C#, две СУБД

Сайт объясняет задания, этот проект создаёт окна Windows. Технология — WPF на .NET 10.

## 1. Подготовить Windows

Установите .NET 10 SDK и Visual Studio с поддержкой .NET 10 и компонентом «Разработка классических приложений .NET». Нужен WPF на современном .NET, не .NET Framework. NuGet восстановите до экзамена, когда доступен Интернет.

Распакуйте **весь архив** в `C:\Exam`. Папки `ExamGuide`, `ExamGuide.Core`, `NotesApi` должны оставаться рядом. Откройте `ExamGuide/ExamGuide.csproj` в Visual Studio. F5 открывает окно входа.

- `App.xaml` — общие стили; `App.xaml.cs` — настройки и первое окно.
- `LoginWindow.xaml` и `.xaml.cs` — логин, пароль и четыре картинки пазла.
- `MainWindow` — рабочий стол вошедшего пользователя, выход и вход в администрирование.
- `AdminWindow` — список, добавление, изменение и снятие блокировки.
- `Data/Database.cs`, `UserRepository.cs`, `AuthService.cs`, `Puzzle.cs` — общая логика, компилируется в `ExamGuide.Core`.
- `NotesApi` — отдельный ASP.NET Core HTTP-проект для задания 5. WPF не становится HTTP-сервером.

## 2. Создать БД

Выберите PostgreSQL либо Microsoft SQL Server. Создайте пустую БД `exam_demo`, подключитесь к ней, выполните `sql/postgres-schema.sql` и `sql/postgres-seed.sql` либо соответствующие `mssql-...` файлы. Скрипты schema/seed предназначены для пустой БД.

В **ExamGuide/appsettings.json** поставьте `Database:Provider` = `Postgres` или `SqlServer`. Для PostgreSQL замените `CHANGE_ME`; для SQL Server укажите настоящий экземпляр (`localhost\SQLEXPRESS`, `localhost` и `(localdb)\MSSQLLocalDB` — разные серверы). Windows Authentication использует `Integrated Security=True`.

Для задания API продублируйте параметры этой же БД в **NotesApi/appsettings.json**. Это две копии настроек разных исполняемых программ.

## 3. Запустить WPF и подготовить данные

Из корня архива:

```powershell
cd C:\Exam
dotnet restore .\ExamGuide\ExamGuide.csproj
dotnet run --project .\ExamGuide\ExamGuide.csproj
```

В окне нажмите **«Подготовить учебные данные»**. Появятся `admin / Admin123!`, `user / User123!`, три заметки и шесть исходных контрагентов. Повторная подготовка не дублирует записи. Пароли только для тренировки.

Введите логин и пароль. Для пазла нажмите первый и второй фрагмент: они поменяются местами. Соберите исходное изображение и нажмите «Войти». После успешного сообщения откроется рабочий стол. У admin есть управление пользователями: выберите строку для изменения или «Новая запись» для добавления. Пустой пароль при изменении сохраняет прежний хеш; для добавления пароль обязателен.

Одна неудачная отправка с неверным паролем и/или пазлом увеличивает счётчик на 1. После третьей попытки блокировка сохраняется в БД даже после перезапуска. Снятие блокировки обнуляет счётчик. Пустые обязательные поля не считаются попыткой.

Проверка роли происходит перед каждым административным действием по свежей записи БД. Изменение роли/блокировка действует и на уже открытый рабочий стол. Тренируйте блокировку на user. Если заблокировали единственного admin, учебное восстановление: `UPDATE users SET is_blocked=0, failed_attempts=0 WHERE login='admin';`.

## 4. Запустить HTTP API отдельно

Откройте второй PowerShell из `C:\Exam`:

```powershell
dotnet run --project .\NotesApi\NotesApi.csproj --urls http://localhost:5080
```

GET `http://localhost:5080/notes` возвращает JSON: `id`, `title_user`, `content`, `formatted_date`. Закрытие окна WPF не останавливает API; Ctrl+C в терминале NotesApi останавливает его. Если порт занят, измените порт, baseUrl коллекции и документацию.

Для подготовки без окна: `dotnet run --project .\NotesApi\NotesApi.csproj -- --seed`.

Импортируйте `notes.postman_collection.json` из корня ZIP. С работающей БД запустите «Штатные проверки»: 200, поля, дата, валидный JSON, 400 и пустой массив. Для отдельной папки «Сбой 500» остановите службу БД, **оставив NotesApi запущенным**, затем включите службу обратно. Документацию переносите в исходный DOCX-шаблон.

## 5. Проверить и передать результат

```powershell
dotnet publish .\ExamGuide\ExamGuide.csproj -c Release -r win-x64 --self-contained true -o .\publish\ExamGuide
.\publish\ExamGuide\ExamGuide.exe
# Для задания 5 публикуется отдельный процесс:
dotnet publish .\NotesApi\NotesApi.csproj -c Release -r win-x64 --self-contained true -o .\publish\NotesApi
.\publish\NotesApi\NotesApi.exe --urls http://localhost:5080
```

Копируйте всю папку публикации, не только `.exe`. Проверьте наличие `Assets/puzzle/1.png–4.png`, `Data/customers.json`, `appsettings.json`; в NotesApi — собственные настройки и библиотеки. Конфигурация WPF читается рядом с `.exe`, не из текущей папки терминала. Восстановите SQL в тестовую БД и повторите запуск с новым подключением на Windows.

Формы WPF запускаются только на Windows. Кросс-сборка и тесты общей логики не заменяют ручную проверку окон на целевой машине. Microsoft SQL Server требуется отдельно проверить на доступном экземпляре Windows.

Нормативная стоимость по принятым нормам — **14 374,28**. В исходнике есть противоречия по кодам изделия и тарифам операций; объяснения и учебное допущение приведены на сайте. Сохраните ER-PDF, SQL и пояснения, исходники WPF/API, файлы публикации, Postman и DOCX по правилам площадки.
