# aTES
ДЗ для курса по Асинхронной архитектуре

[https://lucid.app/lucidchart/712337de-3c2f-4bd8-9ad3-94e2aa68cc2b/edit?invitationId=inv_bc0decea-89cc-4d94-944d-c505eab1873b](https://lucid.app/documents/view/712337de-3c2f-4bd8-9ad3-94e2aa68cc2b)

## TaskTracker

### За что отвечает 
Создание, завершение, ассайн, получение списка тасков.

### Описание
Входящие запросы синхронные, кроме AssignTasks.
На случай проблем с сетью, зависимостями - ретраи из вызывающего кода.

### Методы:
CreateTask(int popugId, description)
После создания публикуется событие TaskAssigned(int taskId, int popugId) в шину

CompleteTask(int taskId)
После завершения публикуется событие TaskCompleted(int taskId, int popugId)

AssignTasks() - запускает асинхронный процесс ReassignTasks. 
По каждому реассайну таска публикуется TaskAssigned(int taskId, int popugId). Прогресс асинхронной задачи нужно сохранять в бд на случай проблем с брокером сообщений

GetTasks(int popugId)

## Accounting

### За что отвечает 
Бухгалтерия. Начисление/списывание денег со счета

### Описание

### Методы:
GetAccountInfo(int popugId):
Баланс + лог

### Асинхронные эндпоинты (обработчики, консумеры)
ChargeOnTaskAssigned(TaskAssigned event): списывает со счета попуга деньги после ассайна таска

PayOnTaskCompleted(TaskCompelted event): начисляет деньги после завершения таска


## UI+Gateway. Веб приложение
### За что отвечает
Отрисовка дашбордов (UI), авторизация (серверная часть), оркестрация запросов по сервисам

### Описание
Сделано одним веб приложением, на Razor Pages
