# Подробное решение DemoExam на WPF и PostgreSQL

Учебная инструкция по архиву DemoExam_WPF_PostgreSQL_2027.zip для КИМ 09.02.07-5-2027. Версия 9 октября 2026 года. Порядок работы основан на исходниках решения; механизмы и интерфейсы сверены с официальной документацией.

## 00 Подготовить Windows и запустить пример

Ваш архив DemoExam: WPF и API на .NET 9, PostgreSQL, pgAdmin и Visual Studio. Подробно устанавливаем инструменты и запускаем готовое решение через интерфейс.

Результат: Открываются окно входа и GET /notes; база demo_exam_2027 содержит исходные данные.

### Что именно мы собираем

Этот маршрут разбирает конкретный архив DemoExam_WPF_PostgreSQL_2027.zip. Приложение называется «Учёт производства». Оно хранит данные мебельного предприятия, считает себестоимость двух столов, проверяет логин, пароль и пазл, даёт администратору менять пользователей и показывает заметки через HTTP API. Все названия файлов, кнопок и таблиц ниже взяты из этого решения.

Двигайтесь в два прохода. Сначала запустите готовые файлы и посмотрите результат. Затем пройдите задания 1–6: спроектируйте схему, создайте базу, разберите SQL, воспроизведите проекты в Visual Studio, проверьте API и заполните документ. Так каждое изменение можно сравнить с работающим примером.

**Учебный пример и условие экзамена.** WPF, .NET 9, PostgreSQL и Npgsql — стек этого архива. Состав установленного ПО и правила использования готовых материалов определяет ваша площадка. Домашняя установка ниже нужна для обучения; на экзамене используйте предоставленную среду. Для ПА выполняются первые три задания; WPF, API и документация относятся к ГИА базового уровня.

| Часть | Где находится | Что делает |
| --- | --- | --- |
| База | Database | 15 таблиц, данные, SQL расчёта, полный дамп |
| Общая логика | Source/DemoExam.Core | Подключение, пароли, вход, пользователи, заметки, стоимость |
| Окна | Source/DemoExam.Wpf | LoginWindow, AdminWindow, UserWindow и PuzzleControl |
| HTTP API | Source/DemoExam.Api | GET /notes на порту 5050 |
| Готовые программы | App, Api, Checks | Можно запустить без пересборки при наличии runtime |
| Результаты сдачи | Documents, Tests | ER PDF, API DOCX, Postman, сценарии и сохранённые отчёты |

WPF напрямую обращается к PostgreSQL через DemoExam.Core. API отдельно обращается к той же базе. Открытый браузер с сайтом «По полочкам» не подключается к вашей БД и не запускает эти программы. Это инструкция, а сами приложения работают на Windows.

- [Скачать именно разобранный архив](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/DemoExam_WPF_PostgreSQL_2027.zip) — Полный исходный комплект с исходниками и готовыми программами.

### Выбрать версии без путаницы

| Инструмент | Для этого примера | Где посмотреть |
| --- | --- | --- |
| Windows | WPF запускается под Windows | Пуск → Параметры → Система → О системе |
| Visual Studio | Для .NET 9 подходит Visual Studio 2022 версии 17.12 или новее с нужными компонентами | Справка → О программе Microsoft Visual Studio |
| .NET 9 SDK | Нужен при создании и сборке проектов | Visual Studio Installer → Отдельные компоненты |
| .NET Desktop Runtime 9 | Нужен для готового WPF при отсутствии соответствующего runtime | Официальная страница .NET 9 → Windows → .NET Desktop Runtime |
| ASP.NET Core Runtime 9 | Нужен для готового API | Официальная страница .NET 9 → Windows → ASP.NET Core Runtime |
| PostgreSQL | У автора дампа PostgreSQL 18; для повторения используем 18 | pgAdmin → сервер → Properties / Свойства |
| Npgsql | В проекте зафиксирован пакет 9.0.3 | DemoExam.Core → Зависимости → Пакеты |
| Postman | Версия, доступная на вашем компьютере | Названия Tests и Scripts → Post-response могут отличаться |

SDK используется для разработки, runtime — для запуска собранной программы. Установка .NET 10 сама по себе не гарантирует запуск этих exe: их runtimeconfig запрашивает семейство .NET 9. Проверяйте именно требуемые среды. Готовый архив содержит библиотеки Npgsql, но это не заменяет .NET runtime и не устанавливает сервер PostgreSQL.

- [Microsoft — установка .NET под Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows) — Различия SDK и runtime, соответствие Visual Studio и .NET.
- [Microsoft — загрузки .NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) — Выбирайте Windows и архитектуру своего компьютера.

### Установить компоненты Visual Studio кнопками

1. Откройте «Пуск», введите Visual Studio Installer и откройте найденную программу. Если Visual Studio ещё не установлена, сначала запустите установщик с официального сайта Microsoft.
2. В карточке вашей версии Visual Studio нажмите «Изменить» / Modify. Если установщик предложил обновление самого себя, завершите его и вернитесь к карточке.
3. На вкладке «Рабочие нагрузки» / Workloads отметьте «Разработка классических приложений .NET» / .NET desktop development. Эта нагрузка добавляет инструменты для WPF.
4. Отметьте «ASP.NET и разработка веб-приложений» / ASP.NET and web development. Она нужна для проекта DemoExam.Api.
5. На вкладке «Отдельные компоненты» / Individual components найдите .NET 9 и проверьте наличие SDK и компонентов разработки для этой версии. Если нужного компонента нет в списке, установите .NET 9 SDK со страницы Microsoft и используйте Visual Studio с его поддержкой.
6. Нажмите «Изменить» / Modify внизу. Дождитесь завершения. Перезапустите Visual Studio.
7. Если окно «Обозреватель решений» скрыто, откройте «Вид → Обозреватель решений» / View → Solution Explorer. Дальше все проекты и файлы ищем в этом дереве.

**На компьютере уже всё установлено.** Если .sln открывается, проекты .NET 9 загружаются и сборка проходит, пропустите установку. На площадке экзамена изменение ПО выполняется по её правилам.

