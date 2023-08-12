# TaskTracker сервис

## Authn
TasksController требует аутентификации. Получает он ее распарсив jwt, полученный через Auth сервис.

```
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});
```

## Authz

Атрибут `MustHaveAnyRoleAttribute` срабатывает уже после аутентификации и проверяет что у пользователя есть одна из указанных ролей. Если есть, то ок, если нет, то 403.


## Стриминг

`/Kafka/Consumers` Консумеры стартуют с запуском приложения, слушают топики, добавляют в бд юзеров, обновляют роли.

## TBD
1. накатывать из топика данные по добавлению пользователей/изменению ролей. Например для первоначальной инициализации
2. идемпотентность сообщений
3. опубликовывать события этого сервиса