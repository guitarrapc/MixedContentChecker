#Requires -Version 6.0
Set-StrictMode -Version Latest
function Get-SitemapUrl {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url
    )

    $res = Invoke-Webrequest "$Url/sitemap.xml"
    if ($res.StatusCode -ne 200) {
        throw $res
    }

    [xml]$index = $res.Content
    $sitemaps = $index.sitemapindex.sitemap.loc
    $urls = $sitemaps | Foreach-Object {
        $eachRes = Invoke-Webrequest $_
        [xml]$page = $eachRes.Content
        $page.urlset.url.loc
    }
    Write-Output $urls
}
