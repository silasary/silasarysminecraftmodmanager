using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections;
using System.Collections.Generic;

namespace SilasarysMinecraftModManager
{
    partial class Program
    {
        public static String minecraftdir = Environment.ExpandEnvironmentVariables("%appdata%\\.minecraft");
        public static string mPath(params String[] path)
        {
            string aPath = minecraftdir;
            foreach (String c in path)
            {
                aPath = Path.Combine(aPath, c);
            }
            return aPath;
        }
        public static string wPath(params String[] path)
        {
            string aPath = mcVersion;
            foreach (String c in path)
            {
                aPath = Path.Combine(aPath, c);
            }
            return aPath;
        }

        public static string minecraftjar 
        {
            get
            {
                return Path.Combine(minecraftdir,"bin","minecraft.jar");
            }
        }

        public static String mcVersion = "";
        public static String patchVersion = "";
        static FastZip f = new FastZip();

        static void Main(string[] args)
        {
            if (Setup(args) == false)
                return;
            Updater.CheckForUpdates();
            if (CheckMods() == false)
            {
                Launch();
                return;
            }
            String wd = wPath("wd");
            String[] texmods = Directory.EnumerateFiles(wPath("TextureMods")).ToArray<String>();
            if (texmods.Length > 0)
            {
                Console.WriteLine("Please select a texture Pack:");
                Console.WriteLine("0 Minecraft Default");
                for (int i = 0; i < texmods.Length; i++)
                {
                    Console.WriteLine(i+1 + " "+ texmods[i]);
                }
                char n = (char)Console.Read();
                int s = int.Parse(n.ToString());
                String tex = wPath("Original.jar");
                if (s == 0)
                {
                    Console.WriteLine("Loading Classic Texture Pack.");
                    CopyAll(wPath("MinecraftJar"), wd,true);
                }
                else
                {
                    tex = texmods[s - 1];
                    Console.WriteLine("Loading " + Path.GetFileNameWithoutExtension(tex));
                    String xPath = wPath("TextureMods",Path.GetFileNameWithoutExtension(tex));
                    if (!Directory.Exists(xPath))
                        f.ExtractZip(tex, xPath, "");
                    CopyAll(xPath, wd,true);
                }

            }
            else
            {
                Console.WriteLine("Adding Base Minecraft Files");
                CopyAll(wPath("MinecraftJar"), wd,true);
            }
            fixNaughtyMods();
            foreach (String Mod in Directory.EnumerateDirectories(wPath("mods")))
            {
                Console.WriteLine("Applying "+Mod);
                CopyAll(Mod, wd,true);
            }
            CopyAuxData();
            if (Directory.Exists(wPath("wd", "META-INF")))
                Directory.Delete(wPath("wd", "META-INF"),true);
            Console.WriteLine("Creating jar."); 
            f.CreateZip("minecraft.jar", wd, true, "");
            Console.WriteLine("Applying Jar.");
            File.Copy("minecraft.jar", minecraftjar, true);
            Directory.Delete(wPath("wd"), true);

            StreamWriter VersionFile = new StreamWriter(mPath("bin", "mversion"));
            VersionFile.WriteLine(patchVersion);
            VersionFile.Close();

            Launch();

            examineConflicts();


            if (!File.Exists(wPath("Mods.7z")))
                DownloadModsArchive(new String[] {"-q"});
        }

        private static void fixNaughtyMods()
        {
            foreach (string mod in Directory.EnumerateDirectories(wPath("Mods")))
            {
                if (Directory.EnumerateFiles(mod).Count() == 0 && Directory.EnumerateDirectories(mod).Count() == 1 && Directory.EnumerateDirectories(wPath("MinecraftJar")).Contains(Directory.EnumerateDirectories(mod).First()))
                    Directory.Move(Directory.EnumerateDirectories(mod).First(), mod);
                bool naughty = false;
                foreach (string folder in Directory.EnumerateDirectories(mod))
                {
                    if (folder.ToLowerInvariant().Contains("minecraft.jar"))
                    {
                        naughty = true;
                        CopyAll(folder, mod,true);
                        Directory.Delete(folder, true);
                    }
                    if (folder.ToLowerInvariant().Contains("resources") && folder.ToLowerInvariant() != Path.Combine(mod.ToLowerInvariant(),"resources"))
                    {
                        naughty = true;
                        Directory.Move(folder, Path.Combine(mod, "resources"));
                    }
                }
                if (naughty)
                    Console.WriteLine(mod + " was naughty.  Fixed.");
            }
        }

        private static void CopyAuxData()
        {
            if (Directory.Exists(wPath("wd", "Mods")))
            {
                Console.WriteLine("Zombe's Mods (or compatible) default values found.");
                CopyAll(wPath("wd", "Mods"), mPath("Mods"), false);
            }
            if (File.Exists(wPath("wd", "BiomeTerrainModSettings.ini")) && !File.Exists(mPath("BiomeTerrainModSettings.ini")))
            {
                Console.WriteLine("Copying default BiomeTerrainModSettings.");
                File.Copy(wPath("wd", "BiomeTerrainModSettings.ini"), mPath("BiomeTerrainModSettings.ini"));
            }
        }

