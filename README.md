# TodoApi

A CRUD application in which you can add tasks and edit their content and done status.
All changes are saved to a PostgreSQL database.

## Requirements

* [Docker engine](https://docs.docker.com/engine/install/)
* Web browser

## Running application locally

1. Install Docker images defined in the compose file.
    ```sh
    sudo docker compose pull
    ```

1. Run compose services.
    ```sh
    sudo docker compose up -d
    ```

1. You can access the application:
    * Go to `http://localhost` to view the webpage.
    * Go to `http://localhost:5122/api/todoitems` to access the API.
    * Use `psql` CLI utility if you want to access the database directly
        ```sh
        psql -h localhost -p 5432 -U postgres -d postgres -W
        # Enter the password when prompted
        # Password: 0000
        ```
