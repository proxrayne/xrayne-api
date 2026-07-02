# XRayne

XRayne Panel - панель управления для работы с `xray-core` и управляемыми
нодами. Репозиторий `xrayne-panel` содержит только код панели: CLI, REST API и
React-интерфейс. Исходного кода отдельного сервиса удаленной ноды в этом
репозитории нет.

## Цели проекта

- Управление жизненным циклом `xray-core`: запуск, остановка, перезапуск,
  проверка статуса, обновление конфигурации и просмотр логов.
- Самостоятельная работа панели управления на отдельном сервере.
- Управление нодами из панели: хранение записей, provisioning, reconnect,
  включение/отключение, удаление и просмотр статуса.
- Генерация SSL-сертификатов, TLS-ключей и других ключевых материалов,
  необходимых для настройки Xray.
- Предоставление удобного REST API для интеграции с внешними системами.
- Предоставление CLI для администрирования без веб-интерфейса.
- Предоставление React-интерфейса для визуального управления сервером,
  пользователями, inbound/outbound-конфигурациями и состоянием Xray.

## Технологии

### API и CLI

- .NET 9
- ASP.NET Core
- REST API
- System.CommandLine или аналогичный CLI-фреймворк
- Hosted Services для фоновых задач
- Dependency Injection
- JSON-конфигурация Xray

### UI

- React
- TypeScript
- TanStack Query
- Axios
- React Router
- Vite

### Инфраструктура

- `xray-core`
- systemd для Linux-сервисов
- Docker image для релизной сборки API + UI и Docker Compose для запуска готовых артефактов
- PostgreSQL для хранения состояния панели
- Entity Framework Core как ORM
- Npgsql для подключения к PostgreSQL
- OpenAPI / Swagger для документации API

## Основной режим работы

Приложение работает автономно на сервере и предоставляет полный
набор функций локального управления:

- локальный REST API;
- веб-интерфейс администратора;
- управление нодами;
- управление пользователями и доступами;
- управление inbound/outbound;
- управление сертификатами и ключами;
- просмотр логов, статуса и диагностической информации.

## Планируемая архитектура

```text
XRayne.Api/             REST API и веб-хост
XRayne.Cli/             CLI для администрирования
XRayne.Infrastructure/  xray-core runtime, service implementations, background tasks, runtime utilities
XRayne.Repositories/    EF Core, PostgreSQL repositories, migrations, external clients
XRayne.Contracts/       DTO, configuration contracts, enums, shared values
XRayne.Dashboard/       React-интерфейс
XRayne.Test/            unit- и integration-тесты
```

## Работа с базой данных

Layer `XRayne.Repositories` contains `AppDbContext`, EF Core repositories,
migrations, PostgreSQL setup, and external clients such as `GitHubRepository`.

Строка подключения задается в секции `ConnectionStrings`:

```json
{
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Port=5432;Database=xrayne;Username=xrayne;Password=xrayne"
  }
}
```

Базовые команды для миграций:

```bash
dotnet ef migrations add InitialCreate \
  --project XRayne.Repositories \
  --startup-project XRayne.Api \
  --context AppDbContext

dotnet ef database update \
  --project XRayne.Repositories \
  --startup-project XRayne.Api \
  --context AppDbContext
```

## Основные модули

### Управление Xray Core

Модуль отвечает за операции над процессом `xray-core`:

- поиск установленного бинарного файла;
- проверка версии;
- запуск и остановка процесса;
- перезапуск после изменения конфигурации;
- проверка состояния;
- валидация конфигурации перед применением;
- чтение логов;
- интеграция с systemd.

### Управление конфигурацией

Модуль отвечает за создание, хранение и применение конфигурации Xray:

- генерация JSON-конфигурации;
- управление inbound;
- управление outbound;
- управление routing rules;
- управление пользователями и клиентами;
- резервное копирование конфигурации;
- откат к предыдущей рабочей версии;
- проверка конфигурации перед перезапуском ядра.

### Управление сертификатами и ключами

Модуль должен покрывать генерацию и хранение материалов, необходимых для Xray:

- самоподписанные SSL-сертификаты;
- private/public keys;
- ключи Reality;
- ключи TLS;
- UUID для клиентов;
- shortId для Reality;
- X25519 key pair;
- экспорт и ротация ключей;
- безопасное хранение секретов.

### REST API

API должно предоставлять методы для:

