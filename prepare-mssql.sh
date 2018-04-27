#!/usr/bin/env bash

echo 12312313213213213213213213213213213213213213213

echo $SA_PASSWORD

/opt/mssql-tools/bin/sqlcmd -U sa -P "x987(!)654" -Q "SELECT @@VERSION"

/opt/mssql-tools/bin/sqlcmd -U sa -P "x987(!)654" -Q "CREATE DATABASE migrations"

/opt/mssql-tools/bin/sqlcmd -U sa -P "x987(!)654" -Q "use migrations;exec sp_executesql N'CREATE SCHEMA Moo'"
