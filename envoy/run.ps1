docker rm envoy-cosmos
docker run --name envoy-cosmos -p 8082:8082 -p 8083:8083 envoy-cosmos:0.1.0