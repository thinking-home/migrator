#!/usr/bin/env bash

echo "create main schema"
mysql -e "CREATE SCHEMA migrations;"

echo "create alt schema"
mysql -e "CREATE SCHEMA Moo;"

echo "create user"
mysql -e "CREATE USER 'migrator' IDENTIFIED BY '123';"

echo "create alt user"
mysql -e "CREATE USER 'migrator'@'localhost' IDENTIFIED BY '123';"

echo "grant privileges: user"
mysql -e "GRANT ALL PRIVILEGES ON *.* TO 'migrator'@'%';"

echo "grant privileges: alt user"
mysql -e "GRANT ALL PRIVILEGES ON *.* TO 'migrator'@'localhost';"

echo "sql_mode: strict"
mysql -e "SET GLOBAL sql_mode='STRICT_ALL_TABLES';"