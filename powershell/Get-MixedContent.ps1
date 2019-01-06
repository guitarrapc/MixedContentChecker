#!/usr/bin/env pwsh
#Requires -Version 6.0
param(
    [Parameter(Mandatory = $true)]
    [string]$Url
)

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
if ( -not (Test-Path -LiteralPath './WebDriver.dll')) {
    Write-Error 'missing WebDriver.dll'
    return
}
if ($IsWindows) {
    if (-not (Test-Path -LiteralPath './chromedriver.exe')) {
        Write-Error 'missing chromedriver'
        return
    }
}
if ( -not (Test-Path -LiteralPath './Get-SitemapUrl.ps1')) {
    Write-Error 'missing Get-SitemapUrl.ps1'
    return
}

# Add-Type
Add-Type -LiteralPath ./WebDriver.dll
# load script
. ./Get-SitemapUrl.ps1

$urls = Get-SitemapUrl -Url $Url
try {
    $options = [OpenQA.Selenium.Chrome.ChromeOptions]::new()
    $options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage", "log-level=3")
    $driver = [OpenQA.Selenium.Chrome.ChromeDriver]::new($here, $options)

    foreach ($url in $urls) {
        Write-Host ("checking $url") -ForegroundColor Green
        $driver.Url = $url

        $logs = $driver.Manage().Logs.GetLog('browser')
        $mixedContentLogs = $logs | Where-Object { $_.Message -like "*Mixed Content:*"}
        if (@($mixedContentLogs).Count -eq 0) {
            continue
        }
        else {
            Write-Warning ('{0} Mixed Content found.' -f @($mixedContentLogs).Count)
            $mixedContentLogs | Format-List
        }
    }
}
finally {
    $driver.Quit()
}
