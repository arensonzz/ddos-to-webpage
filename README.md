# DDoS to Webpage

This project's purpose is to demonstrate a DDoS attack and then try to come up with
defense techniques to mitigate the effects of the attack.

## About the application

TodoApi is a CRUD application in which you can add tasks and edit their content and done status.
All changes are saved to a PostgreSQL database.

All services run as Docker containers. They are defined using the Docker Compose version 2 file format.
Services are seperated into two machines.

First machine (server machine) runs the `docker-compose-server.yml` file.
* **app**: Basic CRUD web API written in .NET 6.0.
* **postgres**: PostgreSQL database.
* **server-1 to server-4**: Apache2 (httpd) web servers serving static web pages.

Second machine (reverse proxy machine) runs the `docker-compose-reverse-proxy.yml` file.
* **proxy-nginx**: Nginx reverse proxy server which redirects incoming requests to upstream web servers.

## Requirements

* [Docker engine](https://docs.docker.com/engine/install/)
* [Python Slowloris DDoS tool](https://github.com/gkbrk/slowloris)
* [XerXes DoS tool](https://github.com/CyberXCodder/XerXes)
* ApacheBench load testing tool

    ```sh
    sudo apt install apache2-utils
    ```
* Web browser

## Running application locally

1. Server Machine
    1. Change URL of the API in `./TodoClient/index.js` according to machine's IP.
        ```js
        const todoApiUrl = "http://<machine_ip>:5122/api/TodoItems";
        ```

    1. Copy docker compose file to root.
        ```sh
        cp ./docker/docker-compose-server.yml docker-compose.yml
        ```

    1. Initialize docker compose with the script.
        ```sh
        bash initialize_compose.sh
        ```

    1. Run the application.
        ```sh
        sudo docker compose run -d
        ```

1. Reverse Proxy Machine
    1. Copy docker compose file to root.
        ```sh
        cp ./docker/docker-compose-reverse-proxy.yml docker-compose.yml
        ```

    1. Run the application.
        ```sh
        sudo docker compose run -d
        ```


1. You can access the application:

    * Go to `http://<reverse_proxy_machine_ip>` to view the webpage.
    * Go to `http://<server_machine_ip>:5122/api/todoitems` to access the API.
    * Go to following pages to access each server.
        ```
        http://<server_machine_ip>:8080
        http://<server_machine_ip>:8081
        http://<server_machine_ip>:8082
        http://<server_machine_ip>:8083
        ```
    * Use `psql` CLI utility if you want to access the database directly
        ```sh
        psql -h <server_machine_ip> -p 5432 -U postgres -d postgres -W
        # Enter the password when prompted
        # Password: 0000
        ```

## Reverse Proxy Configurations

* **Note:** Nginx reverse proxy caches responses from upstream servers by default. This can prevent upstream
server DDoS attacks because not every request reach the upstream server.

### `proxy-nginx.conf`

* [Core functionality](https://nginx.org/en/docs/ngx_core_module.html)
* [HTTP upstream module](https://nginx.org/en/docs/http/ngx_http_upstream_module.html)
* [`worker_processes`](https://nginx.org/en/docs/ngx_core_module.html#worker_processes): Defines the number of worker processes.
* [`worker_connections`](https://nginx.org/en/docs/ngx_core_module.html#worker_connections): Sets the maximum number of simultaneous connections that can be opened by a worker process.
* [`proxy_cache`](https://nginx.org/en/docs/http/ngx_http_proxy_module.html#proxy_cache): Defines a shared memory zone used for caching.
* [`limit_conn`](https://nginx.org/en/docs/http/ngx_http_limit_conn_module.html#limit_conn): Sets the shared memory zone and the maximum allowed number of connections for a given key value. When
    this limit is exceeded, the server will return the error in reply to a request.
* [`limit_req_zone`](https://nginx.org/en/docs/http/ngx_http_limit_req_module.html#limit_req): Sets the shared memory zone and the maximum burst size of requests. If the requests rate exceeds the
    rate configured for a zone, their processing is delayed such that requests are processed at a defined rate. Excessive
    requests are delayed until their number exceeds the maximum burst size in which case the request is terminated with an
    error.

## DDoS Instructions

* **Note:** 
    [Nginx round robin load balancing is not as expected](https://stackoverflow.com/questions/55257595/nginx-round-robin-load-balancing-is-not-as-expected)

    You cannot always see the load balancing action by looking at the server number in the webpage header. Because
    there are total of 3 files (index.html, index.js, styles.css) in the client and each of them cause GET request. You can
    only see the server number which serves the GET request of index.html file.

* **Note:** If you use Nginx as a load balancer, all traffic goes through Nginx and consumes its bandwidth. This way you
cannot exhaust upstream servers by opening more connections. However you can exhaust servers by sending more requests.

### Slowloris Attack

Default Nginx configuration is vulnerable to Slowloris attack. Scarce resource is the maximum number of simultaneous
    worker connections. This number can be calculated as `worker_connections * worker_processes` and equals to 512 in
    default nginx configuration. So, it is quite easy to take down unprotected Nginx.

* **Note:** You must change ulimit in Linux to open more sockets than the default soft limit of 1021.

    ```sh
    ulimit -n 65536
    ```
    * [Maximum number of sockets get limited to 1021](https://github.com/gkbrk/slowloris/issues/17)
    * [Change ulimit in Linux](https://unix.stackexchange.com/a/31728)


* Start the Slowloris from attacker machine.
    
    ```sh
    python slowloris.py -p 80 -s 3000 --sleeptime 1 <victim_ip>
    ```

* SlowLoris will flood a server with connections. In our example if SlowLoris sends 3000 connections per second, and
    Nginx can only handle 2048 connections per second, Nginx cannot
    respond to legit requests so you cannot access the webpage. Load balancing between web servers does
    not solve the problem because reverse proxy is down.

* Now change `proxy-nginx.conf` to mitigate the DDoS attack.
    * Uncomment `limit_conn two 10;` to limit maximum allowed number of connections for a single IP.
    * Uncomment `limit_req zone=one burst=100;` to limit allowed request rate per second.
    * Uncomment `client_body_timeout 1s;`
    * Uncomment `client_header_timeout 1s;`

* You can also increase the `worker_connections` to be able to handle more connections.

### XerXes Attack

* Start by only one server defined in the upstream section of `proxy-nginx.conf`.
    ```
    upstream server_cluster {
        server <server_machine_ip>:8080 weight=1;
        # server <server_machine_ip>:8081 weight=1;
        # server <server_machine_ip>:8082 weight=1;
        # server <server_machine_ip>:8083 weight=1;
        keepalive 32;
    }
    ```

* You can change number of CONNECTIONS and THREADS to use in the attack by modifying `xerxes.c` source code.

    ```c
    #define CONNECTIONS 32
    #define THREADS 96
    ```

* Start the XerXes from attacker machine.

    ```sh
    ./xerxes <server_machine_ip> 8080
    ```

* Benchmark performance of server using ApacheBench.
    ```sh
    ab -t 30 -c 10 http://<reverse_proxy_machine_ip>/
    ```

* Now activate load balancing in the `proxy-nginx.conf`. Uncomment following lines:
    ```
    server <server_machine_ip>:8080 weight=1;
    server <server_machine_ip>:8081 weight=1;
    server <server_machine_ip>:8082 weight=1;
    ```

* Benchmark performance of server using ApacheBench again and compare the results.

## Useful Links

* [Docker Compose file version 2 reference](https://docs.docker.com/compose/compose-file/compose-file-v2/#cpu-and-other-resources)
* [How to setup an Nginx load balancer?](https://www.theserverside.com/blog/Coffee-Talk-Java-News-Stories-and-Opinions/How-to-setup-an-Nginx-load-balancer-example)
* [Nginx load balancing reference](https://docs.nginx.com/nginx/admin-guide/load-balancer/http-load-balancer/)
* [Nginx http_proxy reference](https://nginx.org/en/docs/http/ngx_http_proxy_module.html#proxy_buffering)
* [DDoS Apache servers from a single machine](https://medium.com/@brannondorsey/d%CC%B6dos-apache-servers-from-a-single-machine-f23e91f5d28)
* [Nginx DDoS attack prevention](https://inmediatum.com/en/blog/engineering/ddos-attacks-prevention-nginx/)

