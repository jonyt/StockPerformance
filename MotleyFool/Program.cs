using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MotleyFool.FileWriters;

/***
 * A program to analyze the performance of stocks recommended by The Motley Fool http://www.fool.com
 * After harvesting recommendations we parse them, extract stock tickers, get historical data for the tickers and compare them to major indexes.
 * 
 * */

namespace MotleyFool
{
    class Program
    {
        static void Main(string[] args)
        {
            // Read and process all the documents (read HTML, extract text/date, extract tickers)
            string[] filePaths = Directory.GetFiles("dataFiles");
            IList<Document> documents = new List<Document>();
            foreach (var filepath in filePaths)
            {
                Document document = new Document(filepath);
                document.Process();
                if (document.Date != null && document.Tickers != null) documents.Add(document);
            }

            // We'll only extract ticker data for the latest date
            DateTime maxDate = documents.Max(document => document.Date);
            Console.WriteLine("Will extract data from {0}-{1}-{2} to today", maxDate.Year, maxDate.Month, maxDate.Day);
            // Get all tickers and add NASDAQ and S&P indexes for comparison
            IEnumerable<string> tickers = documents.Select(document => document.Tickers).
                SelectMany(allTickers => allTickers).Distinct().Concat(new string[] { "^GSPC", "^IXIC" });
            Console.WriteLine("Getting data for {0} tickers", tickers.Count());

            // Now get the ticker value from maxDate to today for every ticker
            IDictionary<string, IList<Tuple<DateTime, double>>> allTickerData = new Dictionary<string, IList<Tuple<DateTime, double>>>();
            foreach (var ticker in tickers)
            {
                StockTickerValues tickerDownloader = new StockTickerValues(maxDate, ticker);
                IList<Tuple<DateTime, double>> tickerValues = tickerDownloader.Download();
                if (tickerValues != null)
                {
                    allTickerData[ticker] = tickerValues;
                }
            }

            // Write the data to files. The first one has the raw data, the second has the data normalized.
            // For each ticker we normalize by its value at the beginning of the period
            using (FileWriterBase regularWriter = new RegularWriter(@"C:\Users\yytov\Desktop\ticker_data.tsv", allTickerData))
            using (FileWriterBase normalizedDataWriter = new NormalizedDataWriter(@"C:\Users\yytov\Desktop\ticker_data_normalized.tsv", allTickerData))
            {
                regularWriter.Write();
                normalizedDataWriter.Write();
            }

            Console.WriteLine("Processed {0} documents. Got data for {1} tickers. \r\nDone", documents.Count, tickers.Count());
            Console.ReadLine();
        }
    }
}
