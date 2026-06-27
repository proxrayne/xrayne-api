# XRayne.Node

XRayne.Node - нода и панель управления для работы с `xray-core`. Проект должен
предоставлять CLI, REST API и веб-интерфейс для управления Xray, подключения ноды
к удаленной VPN-сети и автономной работы в режиме отдельной панели по аналогии с
3x-ui.

## Цели проекта

- Управление жизненным циклом `xray-core`: запуск, остановка, перезапуск,
  проверка статуса, обновление конфигурации и просмотр логов.
- Работа в двух режимах:
  - удаленная нода в составе VPN-сети;
  - самостоятельная панель управления на отдельном сервере.
- Генерация SSL-сертификатов, TLS-ключей и других ключевых материалов,
  необходимых для настройки Xray.
- Предоставление удобного REST API для интеграции с внешними системами.
- Предоставление CLI для администрирования без веб-интерфейса.
- Предоставление React-интерфейса для визуального управления сервером,
  пользователями, inbound/outbound-конфигурациями и состоянием ноды.

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

## Основные режимы работы

### Режим удаленной ноды

В этом режиме XRayne.Node подключается к центральной панели или управляющему API
и выполняет команды удаленно:

- регистрация ноды в сети;
- синхронизация конфигурации;
- применение изменений Xray;
- отправка статуса и метрик;
- передача информации о трафике;
- прием команд запуска, остановки и перезапуска ядра.

### Режим отдельной панели

В этом режиме приложение работает автономно на сервере и предоставляет полный
набор функций локального управления:

- локальный REST API;
- веб-интерфейс администратора;
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
- получения статуса ноды;
- управления `xray-core`;
- управления конфигурацией;
- управления пользователями и клиентами;
- генерации сертификатов и ключей;
- просмотра логов;
- просмотра метрик;
- регистрации удаленной ноды;
- синхронизации с центральной панелью.

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
xrayne node register
```

### Web UI

Интерфейс должен быть построен по аналогии с 3x-ui, но с собственной архитектурой
и дизайном:

- dashboard со статусом сервера;
- управление inbound;
- управление клиентами;
- управление сертификатами и ключами;
- просмотр логов;
- настройки ноды;
- настройки подключения к центральной панели;
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

### Этап 5. Режим удаленной ноды

- Добавить модель регистрации ноды.
- Реализовать токены или ключи авторизации ноды.
- Реализовать синхронизацию конфигурации с центральной панелью.
- Реализовать heartbeat и health-check.
- Реализовать отправку метрик и статуса.
- Реализовать прием удаленных команд.

### Этап 6. Режим отдельной панели

- Добавить локальную авторизацию администратора.
- Добавить хранение пользователей, клиентов и настроек.
- Добавить полноценные CRUD endpoints.
- Реализовать dashboard.
- Реализовать управление inbound и клиентами через UI.

### Этап 7. UI

- Настроить React, TypeScript, TanStack Query и Axios.
- Создать API-клиент.
- Реализовать layout панели.
- Реализовать страницы dashboard, inbound, clients, certificates, logs,
  settings.
- Добавить обработку загрузки, ошибок и пустых состояний.
- Добавить базовую защиту маршрутов.

### Этап 8. Тестирование и упаковка

- Добавить unit-тесты доменной логики.
- Добавить integration-тесты API.
- Добавить тесты генерации конфигураций.
- Подготовить systemd unit.
- Подготовить Docker image API + UI и docker-compose для запуска готового образа.
- Подготовить инструкции установки.

## Предварительный список API endpoints

```text
GET    /api/node/status
POST   /api/node/register
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
4. Реализовать первый REST endpoint `GET /api/node/status`.
5. Реализовать первую CLI-команду `xrayne status`.
6. Подготовить минимальный dashboard в UI.

## Статус

Проект находится на этапе планирования и первичной подготовки архитектуры.
