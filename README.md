Server:
cd server
docker build -t server .
docker run -p 5000:80 server

Client:
cd client
docker build -t client .
docker run client <arguments>

Valid arguments for client:
machines 
machines <platform>
add <ip> <name> <platform>
lease <platform> <minutes>
leases
help