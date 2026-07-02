# Ручная проверка: настройки панели

Запустить API + UI локально. Залогиниться super-admin'ом.

## Backend (через Scalar / curl)

- [ ] `GET /api/settings/panel` больше не опубликован.
- [ ] `POST /api/settings/panel/restart` без токена -> 401.
- [ ] `POST /api/settings/panel/restart` под обычным админом (без `super_admin`) -> 403.
- [ ] `POST /api/settings/panel/restart` под super-admin'ом -> 202.

## Frontend (страница /settings)

- [ ] Открыть `/settings` после логина: IP/domain/port/certs на странице отсутствуют.
- [ ] Поле времени жизни сессии на странице отсутствует.
- [ ] Кнопки сохранения/сброса формы отсутствуют.
- [ ] Нажать "Restart panel": кнопка показывает spinner -> при возврате API toast "Панель перезапущена".

## Frontend (статика)

- [ ] `cd XRayne.Dashboard && npm run build` собирается без ошибок.
- [ ] `npm run lint` без ошибок (если настроен).

## Доступ для CI

- [ ] `dotnet test XRayne.Test` - все тесты зелёные при доступном Docker/Testcontainers.
- [ ] Docker compose / systemd unit имеет `restart: unless-stopped` / `Restart=on-failure`, чтобы full-restart endpoint реально поднимал процесс заново.
