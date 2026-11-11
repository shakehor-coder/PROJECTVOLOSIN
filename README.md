# PROJECTVOLOSIN — ShopEase

Учебный интернет-магазин на ASP.NET Core 8 MVC, EF Core и SQL Server.
Основа: https://github.com/masood07/ShopEase
Исходная ревизия: `6da83287d2a4b2dc960bd5a2d9493459a021c3db`.

## Выполненное задание 2

Вариант 1: запуск другого процесса с отслеживанием состояния и завершения.
Админка `/Admin/Processes` запускает `dotnet --info`, показывает PID, вывод,
время выполнения, состояние и код выхода. Есть остановка активного процесса.

## Локальный запуск

Нужны .NET 8 SDK и доступный SQL Server. Ниже пример PowerShell для Windows
и SQL Server Express. Команды выполняются из корня репозитория.

```powershell
cd ShopEase
$env:ConnectionStrings__DefaultConnection = 'Server=localhost\SQLEXPRESS;Database=ShopEaseStudy;Integrated Security=True;Encrypt=False;TrustServerCertificate=True'
$env:SeedAdmin__Email = 'admin@example.test'
# Введите собственный пароль администратора, не сохраняйте его в Git.
$secret = Read-Host 'Пароль администратора' -AsSecureString
$env:SeedAdmin__Password = [System.Net.NetworkCredential]::new('', $secret).Password
dotnet restore
dotnet tool install dotnet-ef --version 8.0.24 --tool-path ./.tools
./.tools/dotnet-ef database update
dotnet run --launch-profile http
```

Сайт: http://localhost:5134. Администратор создаётся только при заданном
`SeedAdmin__Email`; при первом создании требуется `SeedAdmin__Password`.
После создания аккаунта пароль из настроек можно убрать.
Для другого сервера измените строку подключения. Перед первым запуском
обязательно примените миграции. Настройки автора и встроенный пароль удалены.

## Проверки процессов без базы

Из корня репозитория:

```sh
dotnet run --project checks/ProcessChecks/ProcessChecks.csproj -c Release
```

Полная проверка магазина в браузере требует доступного SQL Server.
Размещение сайта и подключение внешней базы выполняются отдельно.

## Выполненное задание 3

Добавлен отчёт `/Admin/Inventory` с асинхронным чтением базы и параллельным
расчётом по категориям на большом каталоге. Описание механизмов, решаемых
проблем и ограничений: [tasks/03_ASYNC_AND_THREADS.md](tasks/03_ASYNC_AND_THREADS.md).

```sh
dotnet run --project checks/InventoryChecks/InventoryChecks.csproj -c Release
```

## Выполненное задание 4

В вычислительную часть отчёта внедрён общий `SemaphoreSlim(1, 1)`:
один расчёт одновременно, асинхронное ожидание и освобождение при ошибке
или отмене. Объяснение: [tasks/04_SYNCHRONIZATION.md](tasks/04_SYNCHRONIZATION.md).

```sh
dotnet run --project checks/SynchronizationChecks/SynchronizationChecks.csproj -c Release
```

## Новые задания

- [DLL](tasks/05_DLL.md): расчёт отчёта вынесен в `ShopEase.Analytics.dll`.
- [Реестр](tasks/06_REGISTRY.md): общие настройки и состояние в `/Admin/Registry`.
- [Хук](tasks/07_HOOK.md): событие успешного отчёта обновляет реестр.
- [Параллельность и память](tasks/08_PARALLELISM_AND_MEMORY.md): описание с примерами; два одинаковых задания объединены.
- [Размещение и результаты проверок](tasks/09_HOSTING_AND_TESTS.md): SQLite, Docker и Render.

Для демонстрации без SQL Server задайте `Database__Provider=Sqlite`.
Создание базы выполняется перед стартом сайта. Для обычного режима SQL Server
по-прежнему нужна строка подключения; миграции теперь применяются при старте.

Проверки новых механизмов:

```sh
dotnet run --project checks/HookRegistryChecks/HookRegistryChecks.csproj -c Release
node checks/http-smoke.cjs
```

## Тестовый сайт

https://projectvolosin.onrender.com/

Администратор: `admin@example.test`; пароль находится в переменной
`SeedAdmin__Password` в [настройках Render](https://dashboard.render.com/web/srv-dbq5s0bl550s739lmgtg).
Обычного покупателя можно зарегистрировать через Register.
Данные тестового сервиса временные и могут сбрасываться после рестарта.
