using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {
        private static string baseUri = "https://en.wikipedia.org";
        private static Uri startUrl = new Uri("https://en.wikipedia.org/wiki/USS_Lexington_(CV-2)");

        //private static string baseUri = "https://golang.org";
        //private static Uri startUrl = new Uri("https://golang.org/");
        private static int successfulUrisCount;

        private static ConcurrentQueue<VisitedUri> consumerQueue = new ConcurrentQueue<VisitedUri>();
        private static ConcurrentBag<VisitedUri> allProcessedUris = new ConcurrentBag<VisitedUri>();

        private static int CRAWL_DEPTH = 1;
        private static int CHECK_TIMEOUT = 2000;

        private struct VisitedUri
        {
            public Uri Uri;
            public int depth;

            public override bool Equals(object obj)
            {
                if (!(obj is VisitedUri))
                    return false;

                var other = (VisitedUri) obj;

                return other.Uri.Equals(this.Uri);
            }

            public override int GetHashCode()
            {
                return Uri.GetHashCode();
            }
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("And awaaaaaaaay we go!");

            var timerCallback = new TimerCallback(CheckState);

            var _ = new Timer(timerCallback, null, CHECK_TIMEOUT, CHECK_TIMEOUT);

            ProcessEntry(startUrl, 0);

            while (true)
            {
                var dequeuedSuccesfully = consumerQueue.TryDequeue(out var processUri);

                if (dequeuedSuccesfully)
                {
                    var newTask = new Task(() => ProcessEntry(processUri.Uri, processUri.depth + 1));
                    newTask.Start();
                }
            }
        }

        private static void CheckState(object state)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.WriteLine(consumerQueue.IsEmpty
                ? $"******* QUEUE EMTPY crawled through {allProcessedUris.Count} pages"
                : "******* Checking queue");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Number of threads: {Process.GetCurrentProcess().Threads.Count}");

            Console.ResetColor();
        }

        private static void ProcessEntry(Uri url, int depth)
        {
            if (depth > CRAWL_DEPTH)
                return;

            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();

            try
            {
                doc = hw.Load(url);
                Interlocked.Increment(ref successfulUrisCount);

                var allLinks = FindAllLinksInPage(doc);
                EnqueueEntries(allLinks.ToList(), depth);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR could not load {url}: {e.Message}");
            }
        }

        private static object bagLock = new object();

        private static IEnumerable<string> FindAllLinksInPage(HtmlDocument doc)
        {
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                var linkHrefValue = link.Attributes.FirstOrDefault(x => x.Name.Equals("href"))?.Value;
                if (string.IsNullOrEmpty(linkHrefValue))
                    continue;

                if (!ValidateUrl(linkHrefValue))
                    continue;

                if (linkHrefValue[0] != '/')
                    linkHrefValue = linkHrefValue.Insert(0, "/");

                var foundUri = baseUri + linkHrefValue;

                yield return foundUri;
            }
        }

        private static void EnqueueEntries(List<string> urls, int depth)
        {
            foreach (var url in urls)
            {
                if (TryCreateEntry(out var entry, url, depth))
                    TryEnqueueEntry(entry);
            }
        }

        private static bool ValidateUrl(string linkHrefValue)
        {
            return !linkHrefValue.StartsWith("//") && !linkHrefValue.StartsWith("#") && !linkHrefValue.StartsWith("http") && !linkHrefValue.Contains("www") && !linkHrefValue.Contains(".com");
        }

        private static bool TryEnqueueEntry(VisitedUri entry)
        {
            lock (bagLock)
            {
                if (allProcessedUris.Contains(entry))
                    return false;
            }

            allProcessedUris.Add(entry);
            consumerQueue.Enqueue(entry);
            // Console.WriteLine(entry.Uri);

            return true;
        }

        private static bool TryCreateEntry(out VisitedUri entry, string foundUri, int depth)
        {
            try
            {
                entry = new VisitedUri
                {
                    Uri = new Uri(foundUri),
                    depth = depth
                };

            }
            catch (UriFormatException e)
            {
                Console.WriteLine($"Wrong URI format: {foundUri}");
                entry = new VisitedUri();
                return false;
            }
            
            return true;
        }
    }
}
