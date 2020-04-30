#!/usr/bin/env bash

sqlplus /nolog <<EOF
connect SYS/oracle AS SYSDBA;

create user TEST IDENTIFIED BY 123;
grant all privileges to TEST;

create user MOO IDENTIFIED BY 123;
grant all privileges to MOO;
EOF



