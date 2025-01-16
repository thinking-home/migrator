#!/usr/bin/env bash

/opt/mssql-tools18/bin/sqlcmd -U sa -P $SA_PASSWORD -C -Q "SELECT @@VERSION"

/opt/mssql-tools18/bin/sqlcmd -U sa -P $SA_PASSWORD -C -Q "CREATE DATABASE migrations"

/opt/mssql-tools18/bin/sqlcmd -U sa -P $SA_PASSWORD -C -Q "use migrations;exec sp_executesql N'CREATE SCHEMA Moo'"
