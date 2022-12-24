# TodoApi

A CRUD application in which you can add tasks and edit their content and done status.
All changes are saved to a PostgreSQL database.

## Requirements

* [Docker engine](https://docs.docker.com/engine/install/)
* Web browser

## Running application locally

1. Initialize docker compose with the script.

    ```sh
    bash initialize_compose.sh
    ```

1. Run the application.

    ```sh
    sudo docker compose run -d
    ```

1. You can monitor outputs of containers with:

    All containers:

    ```sh
    sudo docker compose logs -f
    ```

    One container:

    ```sh
    sudo docker compose logs <container-name> -f
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
