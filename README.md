# DDoS to Webpage

This project's purpose is to demonstrate a DDoS attack and then try to come up with
defense techniques to mitigate the effects of the attack.

## About the application

TodoApi is a CRUD application in which you can add tasks and edit their content and done status.
All changes are saved to a PostgreSQL database.

All services run as Docker containers. They are defined using the Docker Compose version 2 file format.
Services are listed below:
* **app**: Basic CRUD web API written in .NET 6.0.
* **postgres**: PostgreSQL database.
* **server-1 to server-4**: Apache2 web servers serving static web pages.
* **proxy-nginx**: Nginx reverse proxy server which redirects incoming requests to upstream web servers.

## Requirements

* [Docker engine](https://docs.docker.com/engine/install/)
* [Python Slowloris DDoS tool](https://github.com/gkbrk/slowloris)
* ApacheBench load testing tool

    ```sh
    sudo apt install apache2-utils
    ```
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

## Server Configurations

Use Docker Compose file version 2 to set resources of each service in non-swarm mode.

* **Note:** cpu_shares, cpu_quota, cpuset, mem_limit, memswap_limit: These have been replaced by the `resources` key under
`deploy` in compose file version 3. `deploy` configuration only takes effect when using `docker stack deploy`, *and is
ignored by docker-compose*.

* **Note:** Nginx reverse proxy caches and buffers responses from upstream servers by default. This can prevent upstream
server DDoS attacks because each request does n

## DDoS Instructions

* **Note:** 
    [Nginx round robin load balancing is not as expected](https://stackoverflow.com/questions/55257595/nginx-round-robin-load-balancing-is-not-as-expected)

    You cannot always see the load balancing action by looking at the server number in the webpage header. Because
    there are total of 3 files (index.html, index.js, styles.css) in the client and each of them cause GET request. You can
    only see the server number which serves the GET request of index.html file.

* **Note:** If you use Nginx as a load balancer, all traffic goes through Nginx and consumes its bandwidth. This way you
cannot exhaust upstream servers by opening more connections. However you can exhaust servers by sending more requests.

### Slowloris Attack


* **Note:** You must change ulimit in Linux to open more sockets than the default soft limit of 1021.
    * [Maximum number of sockets get limited to 1021](https://github.com/gkbrk/slowloris/issues/17)
    * [Change ulimit in Linux](https://unix.stackexchange.com/a/31728)


* Start the Slowloris from attacker machine.
    
    ```sh
    python slowloris.py -p 80 -s 1024 --sleeptime 1 10.42.0.1
    ```
* If `worker_connections` parameter of Nginx is lower than the Slowloris socket count, Nginx cannot
    respond to legit requests so you cannot access the webpage. Load balancing between web servers does
    not solve the problem because reverse proxy is down.


## Useful Links

* [Docker Compose file version 2 reference](https://docs.docker.com/compose/compose-file/compose-file-v2/#cpu-and-other-resources)
* [How to setup an Nginx load balancer?](https://www.theserverside.com/blog/Coffee-Talk-Java-News-Stories-and-Opinions/How-to-setup-an-Nginx-load-balancer-example)
* [Nginx load balancing reference](https://docs.nginx.com/nginx/admin-guide/load-balancer/http-load-balancer/)
* [Nginx http_proxy reference](https://nginx.org/en/docs/http/ngx_http_proxy_module.html#proxy_buffering)
* [DDoS Apache servers from a single machine](https://medium.com/@brannondorsey/d%CC%B6dos-apache-servers-from-a-single-machine-f23e91f5d28)

