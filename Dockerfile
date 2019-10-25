FROM microsoft/dotnet:2.1-sdk as builder

COPY . .

RUN dotnet publish -r ubuntu-x64 ./DotnetMigrations -o ../app

FROM ubuntu:bionic

COPY --from=builder ./app /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT true

RUN apt update && apt install openssl libssl1.0.0 libssl-dev -y
