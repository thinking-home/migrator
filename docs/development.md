# Разработка

## Как собрать проект

Для сборки проекта вам нужно установить [.NET Core 2.1 SDK](https://www.microsoft.com/net/download).

```bash
git clone https://github.com/thinking-home/migrator.git

cd migrator

dotnet restore

dotnet build

```

## Как запустить тесты

Базовая логика мигратора и логика провайдеров трансформации покрыты интеграционными тестами. Для запуска тестов нужен доступ к работающим экземплярам СУБД. Текущие настройки тестов рассчитаны на запуск СУБД в Docker контейнерах.

### Запуск MS SQL Server для тестов

```sh
docker run --name mssql -d -p 1433:1433\
    -e 'ACCEPT_EULA=Y'\
    -e 'SA_PASSWORD=x987(!)654'\
    -v $(pwd)/bash/init-mssql.sh:/init-mssql.sh\
    microsoft/mssql-server-linux

docker exec mssql /init-mssql.sh
```

### Запуск MySql для тестов

```sh
docker run --name mysql1 -d -p 3306:3306\
    -e 'MYSQL_ROOT_HOST=%'\
    -e 'MYSQL_ALLOW_EMPTY_PASSWORD=true'\
    -v $(pwd)/bash/init-mysql.sh:/init-mysql.sh\
    mysql/mysql-server

docker exec mysql1 /init-mysql.sh
```

### Запуск PostgreSQL для тестов

```sh
docker run --name postgres -d -p 5432:5432\
    -v $(pwd)/bash/init-postgres.sh:/init-postgres.sh\
    postgres

docker exec postgres /init-postgres.sh
```

### Запуск тестов

После запуска всех нужных СУБД вы можете запустить тесты командой `dotnet test`:

```bash
dotnet test ./ThinkingHome.Migrator.Tests -c Release -f netcoreapp2.1
```

## Собственные провайдеры трансформации