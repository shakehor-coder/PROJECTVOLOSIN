# Задание 5 — DLL

Расчёт остатков и семафор вынесены из веб-приложения в проект
`ShopEase.Analytics/ShopEase.Analytics.csproj`. При сборке создаётся
`ShopEase.Analytics.dll`. Веб-приложение и консольные проверки подключают
библиотеку через `ProjectReference`, а не копируют исходный код.

Библиотека содержит неизменяемые DTO, InventoryAnalytics и InventoryCalculationGate.
Она не зависит от EF Core, HTTP и базы данных. Пространство имён сохранено,
чтобы не менять интерфейсы предыдущих заданий. CLR загружает управляемую DLL
из каталога опубликованного приложения; ручной Assembly.Load не нужен.

Проверка: `dotnet build ShopEase/Ecommerce.csproj -c Release`.
Результат: файл `ShopEase/bin/Release/net8.0/ShopEase.Analytics.dll`.
