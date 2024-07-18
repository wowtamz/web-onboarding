#!/bin/bash

if [ -d "./Migrations" ]; then
    rm -rf ./Migrations
fi

dotnet ef migrations add InitialCreate --context ApplicationDbContext

echo "Create migration"

while [ ! -d "./Migrations" ]; do
    sleep 10
done

echo "Migration created"


if [ -f "./data/database.db" ]; then
    rm -f ./data/database.db
fi

echo "Current dir: $(pwd)"
echo "Update database"

# Migration anwenden
dotnet ef database update --context ApplicationDbContext

# While database.db does not exist, wait
while [ ! -f "./data/database.db" ]; do
    sleep 10
done

echo "Database created"


dotnet clean
dotnet restore
dotnet watch

