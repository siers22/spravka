# NotesApi — HTTP-задание информационных систем

API запускается отдельно от WPF и использует ExamGuide.Core, общие таблицы users/notes в exam_demo.

Настройте `NotesApi/appsettings.json` на **ту же БД**, что ExamGuide. Создайте схему и seed из `ExamGuide/sql`. Подготовьте аккаунты, контрагентов и заметки кнопкой WPF либо командой API:

```powershell
cd C:\Exam
dotnet run --project .\NotesApi\NotesApi.csproj -- --seed
dotnet run --project .\NotesApi\NotesApi.csproj --urls http://localhost:5080
```

Оставьте процесс запущенным. GET `/notes` читает настоящую БД и возвращает массив с `id`, `title_user`, `content`, `formatted_date` (dd.MM.yyyy). Необязательный `user_id` — положительное целое; неизвестный/повторный/неверный параметр → 400, отсутствие строк → 200 и [], ошибка БД → 500 с JSON `error`. Авторизация API не требуется по учебному ТЗ.

Для Postman импортируйте коллекцию из корня архива: обычные проверки с работающей БД, сценарий 500 — с остановленной БД и работающим API. Затем включите БД обратно. Окна WPF не являются маршрутами API. Ctrl+C останавливает HTTP-процесс.
