# Start RabbitMQ using Docker
# This script will start RabbitMQ in a Docker container

Write-Host "Starting RabbitMQ in Docker container..." -ForegroundColor Green

docker run -d `
  --name rabbitmq `
  -p 5672:5672 `
  -p 15672:15672 `
  -e RABBITMQ_DEFAULT_USER=guest `
  -e RABBITMQ_DEFAULT_PASS=guest `
  rabbitmq:3-management

Write-Host "RabbitMQ is starting..." -ForegroundColor Yellow
Write-Host "Management UI: http://localhost:15672" -ForegroundColor Cyan
Write-Host "AMQP Port: localhost:5672" -ForegroundColor Cyan
Write-Host "Username: guest" -ForegroundColor Cyan
Write-Host "Password: guest" -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop RabbitMQ: docker stop rabbitmq" -ForegroundColor Yellow
Write-Host "To remove RabbitMQ: docker rm rabbitmq" -ForegroundColor Yellow



