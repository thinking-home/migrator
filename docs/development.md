# Разработка

## Как собрать проект

## Как запустить тесты

Запуск mssql для тестов

```sh
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=x987(!)654' -p 1433:1433 microsoft/mssql-server-linux
```

## Собственные провайдеры трансформации