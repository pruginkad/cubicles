using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FSMonitor
{
    class Program
    {
        static HashSet<string> m_FoldersExclude = new HashSet<string>();
        static void OnChanged(object sender, FileSystemEventArgs e)  
        {  
            if(InExcludes(e.FullPath))
            {
                return;
            }
            Console.WriteLine("File: {0} {1}",   
                   e.FullPath, e.ChangeType);  
        }

        static void OnRenamed(object source, RenamedEventArgs e)  
        {  
            if(InExcludes(e.FullPath))
            {
                return;
            }
            Console.WriteLine("File: {0} renamed to {1}",   
                    e.OldFullPath, e.FullPath);  
        }

        static bool InExcludes(string path)
        {
            path = path.ToLower();
            var match = m_FoldersExclude
                .FirstOrDefault(stringToCheck => path.Contains(stringToCheck));
            return (match != null);
        }
        static void Main(string[] args)
        {
            List<string> l = new List<string>();

            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            string programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
            l.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            l.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            l.Add(programFiles);
            l.Add(programFilesX86);
            l.Add(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            l.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            l.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            l.Add("ntuser.dat");
            l.Add("System Volume Information");
            l.Add("recycle.bin");

            m_FoldersExclude = new HashSet<string>(l.ConvertAll(d => d.ToLower()));

            string[] drives = Environment.GetLogicalDrives();
            foreach (string strDrive in drives)
            {
                try
                {
                    FileSystemWatcher fsWatcher = new FileSystemWatcher();
                    fsWatcher.Path = strDrive;

                    fsWatcher.NotifyFilter =
                      (NotifyFilters.FileName |
                       NotifyFilters.Attributes |
                       NotifyFilters.LastAccess |
                       NotifyFilters.LastWrite |
                       NotifyFilters.Security |
                       NotifyFilters.Size);

                    fsWatcher.Changed += OnChanged;
                    fsWatcher.Created += OnChanged;
                    fsWatcher.Deleted += OnChanged;
                    fsWatcher.Renamed += OnRenamed;

                    fsWatcher.EnableRaisingEvents = true;
                    fsWatcher.IncludeSubdirectories = true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
            Console.WriteLine("Press Enter to quit the sample.");  
            Console.ReadLine( ); 
        }

    }
}
