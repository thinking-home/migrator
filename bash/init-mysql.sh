#!/usr/bin/env bash

mysql -e "CREATE SCHEMA migrations;"

mysql -e "CREATE SCHEMA Moo;"

mysql -e "CREATE USER 'migrator' IDENTIFIED BY '123';"

mysql -e "GRANT ALL PRIVILEGES ON *.* TO 'migrator';"
