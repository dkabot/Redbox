using Redbox.HAL.Client;
using System;
using System.Collections.Generic;
using System.IO;

namespace HALUtilities
{
  internal sealed class CortexDecoderFile
  {
    internal readonly List<CortexDecode> Results = new List<CortexDecode>();
    private readonly string Source;

    internal bool Read()
    {
      try
      {
        char[] splitchar = new char[1]{ ',' };
        Array.ForEach<string>(File.ReadAllLines(this.Source), (Action<string>) (line =>
        {
          string[] strArray = line.Split(splitchar);
          this.Results.Add(new CortexDecode()
          {
            Image = strArray[0],
            Matrix = strArray[6],
            MatrixCount = Convert.ToInt32(strArray[7]),
            SecureCount = Convert.ToInt32(strArray[8]),
            Location = new Location()
            {
              Deck = Convert.ToInt32(strArray[9]),
              Slot = Convert.ToInt32(strArray[10])
            }
          });
        }));
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

    internal CortexDecoderFile(string file)
    {
      this.Source = !string.IsNullOrEmpty(file) && File.Exists(file) ? file : throw new ArgumentException(nameof (file));
    }
  }
}
