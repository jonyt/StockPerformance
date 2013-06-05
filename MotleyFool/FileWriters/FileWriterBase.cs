using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MotleyFool.FileWriters
{
    public abstract class FileWriterBase : IDisposable
    {
        protected TextWriter writer;
        protected IDictionary<string, IList<Tuple<DateTime, double>>> allTickerData;

        protected FileWriterBase(string path, IDictionary<string, IList<Tuple<DateTime, double>>> allTickerData)
        {
            this.writer = File.CreateText(path);
            this.allTickerData = allTickerData;
        }

        public virtual void Write()
        {
            writer.Write("\t");
            writer.WriteLine(
                String.Join(
                    "\t", allTickerData.First().Value.
                            Select(tickerData => String.Format("{0}-{1}", tickerData.Item1.Year, tickerData.Item1.Month.ToString("D2"))
                    )
                )
            );
        }

        public void Dispose()
        {
            if (writer != null) writer.Dispose();
        }
    }
}
