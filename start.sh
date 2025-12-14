#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Starting ThreeBoundedContext Microservices...${NC}"

# Step 1: Clean up previous containers and volumes
echo -e "\n${YELLOW}Step 1: Cleaning up previous containers and volumes...${NC}"
docker-compose down -v --remove-orphans 2>/dev/null || true
sleep 2

# Step 2: Create network
echo -e "\n${YELLOW}Step 2: Creating microservices network...${NC}"
docker network rm microservices-network 2>/dev/null || true
docker network create microservices-network

# Step 3: Build and start services
echo -e "\n${YELLOW}Step 3: Building and starting all services...${NC}"
docker-compose up -d

# Step 4: Wait for services to be healthy
echo -e "\n${YELLOW}Step 4: Waiting for services to be ready...${NC}"
sleep 10

# Step 5: Check service health
echo -e "\n${YELLOW}Step 5: Checking service health...${NC}"

echo -e "\n${BLUE}Checking SQL Server (Port 1433)...${NC}"
docker-compose logs sqlserver | tail -5

echo -e "\n${BLUE}Checking RabbitMQ (Port 5672, UI: 15672)...${NC}"
docker-compose logs rabbitmq | tail -5

echo -e "\n${BLUE}Service Status:${NC}"
docker-compose ps

# Step 6: Show endpoints
echo -e "\n${GREEN}✅ Services are starting!${NC}"
echo -e "\n${BLUE}📍 Service Endpoints:${NC}"
echo -e "  ${YELLOW}User Service API:${NC}      http://localhost:5001"
echo -e "  ${YELLOW}User Service gRPC:${NC}     http://localhost:5002"
echo -e "  ${YELLOW}Booking Service API:${NC}   http://localhost:5003"
echo -e "  ${YELLOW}Finance Service API:${NC}   http://localhost:5005"
echo -e "\n${BLUE}📊 Infrastructure:${NC}"
echo -e "  ${YELLOW}SQL Server:${NC}            localhost:1433"
echo -e "  ${YELLOW}RabbitMQ AMQP:${NC}        localhost:5672"
echo -e "  ${YELLOW}RabbitMQ Management:${NC}  http://localhost:15672"

echo -e "\n${BLUE}📖 Health Check Commands:${NC}"
echo -e "  ${YELLOW}User Service:${NC}         curl http://localhost:5001/health"
echo -e "  ${YELLOW}Booking Service:${NC}      curl http://localhost:5003/health"
echo -e "  ${YELLOW}Finance Service:${NC}      curl http://localhost:5005/health"

echo -e "\n${BLUE}📝 Logs Commands:${NC}"
echo -e "  ${YELLOW}View all logs:${NC}        docker-compose logs -f"
echo -e "  ${YELLOW}User Service logs:${NC}    docker-compose logs -f user-service"
echo -e "  ${YELLOW}Booking Service logs:${NC} docker-compose logs -f booking-service"
echo -e "  ${YELLOW}Finance Service logs:${NC} docker-compose logs -f finance-service"

echo -e "\n${GREEN}✨ Setup Complete!${NC}\n"
