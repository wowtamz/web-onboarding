#!/bin/bash

# Wait for the app to start
INTERVAL=5
TIMEOUT=120

echo "Waiting for the application to start..."

while ! curl -s http://localhost:7003 > /dev/null
do
    echo "Waiting for the application to start..."
    sleep $INTERVAL
    TIMEOUT=$((TIMEOUT - INTERVAL))
    if [ $TIMEOUT -le 0 ]; then
        echo "Timeout waiting for the application to start"
        exit 0
    fi
done