- [Microsoft — изменение рабочих нагрузок](https://learn.microsoft.com/en-us/visualstudio/install/modify-visual-studio?view=vs-2022)

### Подготовить PostgreSQL и pgAdmin

1. Для домашней тренировки скачайте Windows-установщик PostgreSQL с официальной страницы. Запустите его двойным щелчком.
2. В мастере оставьте компоненты PostgreSQL Server и pgAdmin 4. Остальные компоненты не нужны для шагов этой методички.
3. Задайте пароль пользователя postgres и запишите его. Это пароль подключения к СУБД. У автора архива он admin, но на вашем компьютере может быть другим.
4. Оставьте порт 5432, если он свободен. Если сервер уже установлен на другом порту, используйте его фактическое значение в pgAdmin и настройках приложений.
5. Завершите установку. Stack Builder с дополнительными пакетами можно закрыть.
6. Откройте «Пуск → pgAdmin 4». Если pgAdmin просит мастер-пароль для хранения подключений, задайте его по правилам установленной версии. Он отличается от пароля postgres и паролей приложения.
7. В дереве слева раскройте Servers. Если локальный PostgreSQL уже есть, откройте его и введите пароль postgres. Если отсутствует — выполните следующий раздел.

- [PostgreSQL — загрузки для Windows](https://www.postgresql.org/download/windows/)

### Создать подключение в pgAdmin

1. В дереве Object Explorer нажмите правой кнопкой на Servers → Register → Server. Откроется карточка подключения.
2. На вкладке General в поле Name напишите Local PostgreSQL. Это только подпись в pgAdmin.
3. Переключитесь на Connection. Host name/address = 127.0.0.1; Port = 5432; Maintenance database = postgres; Username = postgres.
4. В Password введите ваш пароль сервера. Для домашнего компьютера можно включить Save password, если это соответствует вашим правилам.
5. Нажмите Save. Раскройте созданное подключение. Если видите Databases, соединение установлено.
6. Если подключение не проходит, проверьте адрес, порт и пароль. Для проверки службы откройте «Пуск», найдите «Службы», найдите службу PostgreSQL вашего экземпляра и посмотрите её состояние. Запустите нужную службу, если она остановлена.

Не меняйте пользователя PostgreSQL на admin: admin — логин приложения. База использует Username=postgres. Пароль приложения проверяется по таблице users и не равен автоматически паролю сервера.

- [pgAdmin — поля регистрации сервера](https://www.pgadmin.org/docs/pgadmin4/latest/server_dialog.html)

### Распаковать весь архив и открыть решение

1. На сайте нажмите «Решение DemoExam» в верхней панели. В загрузках Windows найдите DemoExam_WPF_PostgreSQL_2027.zip.
2. Нажмите на ZIP правой кнопкой → «Извлечь всё…». В качестве места выберите, например, C:\Exam. Нажмите «Извлечь». Рабочая папка в этом примере получится C:\Exam\DemoExam.
3. Откройте полученную папку и убедитесь, что рядом лежат App, Api, Checks, Database, Documents, Source и Tests. Если видите ещё одну папку DemoExam, войдите в неё.
4. Не открывайте exe прямо внутри окна ZIP. Распаковка нужна, чтобы программа видела все DLL, appsettings.json и прочие файлы рядом с собой.
5. Откройте Visual Studio → «Открыть проект или решение» / Open a project or solution. Выберите C:\Exam\DemoExam\Source\DemoExam.sln → «Открыть».
6. Дождитесь загрузки. В дереве должны появиться DemoExam.Core, DemoExam.Wpf, DemoExam.Api и DemoExam.Checks. Если проект помечен как недоступный, прочитайте сообщение о недостающей версии SDK или компоненте.
7. Пока не запускайте приложение: сначала создайте базу. Это описано в задании 2. Для самого короткого запуска используйте там раздел «Восстановить готовую базу».

**Где готовые файлы и где исходники.** App/appsettings.json управляет готовым WPF, а Source/DemoExam.Wpf/appsettings.json — WPF, собранным из Visual Studio. То же разделение у Api и Source/DemoExam.Api. Изменение одного файла не меняет автоматически остальные.

### Прописать подключение без терминала

1. Когда база создана, в Проводнике откройте C:\Exam\DemoExam\App. Нажмите appsettings.json правой кнопкой → «Открыть с помощью → Блокнот».
2. В строке Database замените Password=admin на пароль вашего postgres. Если имя БД или порт отличаются, замените Database=demo_exam_2027 и Port=5432.
3. Сохраните файл через «Файл → Сохранить». Сохраните исходное имя appsettings.json, а не appsettings.json.txt. Расширения можно показать через «Вид → Показать → Расширения имён файлов» в Проводнике.
4. Повторите изменение в Api/appsettings.json и Checks/appsettings.json. В API оставьте Urls=http://127.0.0.1:5050 для основного сервера.
5. Для запуска из Visual Studio откройте в дереве Source проекты DemoExam.Wpf и DemoExam.Api и исправьте их appsettings.json теми же значениями. Нажмите «Файл → Сохранить всё».
6. Сверьте строку с pgAdmin: один адрес, один порт, одно имя базы, тот же пользователь postgres и пароль. Заполненный JSON должен сохранять двойные кавычки и запятые.

**Образец appsettings.json для WPF**

```json
{
  "ConnectionStrings": {
    "Database": "Host=127.0.0.1;Port=5432;Database=demo_exam_2027;Username=postgres;Password=admin;Timeout=5;Command Timeout=10"
  }
}
```

В этом примере ConnectionStrings — раздел настроек, Database — имя записи внутри него. Host — адрес сервера, Port — порт PostgreSQL, Database внутри строки — имя самой базы. Timeout и Command Timeout ограничивают ожидание соединения и команды. Если пароль содержит специальные разделители строки подключения, задавайте его по правилам NpgsqlConnectionStringBuilder; не вставляйте случайные кавычки в JSON.

**Если настройки как будто игнорируются.** Database.cs сначала проверяет переменную среды DEMO_DB_CONNECTION. Если она задана, она имеет приоритет над JSON. Для обычного запуска такая переменная не нужна. Посмотрите «Пуск → Изменение системных переменных среды → Переменные среды» и проверьте значение именно DEMO_DB_CONNECTION.

- [Npgsql — параметры подключения](https://www.npgsql.org/doc/connection-string-parameters.html)

### Первый вход и запуск API

1. В Проводнике откройте DemoExam\App и дважды нажмите DemoExam.Wpf.exe. Равнозначный вариант — Start-Wpf.cmd в корне архива. Команды вручную вводить не нужно.
2. В окне «Вход в информационную систему» введите admin в «Логин» и admin в «Пароль». Это исходная учётная запись приложения.
3. Справа нажмите «Образец». Запомните правильную картинку и закройте окно образца.
4. Нажмите на один фрагмент, затем на другой: они поменяются местами. Приведите картинку к образцу. Нажатие «Перемешать» создаёт новую перестановку, а не решает пазл.
5. Нажмите «Войти». Должно появиться «Вы успешно авторизовались». Нажмите OK. Откроется рабочий стол администратора.
6. Переключитесь на «Стоимость заказов». Должны быть материалы 9 974,28; операции 4 400,00; себестоимость 14 374,28; продажа 26 828,00.
7. Для API откройте DemoExam\Api и дважды нажмите DemoExam.Api.exe. Оставьте открывшееся окно сервера работающим. Start-Api.cmd делает то же.
8. В браузере откройте http://127.0.0.1:5050/notes. Ожидается JSON-массив из пяти заметок. Адрес / без notes в этом API не реализован.
9. Чтобы остановить WPF, закройте его окно. Чтобы остановить запущенный двойным щелчком API, закройте его окно. Если API запускается из Visual Studio, используйте «Отладка → Остановить отладку».

| Роль | Логин приложения | Пароль приложения | Что откроется |
| --- | --- | --- | --- |
| Администратор | admin | admin | Управление пользователями и стоимость |
| Пользователь | user25 | user123 | Две свои заметки |
| Пользователь | user26 | user123 | Одна своя заметка |

**Сначала создайте тестового пользователя.** Три неверных входа блокируют существующий аккаунт. Не проверяйте блокировку на единственном admin. Создайте отдельный test/student в окне администратора и тренируйтесь на нём. Неверный пазл при правильном пароле тоже считается ошибкой.

### Собрать и запустить решение в Visual Studio

1. В «Обозревателе решений» нажмите на решение DemoExam правой кнопкой → «Восстановить пакеты NuGet» / Restore NuGet Packages, если пункт доступен. Восстановление также выполняется при обычной сборке.
2. Откройте «Сборка → Собрать решение» / Build → Build Solution. Дождитесь завершения. В «Вид → Вывод» выберите «Сборка»: там виден результат. В «Вид → Список ошибок» прочитайте ошибки, если они есть.
3. В дереве нажмите DemoExam.Wpf правой кнопкой → «Назначить запускаемым проектом» / Set as Startup Project. Убедитесь, что у зелёной кнопки запуска выбран этот проект.
4. Нажмите зелёную кнопку или «Отладка → Начать отладку» / F5. Откроется окно входа. Оно использует appsettings.json из выходной папки сборки.
5. Завершите отладку. Назначьте DemoExam.Api запускаемым проектом. В выпадающем списке запуска выберите профиль самого проекта, если среда предлагает IIS Express. В исходном архиве нет launchSettings.json; для добавленного профиля сверяйте URL с сообщением Now listening on.
6. Запустите API. Откройте в браузере фактический адрес /notes. Если профиль Visual Studio переопределил порт, используйте его или исправьте профиль запуска, чтобы он соответствовал 5050.
7. Для одновременной работы проще запустить готовый API двойным щелчком и отлаживать WPF. Либо используйте свойства решения → Configure Startup Projects / настройку запуска нескольких проектов и выберите Start для WPF и API.

Первая сборка исходников получает Npgsql 9.0.3 через NuGet. Если сети нет и пакет не подготовлен в среде, сборка не завершится. Запуск готового exe и сборка исходников — разные проверки. На экзамене наличие и допустимость этой библиотеки нужно сверить с предоставленной средой.

- [Microsoft — NuGet через интерфейс Visual Studio](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio)
- [Microsoft — создание и отладка WPF](https://learn.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-wpf?view=vs-2022)

### Проверка готовности

- [ ] Открыл DemoExam.sln и проверил требуемое ПО
- [ ] Создал demo_exam_2027 и загрузил исходные данные
- [ ] Настроил appsettings.json для WPF и API
- [ ] Запустил WPF и получил пять заметок через GET /notes
- [ ] Проверил именно .NET 9 и Npgsql 9.0.3 этого архива
- [ ] Настроил отдельно готовые программы и исходные проекты
- [ ] Различаю пароль postgres, мастер-пароль pgAdmin и пароль приложения

## 01 Спроектировать базу и сохранить ER в PDF

Разбираем мебельное производство, отделяем справочники от документов, объясняем 3НФ и строим схему именно DemoExam.

Результат: ER_Diagram.pdf с 15 таблицами, атрибутами, первичными и внешними ключами и 19 связями.

### Начать с документов предметной области

1. Откройте оригинальные приложения к КИМ. В материалах задания 1 найдите заказ покупателя, заказ на производство, спецификацию, цены, расчёт стоимости и JSON контрагентов. Откройте документы доступным редактором таблиц, а JSON — Блокнотом.
2. В заказе покупателя выделите шапку: номер, дату, покупателя и исполнителя. Отдельно выделите строки: изделие, количество, продажную цену и скидку.
3. В заказе на производство найдите ссылку на заказ покупателя, подразделение, исполнителя и плановые даты. Отдельно выпишите продукцию, спецификацию и количество.
4. В спецификации найдите изделие и изготовителя. Затем выпишите два разных состава: материалы с нормами расхода и операции с количеством и нормами времени.
5. В таблице цен найдите цены материалов и операций. Цена продажи из заказа покупателя в эту таблицу себестоимости не подставляется.
6. В JSON найдите id, name, inn, addres, phone и type. Сохраните код как строку: 000000001 должен остаться девятизначным кодом, а не превратиться в число 1.

**Не переносите ошибки источника молча.** В примере зафиксированы разный код одного стола в двух документах, перепутанные единицы евровинта и опоры и противоречивый готовый итог расчёта. Они объясняются в Database_Explanation.md. Эти замечания — выводы автора решения; исходные Excel сверяйте по оригинальным приложениям.

- [Пояснение базы из вашего архива](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Documents/Database_Explanation.md)

### Определить все таблицы

| Таблица | Что хранит | Ключ и важные связи |
| --- | --- | --- |
| counterparties | Покупатели, поставщики, изготовители | PK id varchar(20) |
| units | Единицы шт, тыс. шт, ч | PK id |
| products | Изделие и два исходных кода | PK id; FK unit_id |
| materials | Материалы и цена единицы нормы | PK id; FK unit_id |
| operations | Операции и цена единицы нормы | PK id; FK unit_id |
| specifications | Спецификация изделия | PK id; UNIQUE product_id; FK product_id, manufacturer_id |
| specification_materials | Материалы спецификации и расход | Составной PK specification_id + material_id; оба FK |
| specification_operations | Операции, количество и время | Составной PK specification_id + operation_id; оба FK |
| departments | Подразделения производства | PK id |
| users | Учётные записи приложения | PK id; уникальный lower(login) |
| customer_orders | Шапка заказа покупателя | PK id; FK customer_id, executor_id |
| customer_order_items | Позиции заказа покупателя | PK id; FK order_id, product_id |
| production_orders | Шапка производственного заказа | PK id; FK customer_order_id, department_id, executor_user_id |
| production_order_items | Позиции производства | PK id; FK production_order_id, specification_id |
| notes | Заметки пользователей | PK id; FK id_user |

Названия customer и executor в заказе не означают отдельные справочники: оба ссылаются на counterparties. Для разных назначений используем разные поля внешних ключей. Таблица notes включена в общую схему сразу, потому что понадобится заданию 5.

PK отвечает на вопрос «какую запись мы имеем в виду». FK отвечает на вопрос «на какую существующую запись другой таблицы мы ссылаемся». Например, customer_order_items.order_id ссылается на customer_orders.id. Без FK можно случайно записать строку заказа с несуществующей шапкой.

### Объяснить 3НФ на конкретных данных

Первая нормальная форма: одна ячейка хранит одно значение, а повторяющиеся элементы становятся отдельными строками. Поэтому в заказе нет колонок material1, material2, material3 и списка изделий через запятую. Позиции заказа и состав спецификации вынесены в таблицы строк.

Вторая нормальная форма: атрибут зависит от полного ключа. В specification_materials ключ — пара спецификация + материал. Норма quantity относится именно к этой паре. Название и цена материала относятся только к материалу и поэтому лежат в materials, а не повторяются в каждой строке состава.

Третья нормальная форма: неключевое поле не определяет другое неключевое поле внутри записи. В customer_orders хранится customer_id, а имя, ИНН и адрес заказчика находятся в counterparties. При изменении телефона контрагента не надо переписывать все заказы.

Себестоимость заказа вычисляется представлением order_costs и не хранится как редактируемое число. Это помогает избежать ситуации, когда изменили цену материала, а старое поле total_cost забыли пересчитать.

**Граница модели.** В этом примере у изделия одна спецификация: product_id в specifications уникален. Цена материала текущая, история цен не ведётся. Схема поддерживает этот учебный набор; версии спецификаций и историческую себестоимость пришлось бы моделировать отдельно.

### Нарисовать связи и проверить их количество

| Откуда | Куда | Число FK |
| --- | --- | --- |
| products, materials, operations | units | 3 |
| specifications | products; counterparties | 2 |
| specification_materials | specifications; materials | 2 |
| specification_operations | specifications; operations | 2 |
| customer_orders | counterparties по customer_id и executor_id | 2 |
| customer_order_items | customer_orders; products | 2 |
| production_orders | customer_orders; departments; users | 3 |
| production_order_items | production_orders; specifications | 2 |
| notes | users | 1 |
| Всего | Все объявленные внешние ключи | 19 |

Обычно связь направлена от одной записи справочника к многим записям дочерней таблицы. Исключение: UNIQUE на specifications.product_id делает связь изделия со спецификацией «не более одной спецификации на изделие». Составные первичные ключи запрещают дважды добавлять тот же материал или ту же операцию в одну спецификацию.

Две ссылки из customer_orders на counterparties считаются двумя внешними ключами. На рисунке линии могут визуально накладываться: раздвиньте их или подпишите customer_id и executor_id.

- [Полная схема именно DemoExam в PDF](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Documents/ER_Diagram.pdf) — Готовый пример результата задания 1.
- [Та же схема в SVG](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Documents/ER_Diagram.svg) — Можно увеличить без потери чёткости.

### Получить собственную диаграмму через pgAdmin

1. Для диаграммы, построенной из реальной структуры, сначала выполните задание 2 и создайте все таблицы. Затем вернитесь сюда.
2. В pgAdmin раскройте Servers → ваш сервер → Databases → demo_exam_2027. Нажмите на базу правой кнопкой и найдите Generate ERD. В некоторых версиях этот пункт доступен у схемы public; если его нет, откройте Tools → ERD Tool и добавьте таблицы из дерева.
3. При добавлении вручную перетащите таблицы public на рабочее поле. Проверьте, что добавлены все 15 таблиц. Не ограничивайтесь первыми справочниками.
4. В ERD Tool включите отображение колонок и деталей, если они свёрнуты. В таблицах должны быть видны названия полей, типы и ключи.
5. Примените Auto align / автоматическое размещение, если доступно. Затем вручную раздвиньте таблицы: справочники слева, спецификации в центре, заказы справа, users и notes рядом.
6. Сверьте линии с таблицей 19 FK выше. У каждой линии найдите поле ссылки. Отдельно проверьте две ссылки шапки customer_orders на counterparties.
7. Сохраните редактируемую диаграмму через File → Save As, используя формат установленной версии pgAdmin. Кнопкой Download image сохраните изображение всей схемы.
8. Если pgAdmin не экспортирует PDF напрямую, откройте сохранённое изображение в редакторе или просмотрщике, который умеет печатать. Нажмите «Файл → Печать», выберите Microsoft Print to PDF, альбомную ориентацию и подходящий размер бумаги. Проверьте предварительный просмотр, чтобы схема целиком помещалась и текст читался.
9. Сохраните diagram.pdf. Откройте его двойным щелчком, увеличьте масштаб и проверьте каждую таблицу, окончания линий и отсутствие обрезанных краёв. Для мелкой схемы лучше несколько читаемых страниц, чем одна нечитаемая.

Если делаете схему до базы, используйте предоставленный редактор ER: добавьте таблицы из списка, затем поля из 01_schema.sql и соедините FK. В архиве есть готовый векторный PDF для сравнения. Копия готового PDF показывает результат; собственное построение тренирует проектирование.

- [pgAdmin — ERD Tool](https://www.pgadmin.org/docs/pgadmin4/latest/erd_tool.html)
- [SQL со всеми атрибутами и ключами](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Database/01_schema.sql)

### Проверка готовности

- [ ] Выделил сущности и атрибуты из исходных документов
- [ ] Проверил первичные и внешние ключи
- [ ] Могу объяснить третью нормальную форму
- [ ] Сохранил читаемую ER диаграмму в PDF
- [ ] На диаграмме 15 таблиц и 19 внешних ключей
- [ ] Могу объяснить составной PK и UNIQUE product_id
- [ ] Сохранил PDF и проверил его при увеличении

## 02 Создать базу и импортировать данные

Создаём demo_exam_2027, выполняем три SQL-файла через Query Tool, сохраняем все поля JSON и проверяем ограничения.

Результат: База PostgreSQL с 15 таблицами, 8 контрагентами, 3 пользователями, 5 заметками и представлением order_costs.

### Создать пустую базу в pgAdmin

1. В pgAdmin раскройте Servers → Local PostgreSQL. Нажмите Databases правой кнопкой → Create → Database.
2. В поле Database на вкладке General введите demo_exam_2027. Owner оставьте postgres, если работаете под этой учётной записью.
3. При необходимости проверьте на вкладке Definition кодировку UTF8. Остальные параметры оставьте стандартными для установленного сервера.
4. Нажмите Save. В дереве Databases появится demo_exam_2027. Если не появилась, нажмите Databases правой кнопкой → Refresh.
5. Если такое имя уже занято, не удаляйте чужую базу. Для повторной тренировки создайте demo_exam_2027_practice и затем исправьте Database=... в настройках приложения и API.
6. Нажмите созданную базу правой кнопкой → Query Tool. Убедитесь в заголовке соединения, что редактор подключён именно к demo_exam_2027, а не к postgres.

Создание базы и выполнение таблиц — разные действия. CREATE DATABASE, выполненная в редакторе postgres, не переключает его в новую базу. Поэтому Query Tool нужно открывать у нужной базы явно.

- [pgAdmin — создание базы](https://www.pgadmin.org/docs/pgadmin4/latest/database_dialog.html)

### Восстановить готовую базу одним файлом

Это короткий путь для первого запуска. Если хотите самостоятельно пройти создание схемы и импорт, пропустите его и выполняйте следующие разделы: 01_schema.sql → 02_data.sql → 03_order_cost.sql. Два пути являются альтернативами и применяются к пустой базе.

1. Откройте Query Tool у новой пустой demo_exam_2027.
2. Нажмите значок папки Open File на панели Query Tool или File → Open. Выберите DemoExam\Database\demo_exam_2027.sql.
3. После загрузки файла не выделяйте случайный кусок текста. Убедитесь, что выполнение охватывает весь файл.
4. Наведите мышь на кнопку выполнения и прочитайте подсказку Execute script. Нажмите её или F5. В версиях с отдельными Execute query и Execute script выбирайте выполнение всего скрипта.
5. Дождитесь окончания и посмотрите Messages. Если ошибок нет, нажмите в дереве базы Refresh.
6. Раскройте Schemas → public → Tables: должны появиться 15 таблиц. В Views появится order_costs.
7. В новом Query Tool выполните проверочные SELECT из раздела ниже.

**Это SQL дамп, а не архив Restore.** В приложенном demo_exam_2027.sql используются обычные SQL и INSERT. Его можно выполнить через Query Tool. Диалог Restore предназначен для форматов Custom, Tar или Directory. Не применяйте оба способа загрузки к одной уже заполненной базе.

- [Полный SQL дамп из архива](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Database/demo_exam_2027.sql)
- [pgAdmin — Query Tool](https://www.pgadmin.org/docs/pgadmin4/latest/query_tool.html)

### Построить структуру по 01_schema.sql

1. Для самостоятельного маршрута создайте отдельную пустую базу и откройте у неё Query Tool.
2. Нажмите Open File → выберите Database\01_schema.sql. Перед запуском посмотрите начало BEGIN и конец COMMIT.
3. Нажмите Execute script / F5. Все CREATE TABLE выполнятся в одной транзакции. Успех подтверждается отсутствием ошибок в Messages.
4. Нажмите Refresh на Tables. Посчитайте таблицы. users и notes должны быть вместе с производственными таблицами.
5. Нажмите counterparties правой кнопкой → Properties. Посмотрите Columns и Constraints. У id должен быть первичный ключ, а type ограничен допустимыми значениями.
6. Откройте Properties у specification_materials. В Constraints → Primary key проверьте два поля: specification_id и material_id. В Foreign key должны быть ссылки на specifications и materials.
7. Сохраните скрипт в своей папке результата через Save As. Файл SQL нужен эксперту, даже если база уже работает.

**Полный 01_schema.sql из решения**

```sql
-- Задание 2. Выполнить в новой базе demo_exam_2027.
BEGIN;

CREATE TABLE counterparties (
    id varchar(20) PRIMARY KEY,
    name varchar(200) NOT NULL CHECK (btrim(name) <> ''),
    inn varchar(20) NOT NULL DEFAULT '',
    address text NOT NULL DEFAULT '',
    phone varchar(30) NOT NULL DEFAULT '',
    type varchar(30) NOT NULL CHECK (type IN ('Покупатель', 'Поставщик', 'Изготовитель'))
);

CREATE TABLE units (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    name varchar(20) NOT NULL UNIQUE
);

CREATE TABLE products (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    code varchar(30) NOT NULL UNIQUE,
    production_code varchar(30) UNIQUE,
    name varchar(200) NOT NULL,
    unit_id integer NOT NULL REFERENCES units(id)
);

CREATE TABLE materials (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    code varchar(30) NOT NULL UNIQUE,
    name varchar(200) NOT NULL,
    unit_id integer NOT NULL REFERENCES units(id),
    unit_price numeric(14,2) NOT NULL CHECK (unit_price >= 0)
);

CREATE TABLE operations (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    code varchar(30) NOT NULL UNIQUE,
    name varchar(200) NOT NULL,
    unit_id integer NOT NULL REFERENCES units(id),
    unit_price numeric(14,2) NOT NULL CHECK (unit_price >= 0)
);

CREATE TABLE specifications (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    product_id integer NOT NULL UNIQUE REFERENCES products(id),
    name varchar(200) NOT NULL,
    manufacturer_id varchar(20) NOT NULL REFERENCES counterparties(id)
);

CREATE TABLE specification_materials (
    specification_id integer NOT NULL REFERENCES specifications(id),
    material_id integer NOT NULL REFERENCES materials(id),
    quantity numeric(14,6) NOT NULL CHECK (quantity > 0),
    PRIMARY KEY (specification_id, material_id)
);

CREATE TABLE specification_operations (
    specification_id integer NOT NULL REFERENCES specifications(id),
    operation_id integer NOT NULL REFERENCES operations(id),
    quantity numeric(14,6) NOT NULL CHECK (quantity > 0),
    time_norm numeric(14,6) NOT NULL CHECK (time_norm > 0),
    PRIMARY KEY (specification_id, operation_id)
);

CREATE TABLE departments (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    name varchar(150) NOT NULL UNIQUE
);

CREATE TABLE users (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    login varchar(100) NOT NULL CHECK (btrim(login) <> '' AND login = btrim(login)),
    password_hash text NOT NULL,
    role varchar(30) NOT NULL CHECK (role IN ('Администратор', 'Пользователь')),
    is_blocked boolean NOT NULL DEFAULT false,
    failed_attempts integer NOT NULL DEFAULT 0 CHECK (failed_attempts BETWEEN 0 AND 3)
);
CREATE UNIQUE INDEX users_login_unique ON users (lower(login));

CREATE TABLE customer_orders (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    number varchar(30) NOT NULL UNIQUE,
    order_date date NOT NULL,
    customer_id varchar(20) NOT NULL REFERENCES counterparties(id),
    executor_id varchar(20) NOT NULL REFERENCES counterparties(id)
);

CREATE TABLE customer_order_items (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    order_id integer NOT NULL REFERENCES customer_orders(id),
    product_id integer NOT NULL REFERENCES products(id),
    quantity numeric(14,6) NOT NULL CHECK (quantity > 0),
    sale_price numeric(14,2) NOT NULL CHECK (sale_price >= 0),
    discount_amount numeric(14,2) NOT NULL DEFAULT 0 CHECK (discount_amount >= 0),
    CHECK (discount_amount <= quantity * sale_price),
    UNIQUE (order_id, product_id)
);

CREATE TABLE production_orders (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    number varchar(30) NOT NULL UNIQUE,
    order_date date NOT NULL,
    start_date date NOT NULL,
    customer_order_id integer NOT NULL REFERENCES customer_orders(id),
    department_id integer NOT NULL REFERENCES departments(id),
    executor_user_id integer NOT NULL REFERENCES users(id)
);

CREATE TABLE production_order_items (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    production_order_id integer NOT NULL REFERENCES production_orders(id),
    specification_id integer NOT NULL REFERENCES specifications(id),
    quantity numeric(14,6) NOT NULL CHECK (quantity > 0),
    UNIQUE (production_order_id, specification_id)
);

CREATE TABLE notes (
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    title varchar(200) NOT NULL CHECK (btrim(title) <> ''),
    content text NOT NULL,
    id_user integer NOT NULL REFERENCES users(id),
    created_at date NOT NULL DEFAULT CURRENT_DATE
);

COMMIT;
```

BEGIN начинает группу изменений; COMMIT делает её постоянной. Если команда внутри скрипта завершилась ошибкой, транзакция может перейти в состояние failed. Исправьте причину, выполните ROLLBACK в этом же Query Tool и повторите скрипт в действительно пустой базе. Не запускайте CREATE TABLE поверх существующих таблиц.

- [PostgreSQL — ограничения таблиц](https://www.postgresql.org/docs/18/ddl-constraints.html)

### Понять типы и ограничения

| Запись в SQL | Зачем здесь | Пример |
| --- | --- | --- |
| varchar / text | Хранить строки без потери начальных нулей и плюса | id контрагента, ИНН, телефон |
| integer GENERATED BY DEFAULT AS IDENTITY | Автоматический числовой ключ, можно задать id при импорте | users.id |
| numeric(14,2) | Точные суммы с двумя знаками после точки | material.unit_price |
| numeric(14,6) | Дробные нормы с шестью знаками | 0.012 тыс. шт евровинтов |
| date | Календарная дата без времени | notes.created_at |
| boolean | Да или нет | users.is_blocked |
| NOT NULL | Поле обязательно | login, title, количество |
| CHECK | Проверить допустимые значения | quantity > 0; цена >= 0; роль из двух вариантов |
| UNIQUE | Не допустить повтор | код изделия, номер заказа |
| REFERENCES | Не допустить ссылку на несуществующую запись | notes.id_user → users.id |

Число 0.012 в SQL записывается через точку. Оно означает 0,012 тысячи штук, то есть 12 евровинтов на стол. Если заменить его на 12 и оставить цену за тысячу, расчёт станет в тысячу раз больше.

Уникальность логина реализована индексом users_login_unique на lower(login). Поэтому admin и ADMIN нельзя создать как разные аккаунты. Вход также сравнивает lower(login), а пароль проверяется с учётом регистра.

discount_amount хранит скидку на всю позицию заказа. Ограничение запрещает скидку больше quantity × sale_price. Это согласуется с итогом заказа 26 828,00.

**Пустая строка отличается от NULL.** В JSON у одного контрагента inn = "". Поле inn NOT NULL DEFAULT '' позволяет сохранить пустую строку. Не заменяйте её автоматически на число 0 или NULL.

### Импортировать JSON через SQL редактор

1. Откройте Database\Customers.json в Блокноте. Проверьте шесть объектов и названия полей. В исходнике адрес называется addres с одной s в конце.
2. В Query Tool у базы с уже созданной схемой нажмите Open File и откройте Database\02_data.sql.
3. Найдите начало INSERT INTO counterparties. Скрипт включает JSON внутрь $json$...$json$ и преобразует его функцией jsonb_to_recordset.
4. Проверьте соответствие: SELECT id, name, inn, addres, phone, type записывается в столбцы id, name, inn, address, phone, type. Это явное переименование исходного addres в address.
5. Нажмите Execute script / F5. Скрипт импортирует шесть JSON контрагентов, затем добавит двух контрагентов из Excel и все справочные, пользовательские, заказные данные.
6. После успеха выполните SELECT id, name, inn, address, phone, type FROM counterparties ORDER BY id. Для всей таблицы ожидается восемь записей. Шесть исходных JSON записей имеют цифровые строковые коды, две дополнительные — EXCEL-SELLER и EXCEL-BUYER.
7. Проверьте код 000000001, пустой ИНН и точность адресов. Опечатки исходного JSON здесь сохраняются как данные, а не исправляются автоматически.

**Поясняющий фрагмент импорта одной записи**

```sql
INSERT INTO counterparties (id, name, inn, address, phone, type)
SELECT id, name, inn, addres, phone, type
FROM jsonb_to_recordset($json$[
  {"id":"000000001","name":"ООО \"Поставка\"","inn":"",
   "addres":"г.Пятигорск","phone":"+79198634592","type":"Поставщик"}
]$json$::jsonb)
AS source(id text, name text, inn text, addres text, phone text, type text);
```

**Фрагмент выше объясняет механизм.** В реальном запуске выполняйте полный 02_data.sql один раз. Если запись 000000001 уже импортирована, повтор этого фрагмента даст конфликт PK. jsonb_to_recordset превращает массив объектов в строки; AS source задаёт имена и типы извлекаемых колонок.

- [Полный 02_data.sql](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Database/02_data.sql)
- [Исходный Customers.json](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Database/Customers.json)
- [PostgreSQL — jsonb_to_recordset](https://www.postgresql.org/docs/18/functions-json.html)

### Разобрать порядок загрузки и ID

Сначала импортируются counterparties и units. Затем products, materials и operations, потому что они ссылаются на units. После этого specifications и её два состава. Для заказов нужны контрагенты и изделия; для production_orders дополнительно departments и users. notes загружаются после users. Такой порядок позволяет внешним ключам оставаться включёнными.

Скрипт явно вставляет id = 1, 2, 3. В конце setval находит последовательность identity и устанавливает её значение на MAX(id). Иначе следующая автоматически создаваемая запись могла бы получить уже занятый id.

**Как синхронизируется следующий ID пользователей**

```sql
SELECT setval(
  pg_get_serial_sequence('users', 'id'),
  (SELECT MAX(id) FROM users)
);
```

Пароли admin и user123 уже представлены в 02_data.sql в виде хешей. Не заменяйте password_hash на обычный текст admin: PasswordHelper.Verify ожидает формат соль:хеш. Для добавления нового пользователя проще использовать форму администратора, которая сама создаёт хеш.

1. В Query Tool выполните SELECT id, login, role, is_blocked, failed_attempts FROM users ORDER BY id. Ожидаются admin, user25, user26; все разблокированы и имеют ноль ошибок.
2. Выполните SELECT id, title, id_user, created_at FROM notes ORDER BY id. Ожидаются пять записей; user25 имеет id=2, user26 — id=3.
3. Выполните SELECT * FROM customer_order_items. Одна строка: два стола, продажная цена 14120, скидка 1412.
4. После данных откройте и выполните 03_order_cost.sql. Он создаёт представление и сразу выводит результат расчёта. Подробный разбор — задание 3.

### Проверить базу и связи

**Проверочные SELECT для Query Tool**

```sql
SELECT current_database();

SELECT COUNT(*) AS table_count
FROM information_schema.tables
WHERE table_schema = 'public' AND table_type = 'BASE TABLE';

SELECT COUNT(*) AS fk_count
FROM information_schema.table_constraints
WHERE constraint_schema = 'public' AND constraint_type = 'FOREIGN KEY';

SELECT COUNT(*) AS counterparties_count FROM counterparties;
SELECT COUNT(*) AS users_count FROM users;
SELECT COUNT(*) AS notes_count FROM notes;

SELECT * FROM order_costs ORDER BY order_id;
```

Запускайте проверки по одной или выделяйте нужный SELECT: pgAdmin показывает результаты отдельных запросов согласно версии. Ожидаемые значения: ваша база; 15 базовых таблиц; 19 FK; 8 контрагентов; 3 исходных пользователя; 5 заметок. После создания test количество пользователей изменится — это нормально.

1. Для безопасной проверки ограничения цены откройте отдельный Query Tool и выполните BEGIN; затем UPDATE materials SET unit_price = -1 WHERE id = 1;. Ожидается ошибка CHECK. После неё обязательно выполните ROLLBACK; в той же вкладке.
2. Проверьте отрицательную цену на тестовой копии, а не на рабочем наборе перед показом. Если транзакция откатилась, исходная цена останется 3250.
3. Для проверки внешнего ключа используйте тестовую транзакцию: INSERT INTO notes(title,content,id_user) VALUES ('Проверка','Не сохранять',2147483647);. Ожидается ошибка FK при условии, что такого пользователя нет. Завершите ROLLBACK;
4. Сохраните скриншоты успешных SELECT или выгрузите результат кнопкой Save results to file. Ошибка ограничения подтверждает защиту данных, а не поломку базы.

**Не оставляйте открытую транзакцию.** BEGIN в Query Tool держит изменения и блокировки до COMMIT или ROLLBACK. После каждой проверки с намеренной ошибкой выполните ROLLBACK в той же вкладке.

### Сохранить резервную копию через интерфейс

1. В pgAdmin нажмите demo_exam_2027 правой кнопкой → Backup.
2. В Filename выберите папку результата и имя demo_exam_2027.backup. Для простого восстановления через Restore выберите Format = Custom.
3. Оставьте включёнными структуру и данные. Если есть переключатели Only data или Only schema, не включайте их для полной копии.
4. Нажмите Backup. Посмотрите статус в Processes. Успешное завершение должно быть подтверждено сообщением процесса.
5. Создайте другую пустую базу demo_exam_2027_restore. Нажмите её правой кнопкой → Restore → выберите созданный .backup → Restore.
6. После восстановления повторите SELECT количества строк и order_costs. Это доказывает, что копия действительно восстанавливается.
7. Если нужны именно SQL-файлы, сохраните 01_schema.sql, 02_data.sql и 03_order_cost.sql. Plain backup тоже является текстом, но может содержать команды COPY или служебные команды psql; такой файл нельзя автоматически считать пригодным для Query Tool. Приложенный дамп проверен по структуре: в нём INSERT.

- [pgAdmin — Backup](https://www.pgadmin.org/docs/pgadmin4/latest/backup_dialog.html)
- [pgAdmin — Restore](https://www.pgadmin.org/docs/pgadmin4/latest/restore_dialog.html)

### Проверка готовности

- [ ] Таблицы соответствуют ER диаграмме
- [ ] Типы и ограничения соответствуют данным
- [ ] Все шесть записей исходного JSON сохранены
- [ ] База и SQL доступны для проверки
- [ ] Импортировал 6 JSON контрагентов и вижу 8 вместе с Excel данными
- [ ] Выполнил скрипты в одной правильной базе
- [ ] Проверил 15 таблиц, 19 FK, 3 пользователей и 5 заметок
- [ ] Сохранил копию и восстановил её в отдельную базу

## 03 Рассчитать себестоимость заказа

Почему два стола стоят 14 374,28 ₽, как работают CTE, JOIN, SUM и GROUP BY и почему нельзя напрямую соединить материалы с операциями.

Результат: 03_order_cost.sql и результат: материалы 9 974,28, операции 4 400,00, себестоимость 14 374,28, продажа 26 828,00.

### Сначала посчитать вручную

Для каждой спецификации считаем стоимость материалов на одно изделие: сумма нормы расхода × цены материала. Отдельно считаем операции: сумма количества операции × нормы времени × цены нормы. Складываем обе суммы и умножаем на количество изделий в позиции заказа. Если в заказе несколько позиций, складываем стоимость всех позиций.

| Материал | Норма на стол | Цена нормы ₽ | На стол ₽ |
| --- | --- | --- | --- |
| Столешница | 1 шт | 3250 | 3250,00 |
| Деталь 500×800 | 2 шт | 95 | 190,00 |
| Деталь 600×800 | 4 шт | 140 | 560,00 |
| Евровинт | 0,012 тыс. шт | 595 | 7,14 |
| Опора | 4 шт | 245 | 980,00 |
| Всего |  |  | 4987,14 |

| Операция | Количество | Норма времени | Цена ₽ | На стол ₽ |
| --- | --- | --- | --- | --- |
| Сборка модулей | 1 | 0,75 | 1400 | 1050,00 |
| Распил | 1 | 1,5 | 450 | 675,00 |
| Упаковка | 1 | 0,5 | 950 | 475,00 |
| Всего |  |  |  | 2200,00 |

Один стол: 4987,14 + 2200,00 = 7187,14. Два стола: 7187,14 × 2 = 14374,28. Материалы на два стола: 9974,28. Операции: 4400,00. Продажная сумма отдельно: 2 × 14120 − 1412 = 26828,00.

**Как трактуется расценка операции.** В исходной таблице цен единица расценки операций не подписана. Автор решения применил цены к норме времени из спецификации. Это допущение нужно объяснить при защите. Сумма 14374,28 следует из этого правила; противоречивый итог исходного листа расчёта не копируется.

Интерактивный калькулятор стоимости доступен в уроке на сайте. Ручной расчёт приведён выше.

### Открыть и выполнить полный запрос

1. В pgAdmin нажмите вашу заполненную базу правой кнопкой → Query Tool.
2. Open File → Database\03_order_cost.sql. Посмотрите первую строку CREATE OR REPLACE VIEW order_costs AS.
3. Выполните весь скрипт через Execute script. Представление создастся, а последний SELECT выведет результат.
4. Если Data Output пуст, выделите только SELECT * FROM order_costs ORDER BY order_id; и выполните его отдельно.
5. Сверьте number = 1 и customer = ИП Томилин Александр Сергеевич. Затем сверяйте четыре денежные колонки с ручным расчётом.
6. Нажмите Save As и сохраните запрос в своей папке задания 3. Сохраните результат или снимок Data Output рядом.

**Полный запрос решения**

```sql
-- Задание 3. Сначала суммируем материалы и операции отдельно.
-- Если соединить их напрямую, строки перемножатся и стоимость завысится.
CREATE OR REPLACE VIEW order_costs AS
WITH material_cost AS (
    SELECT sm.specification_id,
           SUM(sm.quantity * m.unit_price) AS cost
    FROM specification_materials sm
    JOIN materials m ON m.id = sm.material_id
    GROUP BY sm.specification_id
), operation_cost AS (
    SELECT so.specification_id,
           SUM(so.quantity * so.time_norm * op.unit_price) AS cost
    FROM specification_operations so
    JOIN operations op ON op.id = so.operation_id
    GROUP BY so.specification_id
)
SELECT o.id AS order_id, o.number, o.order_date,
       c.name AS customer,
       ROUND(COALESCE(SUM(i.quantity * COALESCE(mc.cost, 0)), 0), 2) AS materials_cost,
       ROUND(COALESCE(SUM(i.quantity * COALESCE(oc.cost, 0)), 0), 2) AS operations_cost,
       ROUND(COALESCE(SUM(i.quantity *
           (COALESCE(mc.cost, 0) + COALESCE(oc.cost, 0))), 0), 2) AS total_cost,
       ROUND(COALESCE(SUM(i.quantity * i.sale_price - i.discount_amount), 0), 2) AS sale_total
FROM customer_orders o
JOIN counterparties c ON c.id = o.customer_id
LEFT JOIN customer_order_items i ON i.order_id = o.id
LEFT JOIN specifications s ON s.product_id = i.product_id
LEFT JOIN material_cost mc ON mc.specification_id = s.id
LEFT JOIN operation_cost oc ON oc.specification_id = s.id
GROUP BY o.id, o.number, o.order_date, c.name;

SELECT * FROM order_costs ORDER BY order_id;
```

### Что делает material_cost

**Сумма материалов на одну спецификацию**

```sql
WITH material_cost AS (
    SELECT sm.specification_id,
           SUM(sm.quantity * m.unit_price) AS cost
    FROM specification_materials sm
    JOIN materials m ON m.id = sm.material_id
    GROUP BY sm.specification_id
)
```

sm — короткое имя таблицы specification_materials; m — materials. JOIN находит справочную запись для материала по material_id. У неё берём unit_price, а из строки спецификации — quantity. SUM складывает стоимость всех материалов.

GROUP BY sm.specification_id возвращает одну строку на спецификацию. В нашем наборе это specification_id=1 и cost=4987.14. Временное именованное выражение material_cost существует внутри данного запроса. Это CTE: его удобно воспринимать как заранее посчитанную маленькую таблицу.

1. Для проверки скопируйте WITH material_cost AS (...) целиком в новую вкладку Query Tool.
2. После закрывающей скобки добавьте SELECT * FROM material_cost; и выполните. Получите одну сумму на спецификацию.
3. Сравните сумму с пятью материальными строками вручную. Если уже здесь ошибка, проверяйте нормы, цены и единицы; внешний запрос ещё не участвует.

### Что делает operation_cost

**Сумма операций на одну спецификацию**

```sql
operation_cost AS (
    SELECT so.specification_id,
           SUM(so.quantity * so.time_norm * op.unit_price) AS cost
    FROM specification_operations so
    JOIN operations op ON op.id = so.operation_id
    GROUP BY so.specification_id
)
```

В so.quantity хранится количество выполнения операции, а в so.time_norm — время одного выполнения. Даже если сейчас quantity везде 1, её нельзя выбрасывать: другой набор может содержать два выполнения. op.unit_price — стоимость единицы нормы.

Результат для спецификации стола: 2200.00. Во всём запросе CTE идут после одного WITH и разделяются запятой. Если проверяете operation_cost отдельно, начните текст с WITH operation_cost AS (...), а в конце добавьте SELECT * FROM operation_cost;.

### Почему прямое соединение завышает стоимость

У стола пять материалов и три операции. Если сначала соединить specification_materials и specification_operations по specification_id, получится 5 × 3 = 15 строк. Каждый материал повторится три раза, а каждая операция пять раз. SUM после такого соединения сложит повторяющиеся суммы.

| Подход | Что соединяется | Что получается |
| --- | --- | --- |
| Прямой JOIN двух составов | 5 материальных × 3 операционных строки | 15 строк; материалы ×3 и операции ×5 |
| Сначала отдельные суммы | Одна сумма материалов + одна сумма операций | Одна строка на спецификацию; повторов нет |

Правильный запрос сперва группирует каждый состав отдельно. Затем к строке заказа присоединяются только mc.cost и oc.cost. Поэтому количество материалов не влияет на число повторов операций.

**Не чините эту ошибку через SUM DISTINCT.** Два разных материала могут иметь одинаковую стоимость. SUM(DISTINCT ...) тогда ошибочно уберёт одну из реальных сумм. Нужно исправить состав строк JOIN, а не отбрасывать одинаковые числа.

### Разобрать внешний SELECT

customer_orders o задаёт список заказов. JOIN counterparties c находит покупателя. LEFT JOIN customer_order_items i сохраняет заказ даже без позиций. LEFT JOIN specifications s подбирает спецификацию изделия. Затем LEFT JOIN присоединяет отдельно посчитанные material_cost mc и operation_cost oc.

i.quantity — количество изделий в заказе. Именно на него умножаются mc.cost и oc.cost. SUM складывает позиции одного заказа. COALESCE(..., 0) заменяет отсутствующую сумму нулём. ROUND(..., 2) округляет итог до копеек после суммирования.

GROUP BY o.id, o.number, o.order_date, c.name позволяет получить одну строку на заказ. ORDER BY не зашит в представление: порядок выводится последним SELECT ORDER BY order_id или SQL-кодом приложения.

sale_total вычисляется отдельно через quantity × sale_price − discount_amount. Он не прибавляется к себестоимости. При изменении цены продажи меняется sale_total, а материалы и операции остаются прежними.

**Нулевая стоимость может быть сигналом неполных данных.** LEFT JOIN и COALESCE сохраняют заказ без состава, но отсутствие спецификации даст нулевой вклад. В учебном наборе спецификация есть. Для реальной системы пришлось бы отдельно сигнализировать о неполных нормах.

- [PostgreSQL — соединения таблиц](https://www.postgresql.org/docs/18/queries-table-expressions.html)
- [PostgreSQL — агрегатные функции](https://www.postgresql.org/docs/18/functions-aggregate.html)
- [PostgreSQL — COALESCE](https://www.postgresql.org/docs/18/functions-conditional.html)

### Проверить расчёт без порчи исходных данных

1. В отдельном Query Tool выполните BEGIN;. Затем UPDATE customer_order_items SET quantity = 3 WHERE id = 1;.
2. Выполните SELECT materials_cost, operations_cost, total_cost FROM order_costs WHERE order_id = 1;. Ожидайте 14961,42; 6600,00; 21561,42.
3. Выполните ROLLBACK; в той же вкладке. Снова проверьте SELECT: вернутся суммы для двух столов.
4. Для проверки нескольких изделий и пустого заказа используйте DemoExam.Checks. Проверка меняет набор внутри транзакции и откатывает его.
5. Откройте WPF как администратор → вкладка «Стоимость заказов» → «Обновить расчёт». Суммы должны совпасть с pgAdmin.
6. Если WPF показывает старое значение после уже завершённого COMMIT, нажмите «Обновить расчёт». Если транзакция в pgAdmin ещё открыта, другой процесс не увидит незавершённые изменения.

Представление — сохранённый SQL, а не отдельная таблица с вручную обновляемыми результатами. Когда читаете order_costs, PostgreSQL выполняет запрос на текущих данных. Кнопка приложения обновляет только отображение.

- [PostgreSQL — CREATE VIEW](https://www.postgresql.org/docs/18/sql-createview.html)

### Проверка готовности

- [ ] Запрос использует JOIN SUM и GROUP BY
- [ ] Количество изделий и нормы учтены
- [ ] Материалы и операции суммируются отдельно
- [ ] Итог соответствует выбранным ценам и нормам
- [ ] Объясняю 4987,14 + 2200 на один стол
- [ ] Различаю себестоимость 14374,28 и продажу 26828
- [ ] Проверил, почему прямой JOIN 5 × 3 даёт повторы
- [ ] Тестовые изменения откатил ROLLBACK

## 04 Собрать WPF с входом пазлом и администратором

Создаём Core и WPF через меню Visual Studio, добавляем точные файлы архива, разбираем обработчики и проверяем вход, роли, три ошибки и разблокировку.

Результат: Исходники DemoExam.Core и DemoExam.Wpf, исполняемая программа, правильные сообщения входа, интерактивный пазл и управление пользователями.

### Понять роль каждого файла

| Файл | Назначение | Что смотреть |
| --- | --- | --- |
| DemoExam.Wpf.csproj | Настройки проекта WPF | net9.0-windows, UseWPF, ссылка Core, изображения и JSON |
| App.xaml | Точка старта и общие стили | StartupUri="LoginWindow.xaml" |
| App.xaml.cs | Перехват непредвиденной ошибки интерфейса | DispatcherUnhandledException |
| LoginWindow.xaml | Окно входа | Логин, PasswordBox, кнопка Войти, PuzzleControl |
| LoginWindow.xaml.cs | Событие нажатия Войти | Вызов LoginAsync и открытие окна роли |
| PuzzleControl.xaml и .cs | Четыре интерактивных фрагмента | Кнопки Образец и Перемешать, обмен фрагментов |
| AdminWindow.xaml и .cs | Рабочий стол администратора | Список, поиск, новый пользователь, сохранение, стоимость |
| UserWindow.xaml и .cs | Рабочий стол пользователя | Только чтение собственных заметок |
| UiMessages.cs | Общие окна сообщений | Заголовок и пиктограмма ошибки/информации |
| DemoExam.Core/Database.cs | Все SQL-операции | LoginAsync, GetUsersAsync, SaveUserAsync |
| PasswordHelper.cs | Хеширование и проверка пароля | Случайная соль, PBKDF2 и сравнение |
| Models.cs | Данные для окон | User, LoginResult, Note, OrderCost |
| PuzzleState.cs | Логика порядка фрагментов | Shuffle, Swap, IsSolved |

XAML — описание внешнего вида: расположение, текст, свойства элементов и привязка событий. C# в .xaml.cs — действия после нажатия. Core содержит логику, которую можно читать и проверять отдельно от окна. В этом решении используются обычные обработчики событий и прямые SQL-запросы; дополнительных шаблонов архитектуры для повторения не требуется.

### Создать решение и библиотеку Core с нуля

Если цель сейчас — только разобраться в готовом решении, можно открыть Source/DemoExam.sln и читать файлы по следующим разделам. Для полного повторения создайте отдельную папку, например C:\ExamPractice, чтобы сохранить архив как образец.

1. Откройте Visual Studio → «Файл → Создать → Проект» / File → New → Project. В стартовом окне равнозначная кнопка — Create a new project.
2. В строке поиска шаблонов введите Class Library. Выберите C# «Библиотека классов» для .NET. Шаблон с надписью .NET Framework не подходит для точного повторения.
3. Нажмите «Далее». Project name = DemoExam.Core. Location = C:\ExamPractice. Solution name = DemoExam. Если предлагается размещать решение и проект в одной папке, для удобства оставьте отдельную папку решения.
4. Нажмите «Далее», выберите .NET 9.0, затем «Создать». В дереве появится решение DemoExam и проект DemoExam.Core.
5. Нажмите Class1.cs правой кнопкой → «Удалить». Это пустой файл шаблона, в примере он не нужен.
6. Нажмите проект DemoExam.Core правой кнопкой → «Управление пакетами NuGet…» / Manage NuGet Packages. Выберите вкладку «Обзор» / Browse.
7. В источнике пакетов выберите nuget.org, в поиске введите Npgsql. Выберите пакет Npgsql, установите версию 9.0.3 через поле Version и нажмите Install. Подтвердите окно изменений и лицензию, если появятся.
8. В Dependencies / «Зависимости» → Packages / «Пакеты» должна появиться Npgsql 9.0.3. Если получить пакет невозможно, прочитайте раздел подготовки: для сборки нужен доступный пакет в среде.
9. Добавьте Models.cs, PasswordHelper.cs, PuzzleState.cs и Database.cs способом из следующего раздела. Не переименовывайте DemoExam.Core: пространства имён последующего кода рассчитаны на это имя.

**Как должен выглядеть проект Core**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup><PackageReference Include="Npgsql" Version="9.0.3" /></ItemGroup>
</Project>
```

- [Microsoft — проекты и решения](https://learn.microsoft.com/en-us/visualstudio/ide/solutions-and-projects-in-visual-studio?view=vs-2022)
- [Microsoft — NuGet в Visual Studio](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio)

### Добавить файлы и не получить дубликаты

1. Чтобы написать файл самостоятельно, нажмите DemoExam.Core правой кнопкой → «Добавить → Класс…» / Add → Class. Введите, например, Models.cs → «Добавить».
2. В открывшемся редакторе нажмите Ctrl+A и вставьте полный код соответствующего блока ниже. Он заменяет всё содержимое, включая using и namespace. Не вставляйте полный класс внутрь класса, который создал шаблон.
3. Сохраните через Ctrl+S. Для остальных .cs повторите «Добавить → Класс» с точным именем файла.
4. Чтобы использовать файл из архива без ручного копирования текста, нажмите проект правой кнопкой → «Добавить → Существующий элемент…» / Add → Existing Item. Выберите нужный .cs в Source\DemoExam.Core и нажмите «Добавить». Обычное Add копирует файл в проект; Add As Link для этой тренировки не выбирайте.
5. Используйте один способ для каждого файла. Если Models.cs уже добавлен через класс, не добавляйте его второй копией под другим именем.
6. После добавления всех четырёх .cs нажмите Core правой кнопкой → «Собрать». При успехе переходите к WPF. Если ошибка namespace или тип уже определён, проверьте имя проекта и отсутствие дубликатов.

Для учебного разбора ниже показан полный код важных файлов. Кнопка копирования у блока копирует только код. Название блока указывает, в какой файл его вставить. Код всех файлов также лежит внутри скачанного ZIP.

### Создать модели данных

**DemoExam.Core → Models.cs**

```csharp
namespace DemoExam.Core;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string Role { get; set; } = "Пользователь";
    public bool IsBlocked { get; set; }
    public int FailedAttempts { get; set; }
}

public record LoginResult(bool Success, string Message, User? User = null);

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class OrderCost
{
    public string Number { get; set; } = "";
    public string Customer { get; set; } = "";
    public decimal MaterialsCost { get; set; }
    public decimal OperationsCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal SaleTotal { get; set; }
}
```

User содержит данные для таблицы пользователей. Id = 0 используется формой как признак новой записи; существующая запись имеет ID из базы. PasswordHash не выводится в модель интерфейса, поэтому хеш не попадает в DataGrid. LoginResult передаёт Success, Message и при успешном входе User.

Note содержит дату как DateTime для WPF; формат вывода задаётся в XAML. OrderCost использует decimal для точных денежных значений PostgreSQL numeric. Имена свойств должны совпадать с Binding в XAML: Login, Role, IsBlocked, TotalCost и остальные.

### Создать PasswordHelper и понять хеш

**DemoExam.Core → PasswordHelper.cs**

```csharp
using System.Security.Cryptography;

namespace DemoExam.Core;

public static class PasswordHelper
{
    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    public static bool Verify(string password, string savedHash)
    {
        try
        {
            string[] parts = savedHash.Split(':');
            if (parts.Length != 2) return false;
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expected = Convert.FromBase64String(parts[1]);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}
```

Hash создаёт 16 случайных байт соли и вычисляет 32 байта PBKDF2 с SHA256 и 100000 итераций. Соль и результат кодируются Base64, затем соединяются двоеточием. Поэтому одинаковый пароль двух пользователей обычно имеет разные сохранённые строки.

Verify разделяет сохранённую строку, читает ту же соль и вычисляет результат для введённого пароля. FixedTimeEquals сравнивает байты. Сохранённый пароль не расшифровывается: проверка заново вычисляет производную от введённого текста.

Пароль не обрезается Trim: пробел может быть его частью. Логин обрезается и проверяется отдельно. Нельзя вручную записать user123 в password_hash: это не тот формат данных.

- [Microsoft — PBKDF2](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rfc2898derivebytes.pbkdf2?view=net-9.0)
- [Microsoft — FixedTimeEquals](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.cryptographicoperations.fixedtimeequals?view=net-9.0)

### Создать логику пазла

**DemoExam.Core → PuzzleState.cs**

```csharp
namespace DemoExam.Core;

public class PuzzleState
{
    public int[] Pieces { get; } = [0, 1, 2, 3];
    public bool IsSolved => Pieces.SequenceEqual(new[] { 0, 1, 2, 3 });

    public void Shuffle()
    {
        do { Random.Shared.Shuffle(Pieces); } while (IsSolved);
    }

    public void Swap(int first, int second)
    {
        if (first < 0 || first > 3 || second < 0 || second > 3)
            throw new ArgumentOutOfRangeException(nameof(first));
        (Pieces[first], Pieces[second]) = (Pieces[second], Pieces[first]);
    }
}
```

Pieces хранит номера изображений в четырёх позициях. Правильный порядок — [0,1,2,3]: сверху слева, сверху справа, снизу слева, снизу справа. IsSolved сравнивает весь порядок. Индексы в коде начинаются с нуля, а файлы изображений называются 1.png–4.png.

Shuffle перемешивает массив и повторяет перемешивание, если случайно получилась уже собранная картинка. Swap меняет два значения местами и проверяет допустимые индексы. Алгоритм перестановки здесь отделён от рисования кнопок.

**Пример обмена двух фрагментов**

```text
До обмена:  [2, 1, 0, 3]
Нажаты позиции 0 и 2
После:      [0, 1, 2, 3]
IsSolved = true
```

### Добавить Database.cs и разобраться в подключении

**DemoExam.Core → Database.cs**

```csharp
using System.Text.Json;
using Npgsql;

namespace DemoExam.Core;

// Все обращения к PostgreSQL собраны в одном понятном классе.
public static class Database
{
    public const string BlockedMessage = "Вы заблокированы. Обратитесь к администратору";
    public const string InvalidMessage = "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные";

    public static string ConnectionString { get; set; } = LoadConnectionString();

    private static string LoadConnectionString()
    {
        string? value = Environment.GetEnvironmentVariable("DEMO_DB_CONNECTION");
        if (!string.IsNullOrWhiteSpace(value)) return value;
        string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Рядом с программой отсутствует appsettings.json.");
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        return json.RootElement.GetProperty("ConnectionStrings").GetProperty("Database").GetString()!;
    }

    public static async Task<NpgsqlConnection> OpenAsync()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        try { await connection.OpenAsync(); return connection; }
        catch { await connection.DisposeAsync(); throw; }
    }

    public static async Task<LoginResult> LoginAsync(string login, string password, bool puzzleSolved)
    {
        login = login.Trim();
        if (login.Length == 0 || password.Length == 0)
            return new(false, "Заполните поля «Логин» и «Пароль».");

        await using var connection = await OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        // Блокировка строки не даёт двум одновременным входам потерять счётчик ошибок.
        await using var select = new NpgsqlCommand("""
            SELECT id, login, role, is_blocked, failed_attempts, password_hash
            FROM users WHERE lower(login) = lower(@login) FOR UPDATE
            """, connection, transaction);
        select.Parameters.AddWithValue("login", login);
        User user;
        string hash;
        await using (var reader = await select.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync()) return new(false, InvalidMessage);
            user = new User { Id = reader.GetInt32(0), Login = reader.GetString(1), Role = reader.GetString(2),
                IsBlocked = reader.GetBoolean(3), FailedAttempts = reader.GetInt32(4) };
            hash = reader.GetString(5);
        }
        if (user.IsBlocked) return new(false, BlockedMessage);

        bool passwordCorrect = PasswordHelper.Verify(password, hash);
        bool success = passwordCorrect && puzzleSolved;
        int attempts = success ? 0 : Math.Min(user.FailedAttempts + 1, 3);
        bool blocked = attempts >= 3;
        await using var update = new NpgsqlCommand("""
            UPDATE users SET failed_attempts = @attempts, is_blocked = @blocked WHERE id = @id
            """, connection, transaction);
        update.Parameters.AddWithValue("attempts", attempts);
        update.Parameters.AddWithValue("blocked", blocked);
        update.Parameters.AddWithValue("id", user.Id);
        await update.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
        if (success) { user.FailedAttempts = 0; return new(true, "Вы успешно авторизовались", user); }
        if (blocked) return new(false, BlockedMessage);
        return new(false, passwordCorrect
            ? "Пазл собран неверно. Поменяйте фрагменты местами и повторите вход."
            : InvalidMessage);
    }

    private static async Task RequireAdminAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int actorId)
    {
        await using var command = new NpgsqlCommand("""
            SELECT COUNT(*) FROM users WHERE id = @id AND role = 'Администратор' AND NOT is_blocked
            """, connection, transaction);
        command.Parameters.AddWithValue("id", actorId);
        if (Convert.ToInt64(await command.ExecuteScalarAsync()) != 1)
            throw new InvalidOperationException("Изменять пользователей может только действующий администратор.");
    }

    public static async Task<List<User>> GetUsersAsync(int actorId, string search = "")
    {
        await using var connection = await OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await RequireAdminAsync(connection, transaction, actorId);
        await using var command = new NpgsqlCommand("""
            SELECT id, login, role, is_blocked, failed_attempts FROM users
            WHERE position(lower(@search) in lower(login)) > 0 ORDER BY id
            """, connection, transaction);
        command.Parameters.AddWithValue("search", search.Trim());
        var users = new List<User>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            users.Add(new User { Id = reader.GetInt32(0), Login = reader.GetString(1), Role = reader.GetString(2),
                IsBlocked = reader.GetBoolean(3), FailedAttempts = reader.GetInt32(4) });
        return users;
    }

    public static async Task SaveUserAsync(int actorId, User user, string newPassword)
    {
        user.Login = user.Login.Trim();
        if (user.Login.Length == 0 || user.Login.Length > 100)
            throw new InvalidOperationException("Логин должен содержать от 1 до 100 символов.");
        if (user.Role != "Администратор" && user.Role != "Пользователь")
            throw new InvalidOperationException("Выберите роль пользователя.");
        if (user.Id == 0 && newPassword.Length == 0)
            throw new InvalidOperationException("Для нового пользователя укажите пароль.");
        if (user.Id == actorId && (user.IsBlocked || user.Role != "Администратор"))
            throw new InvalidOperationException("Нельзя заблокировать себя или снять свою роль администратора.");

        await using var connection = await OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await RequireAdminAsync(connection, transaction, actorId);
        await using var duplicate = new NpgsqlCommand("""
            SELECT COUNT(*) FROM users WHERE lower(login) = lower(@login) AND id <> @id
            """, connection, transaction);
        duplicate.Parameters.AddWithValue("login", user.Login);
        duplicate.Parameters.AddWithValue("id", user.Id);
        if (Convert.ToInt64(await duplicate.ExecuteScalarAsync()) > 0)
            throw new InvalidOperationException("Пользователь с указанным логином уже существует. Введите другой логин.");

        string sql = user.Id == 0
            ? "INSERT INTO users (login, password_hash, role, is_blocked, failed_attempts) VALUES (@login, @hash, @role, @blocked, @attempts)"
            : """
              UPDATE users SET login = @login, role = @role, is_blocked = @blocked,
                  failed_attempts = @attempts,
                  password_hash = CASE WHEN @hash = '' THEN password_hash ELSE @hash END
              WHERE id = @id
              """;
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("login", user.Login);
        command.Parameters.AddWithValue("hash", newPassword.Length == 0 ? "" : PasswordHelper.Hash(newPassword));
        command.Parameters.AddWithValue("role", user.Role);
        command.Parameters.AddWithValue("blocked", user.IsBlocked);
        // При снятии блокировки начинается новая последовательность попыток.
        command.Parameters.AddWithValue("attempts", user.IsBlocked ? 3 : 0);
        try
        {
            if (await command.ExecuteNonQueryAsync() != 1)
                throw new InvalidOperationException("Пользователь не найден. Обновите список.");
            await transaction.CommitAsync();
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException("Пользователь с указанным логином уже существует. Введите другой логин.");
        }
    }

    public static async Task<List<Note>> GetNotesAsync(int userId)
    {
        await using var connection = await OpenAsync();
        await using var command = new NpgsqlCommand("SELECT id, title, content, created_at FROM notes WHERE id_user = @id ORDER BY id", connection);
        command.Parameters.AddWithValue("id", userId);
        var notes = new List<Note>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) notes.Add(new Note { Id = reader.GetInt32(0), Title = reader.GetString(1),
            Content = reader.GetString(2), CreatedAt = reader.GetDateTime(3) });
        return notes;
    }

    public static async Task<List<OrderCost>> GetOrderCostsAsync()
    {
        await using var connection = await OpenAsync();
        await using var command = new NpgsqlCommand("SELECT number, customer, materials_cost, operations_cost, total_cost, sale_total FROM order_costs ORDER BY order_id", connection);
        var orders = new List<OrderCost>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) orders.Add(new OrderCost { Number = reader.GetString(0), Customer = reader.GetString(1),
            MaterialsCost = reader.GetDecimal(2), OperationsCost = reader.GetDecimal(3), TotalCost = reader.GetDecimal(4), SaleTotal = reader.GetDecimal(5) });
        return orders;
    }
}
```

ConnectionString загружается один раз при инициализации статического класса. Сначала читается DEMO_DB_CONNECTION. Если её нет, ищется appsettings.json рядом с исполняемым файлом через AppContext.BaseDirectory. Если изменить JSON у уже работающего процесса, для применения требуется перезапуск.

OpenAsync создаёт NpgsqlConnection и открывает соединение. await using освобождает соединение, команду и reader после блока. Асинхронное ожидание позволяет интерфейсу не зависать на время обращения к БД. SQL-параметры @login, @id и @search получают значения через Parameters, а не через склеивание текста запроса.

В этом решении AddWithValue использует именованные параметры. Npgsql поддерживает их; современные примеры Npgsql также показывают позиционные $1. Для точного повторения сохраняйте стиль архива и понимайте главное: пользовательский ввод передаётся отдельно от SQL.

- [Npgsql — соединения команды параметры и чтение](https://www.npgsql.org/doc/basic-usage.html)

### Проследить весь LoginAsync

1. Сначала login.Trim() удаляет края логина. Если логин или пароль пустой, возвращается «Заполните поля „Логин“ и „Пароль“». Соединение ещё не открывается, счётчик не увеличивается.
2. Затем открываются соединение и транзакция. SELECT ищет lower(login) = lower(@login) и читает роль, блокировку, ошибки и хеш.
3. FOR UPDATE блокирует найденную строку пользователя до завершения транзакции. Это нужно, чтобы две одновременные попытки не потеряли изменение счётчика.
4. Если логин не найден, возвращается сообщение о неверном логине или пароле. Нельзя увеличить failed_attempts у несуществующей записи.
5. Если is_blocked = true, сразу возвращается «Вы заблокированы. Обратитесь к администратору». Правильный пароль сам по себе блокировку не снимает.
6. PasswordHelper.Verify проверяет пароль. Успех разрешён только если passwordCorrect && puzzleSolved.
7. При успехе attempts = 0. При неудаче attempts = Math.Min(старое + 1, 3). Три попытки дают blocked = true.
8. UPDATE записывает failed_attempts и is_blocked. CommitAsync сохраняет их в базе до возврата результата.
9. Если вход успешен, возвращается User и «Вы успешно авторизовались». Если достигли трёх ошибок, возвращается сообщение блокировки. Иначе при верном пароле сообщается неверный пазл, при неверном — неверный логин или пароль.

| Действие существующего разблокированного пользователя | Счётчик после | Результат |
| --- | --- | --- |
| Пустое поле | Без изменения | Просьба заполнить поля |
| Неверный пароль с 0 ошибок | 1 | Ошибка данных |
| Правильный пароль + неверный пазл с 1 ошибкой | 2 | Ошибка пазла |
| Ещё одна неудача | 3 | Блокировка |
| Успешный вход до достижения 3 | 0 | Открытие рабочего стола |
| Верный пароль уже заблокированного | 3 | Вход закрыт |

Для завершённых проверок счётчик хранится в PostgreSQL, поэтому закрытие окна и перезапуск WPF не обнуляют его. Перемешивание пазла тоже не является успешным входом и не сбрасывает ошибки.

### Добавить WPF и ссылку на Core

1. В Solution Explorer нажмите решение DemoExam правой кнопкой → «Добавить → Создать проект…» / Add → New Project.
2. В поиске введите WPF. Выберите C# «Приложение WPF» / WPF Application для .NET. Не выбирайте WPF App (.NET Framework). Нажмите «Далее».
3. Project name = DemoExam.Wpf. Создайте его рядом с DemoExam.Core внутри папки решения. Выберите .NET 9.0 → «Создать».
4. Нажмите DemoExam.Wpf правой кнопкой → «Добавить → Ссылка на проект…» / Add → Project Reference. Если пункт расположен у Dependencies, нажмите их правой кнопкой → Add Project Reference.
5. В Reference Manager выберите Projects / «Проекты» → Solution / «Решение», поставьте галочку DemoExam.Core → OK. В зависимостях WPF должна появиться ссылка Core.
6. Откройте DemoExam.Wpf.csproj двойным щелчком по проекту или через «Изменить файл проекта». Сравните с блоком ниже: net9.0-windows, UseWPF=true и ProjectReference на Core.
7. Выполните «Сборка → Собрать решение». На этом этапе шаблонное окно может ещё открываться, но подключение Core уже должно собираться.

**DemoExam.Wpf → файл проекта**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DemoExam.Core\DemoExam.Core.csproj" />
    <Resource Include="Images\*.png" />
    <Content Include="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

**Что подключает NuGet.** Npgsql устанавливается в Core. Через ProjectReference зависимость доступна использующим Core проектам. Не копируйте DLL вручную по случайным папкам. Для самостоятельного проекта сохраняйте согласованные имена и относительный путь к Core.

- [Microsoft — ссылки между проектами](https://learn.microsoft.com/en-us/visualstudio/ide/managing-references-in-a-project?view=vs-2022)
- [Microsoft — WPF проект](https://learn.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-wpf?view=vs-2022)

### Добавить окна и UserControl

1. Нажмите DemoExam.Wpf правой кнопкой → «Добавить → Создать элемент…» / Add → New Item. Найдите WPF Window / «Окно WPF» и назовите LoginWindow.xaml.
2. Повторите для AdminWindow.xaml и UserWindow.xaml. Создаются пары .xaml и .xaml.cs. В дереве code-behind обычно скрыт под стрелкой у XAML.
3. Через Add → New Item выберите WPF User Control / «Пользовательский элемент управления WPF». Имя = PuzzleControl.xaml. Это часть окна, а не отдельное окно приложения.
4. Если нужные шаблоны не видны, проверьте рабочую нагрузку .NET desktop development. Для быстрого повторения можно Add → Existing Item и выбрать обе части каждой пары из архива.
5. Для каждого созданного XAML переключитесь на вкладку XAML внизу дизайнера, выделите весь текст и вставьте полный код соответствующего файла ниже.
6. Откройте вложенный .xaml.cs и замените его полным кодом пары. У XAML x:Class и у C# namespace + имя класса должны совпадать, например DemoExam.Wpf.LoginWindow.
7. Удалите шаблонную пару MainWindow.xaml / MainWindow.xaml.cs, если она больше не нужна. Сначала переключите StartupUri в App.xaml на LoginWindow.xaml, как в следующем разделе.
8. Добавьте UiMessages.cs через Add → Class. Сохраните всё и соберите решение. Если ошибка InitializeComponent не найден, сначала ищите ошибку XAML или несовпадение x:Class; этот метод вручную не пишется.

При добавлении окон из архива имена и пространства имён уже правильные. В SDK-проектах файлы внутри папки проекта часто включаются автоматически: не оставляйте вторую копию пары, если она уже появилась в дереве.

### Настроить стартовое окно и общие стили

**DemoExam.Wpf → App.xaml**

```xml
<Application x:Class="DemoExam.Wpf.App"
 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
 StartupUri="LoginWindow.xaml">
 <Application.Resources>
  <Style TargetType="Window">
   <Setter Property="FontFamily" Value="Segoe UI"/>
   <Setter Property="FontSize" Value="14"/>
   <Setter Property="Background" Value="#F5F6F8"/>
  </Style>
  <Style TargetType="Button">
   <Setter Property="Padding" Value="14,8"/>
   <Setter Property="Margin" Value="0,4,8,4"/>
   <Setter Property="MinHeight" Value="36"/>
  </Style>
  <Style TargetType="TextBox">
   <Setter Property="Padding" Value="7"/>
   <Setter Property="Margin" Value="0,4,0,12"/>
   <Setter Property="VerticalContentAlignment" Value="Center"/>
  </Style>
  <Style TargetType="PasswordBox">
   <Setter Property="Padding" Value="7"/>
   <Setter Property="Margin" Value="0,4,0,12"/>
  </Style>
  <Style TargetType="DataGrid">
   <Setter Property="AutoGenerateColumns" Value="False"/>
   <Setter Property="IsReadOnly" Value="True"/>
   <Setter Property="CanUserAddRows" Value="False"/>
   <Setter Property="SelectionMode" Value="Single"/>
   <Setter Property="SelectionUnit" Value="FullRow"/>
   <Setter Property="RowHeight" Value="34"/>
   <Setter Property="HeadersVisibility" Value="Column"/>
  </Style>
 </Application.Resources>
</Application>
```

**DemoExam.Wpf → App.xaml.cs**

```csharp
using System.Windows;
using System.Windows.Threading;

namespace DemoExam.Wpf;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        UiMessages.Error(e.Exception);
        e.Handled = true;
    }
}
```

1. Откройте App.xaml. Замените содержимое полным блоком. Проверьте StartupUri="LoginWindow.xaml".
2. Откройте App.xaml.cs. Замените содержимое следующим блоком. Не оставляйте второй класс App из шаблона.
3. Добавьте appsettings.json: проект → Add → Existing Item → выберите JSON из Source\DemoExam.Wpf. Или Add → New Item → Text File, затем точное имя appsettings.json.
4. В свойствах appsettings.json настройте Copy to Output Directory = Copy if newer / «Копировать, если новее». В файле проекта эта настройка представлена CopyToOutputDirectory="PreserveNewest".
5. Внесите фактический пароль postgres и имя базы. Сохраните всё и соберите. Найдите JSON рядом с exe в bin\Debug\net9.0-windows.

Ресурсы App.xaml задают общие стили кнопок, текстовых полей и DataGrid. DataGrid IsReadOnly=True означает, что данные редактируются через отдельную форму справа, а не прямо в ячейках. AutoGenerateColumns=False означает, что столбцы описаны явно.

DispatcherUnhandledException показывает ошибку интерфейса и помечает её обработанной. Основные операции всё равно используют try/catch в конкретных обработчиках: так сообщение связано с действием пользователя. Ошибка загрузки ресурса или XAML требует исправления причины.

### Собрать окно входа

**DemoExam.Wpf → LoginWindow.xaml**

```xml
<Window x:Class="DemoExam.Wpf.LoginWindow"
 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
 xmlns:local="clr-namespace:DemoExam.Wpf"
 Title="Вход в информационную систему" Width="780" Height="590" MinWidth="740" MinHeight="560"
 WindowStartupLocation="CenterScreen">
 <Grid Margin="28">
  <Grid.RowDefinitions><RowDefinition Height="Auto"/><RowDefinition Height="*"/></Grid.RowDefinitions>
  <StackPanel Margin="0,0,0,24">
   <TextBlock Text="Учёт производства" FontSize="26" FontWeight="SemiBold"/>
   <TextBlock Text="Вход для администратора и пользователя" Foreground="#526070" Margin="0,6,0,0"/>
  </StackPanel>
  <Grid Grid.Row="1">
   <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="36"/><ColumnDefinition Width="300"/></Grid.ColumnDefinitions>
   <StackPanel>
    <TextBlock Text="Логин"/>
    <TextBox x:Name="LoginTextBox" MaxLength="100" TabIndex="0"/>
    <TextBlock Text="Пароль"/>
    <PasswordBox x:Name="LoginPasswordBox" TabIndex="1"/>
    <Button x:Name="SignInButton" Content="Войти" Click="SignInButton_Click" IsDefault="True" TabIndex="2"/>
    <TextBlock x:Name="LoginStatusText" Text="Заполните оба поля и соберите пазл справа." TextWrapping="Wrap" Margin="0,12,0,0" Foreground="#526070"/>
   </StackPanel>
   <local:PuzzleControl x:Name="LoginPuzzle" Grid.Column="2"/>
  </Grid>
 </Grid>
</Window>
```

**DemoExam.Wpf → LoginWindow.xaml.cs**

```csharp
using System.Windows;
using DemoExam.Core;

namespace DemoExam.Wpf;

public partial class LoginWindow : Window
{
    public LoginWindow() { InitializeComponent(); LoginTextBox.Focus(); }

    private async void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        SignInButton.IsEnabled = false;
        LoginStatusText.Text = "Проверяем данные…";
        try
        {
            var result = await Database.LoginAsync(LoginTextBox.Text, LoginPasswordBox.Password, LoginPuzzle.IsSolved);
            LoginStatusText.Text = result.Message;
            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            UiMessages.Info(result.Message);
            Window desktop = result.User!.Role == "Администратор" ? new AdminWindow(result.User) : new UserWindow(result.User);
            Hide();
            try { desktop.ShowDialog(); }
            finally
            {
                LoginPasswordBox.Clear();
                LoginPuzzle.Shuffle();
                LoginStatusText.Text = "Заполните оба поля и соберите пазл справа.";
                Show();
            }
        }
        catch (Exception ex) { LoginStatusText.Text = "Вход не выполнен. Проверьте подключение."; UiMessages.Error(ex); }
        finally { SignInButton.IsEnabled = true; }
    }
}
```

Grid делит окно на строки и колонки. StackPanel ставит логин, пароль и кнопку последовательно. x:Name создаёт имя для обращения из C#: LoginTextBox.Text и LoginPasswordBox.Password. Click="SignInButton_Click" связывает кнопку с обработчиком. IsDefault=True позволяет вызвать вход клавишей Enter.

1. Запустите WPF с ещё не заполненными полями → «Войти». Прочитайте сообщение и нажмите OK. Если обработчик не срабатывает, проверьте Click и точное имя метода.
2. Введите admin/admin и соберите пазл. Нажмите «Войти». Кнопка временно отключается, статус меняется на «Проверяем данные…».
3. Проверьте окно успешной авторизации. После OK открывается AdminWindow. Для user25/user123 должен открыться UserWindow.
4. Нажмите «Выйти». Окно входа показывается снова, пароль очищается и пазл перемешивается. Логин в поле может оставаться прежним.

Сначала обработчик await вызывает Database.LoginAsync. При неуспехе показывает Warning и возвращается. При успехе выбирает окно по Role, скрывает окно входа и вызывает ShowDialog. В finally возвращает вход и включает кнопку даже после ошибки. ShowDialog ожидает закрытия рабочего стола в рамках этого сценария.

**Три точных сообщения.** Неверные данные: «Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные». Успех: «Вы успешно авторизовались». Блокировка: «Вы заблокированы. Обратитесь к администратору». В решении они заданы строками, а не придумываются заново в каждом окне.

### Добавить изображения как ресурсы WPF

1. Нажмите проект DemoExam.Wpf правой кнопкой → Add → New Folder. Назовите папку Images.
2. Нажмите Images правой кнопкой → Add → Existing Item. Выберите четыре исходных файла 1.png, 2.png, 3.png, 4.png из Source\DemoExam.Wpf\Images.
3. Если используете свойства каждого файла, выделите изображение и нажмите F4. Build Action / «Действие при сборке» должно быть Resource. Copy to Output Directory для встроенного ресурса не требуется.
4. В проекте архива ресурс включается строкой <Resource Include="Images\*.png" />. Используйте согласованную настройку и проверьте отсутствие второй записи того же ресурса.
5. Откройте PuzzleControl.xaml.cs и найдите pack://application:,,,/DemoExam.Wpf;component/Images/{i + 1}.png. Имя сборки DemoExam.Wpf и папка Images должны совпадать с проектом.
6. Соберите и запустите WPF. Все четыре части должны появиться. Если вы видите ошибку Cannot locate resource, проверьте имя файла, Build Action и путь, затем пересоберите проект.

pack URI ищет изображение, встроенное в сборку WPF. Это не абсолютный путь C:\... и не скачивание из интернета. Поэтому при переносе правильно собранного приложения картинки остаются доступными.

- [Microsoft — адреса ресурсов pack URI](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf)

### Подключить интерактивный пазл

**DemoExam.Wpf → PuzzleControl.xaml**

```xml
<UserControl x:Class="DemoExam.Wpf.PuzzleControl"
 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
 <StackPanel>
  <TextBlock Text="Соберите картинку" FontSize="20" FontWeight="SemiBold" Margin="0,0,0,8"/>
  <TextBlock Text="Нажмите на два фрагмента, чтобы поменять их местами." TextWrapping="Wrap" Margin="0,0,0,12"/>
  <UniformGrid x:Name="PuzzleGrid" Rows="2" Columns="2" Width="280" Height="280" HorizontalAlignment="Left"/>
  <StackPanel Orientation="Horizontal" Margin="0,8,0,0">
   <Button Content="Перемешать" Click="ShuffleButton_Click"/>
   <Button Content="Образец" Click="ReferenceButton_Click"/>
  </StackPanel>
  <TextBlock x:Name="PuzzleHintText" Text="После сборки нажмите «Войти»." TextWrapping="Wrap" Margin="0,8,0,0"/>
 </StackPanel>
</UserControl>
```

**DemoExam.Wpf → PuzzleControl.xaml.cs**

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DemoExam.Core;

namespace DemoExam.Wpf;

public partial class PuzzleControl : UserControl
{
    private readonly PuzzleState puzzle = new();
    private readonly BitmapImage[] images = new BitmapImage[4];
    private int selectedPosition = -1;
    public bool IsSolved => puzzle.IsSolved;

    public PuzzleControl()
    {
        InitializeComponent();
        for (int i = 0; i < 4; i++)
            images[i] = new BitmapImage(new Uri($"pack://application:,,,/DemoExam.Wpf;component/Images/{i + 1}.png"));
        Shuffle();
    }

    public void Shuffle()
    {
        puzzle.Shuffle();
        selectedPosition = -1;
        Redraw();
    }

    private void Redraw()
    {
        PuzzleGrid.Children.Clear();
        for (int position = 0; position < 4; position++)
        {
            var button = new Button { Tag = position, Padding = new Thickness(0), Margin = new Thickness(0),
                Background = Brushes.White, BorderThickness = new Thickness(2),
                BorderBrush = position == selectedPosition ? Brushes.DodgerBlue : Brushes.LightGray,
                ToolTip = "Выберите два фрагмента для обмена", Content = new Image { Source = images[puzzle.Pieces[position]], Stretch = Stretch.Fill } };
            button.Click += PieceButton_Click;
            PuzzleGrid.Children.Add(button);
        }
        PuzzleHintText.Text = "После сборки нажмите «Войти».";
    }

    private void PieceButton_Click(object sender, RoutedEventArgs e)
    {
        int position = (int)((Button)sender).Tag;
        if (selectedPosition < 0) selectedPosition = position;
        else { puzzle.Swap(selectedPosition, position); selectedPosition = -1; }
        Redraw();
    }

    private void ShuffleButton_Click(object sender, RoutedEventArgs e) => Shuffle();

    private void ReferenceButton_Click(object sender, RoutedEventArgs e)
    {
        var grid = new UniformGrid { Rows = 2, Columns = 2, Width = 340, Height = 340, Background = Brushes.White };
        foreach (var image in images) grid.Children.Add(new Image { Source = image, Stretch = Stretch.Fill });
        var window = new Window { Title = "Исходное изображение", Content = grid, SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize, Owner = Window.GetWindow(this), WindowStartupLocation = WindowStartupLocation.CenterOwner };
        window.ShowDialog();
    }
}
```

UniformGrid задаёт сетку 2×2. Redraw удаляет старые кнопки и создаёт четыре новых. В Tag хранится позиция, в Content — Image, а синяя рамка показывает выбранный фрагмент. BitmapImage[] хранит изображения в правильном порядке, PuzzleState.Pieces определяет, какое из них показать в каждой позиции.

Первый щелчок запоминает selectedPosition. Второй вызывает puzzle.Swap и очищает выбор. Каждый раз Redraw обновляет картинку. Поэтому это работающая головоломка, а не CheckBox «я не робот». ReferenceButton_Click показывает правильный порядок в отдельном окне.

1. Нажмите «Перемешать». Картинка должна быть несобранной.
2. Нажмите первый фрагмент: появится синяя рамка. Нажмите другой: они обменяются местами.
3. Нажмите «Образец». В отдельном окне должно быть правильное изображение из тех же четырёх частей. Закройте образец.
4. Соберите правильный порядок и проверьте вход тестовым пользователем.
5. Проверьте, что «Образец» только показывает решение и не переставляет фрагменты.

### Собрать окно администратора

**DemoExam.Wpf → AdminWindow.xaml**

```xml
<Window x:Class="DemoExam.Wpf.AdminWindow"
 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
 Title="Рабочий стол администратора" Width="1050" Height="670" MinWidth="960" MinHeight="620"
 WindowStartupLocation="CenterScreen" Loaded="AdminWindow_Loaded">
 <Grid Margin="24">
  <Grid.RowDefinitions><RowDefinition Height="Auto"/><RowDefinition Height="*"/></Grid.RowDefinitions>
  <DockPanel Margin="0,0,0,18">
   <Button DockPanel.Dock="Right" Content="Выйти" Click="LogoutButton_Click"/>
   <StackPanel><TextBlock Text="Администратор" FontSize="26" FontWeight="SemiBold"/><TextBlock x:Name="AdminLoginText" Foreground="#526070"/></StackPanel>
  </DockPanel>
  <TabControl Grid.Row="1">
   <TabItem Header="Пользователи">
    <Grid Margin="14">
     <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="22"/><ColumnDefinition Width="280"/></Grid.ColumnDefinitions>
     <Grid>
      <Grid.RowDefinitions><RowDefinition Height="Auto"/><RowDefinition Height="*"/><RowDefinition Height="Auto"/></Grid.RowDefinitions>
      <DockPanel>
       <Button DockPanel.Dock="Right" Content="Найти" Click="SearchButton_Click"/>
       <TextBox x:Name="SearchTextBox" ToolTip="Введите часть логина. Пустой поиск показывает всех."/>
      </DockPanel>
      <DataGrid x:Name="UsersGrid" Grid.Row="1" SelectionChanged="UsersGrid_SelectionChanged">
       <DataGrid.Columns>
        <DataGridTextColumn Header="Код" Binding="{Binding Id}" Width="50"/>
        <DataGridTextColumn Header="Логин" Binding="{Binding Login}" Width="*" MinWidth="140"/>
        <DataGridTextColumn Header="Роль" Binding="{Binding Role}" Width="150"/>
        <DataGridCheckBoxColumn Header="Блокировка" Binding="{Binding IsBlocked}" Width="90"/>
        <DataGridTextColumn Header="Ошибки" Binding="{Binding FailedAttempts}" Width="70"/>
       </DataGrid.Columns>
      </DataGrid>
      <TextBlock x:Name="UsersStatusText" Grid.Row="2" Margin="0,10,0,0" TextWrapping="Wrap"/>
     </Grid>
     <StackPanel Grid.Column="2">
      <TextBlock x:Name="EditorTitleText" Text="Новый пользователь" FontSize="20" FontWeight="SemiBold" Margin="0,0,0,14"/>
      <TextBlock Text="Логин"/><TextBox x:Name="UserLoginTextBox" MaxLength="100"/>
      <TextBlock Text="Пароль"/><PasswordBox x:Name="UserPasswordBox"/>
      <TextBlock Text="При изменении оставьте пароль пустым, чтобы сохранить прежний." Foreground="#526070" TextWrapping="Wrap" Margin="0,0,0,12"/>
      <TextBlock Text="Роль"/>
      <ComboBox x:Name="RoleComboBox" SelectedIndex="0" Padding="7" Margin="0,4,0,14">
       <ComboBoxItem Content="Пользователь"/><ComboBoxItem Content="Администратор"/>
      </ComboBox>
      <CheckBox x:Name="BlockedCheckBox" Content="Заблокирован" Margin="0,0,0,14"/>
      <TextBlock Text="Снимите отметку и сохраните, чтобы разблокировать учётную запись." Foreground="#526070" TextWrapping="Wrap" Margin="0,0,0,12"/>
      <Button x:Name="SaveUserButton" Content="Сохранить" Click="SaveUserButton_Click"/>
      <Button Content="Добавить нового" Click="NewUserButton_Click"/>
     </StackPanel>
    </Grid>
   </TabItem>
   <TabItem Header="Стоимость заказов">
    <Grid Margin="14">
     <Grid.RowDefinitions><RowDefinition Height="Auto"/><RowDefinition Height="*"/><RowDefinition Height="Auto"/></Grid.RowDefinitions>
     <Button Content="Обновить расчёт" Click="RefreshOrdersButton_Click" HorizontalAlignment="Left"/>
     <DataGrid x:Name="OrdersGrid" Grid.Row="1" Margin="0,12,0,0">
      <DataGrid.Columns>
       <DataGridTextColumn Header="Заказ" Binding="{Binding Number}" Width="60"/>
       <DataGridTextColumn Header="Заказчик" Binding="{Binding Customer}" Width="*"/>
       <DataGridTextColumn Header="Материалы, ₽" Binding="{Binding MaterialsCost, StringFormat=N2}" Width="125"/>
       <DataGridTextColumn Header="Операции, ₽" Binding="{Binding OperationsCost, StringFormat=N2}" Width="125"/>
       <DataGridTextColumn Header="Себестоимость, ₽" Binding="{Binding TotalCost, StringFormat=N2}" Width="155"/>
       <DataGridTextColumn Header="Сумма продажи, ₽" Binding="{Binding SaleTotal, StringFormat=N2}" Width="155"/>
      </DataGrid.Columns>
     </DataGrid>
     <TextBlock Grid.Row="2" Text="Себестоимость рассчитывается по нормам спецификации и ценам. Сумма продажи учитывает скидку." Margin="0,12,0,0" TextWrapping="Wrap"/>
    </Grid>
   </TabItem>
  </TabControl>
 </Grid>
</Window>
```

**DemoExam.Wpf → AdminWindow.xaml.cs**

```csharp
using System.Windows;
using System.Windows.Controls;
using DemoExam.Core;

namespace DemoExam.Wpf;

public partial class AdminWindow : Window
{
    private readonly User currentUser;
    private int editedUserId;

    public AdminWindow(User user)
    {
        InitializeComponent();
        currentUser = user;
        AdminLoginText.Text = "Вы вошли как " + user.Login;
    }

    private async void AdminWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try { await RefreshUsersAsync(); OrdersGrid.ItemsSource = await Database.GetOrderCostsAsync(); }
        catch (Exception ex) { UiMessages.Error(ex); }
    }

    private async Task RefreshUsersAsync()
    {
        var users = await Database.GetUsersAsync(currentUser.Id, SearchTextBox.Text);
        UsersGrid.ItemsSource = users;
        UsersStatusText.Text = users.Count == 0 ? "Пользователи не найдены. Измените поиск или очистите поле." : $"Найдено пользователей: {users.Count}";
    }

    private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not User user) return;
        editedUserId = user.Id;
        EditorTitleText.Text = "Изменение пользователя";
        UserLoginTextBox.Text = user.Login;
        UserPasswordBox.Clear();
        RoleComboBox.SelectedIndex = user.Role == "Администратор" ? 1 : 0;
        BlockedCheckBox.IsChecked = user.IsBlocked;
    }

    private void NewUserButton_Click(object sender, RoutedEventArgs e) => ClearEditor();

    private void ClearEditor()
    {
        editedUserId = 0;
        UsersGrid.SelectedItem = null;
        EditorTitleText.Text = "Новый пользователь";
        UserLoginTextBox.Clear();
        UserPasswordBox.Clear();
        RoleComboBox.SelectedIndex = 0;
        BlockedCheckBox.IsChecked = false;
        UserLoginTextBox.Focus();
    }

    private async void SaveUserButton_Click(object sender, RoutedEventArgs e)
    {
        SaveUserButton.IsEnabled = false;
        try
        {
            var user = new User { Id = editedUserId, Login = UserLoginTextBox.Text,
                Role = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString()!, IsBlocked = BlockedCheckBox.IsChecked == true };
            await Database.SaveUserAsync(currentUser.Id, user, UserPasswordBox.Password);
            UiMessages.Info("Данные пользователя сохранены.");
            await RefreshUsersAsync();
            ClearEditor();
        }
        catch (Exception ex) { UiMessages.Error(ex); }
        finally { SaveUserButton.IsEnabled = true; }
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        try { await RefreshUsersAsync(); }
        catch (Exception ex) { UiMessages.Error(ex); }
    }

    private async void RefreshOrdersButton_Click(object sender, RoutedEventArgs e)
    {
        try { OrdersGrid.ItemsSource = await Database.GetOrderCostsAsync(); }
        catch (Exception ex) { UiMessages.Error(ex); }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e) => Close();
}
```

При Loaded окно получает список пользователей и стоимость заказов. UsersGrid.ItemsSource = users задаёт коллекцию для таблицы. Binding Login, Role, IsBlocked, FailedAttempts берёт одноимённые свойства User. Выбор строки вызывает UsersGrid_SelectionChanged и заполняет форму справа.

editedUserId = 0 — форма добавления, положительный ID — изменение существующей записи. В ClearEditor сбрасываются выбранная строка, пароль, роль и блокировка. SaveUserButton_Click собирает объект User, вызывает SaveUserAsync, обновляет список и очищает форму.

Поиск вызывает GetUsersAsync с текстом SearchTextBox. SQL position(lower(@search) in lower(login)) ищет подстроку без учёта регистра. Пустая строка поиска показывает весь список. Сохранение недоступно повторному нажатию, пока await операция не завершилась.

- [Microsoft — привязка данных WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/)

### Добавить изменить найти и разблокировать пользователя

1. Войдите admin/admin и откройте вкладку «Пользователи». Справа нажмите «Добавить нового»: форма должна быть пустой и иметь заголовок «Новый пользователь».
2. В поле «Логин» введите test. В поле «Пароль» — student. В списке «Роль» выберите «Пользователь». Галочку «Заблокирован» оставьте снятой.
3. Нажмите «Сохранить» → OK в сообщении. В таблице должна появиться запись test. Если включён поиск, очистите его и нажмите «Найти».
4. Нажмите строку test. Справа появится «Изменение пользователя». Измените логин на student, оставьте поле пароля пустым → «Сохранить». Это сохранит старый пароль student.
5. Снова выберите строку student. Введите newpass в «Пароль» → «Сохранить». Следующий вход будет student/newpass.
6. Для проверки уникальности нажмите «Добавить нового», введите STUDENT и любой пароль → «Сохранить». Ожидается сообщение, что логин уже существует. Регистр не создаёт отдельную учётную запись.
7. В поиске над таблицей введите часть логина, например stud → «Найти». Должна остаться подходящая строка. Очистите поиск → «Найти», чтобы показать всех.
8. Для ручной блокировки выберите student → поставьте «Заблокирован» → «Сохранить». При следующем входе этому пользователю будет отказано.
9. Для разблокировки выберите эту же строку → снимите «Заблокирован» → оставьте пароль пустым → «Сохранить». failed_attempts станет 0, прежний пароль останется.
10. Роль меняйте через тот же список и «Сохранить». Новая роль определяется при следующем входе. После изменения не пытайтесь оценивать права только по уже открытому рабочему столу.

SaveUserAsync проверяет логин 1–100 символов, допустимую роль, пароль нового пользователя и уникальность. RequireAdminAsync сверяет actorId с актуальной записью users: роль должна быть «Администратор», блокировка снята. Интерфейс обычного пользователя не даёт открыть форму управления.

Для UPDATE password_hash используется CASE WHEN @hash = '' THEN password_hash ELSE @hash END. Поэтому пустой пароль при изменении означает сохранение прежнего, а при добавлении запрещён. При любом сохранении разблокированного пользователя счётчик устанавливается в 0; при сохранении заблокированного — в 3.

**Себя заблокировать нельзя.** Код запрещает текущему администратору поставить себе блокировку или снять собственную роль. Для сценариев используйте отдельный аккаунт. Наличие проверки роли в этом локальном учебном приложении не превращает actorId в самостоятельный сетевой механизм аутентификации.

### Собрать окно обычного пользователя

**DemoExam.Wpf → UserWindow.xaml**

```xml
<Window x:Class="DemoExam.Wpf.UserWindow"
 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
 Title="Рабочий стол пользователя" Width="850" Height="550" MinWidth="750" MinHeight="450"
 WindowStartupLocation="CenterScreen" Loaded="UserWindow_Loaded">
 <Grid Margin="24">
  <Grid.RowDefinitions><RowDefinition Height="Auto"/><RowDefinition Height="Auto"/><RowDefinition Height="*"/><RowDefinition Height="Auto"/></Grid.RowDefinitions>
  <DockPanel Margin="0,0,0,18">
   <Button DockPanel.Dock="Right" Content="Выйти" Click="LogoutButton_Click"/>
   <StackPanel><TextBlock Text="Мои заметки" FontSize="26" FontWeight="SemiBold"/><TextBlock x:Name="UserLoginText"/></StackPanel>
  </DockPanel>
  <Button Grid.Row="1" Content="Обновить" Click="RefreshButton_Click" HorizontalAlignment="Left"/>
  <DataGrid x:Name="NotesGrid" Grid.Row="2" Margin="0,12,0,0">
   <DataGrid.Columns>
    <DataGridTextColumn Header="Код" Binding="{Binding Id}" Width="55"/>
    <DataGridTextColumn Header="Заголовок" Binding="{Binding Title}" Width="200"/>
    <DataGridTextColumn Header="Содержание" Binding="{Binding Content}" Width="*" MinWidth="280"/>
    <DataGridTextColumn Header="Дата" Binding="{Binding CreatedAt, StringFormat=dd.MM.yyyy}" Width="110"/>
   </DataGrid.Columns>
  </DataGrid>
  <TextBlock x:Name="NotesStatusText" Grid.Row="3" Margin="0,12,0,0"/>
 </Grid>
</Window>
```

**DemoExam.Wpf → UserWindow.xaml.cs**

```csharp
using System.Windows;
using DemoExam.Core;

namespace DemoExam.Wpf;

public partial class UserWindow : Window
{
    private readonly User currentUser;
    public UserWindow(User user)
    {
        InitializeComponent(); currentUser = user;
        UserLoginText.Text = "Вы вошли как " + user.Login;
    }

    private async Task RefreshAsync()
    {
        var notes = await Database.GetNotesAsync(currentUser.Id);
        NotesGrid.ItemsSource = notes;
        NotesStatusText.Text = notes.Count == 0 ? "У вас пока нет заметок." : $"Всего заметок: {notes.Count}";
    }

    private async void UserWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try { await RefreshAsync(); } catch (Exception ex) { UiMessages.Error(ex); }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        try { await RefreshAsync(); } catch (Exception ex) { UiMessages.Error(ex); }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e) => Close();
}
```

Конструктор получает User после успешного входа. RefreshAsync вызывает Database.GetNotesAsync(currentUser.Id). SQL WHERE id_user = @id выбирает записи этого пользователя. WPF показывает заголовок, содержание и дату через StringFormat=dd.MM.yyyy.

1. Выйдите из рабочего стола администратора. Введите user25/user123 и соберите пазл → «Войти».
2. В окне «Мои заметки» ожидаются две записи: «Конференция ИТ» и «Встреча с заказчиком».
3. Нажмите «Обновить». Количество и содержимое останутся теми же, если база не менялась.
4. Нажмите «Выйти». Войдите user26/user123. Ожидается одна запись «Проверка спецификации».
5. У нового тестового пользователя без заметок ожидается «У вас пока нет заметок.». Кнопок добавления заметки в этом WPF примере нет.

Вход с ролью пользователя даёт рабочий стол заметок, а не вкладку «Пользователи». CRUD заметок и удаление пользователей здесь не реализованы: проверяйте функции, которые есть в этом задании и примере.

### Добавить сообщения об ошибках

**DemoExam.Wpf → UiMessages.cs**

```csharp
using System.Windows;
using Npgsql;

namespace DemoExam.Wpf;

public static class UiMessages
{
    public static void Error(Exception exception)
    {
        string message = exception is InvalidOperationException
            ? exception.Message
            : "Не удалось выполнить действие. Проверьте, что PostgreSQL запущен, база demo_exam_2027 создана, а параметры в appsettings.json указаны правильно. Затем повторите действие.";
        MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static void Info(string message) => MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
}
```

Для InvalidOperationException показывается конкретный текст бизнес-проверки: например, «Пользователь с указанным логином уже существует». Для прочих ошибок выводится понятная подсказка про PostgreSQL, базу и appsettings. Ошибки соединения не следует воспринимать как неверный пароль.

MessageBoxButton.OK задаёт кнопку закрытия, MessageBoxImage.Error / Information / Warning — пиктограмму. Сообщение содержит заголовок «Ошибка», «Информация» или «Ошибка входа». Это помогает отличить результат проверки от технического сбоя.

### Проверить три ошибки и сброс счётчика

1. Войдите администратором и создайте отдельного пользователя examtest с паролем student и ролью «Пользователь». Выйдите.
2. Для examtest введите неверный пароль, например wrong, и нажмите «Войти». Повторите ещё раз. Каждую попытку завершайте OK в сообщении.
3. До третьей ошибки введите правильный student, соберите пазл и войдите. Ожидается успех. Выйдите; в pgAdmin SELECT failed_attempts FROM users WHERE login = 'examtest'; должен показать 0.
4. Теперь трижды введите wrong для examtest. На третьей попытке ожидается «Вы заблокированы. Обратитесь к администратору».
5. Закройте WPF и запустите заново. Введите правильный student и соберите пазл. Учётная запись должна оставаться заблокированной.
6. Войдите admin/admin. Выберите examtest → снимите «Заблокирован» → оставьте пароль пустым → «Сохранить». В базе счётчик снова 0.
7. Выйдите. Для examtest введите правильный student, но не собирайте пазл. Нажмите «Войти» три раза. На первых двух попытках ожидается сообщение пазла, на третьей — блокировка.
8. Повторите разблокировку через администратора. Убедитесь, что student всё ещё работает.
9. Проверьте смешанный сценарий после разблокировки: неверный пароль, неверный пазл с правильным паролем, снова неверный пароль. Это три общие ошибки подряд, поэтому результат — блокировка.

Пустые поля не увеличивают счётчик. Неизвестный логин тоже не увеличивает счётчик другого пользователя. Считаются завершённые неудачные проверки существующего аккаунта. После успешного входа последовательность начинается заново.

### Посмотреть работу обработчика в отладчике

1. Остановите приложение. Откройте LoginWindow.xaml.cs и щёлкните слева от строки var result = await Database.LoginAsync(...). Появится красная точка остановки.
2. Запустите WPF через F5. Заполните поля тестового пользователя и нажмите «Войти». Visual Studio остановится перед вызовом.
3. Откройте «Отладка → Окна → Локальные» / Debug → Windows → Locals. Смотрите значения входных полей и затем result, но не публикуйте снимки реальных паролей.
4. Через «Отладка → Шаг с обходом» / F10 выполните вызов. После await посмотрите result.Success и result.Message.
5. Чтобы посмотреть SQL, поставьте точку в Database.LoginAsync перед SELECT и запустите сценарий заново. Не держите выполнение долго после FOR UPDATE: открытая транзакция может блокировать другое обращение к этому пользователю.
6. Нажмите F5 для продолжения. Завершите сценарий. Уберите точки остановки повторным щелчком по красным точкам и остановите отладку.

- [Microsoft — отладка WPF и точки остановки](https://learn.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-wpf?view=vs-2022)

### Запустить проверки и собрать папку приложения

1. В готовом Source/DemoExam.sln назначьте DemoExam.Checks запускаемым проектом. Исправьте настройки подключения WPF: Checks копирует именно этот JSON при сборке.
2. Запустите через «Отладка → Запуск без отладки» / Ctrl+F5. Это консольный проект: вводить команды не нужно, но окно покажет PASS и итог проверки.
3. В приложенном отчёте Application-checks.txt сохранён результат 37 проверок. Это отчёт из архива. Собственный новый прогон подтвердите на своей Windows и своей базе.
4. Вернитесь к DemoExam.Wpf и откройте Publish / «Опубликовать» через контекстное меню проекта. Выберите Folder / «Папка», например C:\ExamOutput\App. Если мастер спрашивает второй раз Folder, выберите локальную папку.
5. В настройках публикации задайте Release, Target framework net9.0-windows. Для среды с .NET 9 выбирайте Framework-dependent. Для домашнего переноса без runtime можно выбрать Self-contained и соответствующую архитектуру, например win-x64; такой режим может потребовать дополнительные runtime packs при публикации.
6. Нажмите Publish. После успеха откройте папку результата. Там должны быть exe, DLL, runtimeconfig и appsettings.json. Переносите всю папку.
7. Проверьте appsettings.json опубликованной программы и запустите её из этой папки. Изменения опубликованной копии не заменяют настройки исходного проекта.
8. В папку сдачи отдельно положите исходники Core и WPF с проектами и изображениями. Публикация exe не заменяет исходники.

**Сборка и фактический запуск.** В этой методичке код сверяется с архивом. WPF требует Windows; успешный запуск и новые интеграционные проверки подтверждаются на вашем компьютере. Файлы Tests/Results показывают сохранённые результаты автора, а не автоматическую гарантию работы любой среды.

- [Microsoft — варианты публикации .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/)
- [Сохранённый отчёт Application checks](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Tests/Results/Application-checks.txt)
- [Сценарии проверки интерфейса из архива](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Tests/Manual_UI_Checks.md)

### Проверка готовности

- [ ] Обязательные поля и сообщения входа работают
- [ ] Вход открывает рабочий стол соответствующей роли
- [ ] Пазл меняет фрагменты и проверяет порядок
- [ ] Три ошибки блокируют пользователя в БД
- [ ] Администратор добавляет изменяет и разблокирует пользователей
- [ ] Создал ссылку WPF → Core и Npgsql 9.0.3
- [ ] Добавил все пары XAML и code-behind без дубликатов
- [ ] Картинки встроены как Resource и окно стартует через App.xaml
- [ ] На отдельном пользователе проверил пароль пазл блокировку и сброс
- [ ] Публикация запускается из новой папки вместе с DLL и JSON

## 05 Собрать API и проверить его в Postman

Отдельный проект ASP.NET Core, GET /notes, фильтр user_id, преобразование данных и шесть запросов с тестами. Настоящий 500 проверяем на копии API без PowerShell.

Результат: Работающий GET /notes, коллекция Postman с проверками 200/400/500 и JSON, сохранённый результат собственного прогона.

### Разобрать точный контракт API

В этом примере единственный реализованный маршрут — GET /notes. Метод получает данные из таблиц notes и users и возвращает JSON-массив. Приложение WPF само этот адрес не обслуживает: нужен отдельно запущенный DemoExam.Api.

| Элемент | Значение | Пример |
| --- | --- | --- |
| Базовый URL | http://127.0.0.1:5050 | Основной локальный сервер |
| HTTP метод | GET | Чтение списка |
| Маршрут | /notes | http://127.0.0.1:5050/notes |
| Необязательный параметр | user_id — положительный Int32 | /notes?user_id=2 |
| Поле ответа id | Числовой код заметки | 1 |
| Поле ответа title_user | Заголовок + " - " + логин | Конференция ИТ - user25 |
| Поле ответа content | Содержание без изменений | Содержимое заметки |
| Поле ответа formatted_date | Строка ДД.ММ.ГГГГ | 15.03.2027 |

Параметр URL называется user_id, а столбец notes — id_user. Это разные имена на разных уровнях. Не переименовывайте SQL-поле в user_id без изменения схемы.

**Что доступно по HTTP.** API архива не требует логина, пароля, cookie или Bearer token. Без параметра выдаются все заметки; с user_id — выбранного пользователя. WPF фильтрует заметки своего пользователя отдельно. Не заявляйте в документации сетевую авторизацию, которой в Program.cs нет.

### Создать проект ASP.NET Core кнопками

1. В решении DemoExam нажмите на решение правой кнопкой → Add → New Project.
2. Введите ASP.NET Core Empty в поиске. Выберите C# «Пустой проект ASP.NET Core» / ASP.NET Core Empty и нажмите «Далее».
3. Назовите проект DemoExam.Api и разместите рядом с Core и WPF. На странице дополнительных настроек выберите .NET 9.0.
4. Для повторения локального HTTP примера снимите Configure for HTTPS, если такой переключатель есть. Docker/Container support не требуется. Создайте проект.
5. Если доступен только шаблон ASP.NET Core Web API, создайте его без контроллеров и затем замените Program.cs и .csproj точными файлами ниже. Удалите неиспользуемые WeatherForecast, контроллеры и пример OpenAPI из шаблона.
6. Добавьте ссылку DemoExam.Api → DemoExam.Core через Add → Project Reference → Projects → Solution → галочка Core → OK.
7. Откройте Program.cs, выделите весь шаблонный текст и вставьте код следующего раздела. Не оставляйте рядом вторую строку app.Run() из шаблона.
8. Откройте appsettings.json и замените содержимое блоком ниже. Настройте пароль postgres и фактическое имя базы.
9. Если шаблон создал Properties/launchSettings.json, откройте его. Проверьте applicationUrl у профиля проекта: выставьте http://127.0.0.1:5050 для этого профиля или используйте фактический порт запуска во всех тестах. Профиль может переопределять Urls из appsettings.
10. Нажмите Build → Build Solution. Затем DemoExam.Api → Set as Startup Project. У кнопки запуска выберите профиль DemoExam.Api вместо IIS Express.

**DemoExam.Api → файл проекта**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup><ProjectReference Include="..\DemoExam.Core\DemoExam.Core.csproj" /></ItemGroup>
</Project>
```

**DemoExam.Api → appsettings.json**

```json
{
  "Urls": "http://127.0.0.1:5050",
  "ConnectionStrings": {
    "Database": "Host=127.0.0.1;Port=5432;Database=demo_exam_2027;Username=postgres;Password=admin;Timeout=5;Command Timeout=10"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } }
}
```

SDK Microsoft.NET.Sdk.Web включает ASP.NET Core. Дополнительный пакет Entity Framework не используется. Npgsql приходит через Core. Значение Urls описывает адрес HTTP API, а Port=5432 внутри строки Database — порт PostgreSQL: они не обязаны и не должны совпадать.

- [Microsoft — Minimal API](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-9.0)

### Добавить полный Program.cs

**DemoExam.Api → Program.cs**

```csharp
using System.Globalization;
using DemoExam.Core;
using Npgsql;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory });
// Для локального запуска достаточно консоли. Журнал Windows может требовать права администратора.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var app = builder.Build();

// В том числе непредвиденные ошибки превращаются в JSON, а не в HTML-страницу.
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Ошибка обработки запроса");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Внутренняя ошибка сервера" });
    }
});

app.MapGet("/notes", async (HttpRequest request) =>
{
    // Необязательный фильтр позволяет проверить и пустой результат, и неверный тип.
    int? userId = null;
    foreach (var key in request.Query.Keys)
        if (key != "user_id") return Results.Json(new { error = "Неизвестный параметр: " + key }, statusCode: 400);
    if (request.Query.ContainsKey("user_id"))
    {
        var values = request.Query["user_id"];
        if (values.Count != 1 || !int.TryParse(values[0], out int parsed) || parsed <= 0)
            return Results.Json(new { error = "Параметр user_id должен быть положительным целым числом" }, statusCode: 400);
        userId = parsed;
    }

    try
    {
        await using var connection = await Database.OpenAsync();
        await using var command = new NpgsqlCommand("""
            SELECT n.id, n.title, u.login, n.content, n.created_at
            FROM notes n JOIN users u ON u.id = n.id_user
            WHERE (@user_id IS NULL OR n.id_user = @user_id)
            ORDER BY n.id
            """, connection);
        command.Parameters.AddWithValue("user_id", NpgsqlTypes.NpgsqlDbType.Integer, (object?)userId ?? DBNull.Value);
        var result = new List<object>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(new { id = reader.GetInt32(0), title_user = reader.GetString(1) + " - " + reader.GetString(2),
                content = reader.GetString(3), formatted_date = reader.GetDateTime(4).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) });
        return Results.Json(result);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Не удалось получить заметки из PostgreSQL");
        return Results.Json(new { error = "Ошибка получения данных из базы данных" }, statusCode: 500);
    }
});

app.Run();

// Класс виден проекту проверок.
public partial class Program { }
```

WebApplication.CreateBuilder создаёт приложение ASP.NET Core. ContentRootPath=AppContext.BaseDirectory помогает использовать папку опубликованной программы как основу настроек. Logging оставляет консольный журнал, чтобы локальный запуск не зависел от прав на журнал Windows.

app.Use оборачивает последующие обработчики: непредвиденное исключение журналируется, клиент получает 500 и JSON error. Сам обработчик /notes имеет отдельный try/catch для ошибок БД и возвращает более конкретное сообщение. app.Run запускает сервер и ждёт запросов.

app.MapGet связывает GET и путь /notes с асинхронным обработчиком. Этот код не содержит POST, PUT или DELETE. На неизвестный маршрут / будет обычный 404, а не список заметок.

### Проверить входные параметры до SQL

Цикл request.Query.Keys разрешает только ключ user_id. Любой другой ключ даёт HTTP 400 с сообщением «Неизвестный параметр: ...». Имя параметра проверяется в точном написании: USER_ID не равно user_id.

Если user_id есть, код требует ровно одно значение, успешный int.TryParse и число > 0. Поэтому abc, 0, -1, пустое значение, переполнение Int32 и повтор параметра отклоняются. Когда user_id отсутствует, переменная остаётся null и фильтр не ограничивает список.

| Запрос | Ожидаемый статус | Причина |
| --- | --- | --- |
| /notes | 200 | Параметр необязателен |
| /notes?user_id=2 | 200 | Корректный фильтр |
| /notes?user_id=2147483647 | 200 и [] | Корректное число, нет заметок в исходном наборе |
| /notes?user_id=abc | 400 | Не целое число |
| /notes?user_id=0 | 400 | Нужно число больше нуля |
| /notes?user_id=-1 | 400 | Нужно положительное число |
| /notes?user_id= | 400 | Пустое значение |
| /notes?user_id=2&user_id=3 | 400 | Повтор параметра |
| /notes?unknown=1 | 400 | Неизвестное имя параметра |

Несуществующий положительный user_id не является ошибкой запроса: данные могут отсутствовать. Поэтому ответ — 200 и пустой массив, а не 400 или 404. 2147483647 является допустимым Int32 и в исходной базе отсутствует.

### Проследить SELECT и преобразование ответа

**SQL внутри GET /notes**

```sql
SELECT n.id, n.title, u.login, n.content, n.created_at
FROM notes n JOIN users u ON u.id = n.id_user
WHERE (@user_id IS NULL OR n.id_user = @user_id)
ORDER BY n.id
```

JOIN добавляет login автора к заметке. Условие @user_id IS NULL разрешает все строки, если параметр не передан. Иначе проверяется совпадение id_user. ORDER BY n.id делает порядок результатов определённым.

Параметр добавляется с типом NpgsqlDbType.Integer, а при отсутствии значения используется DBNull.Value. Явный тип важен: драйвер должен понимать, что null относится к целому числу. Не вставляйте user_id в SQL через конкатенацию.

reader.GetInt32(0) читает id, GetString(1) — title, GetString(2) — login, GetString(3) — content, GetDateTime(4) — created_at. Порядок индексов соответствует SELECT. Если поменять порядок колонок SQL, надо поменять и чтение.

**Объект одной заметки в JSON**

```csharp
new {
    id = reader.GetInt32(0),
    title_user = reader.GetString(1) + " - " + reader.GetString(2),
    content = reader.GetString(3),
    formatted_date = reader.GetDateTime(4)
        .ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
}
```

Две буквы MM означают месяц, а mm — минуты; поэтому формат пишется именно dd.MM.yyyy. InvariantCulture задаёт предсказуемое форматирование. Результат с пятью объектами возвращается через Results.Json, а пустой List<object> превращается в [].

**Фактический ожидаемый ответ /notes?user_id=3**

```json
[
  {
    "id": 4,
    "title_user": "Проверка спецификации - user26",
    "content": "Проверить нормы расхода материалов.",
    "formatted_date": "18.03.2027"
  }
]
```

### Запустить сервер и отправить первый запрос

1. Запустите API через Visual Studio или готовый Api/DemoExam.Api.exe. Если уже работает сервер на 5050, сначала остановите его; два процесса не могут слушать один адрес и порт.
2. В окне или выводе сервера найдите Now listening on: http://127.0.0.1:5050. Если порт другой, проверьте launchSettings и выбранный профиль.
3. Откройте http://127.0.0.1:5050/notes в браузере. Должен быть массив из пяти объектов. Это первая проверка соединения API → PostgreSQL.
4. Откройте /notes?user_id=2. В исходной базе ожидаются заметки с id 1 и 3.
5. Откройте /notes?user_id=3. Ожидается одна заметка id 4 с датой 18.03.2027.
6. Оставьте сервер запущенным и переходите к Postman. Закрытие окна API останавливает сервер; открытое окно WPF не заменяет его.

**127.0.0.1 — текущий компьютер.** Этот адрес подходит, когда API и Postman работают на одной машине. Postman Desktop Agent/локальный клиент должен отправлять запрос локально. Облачный агент другого сервера не получает доступ к вашему localhost. Для этого примера используйте настольный клиент, доступный в среде.

### Импортировать точную коллекцию архива

1. Откройте настольный Postman. На экзамене используйте предоставленную версию и учётную запись, если она нужна. Для локального запроса личная облачная синхронизация не является частью этого решения.
2. Нажмите Import. Выберите файл DemoExam\Tests\Notes.postman_collection.json. В версиях с перетаскиванием можно перетащить JSON в окно Import.
3. Подтвердите импорт. В Collections появится «ДЭ 09.02.07-5-2027 — API заметок». Раскройте её: там шесть запросов.
4. Нажмите название коллекции и откройте Variables. Проверьте baseUrl = http://127.0.0.1:5050 и errorBaseUrl = http://127.0.0.1:5051. В версиях с Initial/Current value заполните активное значение, используемое при отправке.
5. Откройте «01 Все заметки — 200 и JSON». Метод должен быть GET, URL = {{baseUrl}}/notes. Нажмите Send.
6. В ответе проверьте статус 200 OK, Body в режиме JSON и заголовок Content-Type с application/json.
7. Откройте Test Results. Должны пройти проверки статуса, структуры, валидного JSON и заголовка. Сами тесты находятся в Scripts → Post-response, а в старых версиях на вкладке Tests.
8. Отправьте остальные запросы 02–05. Для 06 сначала подготовьте отдельный экземпляр с ошибкой подключения по следующему разделу.

| Запрос коллекции | Что проверяет |
| --- | --- |
| 01 Все заметки | 200, JSON массив, обязательные поля и Content-Type |
| 02 Фильтр пользователя | Две заметки user25 и правильные значения |
| 03 Нет данных | 200 и пустой массив |
| 04 Неверный параметр | 400 и JSON error |
| 05 Неизвестный параметр | 400 и JSON error |
| 06 Ошибка БД | 500 на порту 5051 и JSON error |

- [Скачать коллекцию именно DemoExam](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Tests/Notes.postman_collection.json)
- [Postman — импорт коллекций](https://learning.postman.com/docs/getting-started/importing-and-exporting/importing-data/)

### Получить настоящий 500 без PowerShell

Ошибка 500 должна прийти от работающего HTTP сервера, который не смог прочитать БД. Если просто закрыть API, Postman покажет Could not send request: это ошибка соединения, а не HTTP 500. Поэтому используем второй экземпляр API с неверным портом PostgreSQL.

1. В Проводнике откройте корень DemoExam. Выделите папку Api → Ctrl+C → Ctrl+V. Переименуйте копию в ApiError. Копируется вся папка со всеми DLL и JSON.
2. В ApiError откройте appsettings.json через Блокнот. Измените Urls на http://127.0.0.1:5051. Это порт тестового HTTP сервера.
3. В строке Database измените только порт PostgreSQL на Port=1 и задайте Timeout=1;Command Timeout=2;Pooling=false. Для локального примера порт 1 обычно не обслуживает PostgreSQL; при необходимости выберите другой заведомо недоступный порт БД.
4. Проверьте полный образец JSON ниже. Сохраните его в ApiError. Настройки основной папки Api оставьте рабочими.
5. Дважды нажмите ApiError\DemoExam.Api.exe. В окне ожидается Now listening on: http://127.0.0.1:5051. Сам HTTP сервер может запуститься, потому что подключение к БД выполняется внутри запроса.
6. В Postman откройте «06 Ошибка БД — 500 и JSON» и нажмите Send. URL должен использовать {{errorBaseUrl}}/notes.
7. Ожидайте HTTP 500 и {"error":"Ошибка получения данных из базы данных"}. В Test Results должны пройти проверки 500 и JSON error.
8. Снова отправьте первый запрос на 5050: он должен по-прежнему вернуть 200 и пять заметок. Это подтверждает, что вы изменили копию, а не рабочий API.
9. После проверки закройте окно ApiError. Для повторного полного запуска коллекции понадобится снова открыть оба сервера.

**ApiError → appsettings.json для проверки 500**

```json
{
  "Urls": "http://127.0.0.1:5051",
  "ConnectionStrings": {
    "Database": "Host=127.0.0.1;Port=1;Database=demo_exam_2027;Username=postgres;Password=admin;Timeout=1;Command Timeout=2;Pooling=false"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Если копия всё равно подключается успешно.** Проверьте, что запускаете exe из ApiError, JSON сохранён в этой же папке, а DEMO_DB_CONNECTION не переопределяет строку. Сервер должен слушать 5051, но обращаться к недоступному порту БД. Скрипт Tests/Start-Error-Api.ps1 из архива делает тот же сценарий через переменную среды; он не нужен для описанного пути.

- [Microsoft — обработка ошибок Minimal API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/handle-errors?view=aspnetcore-9.0)

### Добавить собственные тесты через вкладку Scripts

1. В Postman создайте новый запрос через New → HTTP Request либо кнопку + у запросов.
2. Выберите GET и URL http://127.0.0.1:5050/notes?user_id=3. Нажмите Save и сохраните запрос в учебную коллекцию.
3. Откройте Scripts → Post-response. Если интерфейс старый, откройте Tests. Вставьте следующий код.
4. Нажмите Send. В нижней части ответа откройте Test Results. Все проверки должны пройти на исходном наборе данных.
5. Чтобы убедиться, что тест проверяет значение, временно измените ожидаемую дату на неверную и отправьте запрос снова: тест должен стать красным. Верните правильную дату и сохраните.
6. Для ошибок создайте отдельные запросы, а не смешивайте ожидание 200 и 400 у одного и того же URL. Тест проверяет тот ответ, который приходит после конкретного Send.

**Post-response для /notes?user_id=3**

```javascript
pm.test("HTTP 200", function () {
  pm.response.to.have.status(200);
});
pm.test("JSON и правильная заметка user26", function () {
  const body = pm.response.json();
  pm.expect(body).to.be.an("array").with.lengthOf(1);
  pm.expect(body[0]).to.have.all.keys(
    "id", "title_user", "content", "formatted_date"
  );
  pm.expect(body[0].id).to.equal(4);
  pm.expect(body[0].title_user).to.equal(
    "Проверка спецификации - user26"
  );
  pm.expect(body[0].content).to.equal(
    "Проверить нормы расхода материалов."
  );
  pm.expect(body[0].formatted_date).to.equal("18.03.2027");
});
```

pm.test задаёт имя проверки. pm.response содержит HTTP ответ. pm.response.json() разбирает тело и завершится ошибкой при невалидном JSON. pm.expect проверяет тип, количество и конкретные значения. Проверка даты по регулярному выражению подтверждает формат, а сравнение с ожидаемой датой подтверждает содержание.

- [Postman — post-response scripts и Test Results](https://learning.postman.com/docs/tests-and-scripts/write-scripts/test-scripts/)

### Запустить коллекцию целиком и сохранить результат

1. Убедитесь, что основной API работает на 5050, тестовый — на 5051, а база основного сервера содержит исходные данные.
2. У коллекции нажмите меню ⋯ → Run collection либо кнопку Run в открытой коллекции. Название зависит от версии Postman.
3. В Runner выберите все шесть запросов, Iterations = 1, без data file. Проверьте, что выбранные переменные указывают на ваши локальные серверы.
4. Нажмите Run. Просмотрите статусы запросов и Passed/Failed для тестов. Для исходной коллекции приложенный отчёт автора содержит 24 успешных утверждения; свой прогон оценивайте по фактическому результату.
5. Если шестой запрос не отправился, проверьте запуск ApiError на 5051. Если статус не 500, проверьте его подключение к БД.
6. Если доступна Export results, сохраните отчёт Runner в папку Tests\Results. Если экспорт в вашей версии недоступен, сохраните скриншот результатов и текстовый список фактических статусов.
7. Для передачи самой коллекции выберите меню ⋯ у коллекции → Export → Collection v2.1, если предлагается формат. Сохраните JSON. Не путайте экспорт коллекции и отчёт выполнения: первый хранит запросы и тесты, второй — результаты.
8. Если правили URL в запросе, исправьте также базовый URL в документации. В папке результата не должно быть «успешного» отчёта от другой версии коллекции.

В Tests/Results архива лежат Application-checks.txt, API-checks.txt и Postman-report.json. Они полезны как образцы и подтверждают сохранённый прогон автора. Они не заменяют новый тест после ваших изменений или переноса на другой компьютер.

- [Postman — запуск коллекции](https://learning.postman.com/docs/tests-and-scripts/running-collections/intro-to-collection-runs/)
- [Отчёт Postman из архива](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Tests/Results/Postman-report.json)
- [Сохранённые API проверки автора](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Tests/Results/API-checks.txt)

### Опубликовать локальный API в папку

1. Остановите отладку API. В Visual Studio нажмите DemoExam.Api правой кнопкой → Publish.
2. Выберите Folder и отдельную папку C:\ExamOutput\Api. Не публикуйте API поверх App: у них разные appsettings.json.
3. Проверьте Release, net9.0 и подходящий режим Framework-dependent или Self-contained для домашнего переноса. Нажмите Publish.
4. В папке результата найдите DemoExam.Api.exe и appsettings.json. Настройте рабочую строку PostgreSQL и Urls=http://127.0.0.1:5050.
5. Запустите exe из опубликованной папки и повторите первый запрос Postman. Сервер должен работать независимо от Visual Studio.
6. Сохраните папку Api вместе со всеми зависимостями и отдельно исходники Api + Core. Тестовую ApiError пометьте как копию для 500, чтобы её не приняли за рабочую конфигурацию.

Этот маршрут использует встроенный HTTP сервер ASP.NET Core для локальной проверки. IIS, сертификат HTTPS и внешняя публикация не нужны для повторения приведённого контракта. При передаче проекта сохраните фактический способ запуска в README.

### Проверка готовности

- [ ] GET /notes работает отдельно от WPF
- [ ] JSON содержит преобразованный заголовок дату и содержание
- [ ] Проверены 200 и пустой массив
- [ ] Неверные параметры дают 400 сбой БД даёт 500
- [ ] Коллекция сохраняет запросы и тесты
- [ ] Коллекция именно этого архива содержит шесть запросов
- [ ] Фильтр user_id=2 возвращает id 1 и 3
- [ ] Получил настоящий HTTP 500 от отдельного API на 5051
- [ ] Сохранил новый прогон Postman и проверил JSON поля

## 06 Заполнить документацию и подготовить сдачу

Заполняем исходный шаблон в Word, описываем единственный GET, параметры и ответы, сверяем документ с API и собираем комплект для эксперта.

Результат: API_Documentation.docx по исходному шаблону с GET /notes, user_id, примерами и 200/400/500. Папка сдачи открывается и проверяется.

### Открыть исходный шаблон и сохранить копию

1. В материалах сайта скачайте «Шаблон документации». Это исходный DOCX задания 6. Готовый Documents/API_Documentation.docx из вашего ZIP используйте как пример заполнения.
2. Откройте шаблон в Microsoft Word или предоставленном совместимом редакторе. При появлении защищённого просмотра включайте редактирование для этого учебного файла, если вы доверяете его источнику.
3. Выберите «Файл → Сохранить как → Обзор». Создайте копию в папке своей работы, например Documents\API_Documentation.docx. Исходный шаблон сохраняйте отдельно.
4. Включите «Вид → Разметка страницы», чтобы видеть реальные границы страниц и таблиц. При необходимости включите отображение знаков абзаца кнопкой ¶.
5. Не создавайте вместо шаблона новый пустой DOCX. Заполняйте существующие таблицы и сохраняйте их структуру. В готовом образце первая таблица содержит базовый URL и описание метода, вторая — коды HTTP.
6. Если текст не помещается, сначала разрешите перенос внутри ячейки и немного перераспределите ширины. Не удаляйте обязательные колонки и не заменяйте весь шаблон скриншотом.

- [Готовый DOCX именно из DemoExam](https://siers22.github.io/spravka/materials/2027/information-systems/versions/b8fb2557df994c5e-demoexam/Documents/API_Documentation.docx) — Референс результата задания 6; документ из исходного архива.

### Заполнить таблицу метода

1. В строке «Базовый URL» укажите http://127.0.0.1:5050. Без /notes: это общая основа адресов.
2. В «Название метода» напишите GET /notes. Не указывайте POST, PUT или DELETE: они отсутствуют в Program.cs.
3. В «Параметры» напишите user_id; integer, положительное число; необязательный фильтр по пользователю. При отсутствии возвращаются все заметки.
4. В «Описание метода» напишите: «Возвращает список заметок. title_user объединяет заголовок и логин через пробел, дефис, пробел. formatted_date имеет формат ДД.ММ.ГГГГ. content передаётся без изменений».
5. В «Формат ответа» напишите JSON-массив объектов: id — integer; title_user, content, formatted_date — string. При отсутствии данных возвращается [].
6. Укажите, что доступ к методу в этом примере не требует авторизации. Не добавляйте API key или Bearer token, если их нет в реализации.
7. Сохраните документ через Ctrl+S. Далее проверьте все примеры запросами в Postman.

| Столбец шаблона | Текст для заполнения |
| --- | --- |
| Название метода | GET /notes |
| Параметры | user_id: integer > 0, необязательный. Только один параметр этого имени. |
| Описание метода | Читает notes с login из users; преобразует title_user и дату; сохраняет content. |
| Формат ответа | application/json; массив объектов с id, title_user, content, formatted_date. |

В исходном шаблоне ячейка базового URL может быть объединённой. Щёлкните в видимую ячейку и замените текст один раз, а не создавайте несколько одинаковых строк.

### Заполнить таблицу HTTP кодов

| Код | Описание | Пример тела |
| --- | --- | --- |
| 200 | Успешный запрос. Массив заметок; при отсутствии данных пустой массив. | [...] или [] |
| 400 | Неверное значение, повтор user_id или неизвестный параметр. | {"error":"Параметр user_id должен быть положительным целым числом"} |
| 500 | Сбой подключения или чтения БД. | {"error":"Ошибка получения данных из базы данных"} |

1. Щёлкните описание строки 200 и внесите оба случая: есть данные и нет данных.
2. В строке 400 объясните abc, 0, отрицательное значение, повтор user_id и неизвестные параметры. Не описывайте это как «пользователь не найден».
3. В строке 500 опишите технический сбой базы. Для воспроизведения укажите тестовый экземпляр на 5051, а не закрытие HTTP сервера.
4. Если документ описывает также общий перехват непредвиденной ошибки, дополнительно укажите возможный JSON {"error":"Внутренняя ошибка сервера"}. Для сценария недоступной БД фактическое сообщение другое.
5. Сверьте текст error по ответу Postman. Пишите то сообщение, которое действительно возвращает Program.cs, включая поле error.

HTTP статус передаётся в самом ответе. Число 500 в текстовом поле JSON не заменяет StatusCode=500. В Program.cs оба error ответа задают соответствующий statusCode.

### Добавить полные запросы и ответы

После таблиц заполните или добавьте раздел примеров согласно исходному шаблону. Для успешного ответа удобно использовать user_id=3: там одна запись и можно показать весь массив, а не сокращённый [...].

**Успешный пример для документа**

```text
GET http://127.0.0.1:5050/notes?user_id=3
HTTP 200
Content-Type: application/json

[
  {
    "id": 4,
    "title_user": "Проверка спецификации - user26",
    "content": "Проверить нормы расхода материалов.",
    "formatted_date": "18.03.2027"
  }
]
```

**Пустой ответ и ошибки для документа**

```text
GET http://127.0.0.1:5050/notes?user_id=2147483647
HTTP 200
[]

GET http://127.0.0.1:5050/notes?user_id=abc
HTTP 400
{"error":"Параметр user_id должен быть положительным целым числом"}

GET http://127.0.0.1:5050/notes?unknown=1
HTTP 400
{"error":"Неизвестный параметр: unknown"}

GET http://127.0.0.1:5051/notes
HTTP 500
{"error":"Ошибка получения данных из базы данных"}
```

1. Вставляйте JSON текстом в Word, чтобы эксперт мог прочитать поля. Для кода выберите моноширинный шрифт, например Consolas, и обычные прямые кавычки.
2. Для каждого примера укажите полный URL, метод и статус. У ошибки БД явно подпишите, что это отдельный тестовый экземпляр API.
3. Добавьте порядок запуска: создать базу → исправить строку подключения → запустить Api\DemoExam.Api.exe → импортировать коллекцию → отправить запрос.
4. Если вы проверяли 500 через ApiError, опишите именно путь через копию и JSON, а не обязательный запуск PowerShell. Оригинальный DOCX архива содержит другой способ через Start-Error-Api.ps1.
5. После вставки проверьте, что Word не заменил кавычки внутри примера и не обрезал строку URL за границей ячейки.

### Проверить документ перед сохранением

1. В Word выберите «Файл → Печать» и посмотрите предварительный просмотр всех страниц. Таблицы не должны выходить за границы бумаги.
2. Проверьте, что строка заголовков таблицы и первая строка данных не разорваны неудобным переносом. Для многостраничной таблицы можно выбрать строку заголовков → «Макет таблицы → Повторить строки заголовков».
3. Проверьте имена id, title_user, content, formatted_date и user_id. В ответе нет created_at, title или отдельного login.
4. Откройте Postman и отправьте каждый приведённый URL. Сверьте текст, дату, количество записей и статусы. При изменённых данных исправьте примеры.
5. Сохраните DOCX и закройте Word. Откройте сохранённый файл повторно, чтобы проверить, что изменения действительно записаны.
6. Если площадка дополнительно просит PDF документации, используйте «Файл → Сохранить как → PDF». Для задания с DOCX оставьте сам DOCX по исходному шаблону.

Шаблон может иметь свои поля и подписи; сохраняйте их и заполняйте по фактическому API. Не обещайте методы, поля или проверку безопасности, которых нет в реализации.

### Собрать полный комплект в одной папке

| Результат | Что положить | Как проверить |
| --- | --- | --- |
| Задание 1 | Documents/ER_Diagram.pdf и редактируемую схему | PDF открывается, 15 таблиц и 19 FK видны |
| Задание 2 | Database/01_schema.sql, 02_data.sql, Customers.json; дамп/backup по правилам площадки | Новая пустая база восстанавливается |
| Задание 3 | Database/03_order_cost.sql и результат расчёта | 9974,28 + 4400,00 = 14374,28 |
| Задание 4 | Source с Core и WPF; App со всеми зависимостями | Вход, пазл, admin, блокировка и разблокировка |
| Задание 5 | Source/Api; Api; Postman JSON и свои отчёты | 200, [], 400, настоящий 500 и преобразованные поля |
| Задание 6 | DOCX по исходному шаблону | URL, поля и примеры совпадают с Postman |
| Общий README | ПО, настройки, запуск, логины учебной базы | Эксперт может повторить запуск без ваших устных подсказок |

1. Используйте папку и способ передачи, указанные площадкой. Названия папок в архиве — учебный пример структуры.
2. Копируйте проекты вместе с Core: ProjectReference использует относительные пути. Не переносите одну папку WPF без соседней библиотеки.
3. Копируйте опубликованные App и Api целиком. Одна exe не содержит все зависимости framework-dependent публикации.
4. Укажите в README две разные группы учётных данных: приложение admin/admin и user25/user123; PostgreSQL — адрес, порт, имя БД, пользователь и пароль вашей учебной среды.
5. Уберите случайные несохранённые изменения в настройках. В Api рабочий порт БД, в ApiError ошибочный — только если передаёте тестовую копию с явной пометкой.
6. Проверьте повторную распаковку или копирование в другую папку Windows. Программа не должна зависеть от абсолютного пути вашей исходной папки.
7. Для ПА готовьте задания 1–3. Для ГИА базового уровня — весь комплект 1–6. Время 20+30+20+60+60+20 = 210 минут относится к экзамену; первый подробный учебный проход займёт больше.

### Исправить типичные ошибки по симптомам

| Что видите | Где проверить | Что сделать через интерфейс |
| --- | --- | --- |
| Не виден шаблон WPF | Visual Studio Installer → Workloads | Добавить .NET desktop development |
| NETSDK1045 или .NET 9 отсутствует | Версия Visual Studio и SDK | Проверить поддержку .NET 9 и установленный SDK 9 |
| NuGet NU1301 / пакет не найден | Manage NuGet Packages → source | Проверить источник и доступный Npgsql 9.0.3; офлайн нужна подготовленная среда |
| Cannot locate resource mainwindow.xaml | App.xaml → StartupUri | Указать LoginWindow.xaml и пересобрать |
| InitializeComponent не найден | XAML x:Class, C# namespace, первая ошибка сборки | Исправить пару файлов и XAML; не писать метод вручную |
| Фрагменты пазла не загружаются | Images и свойства Resource | Проверить имена 1.png–4.png и pack URI |
| Рядом с программой отсутствует appsettings.json | Выходная папка сборки | Copy if newer и пересборка; запускать всю папку |
| JSON не читается | appsettings.json | Вернуть корректные кавычки и запятые; проверить расширение |
| password authentication failed | Пароль postgres в pgAdmin и JSON | Поставить фактический пароль сервера |
| database does not exist | Database в строке подключения | Создать базу или исправить имя |
| relation users / order_costs does not exist | Query Tool → current_database() | Выполнить схему и 03_order_cost.sql в правильной базе |
| relation already exists | Путь восстановления базы | Не запускать полный импорт поверх заполненной базы |
| current transaction is aborted | Query Tool текущей вкладки | ROLLBACK; затем устранить исходную ошибку |
| admin заблокирован | users.is_blocked и failed_attempts | Использовать другого администратора; учебное восстановление ниже |
| Кнопки WPF молчат | Click в XAML и точка остановки | Сверить имя обработчика и событие |
| Could not send request в Postman | Запуск API и фактический URL | Запустить локальный API и проверить порт |
| 404 по корню / | Маршрут | Открыть /notes, а не только базовый URL |
| Address already in use | Два экземпляра API | Остановить предыдущий процесс или выбрать другой порт |
| Тест 500 получил 200 | ApiError/appsettings и DEMO_DB_CONNECTION | Проверить, что именно копия использует недоступный порт БД |
| Сумма материалов выросла втрое | SQL JOIN составов | Сначала отдельно агрегировать материалы и операции |
| Следующий INSERT конфликтует по ID | setval после ручного seed | Выполнить синхронизацию identity из 02_data.sql |

Если единственный admin случайно заблокирован в вашей локальной учебной базе, откройте Query Tool под владельцем базы и выполните запрос ниже. Это аварийное восстановление учебной среды: обычный сценарий задания — разблокировка через другого действующего администратора.

**Восстановление admin в своей учебной базе**

```sql
UPDATE users
SET is_blocked = false, failed_attempts = 0
WHERE login = 'admin';
```

### Что уметь показать и объяснить эксперту

1. Откройте ER PDF и объясните разделение шапок и строк, составные PK, справочники и одну спецификацию на изделие.
2. В pgAdmin покажите исходный строковый код с нулями, импорт addres → address и контрольные 15 таблиц / 19 FK.
3. Откройте SQL и объясните две отдельные суммы. Покажите ручной расчёт 14374,28 и отличие продажной суммы 26828.
4. Запустите WPF, соберите пазл, войдите по двум ролям. На отдельном пользователе покажите три ошибки, сохранение блокировки после перезапуска и разблокировку с пустым полем пароля.
5. Покажите путь одной кнопки: Click в XAML → обработчик C# → метод Database → параметризованный SQL → ItemsSource таблицы.
6. В Postman отправьте /notes, фильтр, пустой результат, неверный параметр и настоящий сбой БД на отдельном экземпляре. Объясните HTTP статус и JSON отдельно.
7. Откройте DOCX и покажите, что базовый URL, параметр и поля ответа совпадают с текущим Program.cs и реальными запросами.
8. Если спрашивают про расхождения источников, объясните выбранные единицы и применение цены операции к времени. Не подменяйте пересчёт готовым ошибочным итогом из листа.

### Проверка готовности

- [ ] Использовал исходный шаблон документации
- [ ] Описал метод параметр и формат ответа
- [ ] Привёл примеры 200 400 500
- [ ] Документ совпадает с работающим API
- [ ] DOCX заполнил по исходному шаблону
- [ ] В документе один реальный GET и фактические поля ответа
- [ ] Проверил каждый URL из документа в Postman
- [ ] Повторно запустил комплект из другой папки

## Словарь

**WPF.** Технология оконного приложения .NET для Windows. В этом решении внешний вид задаётся XAML, действия — C#.

**XAML.** Описание окна и его элементов. x:Name даёт имя для C#, Click связывает кнопку с обработчиком.

**Code behind.** Файл .xaml.cs с логикой конкретного окна или элемента управления.

**Решение и проект.** Решение .sln объединяет Core, WPF, API и Checks; каждый проект .csproj задаёт свои файлы и зависимости.

**SDK и Runtime.** SDK нужен для сборки, runtime — для запуска. Этот архив собран под семейство .NET 9.

**NuGet и Npgsql.** NuGet управляет пакетами .NET. Npgsql 9.0.3 в этом примере передаёт команды PostgreSQL.

**PK.** Первичный ключ, однозначно определяющий запись. Может состоять из нескольких колонок.

**FK.** Внешний ключ: поле ссылки на существующую запись другой таблицы. В DemoExam их 19.

**3НФ.** Организация таблиц, при которой данные зависят от ключа и справочные свойства не дублируются в документах.

**IDENTITY.** Автоматическая выдача числового ключа. После ручной загрузки ID последовательность синхронизируют setval.

**numeric и decimal.** Точные десятичные числа PostgreSQL и C#, применённые для норм и денежных сумм.

**CTE.** Именованное выражение после WITH. В расчёте отдельно создаются material_cost и operation_cost.

**JOIN.** Соединение строк по условию. Два состава нельзя соединять напрямую перед SUM: строки перемножатся.

**LEFT JOIN.** Сохраняет левую запись даже без соответствующей правой. Используется для заказа без строк.

**SUM и GROUP BY.** SUM складывает значения, GROUP BY задаёт, для какой спецификации или заказа считается сумма.

**COALESCE.** Берёт первое значение, отличное от NULL. В расчёте отсутствие суммы заменяется нулём.

**VIEW.** Сохранённый запрос. order_costs вычисляет стоимость при чтении, а не хранит вручную редактируемый итог.

**Транзакция.** Группа действий между BEGIN и COMMIT или ROLLBACK. Вход сохраняет счётчик ошибок атомарно.

**FOR UPDATE.** Блокировка найденной строки пользователя на время транзакции, чтобы не потерять параллельные изменения счётчика.

**Параметризованный SQL.** Значение передаётся через Parameters отдельно от текста запроса, например @login.

**PBKDF2 и соль.** Способ получать хеш пароля со случайной солью. Verify заново вычисляет результат, а не расшифровывает пароль.

**ItemsSource и Binding.** ItemsSource задаёт коллекцию для DataGrid, Binding выбирает свойство объекта для конкретной колонки.

**pack URI.** Адрес встроенного ресурса WPF, например Images/1.png внутри сборки.

**API и endpoint.** HTTP интерфейс и конкретный маршрут. Здесь endpoint GET /notes работает в отдельном DemoExam.Api.

**Query parameter.** Параметр после вопросительного знака URL: /notes?user_id=2. Не является частью пути /notes.

**HTTP 200 400 500.** Успех, ошибка параметров, технический сбой сервера. Отсутствие данных даёт 200 и [].

**Post response test.** Скрипт Postman, выполняемый после ответа. Результаты находятся в Test Results.

**Framework dependent.** Публикация требует соответствующий runtime на компьютере. Нужно переносить всю папку зависимостей.

**Self contained.** Публикация включает runtime для выбранной платформы, но требует доступных runtime packs при сборке.

**localhost.** Текущий компьютер. Облачный клиент другого сервера не обращается к вашему 127.0.0.1.
