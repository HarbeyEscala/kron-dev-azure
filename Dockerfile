# BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

COPY . .

RUN dotnet restore Kron.Counting.API/Kron.Counting.API.csproj

RUN dotnet publish \
    Kron.Counting.API/Kron.Counting.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# RUNTIME STAGE
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Kron.Counting.API.dll"]