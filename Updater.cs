using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace SilasarysMinecraftModManager
{
    class Updater
    {
        public static void Update()
        {
            File.WriteAllText(Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".dcupdate",
                "<Local><Label>"+ Application.ProductName +"</Label>\n" +
                "<IconFile>"+Application.ExecutablePath+"</IconFile>"   + 
                "<Version>" + Application.ProductVersion + "</Version>" +
                "<VersionFileRemote>" + Properties.Settings.Default.VersionFileURL  +"</VersionFileRemote>" +
                "<WebPage>"+ Properties.Settings.Default.WebPage + "</WebPage>" +
                "<UpdateMethod>"+Properties.Settings.Default.UpdateMethod+"</UpdateMethod>" +
                "<CloseForUpdate>"+Path.GetFileName(Application.ExecutablePath)+"</CloseForUpdate>" +
                "<UpdateFile>"+Properties.Settings.Default.UpdateFile+"</UpdateFile>" +
                "</Local>");

            if (File.Exists("dcuhelper.exe"))
            {
                System.Diagnostics.ProcessStartInfo dcuhelper = new System.Diagnostics.ProcessStartInfo("dcuhelper.exe", "-ri \"" + Application.ProductName + "\" \".\" \"Updater\" -shownew -check -nothingexit");
                dcuhelper.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                System.Diagnostics.Process.Start(dcuhelper);
            }
        }
		
		public static void CheckForUpdates()
		{
			DateTime lastUpdate = new DateTime();
            DateTime.TryParse(Properties.Settings.Default.LastUpdate, out lastUpdate);
			if (TimeSpan.FromTicks(DateTime.Now.Ticks - lastUpdate.Ticks).Days > 6)
            {
				Update();
                Properties.Settings.Default.LastUpdate =  DateTime.Now.ToString();
			}
			if (Application.ProductVersion != Properties.Settings.Default.LastVersion)
			{
				Update();
				Properties.Settings.Default.LastVersion = Application.ProductVersion;
                try
                {
                    ProgramUpdated.Invoke(Properties.Settings.Default.LastVersion, Application.ProductVersion);
                }
                catch (NullReferenceException)
                { }
			}
		}

        public delegate void ProgramUpdatedHandler(string OldVersion, string NewVersion);

        public static event ProgramUpdatedHandler ProgramUpdated;

        
    }
}
