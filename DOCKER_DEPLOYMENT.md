# Docker Deployment Guide

Bu rehber WatchSyncMovie Server uygulamasını Docker Hub'a yayınlamak ve deploy etmek için gerekli adımları içerir.

## Ön Gereksinimler

1. Docker Desktop yüklü olmalı
2. Docker Hub hesabınız olmalı
3. Docker Hub'da giriş yapmış olmalısınız

## 1. Docker Image Build Etme

### Windows için:
```cmd
build-docker.bat [docker-hub-kullanici-adi] [versiyon]
```

### Linux/Mac için:
```bash
chmod +x build-docker.sh
./build-docker.sh [docker-hub-kullanici-adi] [versiyon]
```

### Örnek:
```bash
./build-docker.sh meliksah v1.0.0
```

## 2. Manuel Build (Alternatif)

```bash
# Image build et
docker build -t your-username/watchsync-server:latest ./WatchSycnMovie.Server

# Docker Hub'a login
docker login

# Image'ı push et
docker push your-username/watchsync-server:latest
```

## 3. Docker Compose ile Çalıştırma

```bash
# Servisi başlat
docker-compose up -d

# Logları görüntüle
docker-compose logs -f

# Servisi durdur
docker-compose down
```

## 4. Manuel Docker Run

```bash
docker run -d \
  --name watchsync-server \
  -p 8080:80 \
  -v $(pwd)/videos:/app/wwwroot/videos \
  your-username/watchsync-server:latest
```

## 5. Environment Variables

Container çalıştırırken kullanabileceğiniz environment variables:

- `ASPNETCORE_ENVIRONMENT`: Production, Development, Staging
- `ASPNETCORE_URLS`: http://+:80 (default)

## 6. Port Mapping

- **Container Port**: 80
- **Host Port**: 8080 (örnekte, istediğiniz port'u kullanabilirsiniz)

## 7. Volume Mapping

Yüklenen video dosyalarının kalıcı olması için:
```bash
-v /host/path/videos:/app/wwwroot/videos
```

## 8. Production Deployment

### Cloud Providers için örnek komutlar:

#### AWS ECS/Fargate:
```bash
# ECR'a push
aws ecr get-login-password --region region | docker login --username AWS --password-stdin account.dkr.ecr.region.amazonaws.com
docker tag watchsync-server:latest account.dkr.ecr.region.amazonaws.com/watchsync-server:latest
docker push account.dkr.ecr.region.amazonaws.com/watchsync-server:latest
```

#### Google Cloud Run:
```bash
# Google Container Registry'e push
docker tag watchsync-server:latest gcr.io/project-id/watchsync-server:latest
docker push gcr.io/project-id/watchsync-server:latest
```

#### Azure Container Instances:
```bash
# Azure Container Registry'e push
docker tag watchsync-server:latest myregistry.azurecr.io/watchsync-server:latest
docker push myregistry.azurecr.io/watchsync-server:latest
```

## 9. Troubleshooting

### Container loglarını kontrol et:
```bash
docker logs container-id
```

### Container içine gir:
```bash
docker exec -it container-id bash
```

### Health check:
```bash
curl http://localhost:8080/health
```

## 10. Güvenlik Notları

1. Production'da HTTPS kullanın
2. Güvenli ortam değişkenleri kullanın
3. Container'ı root olmayan kullanıcı ile çalıştırın
4. Gereksiz portları kapatın
5. Volume permission'larını kontrol edin