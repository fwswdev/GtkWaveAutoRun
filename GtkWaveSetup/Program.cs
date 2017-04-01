using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace GtkWaveSetup
{
    class UrlFilePair
    {
        public string Url { get; set; }
        public string Filename { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("GtkWave Auto-Setup for Win32 by Jeff");
            Console.WriteLine("===========================================");
            Console.ForegroundColor = origColor;

            // Download

            using (var client = new WebClient())
            {
                var lst = new List<UrlFilePair>();

                lst.Add(new UrlFilePair() { Filename = "gtkwave.exe.gz", Url = "http://www.dspia.com/gtkwave.exe.gz" });
                lst.Add(new UrlFilePair() { Filename = "all_libs.tar.gz", Url = "http://www.dspia.com/all_libs.tar.gz" });

                foreach (var x in lst)
                {
                    var file = x.Filename;
                    var downloadurl = x.Url;

                    if (File.Exists(file))
                    {
                        //Console.WriteLine(file + " already exists!");
                        File.Delete(file);
                    }
                    //else
                    {
                        Console.WriteLine("Downloading: " + downloadurl + " ...");
                        client.DownloadFile(downloadurl, file);
                        Console.WriteLine("Download Done.");
                    }
                }
            }

            // Extract


            Console.Beep(); Console.Beep(); Console.Beep();
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
