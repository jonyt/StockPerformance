using System;
using System.IO;
using System.Linq;
using Calais;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

/**
 * Holds one article and processes it. Recommendation text and date is parsed from HTML and stock tickers are extracted using
 * OpenCalais' API.
 * 
 * */

namespace MotleyFool
{
    public class Document
    {
        private string filepath;
        
        public string Html { get; private set; }
        public HtmlDocument HtmlDoc { get; private set; }
        public string Text { get; private set; }
        public DateTime Date { get; private set; }
        public string[] Tickers { get; private set; }

        public Document(string filepath)
        {
            this.filepath = filepath;
        }

        public void Process()
        {
            LoadFile();
            GetText();
            GetDate();
            GetTickers();
        }

        private void LoadFile()
        {
            Console.WriteLine("Loading {0}", filepath);
            using (TextReader reader = File.OpenText(filepath))
            {
                Html = reader.ReadToEnd();
                HtmlDoc = new HtmlDocument();
                HtmlDoc.LoadHtml(Html);
                if (HtmlDoc.DocumentNode == null)
                {
                    throw new Exception("Document has no root node");
                }
            }
        }

        private void GetText()
        {
            Console.WriteLine("Extracting text");
            HtmlNode footer = HtmlDoc.DocumentNode.SelectSingleNode("//div[@class='newFooter']");
            if (footer == null) footer = HtmlDoc.DocumentNode.SelectSingleNode("//div[@class='entry-content']/p[last()]");
            try
            {
                Text = footer.InnerText;
            }
            catch (Exception e)
            {
                Console.WriteLine("WARNING: No text found. {0}", e.Message);
            }
        }

        private void GetDate()
        {
            Console.WriteLine("Extracting date");
            HtmlNode dateNode = HtmlDoc.DocumentNode.SelectSingleNode("//span[@class='dateline']");
            if (dateNode != null) Date = DateTime.Parse(dateNode.InnerText);
        }

        private void GetTickers()
        {
            if (Date == null || Text == null)
            {
                Console.WriteLine("Skipping ticker extraction since either date or text were not found");
                return;
            }                
            Console.WriteLine("Extracting tickers");
            var calais = new CalaisDotNet("hb6zpfx3mdyywryvmdjfz7n9", Text);            
            var document = calais.Call<CalaisJsonDocument>();

            try
            {
                JObject json = JObject.Parse(document.RawOutput);
                Tickers = json.
                    Properties().
                    Where(key => (string)key.Value.SelectToken("_type") == "Company").
                    Select(key => (string)key.Value.SelectToken("resolutions[0].ticker")).
                    Where(ticker => ticker != null).
                    ToArray();
                Console.WriteLine("Found tickers: {0}", String.Join(", ", Tickers));
            }
            catch (Exception e)
            {
                Console.WriteLine("WARNING: could not extract tickers from this document. {0}", e.Message);
            }            
        }
    }
}
