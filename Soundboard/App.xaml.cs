using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Soundboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Soundboard.Properties.Settings.Default.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Soundboard.Properties.Settings.Default.RunOnStartup))
                    CheckStartupShortcut();
            };
            //Task.Run(CheckStartupShortcut);
        }

        //protected override void OnExit(ExitEventArgs e)
        //{
        //    Soundboard.Properties.Settings.Default.Save();
        //    base.OnExit(e);
        //}

        void CheckStartupShortcut()
        {
            var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var shortcutLinkFilePath = Path.Combine(startupFolderPath, Soundboard.Properties.Resources.AppName + ".lnk");

            if (Soundboard.Properties.Settings.Default.RunOnStartup == false)
            {
                if (System.IO.File.Exists(shortcutLinkFilePath))
                    System.IO.File.Delete(shortcutLinkFilePath);
                return;
            }

            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLinkFilePath);

            var targetPath = Process.GetCurrentProcess().MainModule.FileName;
            var workingDirectory = new FileInfo(targetPath).Directory.FullName;

            shortcut.IconLocation = Path.Combine(workingDirectory, @"Assets\music.ico");
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.TargetPath = targetPath;

            shortcut.Save();
        }
    }
}
