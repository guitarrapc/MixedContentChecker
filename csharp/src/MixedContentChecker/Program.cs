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
    internal class Program
    {
        private static string logPath;
        private static ConsoleColor orgColor;

        private static async Task Main(string[] args)
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

            // get sitemap
            var urls = await GetHatenaBlogEntries(url);

            // init
            Directory.CreateDirectory("logs");
            orgColor = Console.ForegroundColor;
            logPath = $"logs/headless_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.log";

            // run
            GetMixedContent(urls);
        }

        private static async Task<string[]> GetHatenaBlogEntries(string url)
        {
            var client = new HttpClient();
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            async Task<string[]> FetchSitemapAsync(string dest)
            {
                var content = await client.GetStringAsync(dest);
                return XElement.Parse(content).Descendants(ns + "loc").Select(x => x.Value).ToArray();
            }

            var sitemaps = await FetchSitemapAsync(url);
            var urls = await Task.WhenAll(sitemaps.Select(x => FetchSitemapAsync(x)));
            var result = urls.SelectMany(x => x).ToArray();
            return result;
        }

        private static void GetMixedContent(string[] urls)
        {
            // generate log file and header
            WriteHeader();

            // chrome option
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");

            // set parallelism
            // 1. not define -> set default value (10)
            // 2. invalid value -> sequential
            // 3. below 1 -> sequential
            // 4. others -> parallel
            var useParallel = Environment.GetEnvironmentVariable("COUNT") ?? "10";
            var isSequential = !int.TryParse(useParallel, out var parallelism) || parallelism <= 1;

            // run
            if (isSequential)
            {
                using (var chrome = new ChromeDriver(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), options))
                {
                    foreach (var url in urls)
                    {
                        RunDriver(chrome, url);
                    }
                }
            }
            else
            {
                Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = parallelism }, (url, state, i) =>
                {
                    using (var chrome = new ChromeDriver(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), options))
                    {
                        RunDriver(chrome, url);
                    }
                });
            }
        }

        private static void RunDriver(ChromeDriver chrome, string url)
        {
            Console.WriteLine($"checking {url}");
            var manager = chrome.Manage();
            manager.Timeouts().ImplicitWait = TimeSpan.FromMinutes(2);

            chrome.Url = url;
            var logs = manager.Logs.GetLog("browser");
            var mixedContentLogs = logs.Where(x => Regex.IsMatch(x.Message, ".*Mixed Content:.*")).ToArray();
            if (mixedContentLogs.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var log in mixedContentLogs)
                {
                    Console.WriteLine($"  > {mixedContentLogs.Length} MixedContent found.");
                    Console.WriteLine($"  * {log.Timestamp} ({log.Level}): {log.Message}");
                    WriteContent(url, log.Timestamp.ToString("s"), log.Level.ToString(), log.Message);
                }
                Console.ForegroundColor = orgColor;
            }
        }

        private static void WriteHeader()
        {
            var header = @"url | timestamp | loglevel | log
---- | ---- | ---- | ----";
            File.WriteAllLines(logPath, new[] { header }, Encoding.UTF8);
        }
        private static void WriteContent(string url, string timestamp, string loglevel, string log)
        {
            var line = $@"{url} | {timestamp} | {loglevel} | {log}";
            File.AppendAllLines(logPath, new[] { line }, Encoding.UTF8);
        }
    }
}
