using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MixedContentChecker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var url = Environment.GetEnvironmentVariable("SITE_MAP_URL");
            if (string.IsNullOrWhiteSpace(url))
            {
                if (args.Length == 0)
                {
                    throw new ArgumentOutOfRangeException($"Usage: {Assembly.GetExecutingAssembly().Location} URL");
                }
                else
                {
                    url = args[0];
                }
            }

            var urls = await GetHatenaBlogEntries(url);
            GetMixedContent(urls);
        }

        static async Task<string[]> GetHatenaBlogEntries(string url)
        {
            var client = new HttpClient();
            var res = await client.GetStringAsync(url);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var sitemaps = XElement.Parse(res).Descendants(ns + "loc").Select(x => x.Value).ToArray();
            var urls = await Task.WhenAll(sitemaps.Select(async x =>
            {
                var eachRes = await client.GetStringAsync(x);
                return XElement.Parse(eachRes).Descendants(ns + "loc").Select(y => y.Value).ToArray();
            }));
            var result = urls.SelectMany(x => x).ToArray();
            return result;
        }

        static void GetMixedContent(string[] urls)
        {
            var orgColor = Console.ForegroundColor;
            Directory.CreateDirectory("logs");
            var path = $"logs/headless_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.log";
            WriteHeader(path);

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");

            using (var chrome = new ChromeDriver(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), options))
            {
                foreach (var item in urls)
                {
                    Console.WriteLine($"checking {item}");
                    var manager = chrome.Manage();
                    manager.Timeouts().ImplicitWait = TimeSpan.FromMinutes(2);

                    chrome.Url = item;
                    var logs = manager.Logs.GetLog("browser");
                    var mixedContentLogs = logs.Where(x => Regex.IsMatch(x.Message, ".*Mixed Content:.*")).ToArray();
                    if (mixedContentLogs.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        foreach (var log in mixedContentLogs)
                        {
                            Console.WriteLine($"  > {mixedContentLogs.Length} MixedContent found.");
                            Console.WriteLine($"  * {log.Timestamp} ({log.Level}): {log.Message}");
                            WriteContent(path, item, log.Timestamp.ToString("s"), log.Level.ToString(), log.Message);
                        }
                        Console.ForegroundColor = orgColor;
                    }
                }
            }
        }

        static void WriteHeader(string path)
        {
            var header = @"url | timestamp | loglevel | log
---- | ---- | ---- | ----";
            File.WriteAllLines(path, new[] { header }, Encoding.UTF8);
        }

        static void WriteContent(string path, string url, string timestamp, string loglevel, string log)
        {
            var header = $@"{url} | {timestamp} | {loglevel} | {log}";
            File.AppendAllLines(path, new[] { header }, Encoding.UTF8);
        }
    }
}
