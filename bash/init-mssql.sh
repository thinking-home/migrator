#!/usr/bin/env bash

/opt/mssql-tools/bin/sqlcmd -U sa -P $SA_PASSWORD -Q "SELECT @@VERSION"

/opt/mssql-tools/bin/sqlcmd -U sa -P $SA_PASSWORD -Q "CREATE DATABASE migrations"

/opt/mssql-tools/bin/sqlcmd -U sa -P $SA_PASSWORD -Q "use migrations;exec sp_executesql N'CREATE SCHEMA Moo'"