        private static void examineConflicts()
        {
            List<string> classes = new List<string>();
            StreamWriter classWriter = new StreamWriter(wPath("AllMods.txt"));
            List<string> conflicts = new List<string>();
            bool error = false;
            foreach (string Class in Directory.EnumerateFiles(wPath("Mods"), "*.class", SearchOption.AllDirectories))
            {
                if (classes.Contains(Path.GetFileNameWithoutExtension(Class)))
                {
                    error = true;
                    conflicts.Add(Path.GetFileNameWithoutExtension(Class));
                }
                classes.Add(Path.GetFileNameWithoutExtension(Class));
                classWriter.WriteLine(Path.GetFileNameWithoutExtension(Class));
            }
            classWriter.Close();
            if (error)
            {
                Console.WriteLine("!!!CONFLICTS DETECTED!!!");
                Console.Beep();
                foreach (string conflict in conflicts)
                {
                    foreach (string c in Directory.EnumerateFiles(wPath("Mods"), "*.class", SearchOption.AllDirectories).Where(x => x.Contains(conflict)))
                        Console.WriteLine(c);
                }
                Console.WriteLine("Note that these are just potential conflicts, and may be harmless.");
                Console.ReadKey();
            }
        }

        private static void Launch()
        {
            if (File.Exists(wPath("MinecraftPortable.exe")))
                Process.Start(wPath("MinecraftPortable.exe"));
            else if (File.Exists("Minecraft.exe"))
                Process.Start("Java.exe", "-Xmx1024M -Xms512M -jar Minecraft.exe");
        }

        private static bool CheckMods()
        {
            String cVer = mcVersion;
            if (File.Exists(mPath("bin", "mversion")))
            {
                StreamReader VersionFile = new StreamReader(mPath("bin", "mversion"));
                patchVersion = VersionFile.ReadLine();
                foreach (String Mod in Directory.EnumerateDirectories(wPath("mods")))
                {
                    cVer += ";" + Mod;
                }
            }
            if (cVer == patchVersion)
                return false;
            patchVersion = cVer;
            return true;
        }

        private static bool Setup(string[] args)
        {
            if (args.Length > 1)
            {
                if (args[0] == "-b")
                {
                    minecraftdir = args[1];
                }
            }
            #region findjar
            if (!File.Exists(minecraftjar))
            {
                if (File.Exists("Minecraft.exe"))
                {
                    MessageBox.Show("Regenerating Minecraft.jar;  Please wait for Minecraft to load, then re-open AutoModder.");
                    Process.Start("Minecraft.exe");
                    //Main(args);
                    return false;
                }
                else
                {
                    MessageBox.Show("Could not fine minecraft.jar.  Please open and close Minecraft, then relaunch AutoModder.");
                    return false;
                }
            }
            #endregion

            StreamReader VersionFile = new StreamReader(mPath("bin", "version"));
                VersionFile.ReadLine(); //null
                mcVersion = VersionFile.ReadLine(); //version int.  Meaningless to me, actually.
            VersionFile.Close();
            foreach (string a in args)
                if (a[0] != '-' && !a.Contains(Path.VolumeSeparatorChar))
                    mcVersion = a;
            if (File.Exists(wPath("version.txt")))
            {
                VersionFile = new StreamReader(wPath("version.txt"));
                mcVersion = VersionFile.ReadLine();
            }
            else
            {
                //Console.WriteLine("You may want to create a version.txt");
            }
            Console.WriteLine("Current Version:  " + mcVersion);

            if (File.Exists(wPath("MinecraftPortable.exe")))
            {
                minecraftdir = wPath("Data", ".minecraft");
                if (!File.Exists(minecraftjar))
                {
                    Launch();
                    return false;
                }
            }

            if (args.Contains("-dm"))
                DownloadModsArchive(args);

            if (!Directory.Exists(mcVersion) || !File.Exists(wPath("Original.jar")) || !File.Exists(wPath("MinecraftJar","terrain.png")))
            {
                Directory.CreateDirectory(mcVersion);
                Process.Start("Explorer.exe", Path.GetFullPath(mcVersion));
                if (!File.Exists(wPath("original.jar")))
                    File.Copy(minecraftjar, wPath("original.jar"));
                f.ExtractZip(wPath("original.jar"), wPath("minecraftJar"),"");
                Directory.CreateDirectory(wPath("Mods"));
                Directory.CreateDirectory(wPath("TextureMods"));
            }
            if (!File.Exists(wPath("Run.bat")))
            {
                Console.WriteLine("Creating Run.bat; You can use it to launch this particular version of Minecraft.");
                StreamWriter runBat = new StreamWriter(wPath("Run.bat"));
                runBat.WriteLine("::This file will launch MCAutoPatcher for the version of Minecraft contained in this folder (" + mcVersion + ").");
                runBat.WriteLine("@cd " + Path.GetDirectoryName(Application.ExecutablePath));
                runBat.WriteLine("@\"" + Application.ExecutablePath + "\" " + mcVersion);
                runBat.Close();
                File.WriteAllText(wPath("RemoveMods.bat"), "copy Original.jar %appdata%\\.minecraft\\bin\\minecraft.jar\ndel %appdata%\\.minecraft\\bin\\mversion\n\"" + Path.GetDirectoryName(Application.ExecutablePath) + "\\Minecraft.exe\"");
            }
            if (Directory.Exists(wPath("wd")))
                Directory.Delete(wPath("wd"), true);
            Directory.CreateDirectory(wPath("wd"));
            return true;
        }

        public static void CopyAll(String source, String target, Boolean Override)
        {
            CopyAll(new DirectoryInfo(source), new DirectoryInfo(target), Override);
        }

        public static Exception[] CopyAll(DirectoryInfo source, DirectoryInfo target, Boolean Override)
        {
        // Check if the target directory exists, if not, create it.
        if (Directory.Exists(target.FullName) == false)
        {
            Directory.CreateDirectory(target.FullName);
        }
        List<Exception> exceptions = new List<Exception>();
        // Copy each file into it’s new directory.
        foreach (FileInfo fi in source.GetFiles())
        {
            //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
            try
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), Override);
            }
            catch (Exception v)
            { exceptions.Add(v); }
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir,Override);
        }

        return exceptions.ToArray();
    }
    }
}
