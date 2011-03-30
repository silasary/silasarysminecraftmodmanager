using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Web;

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

        public static void DownloadVersionTxt(string i,string versionfile)
        {
            try
            {
                WebClient wc = new WebClient();
                Directory.CreateDirectory(Path.GetDirectoryName(versionfile));
                wc.DownloadFile("http://dl.dropbox.com/u/4187827/Minecraft/versions/" + i, versionfile);
            }
            catch (Exception)
            { }
        }

        public static void DownloadMinecraftExe()
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile("http://www.minecraft.net/download/Minecraft.exe?v=1301446270071", "minecraft.exe");
                Process.Start("Java.exe", "-Xmx1024M -Xms512M -jar Minecraft.exe");
            }
            catch (Exception e)
            { }
        }

        private static void SendToPastebin(string text)
        {
            /// Method 1

            //string name = "Error Report: SMMM";
            //string email = String.Empty;
            //string hl = "csharp";
            //string expire = String.Empty;
            //string subdomain = "Silasary";
            //string privacy = String.Empty;
            //string data = String.Format("&paste_name={0}&paste_email={1}&paste_format={2}&paste_expire_date={3}&paste_subdomain={4}&paste_private={5}&paste_code={6}", HttpUtility.UrlEncode(name), HttpUtility.UrlEncode(email), HttpUtility.UrlEncode(hl), HttpUtility.UrlEncode(expire), HttpUtility.UrlEncode(subdomain), HttpUtility.UrlEncode(privacy), HttpUtility.UrlEncode(text));
            //WebClient pasteClient = new WebClient();
            //Uri uri = new Uri("http://www.pastebin.com/api_public.php");

            //pasteClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            //pasteClient.UploadString(uri, data);

            /// Method 2
            
            //try
            //{

            //    HttpWebRequest request = (HttpWebRequest)
            //        WebRequest.Create("http://davux.pastebin.com/pastebin.php");

            //    request.AllowAutoRedirect = false;
            //    request.Method = "POST";

            //    string post = "&amp;parent_pid=&amp;format=text&amp;code2=" + HttpUtility.UrlEncode(text) + "&amp;poster=Dave&amp;paste=Send&amp;expiry=m&amp;email=";
            //    byte[] data = System.Text.Encoding.ASCII.GetBytes(post);

            //    request.ContentType = "application/x-www-form-urlencoded";
            //    request.ContentLength = data.Length;

            //    Stream response = request.GetRequestStream();

            //    response.Write(data, 0, data.Length);

            //    response.Close();

            //    HttpWebResponse res = (HttpWebResponse)request.GetResponse();
            //    res.Close();
            //    // note that there is no need to hook up a StreamReader and
            //    // look at the response data, since it is of no need

            //    if (res.StatusCode == HttpStatusCode.Found)
            //    {
            //        Console.WriteLine(res.Headers["location"]);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Error");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error: " + ex.Message);
            //}
        }

    }
}
