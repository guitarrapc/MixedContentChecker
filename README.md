## MixedContentChecker

Https detect Mixed Content with http resources.
This tool will detect with chrome-headless browser + selenium.

Please pass sitemap.xml url to check around all your url entries.

## CSharp

You can run by directly w/dotnet-sdk or docker.

```powershell
cd csharp

# dotnet on Windows
dotnet clean
dotnet build
dotnet MixedContentCheker https://tech.guitarrapc.com/sitemap.xml
```

run docker w/powershell.

```powershell
cd powershell
docker build -t mixedcontentchecker .
docker run -it --rm --mount type=bind,source="$($pwd.Path)/logs",target=/app/logs -e SITE_MAP_URL=https://tech.guitarrapc.com/sitemap.xml mixedcontentchecker
```

run docker w/bash.

```bash
cd csharp
docker build -t mixedcontentchecker .
docker run -it --rm --mount type=bind,source="${pwd}/logs",target=/app/logs -e SITE_MAP_URL=https://tech.guitarrapc.com/sitemap.xml mixedcontentchecker
```

## PowerShell

Download Selenium Webdriver, chrcome-driver and google-chrome.

```powershell
cd powershell
pwsh -File ./Get-MixedContent.ps1 -Url https://tech.guitarrapc.com/sitemap.xml
```

You can run with docker.

```bash
cd powershell
docker build -t mixedcontentchecker_ps .
docker run -it --rm -e SITE_MAP_URL=https://tech.guitarrapc.com/sitemap.xml mixedcontentchecker_ps
```

## Golang

[TBD]