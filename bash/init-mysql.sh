#!/usr/bin/env bash

echo "create main schema"
mysql -e "CREATE SCHEMA migrations;"

echo "create alt schema"
mysql -e "CREATE SCHEMA Moo;"

echo "create user"
mysql -e "CREATE USER 'migrator' IDENTIFIED BY '123';"

echo "create alt user"
mysql -e "CREATE USER 'migrator'@'localhost' IDENTIFIED BY '123';"

echo "333"
mysql -e "GRANT ALL PRIVILEGES ON *.* TO 'migrator'@'%';"

echo "444"
mysql -e "GRANT ALL PRIVILEGES ON *.* TO 'migrator'@'localhost';"
