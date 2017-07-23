#!/usr/bin/env bash

#exit if any command fails
set -e

dotnet restore

# Ideally we would use the 'dotnet test' command to test netcoreapp and net451 so restrict for now 
# but this currently doesn't work due to https://github.com/dotnet/cli/issues/3073 so restrict to netcoreapp

dotnet test ./ThinkingHome.Migrator.Tests -c Release -f netcoreapp2.0
