# Auth сервис

Позволяет регистрировать пользователей, есть метод для аутентификации, который выдает jwt.

Модель пользователя:
```
public class PopugUser
{
    public int Id { get; set; }
    public string Email { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public PopugRoles Role { get; set; }
    public string PasswordHash { get; set; }
}
```
Email - уникальное поле, однозначно идентифицирующее пользователя, по нему будет проходить аутентификация.

Регистртуем пользователя так:

```
curl --location 'http://localhost:5286/api/userauthentication/register' \
--header 'accept: */*' \
--header 'Content-Type: application/json' \
--data-raw '{
  "userName": "PopugWorker1",
  "email": "PopugWorker1@popug.com",
  "password": "string",
  "role": "Worker"
}'
```

Доступные значения для ролей: `Worker`, `Admin`, `Manager`, `Accounter`

Токен получаем так:

```
curl --location 'http://localhost:5286/api/userauthentication/authenticate' \
--header 'accept: text/plain' \
--header 'Content-Type: application/json' \
--data-raw '{
"email": "PopugWorker1@popug.com",
"password": "string"
}'
```

Если хеши паролей совпали, вернется jwt, который будем прикладывать к запросам других сервисов через хедер: `Authentication: Bearer <token>`

Оно как бы логично и очевидно, что на этот сервис тоже должна быть аутентификация, и новых пользователей должен создавать только админ, но тогда дюже неудобно разрабатывать это всё. Оставим это за скобками, все методы можно вызывать без аутентификации.

### Стриминг
Стримится через Кафку отправкой целикового объекта с пользователем (см. методы ChangeRole и RegisterUser). По-хорошему нужны отдельные модели, чтобы туда не попадало всякое, ну или сериализатор кастомный, но раз уж разрешили говнокодить, то вот.