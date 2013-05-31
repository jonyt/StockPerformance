using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            // Find the lastest date for which a ticker was recommended. 
            IDictionary<string, DateTime> ticker2MaxDate = new Dictionary<string, DateTime>();
            foreach (var doc in documents)
            {
                foreach (var ticker in doc.Tickers)
	            {
                    if (!ticker2MaxDate.ContainsKey(ticker) || ticker2MaxDate[ticker] > doc.Date)
                    {
                        ticker2MaxDate[ticker] = doc.Date;
                    }
	            }                
            }

            // We'll only extract ticker data for the latest date
            DateTime maxDate = ticker2MaxDate.Max(kvp => kvp.Value);

            // Now get the ticker value from maxDate to today for every ticker
            IDictionary<string, IList<Tuple<DateTime, double>>> allTickerData = new Dictionary<string, IList<Tuple<DateTime, double>>>();
            IEnumerable<string> tickers = ticker2MaxDate.Keys.Concat(new string[] { "^GSPC", "^IXIC" }); // Add NASDAQ and S&P indexes for comparison
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
            using (TextWriter writer = File.CreateText(@"C:\Users\yytov\Desktop\ticker_data.tsv"))
            using (TextWriter writerNormalized = File.CreateText(@"C:\Users\yytov\Desktop\ticker_data_normalized.tsv"))
            {
                foreach (var kvp in allTickerData)
                {
                    writer.WriteLine("{0}\t{1}", kvp.Key, String.Join("\t", kvp.Value.Select(tickerData => tickerData.Item2)));
                    double firstValue = kvp.Value.First().Item2;
                    writerNormalized.WriteLine("{0}\t{1}", kvp.Key, String.Join("\t", kvp.Value.Select(tickerData => tickerData.Item2 / firstValue)));
                }
                writer.Write("\t");
                writer.WriteLine(String.Join("\t", allTickerData.First().Value.Select(tickerData => String.Format("{0}-{1}", tickerData.Item1.Year, tickerData.Item1.Month.ToString("D2")))));
                writerNormalized.Write("\t");
                writerNormalized.WriteLine(String.Join("\t", allTickerData.First().Value.Select(tickerData => String.Format("{0}-{1}", tickerData.Item1.Year, tickerData.Item1.Month.ToString("D2")))));
            }


            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
