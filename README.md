# ThinkingHome.Migrator [![Travis](https://img.shields.io/travis/thinking-home/migrator.svg)](https://travis-ci.org/thinking-home/migrator) [![NuGet Pre Release](https://img.shields.io/nuget/vpre/ThinkingHome.Migrator.Framework.svg)](https://www.nuget.org/packages?q=thinkinghome.migrator)

Версионная миграция структуры БД для .NET Core.

Документация [здесь](https://github.com/dima117/ecm7migrator).

Запуск mssql для тестов

```sh
docker run --name mssql -d -p 1433:1433\
    -e 'ACCEPT_EULA=Y'\
    -e 'SA_PASSWORD=x987(!)654'\
    -v $(pwd)/bash/init-mssql.sh:/init-mssql.sh\
    microsoft/mssql-server-linux

docker exec mssql /init-mssql.sh
```

Запуск mysql для тестов

```sh
docker run --name mysql1 -d -p 3306:3306\
    -e 'MYSQL_ROOT_HOST=%'\
    -e 'MYSQL_ALLOW_EMPTY_PASSWORD=true'\
    -v $(pwd)/bash/init-mysql.sh:/init-mysql.sh\
    mysql/mysql-server

docker exec mysql1 /init-mysql.sh
```

Запуск PostgreSQL для тестов

```sh
docker run --name postgres -d -p 5432:5432\
    -v $(pwd)/bash/init-postgres.sh:/init-postgres.sh\
    postgres

docker exec postgres /init-postgres.sh
```

### todo

- [ ] починить тесты mysql в travis
- [x] вынести инициализацию в sh, написать команды для локального запуска  всех БД в docker, использовать общую инициализацию для travis и локально
- [ ] перенести в проект документацию и актуализировать
- [ ] написать консольную утилиту
