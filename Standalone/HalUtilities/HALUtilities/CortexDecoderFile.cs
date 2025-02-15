using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Client;

namespace HALUtilities
{
    internal sealed class CortexDecoderFile
    {
        internal readonly List<CortexDecode> Results = new List<CortexDecode>();
        private readonly string Source;

        internal CortexDecoderFile(string file)
        {
            Source = !string.IsNullOrEmpty(file) && File.Exists(file)
                ? file
                : throw new ArgumentException(nameof(file));
        }

        internal bool Read()
        {
            try
            {
                var splitchar = new char[1] { ',' };
                Array.ForEach(File.ReadAllLines(Source), line =>
                {
                    var strArray = line.Split(splitchar);
                    Results.Add(new CortexDecode
                    {
                        Image = strArray[0],
                        Matrix = strArray[6],
                        MatrixCount = Convert.ToInt32(strArray[7]),
                        SecureCount = Convert.ToInt32(strArray[8]),
                        Location = new Location
                        {
                            Deck = Convert.ToInt32(strArray[9]),
                            Slot = Convert.ToInt32(strArray[10])
                        }
                    });
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception during read");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }
    }
}