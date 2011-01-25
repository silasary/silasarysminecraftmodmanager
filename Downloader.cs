using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace SilasarysMinecraftModManager
{
    partial class Program
    {
        private static void DownloadModsArchive(string[] args)
        {
            bool quiet = args.Contains("-q");
            int s = 0;
            if (!File.Exists("mcVersion.txt") && PrettyVersionNumber(mcVersion) == "")
            {
                Console.WriteLine("What is the current Version Number? (IE: B1.1_02)");
                Console.WriteLine("\tFeel free to launch Minecraft to check.\n\tIt's in the top left corner of the Main Menu");
                Console.Write(">");
                string v = Console.ReadLine(); // Working on a better way of doing this.
                if (PrettyVersionNumber(v) != "")
                    File.WriteAllText(wPath("Version.txt"), v);
            }
            string url ;
            if (File.Exists("mcVersion.txt"))
                url = "http://dl.dropbox.com/u/15662704/Minecraft/" + PrettyVersionNumber(File.ReadAllText("mcVersion.txt")) + ".7z";
            else
                url = "http://dl.dropbox.com/u/15662704/Minecraft/" + PrettyVersionNumber(mcVersion) + ".7z";
            WebClient client = new WebClient();
            client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Revalidate); // It's around 10MB, so cache if possible.
            if (!quiet)
                Console.WriteLine("Downloading: "+ url);
            try
            {
                client.DownloadFile(url, wPath("Mods.7z"));
            }
            catch (WebException v)
            {
                if (!quiet) 
                    Console.WriteLine(v.Message);
            }
            if (!quiet) 
                Console.WriteLine("Finished Download.");
        }

        private static string PrettyVersionNumber(string n)
        {
            String Out = "";
            Regex r = new Regex("[z-zA-Z][0-9\\._]+",RegexOptions.IgnorePatternWhitespace);
            Match m = r.Match(n);
            if (m.Success)
            {
                Out = m.Captures[0].Value.Replace("A", "Alpha ");
                Out = m.Captures[0].Value.Replace("B", "Beta ");
            }
            else if ((m = new Regex("[0-9\\.]+",RegexOptions.IgnorePatternWhitespace).Match(n)).Success)
            {
                Out = "Beta " + m.Captures[0].Value;
            }
            else
            {
                Out = "";
                Console.WriteLine("Unrecognized Version String.");
            }
            return Out;
        }
    }
}
