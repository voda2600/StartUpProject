# Базовый образ для runtime (используем не-root пользователя для безопасности)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Создание не-root пользователя и установка прав
RUN adduser -u 1000 --disabled-password --gecos "" appuser && chown appuser /app
USER appuser

# Образ для сборки (с SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копирование и восстановление проекта (предполагаем, что .csproj в корне)
COPY ["AuthService.csproj", "."]
RUN dotnet restore "AuthService.csproj"

# Копирование всего проекта и сборка
COPY . .
WORKDIR "/src"
RUN dotnet build "AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Стадия публикации
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ (runtime)
FROM base AS final
WORKDIR /app

# Копирование опубликованных файлов
COPY --from=publish /app/publish .

# Указание точки входа
ENTRYPOINT ["dotnet", "AuthService.dll"]