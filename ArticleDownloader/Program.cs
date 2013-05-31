using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace ArticleDownloader
{
    class Program
    {
        private static IDictionary<string, bool> seen = new Dictionary<string, bool>();
        private const string DOWNLOADS_FOLDER = "downloads";
        private const int DOWNLOAD_LIMIT = 50;

        static void Main(string[] args)
        {
            string seed = "http://web.archive.org/web/20100101163446/http://www.fool.com/";
            int downloadCounter = 0;
            UniqueQueue<string> urlQueue = new UniqueQueue<string>();            
            WebClient client = new WebClient();

            if (!Directory.Exists(DOWNLOADS_FOLDER)) Directory.CreateDirectory(DOWNLOADS_FOLDER);

            urlQueue.Enqueue(seed);            
            while (urlQueue.Count > 0 && downloadCounter < DOWNLOAD_LIMIT)
            {
                string url = urlQueue.Dequeue();
                Console.WriteLine("Downloading from {0}", url);
                try
                {
                    string html = client.DownloadString(url);
                    string filename = (url.Split('/').Last().Length > 0 ? url.Split('/').Last() : DateTime.Now.Ticks.ToString() + ".html");
                    using (StreamWriter outfile = new StreamWriter(Path.Combine(DOWNLOADS_FOLDER, filename)))
                    {
                        outfile.Write(html);
                    }

                    IEnumerable<string> links = GetLinks(html);
                    foreach (var link in links)
                    {
                        urlQueue.Enqueue(link);
                    }
                    seen[url] = true;
                    downloadCounter++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("{0} URLs downloaded, {1} URLs remaining in queue", downloadCounter, urlQueue.Count);
            Console.ReadLine();
        }

        private static IEnumerable<string> GetLinks(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            if (htmlDoc.DocumentNode == null)
            {
                throw new Exception("Document has no root node");
            }

            IEnumerable<string> links = htmlDoc.DocumentNode.SelectNodes("//a").
                Where(link => link.HasAttributes && link.Attributes["href"] != null && link.Attributes["href"].Value.Contains("www.fool.com/investing") &&
                    link.Attributes["href"].Value.EndsWith("aspx") && link.Attributes["href"].Value.Split('-').Length > 2).
                Select(link => "http://web.archive.org" + link.Attributes["href"].Value).
                Distinct().
                Where(link => !seen.ContainsKey(link));

            Console.WriteLine("Found {0} links", links.Count());

            return links;
        }

        private class UniqueQueue<T>
        {
            private Queue<T> queue = new Queue<T>();

            public void Enqueue(T item)
            {
                if (!queue.Contains(item)) queue.Enqueue(item);
            }

            public T Dequeue()
            {
                return queue.Dequeue();
            }

            public int Count { get { return queue.Count; } }
        }
    }
}
