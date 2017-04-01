/* ===================================================================
Author: Jeff
License: http://unlicense.org/
=================================================================== */

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace GtkWaveSetup
{
    class UrlFilePair
    {
        public string Url { get; set; }
        public string Filename { get; set; }
        public string DestPath { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("GtkWave AutoSetup/AutoRun v00.08.01 for Win32 by Jeff");
            Console.WriteLine("===========================================");
            Console.ForegroundColor = origColor;

            const string LOCAL_GTKW_PATH = "c:\\gtkw\\";
            const string LOCAL_GTKWBIN_PATH = "c:\\gtkw\\bin\\";
            // File List
            var lst = new List<UrlFilePair>();
            lst.Add(new UrlFilePair() { Filename = "gtkwave.exe.gz", Url = "http://www.dspia.com/gtkwave.exe.gz", DestPath = LOCAL_GTKW_PATH });
            lst.Add(new UrlFilePair() { Filename = "all_libs.tar.gz", Url = "http://www.dspia.com/all_libs.tar.gz", DestPath = LOCAL_GTKW_PATH });

            // Create directory
            if(Directory.Exists(LOCAL_GTKW_PATH)==false)
            {
                Directory.CreateDirectory(LOCAL_GTKW_PATH);
            }

            // Download and extract
            using (var client = new WebClient())
            {
                foreach (var x in lst)
                {
                    var file = x.Filename;
                    var downloadurl = x.Url;

                    if (File.Exists(file))
                    {
                        Console.WriteLine(file + " already exists");
                        //File.Delete(file);
                    }
                    else
                    {
                        // downloading
                        Console.Write("Downloading: " + downloadurl + " ...");
                        client.DownloadFile(downloadurl, file);
                        Console.WriteLine("Done");

                        // extracting
                        Console.Write("Extracting " + x.Filename + "...");
                        if (x.Filename.EndsWith("tar.gz"))
                        {
                            ExtractTGZ(x.Filename, x.DestPath);
                        }
                        else
                        {
                            ExtractGZipSample(x.Filename, x.DestPath);
                        }
                        Console.WriteLine("Done");
                    }
                }
            }

            Process pp = new Process();
            string epath = pp.StartInfo.EnvironmentVariables["PATH"];
            epath += (";" + LOCAL_GTKWBIN_PATH);
            pp.StartInfo.EnvironmentVariables["PATH"] = epath;
            pp.StartInfo.UseShellExecute = false;
            pp.StartInfo.FileName = LOCAL_GTKW_PATH + "gtkwave.exe";
            pp.Start();
            Console.WriteLine("Done");
            //Console.ReadKey();
        }


        static void ExtractTGZ(String gzArchiveName, String destFolder)
        {
            Stream inStream = File.OpenRead(gzArchiveName);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }


        static void ExtractGZipSample(string gzipFileName, string targetDir)
        {
            // Use a 4K buffer. Any larger is a waste.    
            byte[] dataBuffer = new byte[4096];

            using (System.IO.Stream fs = new FileStream(gzipFileName, FileMode.Open, FileAccess.Read))
            {
                using (GZipInputStream gzipStream = new GZipInputStream(fs))
                {

                    // Change this to your needs
                    string fnOut = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(gzipFileName));

                    using (FileStream fsOut = File.Create(fnOut))
                    {
                        StreamUtils.Copy(gzipStream, fsOut, dataBuffer);
                    }
                }
            }
        }



    }
}
