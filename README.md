# Hardware Leaser
A service for hardware leasing and a CLI application to interact with it.

## Server:
### Build:
From the server directory:
```
docker build -t server .
```
### Run:
From the server directory:
```
docker run -p 5000:80 server
```

## Client:
### Build:
From the client directory:
```
docker build -t client .
```
### Run:
From the client directory:
```
docker run client

docker run client machines
docker run client machines <platform>
docker run client add <ip> <name> <platform>
docker run client lease <platform> <minutes>
docker run client leases
docker run client help
```
