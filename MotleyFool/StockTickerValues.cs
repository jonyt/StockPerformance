using System;
using System.Collections.Generic;
using System.Linq;
using MaasOne.Base;
using MaasOne.Finance;
using MaasOne.Finance.YahooFinance;

/**
 * Extract historical data for a stock ticker.
 * 
 * */

namespace MotleyFool
{
    public class StockTickerValues
    {
        private string ticker;
        private HistQuotesDownload downloader = new MaasOne.Finance.YahooFinance.HistQuotesDownload();
        private DateTime fromDate;

        public StockTickerValues(DateTime fromDate, string ticker)
        {
            this.fromDate = fromDate;
            this.ticker = ticker;
        }

        public IList<Tuple<DateTime, double>> Download()
        {
            Console.WriteLine("Extracting tickers from {0} till today, monthly", fromDate.Date);
            Console.WriteLine("Ticker: {0}", ticker);
            Response<HistQuotesResult> response = downloader.Download(ticker, fromDate, DateTime.Now, HistQuotesInterval.Monthly);

            if (response.Connection.State == MaasOne.Base.ConnectionState.Success)
            {
                HistQuotesResult result = response.Result;
                HistQuotesDataChain chain = result.Chains[0];
                IList<Tuple<DateTime, double>> tickerResults = new List<Tuple<DateTime, double>>();
                foreach (var item in chain)
                {
                    Tuple<DateTime, double> periodResult = new Tuple<DateTime, double>(item.TradingDate, item.Close);
                    tickerResults.Add(periodResult);
                }
                return tickerResults;
            }
            else
            {
                Console.WriteLine("WARNING: Error downloading stock ticker {0}. {1} ", ticker, response.Connection.State);
                return null;
            }
        }
    }
}
