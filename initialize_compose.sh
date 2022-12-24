#!/usr/bin/env bash

# This script copies web server client files into each server's docker volume.
# This way each server has a seperate volume, so we can modify one server's files without
# affecting others.
sudo docker compose down --volumes
sudo docker compose up -d
for (( i = 1; i <= 10; i++ )); do
    sudo docker compose cp ./TodoClient/. "server-nginx-$i:/var/www/todo-app"
done
sudo docker compose down
