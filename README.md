# ThinkingHome.Migrator [![Travis](https://img.shields.io/travis/thinking-home/migrator.svg)](https://travis-ci.org/thinking-home/migrator) [![NuGet Pre Release](https://img.shields.io/nuget/vpre/ThinkingHome.Migrator.Framework.svg)](https://www.nuget.org/packages?q=thinkinghome.migrator)

Версионная миграция структуры БД для .NET Core.

Документация [здесь](https://github.com/dima117/ecm7migrator).

Запуск mssql для тестов

```sh
docker run -d -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=x987(!)654' -p 1433:1433 microsoft/mssql-server-linux
```

Запуск mysql для тестов

```sh
docker run --name=mysql1 -d -p 3306:3306\
    -e 'MYSQL_ROOT_HOST=%'\
    -e 'MYSQL_ROOT_PASSWORD=123'\
    -e 'MYSQL_DATABASE=migrations'\
    -e 'MYSQL_USER=migrator'\
    -e 'MYSQL_PASSWORD=123'\
    mysql/mysql-server
```

Запуск PostgreSQL для тестов

```sh
docker run --name postgres -d -p 5432:5432\
    -e POSTGRES_PASSWORD=123\
    -e POSTGRES_USER=migrator\
    -e POSTGRES_DB=migrations\
    postgres
```