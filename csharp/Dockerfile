FROM microsoft/dotnet:2.2-sdk-bionic AS build
WORKDIR /app

# restore
COPY src/MixedContentChecker/*.csproj ./MixedContentChecker/
WORKDIR /app/MixedContentChecker
RUN dotnet restore

# build
WORKDIR /app
COPY src/. /app/
WORKDIR /app
RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

# runtime
FROM microsoft/dotnet:2.2-runtime-bionic AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y wget
RUN apt install -y gdebi \
    && wget https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb \
    && gdebi -n google-chrome-stable_current_amd64.deb \
    && rm google-chrome-stable_current_amd64.deb
COPY --from=build /app/MixedContentChecker/out ./
CMD ["dotnet", "MixedContentChecker.dll"]