# TestTaskAton API

Это бэкенд-приложение ASP.NET Core Web API, разработанное как тестовое задание. Оно предоставляет API для управления пользователями с функциями создания, обновления, удаления (мягкого/жесткого), изменения пароля/логина, а также получения информации о пользователях. Аутентификация реализована с использованием JWT, а управление пользователями и ролями — через ASP.NET Core Identity.

## Начало работы

Для запуска приложения выполните следующие шаги.

### 1. Подготовка Базы Данных (PostgreSQL)

Приложение использует PostgreSQL в качестве базы данных. Вам необходимо запустить экземпляр PostgreSQL.

* **Используя Docker (рекомендуется):**
  Если у вас установлен Docker, вы можете легко запустить PostgreSQL с помощью следующей команды:
    ```bash
    docker run --name some-postgres -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 -d postgres
    ```
  Замените `mysecretpassword` на желаемый пароль.

* **Локальная установка:**
  Вы также можете установить PostgreSQL локально, следуя инструкциям на [официальном сайте PostgreSQL](https://www.postgresql.org/download/).

### 2. Конфигурация приложения (`appsettings.json`)

Вам необходимо настроить строки подключения к базе данных и параметры JWT, а также данные для начального пользователя-администратора.

Откройте файл `appsettings.json` (или `appsettings.Development.json` для среды разработки) и обновите следующие секции:

* **`ConnectionStrings:DefaultConnection`**: Укажите строку подключения к вашей базе данных PostgreSQL.
    * Пример: `"Host=localhost;Port=5432;Database=AtonData;Username=postgres;Password=mysecretpassword"`
    * Убедитесь, что `Database` соответствует имени вашей базы данных, а `Username` и `Password` — учетным данным вашей PostgreSQL.

* **`JWT:SigningKey`**: Укажите длинный и безопасный секретный ключ для подписи JWT-токенов. **Это крайне важно для безопасности!** Длина ключа должна быть не менее 16 символов.
    * Пример: `"YourSuperSecretJWTKeyThatIsAtLeast16CharactersLongAndVeryComplex"`

* **`AdminSettings`**: Укажите логин, пароль и имя для пользователя-администратора, который будет создан при первом запуске приложения (если его еще нет).
    * Пример:
        ```json
        "Admin" : {
          "Login":"<Логин первого админа>",
          "Password":"<Пароль первого админа>",
          "Name":"<Имя первого админа>"
        }
        ```
      **Важно:** Убедитесь, что `AdminPassword123!` соответствует требованиям к сложности пароля, заданным в приложении (по умолчанию: цифра, строчная, заглавная буквы, спец. символ, мин. 8 символов). Если вы изменили требования в `Program.cs`, учтите это. После первого успешного запуска **настоятельно рекомендуется изменить этот пароль** через API.

**Пример файла `appsettings.json` можно найти в корневом каталоге репозитория `appsettingsexample.json`.**

### 3. Запуск приложения

После настройки базы данных и `appsettings.json` вы можете запустить приложение.

1.  **Откройте терминал** в корневом каталоге проекта `TestTaskAton`.
2.  **Выполните команду:**
    ```bash
    dotnet run
    ```
    или используйте вашу IDE (Visual Studio, Rider) для запуска проекта.

При первом запуске приложение автоматически применит все необходимые миграции к базе данных и создаст пользователя-администратора, если он еще не существует.

### 4. Доступ к API и Swagger UI

После запуска приложения, API будет доступен по адресу `https://localhost:your_port_number` (порт обычно отображается в консоли при запуске).

### 5. Контакты для связи

1.  [Telegram](t.me/talubarni)
2.  [HeadHunter](https://kazan.hh.ru/resume/95f6ea50ff0ec813bd0039ed1f707773686739)
3.  Gmail - nabiullin.bulat007@gmail.com
    

---
Если у вас возникнут какие-либо проблемы при запуске, проверьте логи в консоли и убедитесь, что все шаги выполнены корректно.
