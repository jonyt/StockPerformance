using System;
using System.Linq;
using System.Collections.Generic;

namespace MotleyFool.FileWriters
{
    public class RegularWriter : FileWriterBase
    {
        public RegularWriter(string path, IDictionary<string, IList<Tuple<DateTime, double>>> allTickerData) : base(path, allTickerData){}

        public override void Write()
        {
            // Write a line with the dates
            base.Write();
            foreach (var kvp in allTickerData)
            {
                writer.WriteLine("{0}\t{1}", kvp.Key, String.Join("\t", kvp.Value.Select(tickerData => tickerData.Item2)));                
            }
        }
    }
}
