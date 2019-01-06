FROM mcr.microsoft.com/powershell:ubuntu-xenial
WORKDIR /root
RUN apt-get update \
    && apt-get install -y wget unzip
RUN apt install -y gdebi \
    && wget https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb \
    && gdebi -n google-chrome-stable_current_amd64.deb \
    && rm google-chrome-stable_current_amd64.deb
RUN wget https://chromedriver.storage.googleapis.com/2.45/chromedriver_linux64.zip \
    && unzip chromedriver_linux64.zip \
    && chmod +x ./chromedriver \
    && rm chromedriver_linux64.zip
RUN wget https://www.nuget.org/api/v2/package/Selenium.WebDriver/3.141.0 -O selenium.webdriver.zip \
    && unzip selenium.webdriver.zip -d webdriver \
    && cp webdriver/lib/netstandard2.0/WebDriver.dll . \
    && chmod +x ./WebDriver.dll \
    && rm -rf webdriver \
    && rm selenium.webdriver.zip
COPY Get-MixedContent.ps1 Get-MixedContent.ps1
COPY Get-SitemapUrl.ps1 Get-SitemapUrl.ps1

# pwsh -File ./Get-MixedContent.ps1  -Url https://tech.guitarrapc.com