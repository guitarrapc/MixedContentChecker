## MixedContentChecker

## CSharp

```powershell
docker run -it --rm --mount type=bind,source="$($pwd.Path)/logs",target=/app/logs -e SITE_MAP_URL=https://tech.guitarrapc.com/sitemap.xml mixedcontentchecker
```

```powershell
docker run -it --rm --mount type=bind,source="${pwd}/logs",target=/app/logs -e SITE_MAP_URL=https://tech.guitarrapc.com/sitemap.xml mixedcontentchecker
```

## PowerShell



## Golang