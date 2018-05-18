#!/usr/bin/env bash

psql -c "CREATE DATABASE migrations;" -U postgres

psql -c "CREATE USER migrator WITH PASSWORD '123';" -U postgres

psql -c 'CREATE SCHEMA "Moo" AUTHORIZATION migrator;' -U postgres -d migrations
