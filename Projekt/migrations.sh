#!/bin/bash

cd ./SoPro24Team06

dotnet tool install --global dotnet-ef --version 7.0.5
export PATH="$PATH:$HOME/.dotnet/tools"

echo "Current dir: $(pwd)"

dotnet ef migrations add InitialCreate --context ApplicationDbContext

echo "Create migration"

while [ ! -d "./Migrations" ]; do
    sleep 10
done

echo "Current dir: $(pwd)"

# Migration anwenden
dotnet ef database update --context ApplicationDbContext

dotnet clean 
dotnet restore

# go to prevous directory
cd .. 

exit 0