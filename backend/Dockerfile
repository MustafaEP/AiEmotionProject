# ---- Build aşaması ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve derle
COPY . .
RUN dotnet publish -c Release -o /app/out

# ---- Çalışma aşaması ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Build aşamasından çıkan dosyaları al
COPY --from=build /app/out ./

# Render PORT değişkeni üzerinden çalıştırır
CMD ["dotnet", "backend.dll"]
