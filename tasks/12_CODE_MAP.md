# Карта кода и рефакторинг

| Задача | Файл или папка |
| --- | --- |
| Общая последовательность запуска | `ShopEase/Program.cs` |
| Подключение базы, Identity, сессий, сервисов и репозиториев | `ShopEase/Configuration/ServiceCollectionExtensions.cs` |
| HTTP-конвейер, маршруты, запрет регистрации, health endpoint | `ShopEase/Configuration/ApplicationBuilderExtensions.cs` |
| Подготовка базы перед запуском | `ShopEase/Data/ApplicationInitializer.cs` |
| Создание ролей и администратора | `ShopEase/Data/IdentityInitializer.cs` |
| Алгоритм заполнения тестовых данных | `ShopEase/Data/DbInitializer.cs` |
| Списки тестовых категорий и товаров | `ShopEase/Data/Seed/SampleCatalog.cs` |
| HTTP-запросы оформления и переходы между страницами | `ShopEase/Controllers/CheckoutController.cs` |
| Проверки заказа, цена, транзакция и списание остатков | `ShopEase/Services/CheckoutService.cs` |
| Результат оформления | `ShopEase/Services/CheckoutResult.cs` |
| Фильтрация, сортировка и страницы каталога | `ShopEase/Repositories/ProductRepository.cs` |
| Объекты базы / данные страниц / HTML | `ShopEase/Models` / `ShopEase/ViewModels` / `ShopEase/Views` |
| Административные страницы и контроллеры | `ShopEase/Areas/Admin` |
| Расчёты отчёта и синхронизация | `ShopEase.Analytics` |
| Работа с процессами, реестр, подписка на событие | `ShopEase/Services/ProcessMonitor.cs`, `ApplicationRegistry.cs`, `InventoryReportHook.cs` |

## Что изменилось

- `Program.cs` показывает только последовательность запуска. Настройки и заполнение базы вынесены в именованные файлы. Контроллер оформления передаёт бизнес-операцию сервису через DI.
- Оформление загружает все товары одним запросом вместо отдельных обращений для каждой позиции. Адрес, заказ и строки заказа сохраняются одним `SaveChangesAsync`; условные обновления остатков остаются отдельными запросами на каждую позицию.
- Операции оформления выполняются асинхронно с отменой запроса. Цена и итог пересчитываются по базе: если цена изменилась после добавления в корзину, заказ использует новую цену. Предварительный итог корзины может отличаться от суммы заказа.
- Списание остатков выполняется условным `UPDATE` в транзакции. Если товара недостаточно, предыдущие списания откатываются. Повторяющиеся позиции и неположительное количество отклоняются. Это не обеспечивает идемпотентность повторной отправки одного заказа.
- Каталог использует общий фильтр для списка и количества, `AsNoTracking` для чтения и дополнительную сортировку по ID для равных значений. Отрицательные страницы нормализуются, расчёт смещения защищён от переполнения.
- Маршруты, публичные страницы и файлы учебных заданий сохранены. Миграции схемы для этих изменений не нужны.

Изменения уменьшают число запросов и упрощают поиск кода; численного замера ускорения под нагрузкой нет. SQL Server необходимо отдельно проверить в целевом окружении.

## Проверки

Из корня репозитория, с установленным .NET 8 SDK:

```sh
dotnet build ShopEase/Ecommerce.csproj -c Release
dotnet run --project checks/CheckoutChecks/CheckoutChecks.csproj -c Release
dotnet run --project checks/ProcessChecks/ProcessChecks.csproj -c Release
dotnet run --project checks/InventoryChecks/InventoryChecks.csproj -c Release
dotnet run --project checks/SynchronizationChecks/SynchronizationChecks.csproj -c Release
dotnet run --project checks/HookRegistryChecks/HookRegistryChecks.csproj -c Release
```

`CheckoutChecks` работает с SQLite в памяти: проверяет актуальные цены, списание остатков, откат частично выполненного оформления, отсутствие лишнего адреса и заказа при ошибке, проверку адреса, количества, дубликатов и границ пагинации. Это не нагрузочный тест конкурентных заказов.

## Результаты в среде подготовки

- `git diff --check`: успешно.
- Roslyn из .NET SDK 8.0.428: синтаксический разбор всех 63 C#-файлов — без ошибок. Это не полная проверка типов и зависимостей.
- Существующие `ProcessChecks`, `InventoryChecks`, `SynchronizationChecks`, `HookRegistryChecks`: успешно. Из-за ошибки NuGet они скомпилированы напрямую через Roslyn с локальными framework references; аналитика собрана как отдельная `ShopEase.Analytics.dll`.
- Полная сборка `dotnet build ShopEase/Ecommerce.csproj -c Release` остановилась на восстановлении зависимостей: `System.IO.IOException ... 'NuGet-Migrations'` при создании системного mutex. В среде отсутствует `/tmp`; SDK установлен в рабочую папку, однако настройка `TMPDIR` не устранила ошибку mutex.
- Новые `CheckoutChecks`, запуск веб-приложения и SQL Server не проверены из-за этого ограничения. Перед использованием изменений выполните приведённые выше команды в обычном окружении .NET 8.
