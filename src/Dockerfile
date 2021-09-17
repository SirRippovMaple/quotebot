FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

COPY *.sln .
COPY QuoteBot/*.csproj ./QuoteBot/
RUN dotnet restore --verbosity Normal

COPY QuoteBot/. ./QuoteBot/
WORKDIR /source/QuoteBot
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0

ENV DOTNET_EnableDiagnostics=0
WORKDIR /App
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "QuoteBot.dll"]
