# Ручная проверка: реактивные настройки панели

Запустить API + UI локально. Залогиниться super-admin'ом.

## Backend (через Scalar / curl)

- [ ] `GET /api/settings/panel` под Bearer токеном super-admin'а возвращает 200 и JSON с дефолтами + `fieldImpacts`.
- [ ] `GET /api/settings/panel` без токена -> 401.
- [ ] `GET /api/settings/panel` под токеном обычного админа (без `super_admin`) -> 403.
- [ ] `PUT /api/settings/panel` с `domain = "https://test.com"` -> `{ requiresRestart: false, ... }`.
- [ ] `PUT /api/settings/panel` с `port = 5099` -> `{ requiresRestart: true, ... }`. Повторный `GET` показывает `pendingRestart: true`.
- [ ] `PUT` с `port = -1` -> 400 с ошибкой валидации.
- [ ] `PUT` с `webBasePath = "no-slash"` -> 400.
- [ ] `PUT` с `trustedProxyCidrs = "not-a-cidr"` -> 400.
- [ ] `POST /api/settings/panel/restart` под super-admin'ом -> 202.

## Frontend (страница /settings)

- [ ] Открыть `/settings` после логина — видны три аккордеона (Панель, Сертификаты, Пути), дефолты подтянуты.
- [ ] Кнопка "Сохранить" disabled, пока форма pristine.
- [ ] Изменить любое поле — кнопка "Сохранить" активна, появляется жёлтый баннер "Требуется перезапуск".
- [ ] Изменить hot-reload поле (Domain), сохранить — toast "Сохранено. Изменения применены.", баннер исчезает.
- [ ] Изменить full-restart поле (Port), сохранить — toast "Сохранено. Изменения требуют перезапуска". Баннер остаётся.
- [ ] Refresh страницы при `pendingRestart: true` с сервера — баннер виден сразу.
- [ ] Поля с `FullRestart` impact показывают янтарный бейдж "Требует перезапуск".
- [ ] Поля с `HotReload` impact показывают зелёный бейдж "Применяется на лету".
- [ ] Невалидный порт (например 99999) — submit показывает ошибку под полем, API не вызывается.
- [ ] Невалидный `webBasePath` (без `/` в конце) — submit показывает ошибку.
- [ ] Нажать кнопку "Перезапуск панели" — confirm dialog -> подтвердить -> кнопка показывает spinner -> при возвращении API toast "Панель перезапущена".
- [ ] Кнопка "Перезапуск панели" видна всегда (даже без изменений в форме).

## Frontend (статика)

- [ ] `cd XRayne.Dashboard && npx vite build` — собирается без ошибок.
- [ ] `npm run lint` — без ошибок (если настроен).

## Доступ для CI

- [ ] `dotnet test XRayne.Test` — все тесты зелёные.
- [ ] Docker compose / systemd unit имеет `restart: unless-stopped` / `Restart=on-failure`, чтобы full-restart endpoint реально поднимал процесс заново.