- авторизации администратора;
- получения статуса сервера и `xray-core`;
- управления `xray-core`;
- управления конфигурацией;
- управления пользователями и клиентами;
- генерации сертификатов и ключей;
- просмотра логов;
- просмотра метрик.

### Управление нодами

Панель управляет нодами как доменной сущностью, но не содержит отдельный
исходный проект сервиса ноды. Backend отвечает за:

- CRUD записей нод;
- provisioning удаленного хоста;
- хранение защищенных секретов подключения;
- reconnect и проверку доступности;
- streaming статуса установки и подключения.

### CLI

CLI должно позволять выполнять основные операции из терминала:

```bash
xrayne version
xrayne api install --version latest
xrayne status
xrayne start
xrayne stop
xrayne restart
xrayne config validate
xrayne config apply
xrayne cert generate
xrayne keys reality
```

### Web UI

Интерфейс должен быть построен по аналогии с 3x-ui, но с собственной архитектурой
и дизайном:

- dashboard со статусом сервера;
- управление нодами;
- управление inbound;
- управление клиентами;
- управление сертификатами и ключами;
- просмотр логов;
- настройки панели;
- страницы авторизации;
- индикаторы состояния Xray и системных сервисов.

## Этапы разработки

### Этап 1. Базовая структура проекта

- Создать .NET solution.
- Добавить проекты `XRayne.Api`, `XRayne.Cli`,
  `XRayne.Infrastructure`, `XRayne.Contracts`.
- Добавить React-приложение в `XRayne.Dashboard`.
- Настроить форматирование, базовые скрипты запуска и README.
- Подключить Swagger для API.

### Этап 2. Управление Xray Core

- Реализовать сервис управления процессом Xray.
- Добавить команды CLI для `status`, `start`, `stop`, `restart`.
- Добавить REST endpoints для управления ядром.
- Реализовать проверку установленного `xray-core`.
- Добавить чтение логов.

### Этап 3. Конфигурация Xray

- Описать модели конфигурации.
- Реализовать генерацию базового Xray config.
- Реализовать валидацию конфигурации.
- Добавить применение конфигурации с backup и rollback.
- Добавить API и CLI для работы с конфигурацией.

### Этап 4. Сертификаты и ключи

- Реализовать генерацию UUID.
- Реализовать генерацию Reality keys и shortId.
- Реализовать генерацию X25519 key pair.
- Реализовать генерацию самоподписанных SSL-сертификатов.
- Добавить безопасное хранение и экспорт ключевых материалов.
- Добавить API, CLI и UI для этих операций.

### Этап 5. Панель управления

- Добавить локальную авторизацию администратора.
- Добавить хранение пользователей, клиентов и настроек.
- Добавить полноценные CRUD endpoints.
- Реализовать dashboard.
- Реализовать управление inbound и клиентами через UI.

### Этап 6. UI

- Настроить React, TypeScript, TanStack Query и Axios.
- Создать API-клиент.
- Реализовать layout панели.
- Реализовать страницы dashboard, inbound, clients, certificates, logs,
  settings.
- Добавить обработку загрузки, ошибок и пустых состояний.
- Добавить базовую защиту маршрутов.

### Этап 7. Тестирование и упаковка

- Добавить unit-тесты доменной логики.
- Добавить integration-тесты API.
- Добавить тесты генерации конфигураций.
- Подготовить systemd unit.
- Подготовить Docker image API + UI и docker-compose для запуска готового образа.
- Подготовить инструкции установки.

## Предварительный список API endpoints

```text
GET    /api/core/status
POST   /api/xray/start
POST   /api/xray/stop
POST   /api/xray/restart
GET    /api/xray/status
GET    /api/xray/logs
GET    /api/config
POST   /api/config/validate
POST   /api/config/apply
GET    /api/inbounds
POST   /api/inbounds
PUT    /api/inbounds/{id}
DELETE /api/inbounds/{id}
GET    /api/clients
POST   /api/clients
PUT    /api/clients/{id}
DELETE /api/clients/{id}
POST   /api/certificates/self-signed
POST   /api/keys/reality
POST   /api/keys/x25519
POST   /api/keys/uuid
```

## Ближайшие задачи

1. Инициализировать .NET solution и базовые проекты.
2. Инициализировать React-приложение.
3. Описать доменные интерфейсы для управления Xray.
4. Реализовать первый REST endpoint `GET /api/core/status`.
5. Реализовать первую CLI-команду `xrayne status`.
6. Подготовить минимальный dashboard в UI.

## Статус

Проект находится на этапе планирования и первичной подготовки архитектуры.
