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
            Console.WriteLine("GtkWave Auto-Setup for Win32 by Jeff");
            Console.WriteLine("===========================================");
            Console.ForegroundColor = origColor;

            const string LOCAL_GTKW_PATH = "c:\\gtkw\\";
            const string LOCAL_GTKWBIN_PATH = "c:\\gtkw\\bin\\";
            // File List
            var lst = new List<UrlFilePair>();
            lst.Add(new UrlFilePair() { Filename = "gtkwave.exe.gz", Url = "http://www.dspia.com/gtkwave.exe.gz", DestPath = LOCAL_GTKW_PATH });
            lst.Add(new UrlFilePair() { Filename = "all_libs.tar.gz", Url = "http://www.dspia.com/all_libs.tar.gz", DestPath = LOCAL_GTKW_PATH });

            // Download
            using (var client = new WebClient())
            {
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
                        Console.Write("Downloading: " + downloadurl + " ...");
                        client.DownloadFile(downloadurl, file);
                        Console.WriteLine("Done");
                    }
                }
            }

            // Extract
            if (Directory.Exists(LOCAL_GTKW_PATH)) Directory.Delete(LOCAL_GTKW_PATH, true);
            Directory.CreateDirectory(LOCAL_GTKW_PATH);

            foreach (var x in lst)
            {
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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"Please add the path  c:\gtkw\bin on your environment path.");
            Console.ForegroundColor = origColor;

            Thread.Sleep(1000);
            //var name = "PATH";
            //var value = LOCAL_GTKWBIN_PATH;
            //var target = EnvironmentVariableTarget.Machine;
            //Environment.SetEnvironmentVariable(name, value, target);
            //Process.Start(LOCAL_GTKW_PATH + "gtkwave.exe");

            //example in C#
            //in wrapper application main()
            Process pp = new Process();
            string epath = pp.StartInfo.EnvironmentVariables["PATH"];
            epath += (";" + LOCAL_GTKWBIN_PATH);
            pp.StartInfo.EnvironmentVariables["PATH"] = epath;
            pp.StartInfo.UseShellExecute = false;
            pp.StartInfo.FileName = LOCAL_GTKW_PATH + "gtkwave.exe";
       
            pp.Start();

            Console.Beep(); Console.Beep(); Console.Beep();
            Console.WriteLine("Done");
            Console.ReadKey();
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

        // routine from https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples
        static void extractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
    }
}
