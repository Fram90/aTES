# 1 Неделя

## Картинки всякие

Miro https://miro.com/app/board/uXjVMxQR4GM=/?share_link_id=748782069026
На доске есть комментарии текстом по некоторым местам

## Разобрать каждое требование на составляющие (актор, команда, событие, query)
### TaskTracker

4. Новые таски может создавать кто угодно (администратор, начальник, разработчик, менеджер и любая другая роль). У задачи должны быть описание, статус (выполнена или нет) и рандомно выбранный попуг (кроме менеджера и администратора), на которого заассайнена задача.

- Actor — Any user 
- Command — Create task
- Data — Task, Task.Description, Task.Status User (any except manager or admin), User.Role, Price
- Event — Task.Created

</br>

- Actor — Task.Created event
- Command — Get Random User
- Data — User (any except manager or admin)
- Event — User.Selected

</br>

- Actor — User.Selected event
- Command — Assign task to user
- Data — Task, User (any except manager or admin)
- Event — Task.Assigned


5. Менеджеры или администраторы должны иметь кнопку «заассайнить задачи», которая возьмёт все открытые задачи и рандомно заассайнит каждую на любого из сотрудников (кроме менеджера и администратора) . Не успел закрыть задачу до реассайна — сорян, делай следующую.

a) Ассайнить задачу можно на кого угодно (кроме менеджера и администратора), это может быть любой существующий аккаунт из системы.

- Actor — User. Manager or Admin
- Command — AssignTasks
- Data — Tasks with Status.Open, Users with Role != Manager or Admin
- Events — Task.Assigned

c) При нажатии кнопки «заассайнить задачи» все текущие не закрытые задачи должны быть случайным образом перетасованы между каждым аккаунтом в системе
- Actor — User. Manager or Admin
- Command — ReassignTasks
- Data — Tasks with Status.Open, Users with Role != Manager or Admin
- Events — Task.Reassigned for each Task

6. Каждый сотрудник должен иметь возможность видеть в отдельном месте список заассайненных на него задач + отметить задачу выполненной.

- Actor — User
- Command — GetAssignedToMeTasks
- Data — Tasks with Status.Open and Assigned = current_user
- Events — --

</br>

- Actor — User
- Command — CompleteTask
- Data — Tasks with Status.Open and Assigned = current_user
- Events — Task.Completed

### Accounting

1. Аккаунтинг должен быть в отдельном дашборде и доступным только для администраторов и бухгалтеров. У админов и бухгалтеров должен быть доступ к общей статистике по деньгами заработанным (количество заработанных топ-менеджментом за сегодня денег + статистика по дням).

- Actor — User.Admin and User.Accounter
- Command — GetAccountringDashboard
- Data — TodayDailyProfit, HistoryDailyProfit
- Events — --

a) у обычных попугов доступ к аккаунтингу тоже должен быть. Но только к информации о собственных счетах (аудит лог + текущий баланс). 

- Actor — User.Worker
- Command — Get AccountringInfo
- Data — Account.Balance, AuditLog
- Events — --

3. У каждого из сотрудников должен быть свой счёт, который показывает, сколько за сегодня он получил денег. У счёта должен быть аудитлог того, за что были списаны или начислены деньги, с подробным описанием каждой из задач.

- Actor — Account.Charged event
- Command — Create AuditLog item
- Data — Task.Id, Task.Description, Task.Price, User
- Events — AuditLog.ItemCreated

</br>

- Actor — Account.Paid event
- Command — Create AuditLog item
- Data — Task.Id, Task.Description, Task.Price, User
- Events — AuditLog.ItemCreated

4. Цены на задачу определяется единоразово, в момент появления в системе (можно с минимальной задержкой). Цены рассчитываются без привязки к сотруднику

- Actor — Task.Created event
- Command — CreatePricesForTask
- Data — Task.Id, Price
- Events — Price.Created  

</br>
	Деньги списываются сразу после ассайна на сотрудника, а начисляются после выполнения задачи.

</br>

- Actor — Task.Assigned event
- Command — ChargeTaskCost
- Data — Price, User
- Events — Account.Charged
	
</br>

- Actor — Task.Completed event
- Command — PayTaskCost
- Data — Price, User
- Events — Account.Paid


6. В конце дня необходимо:
- считать сколько денег сотрудник получил за рабочий день

</br>

- Actor — System cron
- Command — CloseDay
- Data — Account, User (или AuditLog, зависит от реализации)
- Events — DayClosed

</br>

- отправлять на почту сумму выплаты

</br>

- Actor — DayClosed event
- Command — SendDailySum
- Data — AuditLog
- Events — DailySumSent

7. После выплаты баланса (в конце дня) он должен обнуляться, и в аудитлоге всех операций аккаунтинга должно быть отображено, что была выплачена сумма.

- Actor — DayClosed event
- Command — FlushAccountBalance
- Data — Account
- Events — Account.Flushed

</br>

- Actor — Account.Flushed event
- Command — Create AuditLog item
- Data — AuditLog
- Events — Account.Flushed

### Analytics
1. Аналитика — это отдельный дашборд, доступный только админам.

2. Нужно указывать, сколько заработал топ-менеджмент за сегодня и сколько попугов ушло в минус.

3. Нужно показывать самую дорогую задачу за день, неделю или месяц.

	a) самой дорогой задачей является задача с наивысшей ценой из списка всех закрытых задач за определенный период времени

	b) пример того, как это может выглядеть:

	03.03 — самая дорогая задача — 28$ 
	02.03 — самая дорогая задача — 38$ 
	01.03 — самая дорогая задача — 23$ 
	01-03 марта — самая дорогая задача — 38$

Тут все считается по застримленным данным.

- Actor — User.Admin
- Command — Get Analytics Dashboard
- Data — CUD AuditLog data
- Events — --