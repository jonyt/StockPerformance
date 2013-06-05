using System;
using System.Collections.Generic;
using System.Linq;

namespace MotleyFool.FileWriters
{
    public class NormalizedDataWriter : FileWriterBase
    {
        public NormalizedDataWriter(string path, IDictionary<string, IList<Tuple<DateTime, double>>> allTickerData) : base(path, allTickerData) { }

        public override void Write()
        {
            base.Write();
            foreach (var kvp in allTickerData)
            {            
                double firstValue = kvp.Value.First().Item2;
                writer.WriteLine("{0}\t{1}", kvp.Key, String.Join("\t", kvp.Value.Select(tickerData => tickerData.Item2 / firstValue)));
            }        
        }
    }
}
