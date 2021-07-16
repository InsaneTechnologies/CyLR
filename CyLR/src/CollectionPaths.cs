// DR version of code

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Win32;

namespace CyLR
{
    internal static class CollectionPaths
    {
        private static List<string> AllFiles;
        //private static List<string> WinRecycleFolders;
        //private static List<string> WinUserFolders;
        private static List<string> tempPaths;
        //private static List<string> WintempPaths;

        private static IEnumerable<string> RunCommand(string OSCommand, string CommandArgs)
        {
            var newPaths = new List<string> { };
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = OSCommand,
                    Arguments = CommandArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.StartInfo.CreateNoWindow = false;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                yield return  proc.StandardOutput.ReadLine();
            };
        }
        public static List<string> GetPaths(Arguments arguments, List<string> additionalPaths, bool Usnjrnl, bool Desktop, bool OtherMFT)
        {

            var defaultPaths = new List<string>
            {
                $@"{arguments.SystemDrive}\inetpub\logs", // add in IIS logs
                $@"{arguments.SystemDrive}\ProgramData\Microsoft\Diagnosis\EventTranscript\EventTranscript.db", // added 16.07.2021 based on https://www.kroll.com/en/insights/publications/cyber/forensically-unpacking-eventtranscript
                $@"{arguments.SystemDrive}\ProgramData\Microsoft\Search\Data\Applications\Windows",
                $@"{arguments.SystemDrive}\ProgramData\Microsoft\Network\Downloader",
                $@"{arguments.SystemDrive}\ProgramData\Microsoft\RAC\PublishedData",
                $@"{arguments.SystemDrive}\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup",
                $@"{arguments.SystemDrive}\Program Files (x86)\TeamViewer\Connections_incoming.txt",
                $@"{arguments.SystemDrive}\Program Files\TeamViewer\Connections_incoming.txt",
                $@"{arguments.SystemDrive}\Windows\Appcompat\Programs\install",
                $@"{arguments.SystemDrive}\Windows\Appcompat\Programs\Amcache.hve",
                $@"{arguments.SystemDrive}\Windows\Appcompat\Programs\Amcache.hve.LOG1",
                $@"{arguments.SystemDrive}\Windows\Appcompat\Programs\Amcache.hve.LOG2",
                $@"{arguments.SystemDrive}\Windows\Appcompat\Programs\Amcache.hve.tmp.LOG1",
                $@"{arguments.SystemDrive}\Windows\Appcompat\Programs\Amcache.hve.tmp.LOG2",
                $@"{arguments.SystemDrive}\Windows\Appcompat\Programs\recentfilecache.bcf",
                $@"{arguments.SystemDrive}\Windows\System Volume Information\syscache.hve",
                $@"{arguments.SystemDrive}\Windows\System Volume Information\syscache.hve.LOG1",
                $@"{arguments.SystemDrive}\Windows\System Volume Information\syscache.hve.LOG2",
                $@"{arguments.SystemDrive}\Windows\inf\setupapi.dev.log",
                $@"{arguments.SystemDrive}\Windows\Logs",
                $@"{arguments.SystemDrive}\Windows\NTDS", // add in collection of Active Directory NTDS folder
                $@"{arguments.SystemDrive}\Windows\Prefetch",
                $@"{arguments.SystemDrive}\Windows\SchedLgU.Txt",
                $@"{arguments.SystemDrive}\Windows\ServiceProfiles\LocalService",
                $@"{arguments.SystemDrive}\Windows\ServiceProfiles\NetworkService",
                $@"{arguments.SystemDrive}\Windows\System32\config",
                $@"{arguments.SystemDrive}\Windows\System32\config\SAM.LOG1",
                $@"{arguments.SystemDrive}\Windows\System32\config\SAM.LOG2",
                $@"{arguments.SystemDrive}\Windows\System32\config\SECURITY.LOG1",
                $@"{arguments.SystemDrive}\Windows\System32\config\SECURITY.LOG2",
                $@"{arguments.SystemDrive}\Windows\System32\config\SOFTWARE.LOG1",
                $@"{arguments.SystemDrive}\Windows\System32\config\SOFTWARE.LOG2",
                $@"{arguments.SystemDrive}\Windows\System32\config\SYSTEM.LOG1",
                $@"{arguments.SystemDrive}\Windows\System32\config\SYSTEM.LOG2",
                $@"{arguments.SystemDrive}\Windows\System32\dhcp", // add in collection of DHCP logs and database
                $@"{arguments.SystemDrive}\Windows\System32\dns", // add in collection of DNS logs and database
                $@"{arguments.SystemDrive}\Windows\System32\drivers\etc\hosts",
                $@"{arguments.SystemDrive}\Windows\System32\LogFiles",
                $@"{arguments.SystemDrive}\Windows\System32\bits.log",
                $@"{arguments.SystemDrive}\Windows\System32\sru",
                $@"{arguments.SystemDrive}\Windows\System32\Tasks",
                $@"{arguments.SystemDrive}\Windows\System32\wbem\Repository",
                $@"{arguments.SystemDrive}\Windows\System32\winevt\logs",
                $@"{arguments.SystemDrive}\Windows\Tasks"
            };

            if(arguments.SystemDrive.Equals("C:") && OtherMFT)
            {
                try
                {
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo d in allDrives)
                    {
                        if (d.DriveType == DriveType.Fixed && d.DriveFormat == "NTFS")
                        {
                            defaultPaths.Add($@"{d.Name}$LogFile");
                            defaultPaths.Add($@"{d.Name}$MFT");
                            defaultPaths.Add($@"{d.Name}$secure:$SDS");
                            defaultPaths.Add($@"{d.Name}$Boot");
                            if (Usnjrnl)
                            {
                                defaultPaths.Add($@"{d.Name}$Extend\$UsnJrnl:$J");
                                defaultPaths.Add($@"{d.Name}$Extend\$UsnJrnl:$Max");
                                defaultPaths.Add($@"{d.Name}$Extend\$RmMetadata\$TxfLog\$Tops:$T");
                            }
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    // FAIL
                }
            } 
            else 
            {
                defaultPaths.Add($@"{arguments.SystemDrive}\$LogFile");
                defaultPaths.Add($@"{arguments.SystemDrive}\$MFT");
                defaultPaths.Add($@"{arguments.SystemDrive}\$secure:$SDS");
                defaultPaths.Add($@"{arguments.SystemDrive}\$Boot");
                if (Usnjrnl)
                {
                    defaultPaths.Add($@"{arguments.SystemDrive}\$Extend\$UsnJrnl:$J");
                    defaultPaths.Add($@"{arguments.SystemDrive}\$Extend\$UsnJrnl:$Max");
                    defaultPaths.Add($@"{arguments.SystemDrive}\$Extend\$RmMetadata\$TxfLog\$Tops:$T");
                }
            }

            defaultPaths = defaultPaths.Select(Environment.ExpandEnvironmentVariables).ToList();

            if (!Platform.IsUnixLike())
            {
                // adding in option to replace C: with whatever comes from -drive
                // it's a hack - sue me
                if (!arguments.SystemDrive.Equals("C:"))
                {
                    defaultPaths = defaultPaths.Select(path => arguments.SystemDrive + path.Substring(2)).ToList();
                }


                if (arguments.RecycleBin)
                {
                    // Enumerate Recyle Bin -- this is not ready for production yet ...
                    string RecyclePath = arguments.SystemDrive + "\\$Recycle.Bin\\";
                    string[] WinRecycleFolders = Directory.GetDirectories(RecyclePath);
                    foreach (var RecycleBinFolder in WinRecycleFolders)
                    {
                        defaultPaths.Add($@"{RecycleBinFolder}\");
                    }
                }
                

                string UserPath = arguments.SystemDrive + "\\Users\\";
                string[] WinUserFolders = Directory.GetDirectories(UserPath);
                foreach (var User in WinUserFolders)
                {
                    defaultPaths.Add($@"{User}\NTUSER.DAT");
                    defaultPaths.Add($@"{User}\NTUSER.DAT.LOG1");
                    defaultPaths.Add($@"{User}\NTUSER.DAT.LOG2");
                    if(arguments.Desktop)
                    {
                        defaultPaths.Add($@"{User}\Desktop");
                    }
                    defaultPaths.Add($@"{User}\AppData\Local\Putty.rnd");
                    defaultPaths.Add($@"{User}\AppData\Local\ConnectedDevicesPlatform");
                    defaultPaths.Add($@"{User}\AppData\Local\Google\Chrome\User Data");
                    defaultPaths.Add($@"{User}\AppData\Local\Google\Chrome SxS\User Data");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Edge");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Terminal Service Client\Cache");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\Internet Explorer");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\Cookies");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\Explorer");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\IEDownloadHistory");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\INetCache");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\INetCookies");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\History");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\Temporary Internet Files"); 
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\UsrClass.dat");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\UsrClass.dat.LOG1");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\UsrClass.dat.LOG2");
                    defaultPaths.Add($@"{User}\AppData\Local\Microsoft\Windows\WebCache");
                    defaultPaths.Add($@"{User}\AppData\Local\Mozilla\Firefox\Profiles");
                    defaultPaths.Add($@"{User}\AppData\Local\Packages\Microsoft.MicrosoftEdge_8wekyb3d8bbwe");
                    defaultPaths.Add($@"{User}\AppData\Roaming\winscp.ini");
                    defaultPaths.Add($@"{User}\AppData\Roaming\winscp.rnd");
                    defaultPaths.Add($@"{User}\AppData\Roaming\Google\Chrome\User Data"); // added 03.07.2021 after finding Google Chrome data in Roaming not Local on a case
                    defaultPaths.Add($@"{User}\AppData\Roaming\Google\Chrome SxS\User Data");                    
                    defaultPaths.Add($@"{User}\AppData\Roaming\Opera");
                    defaultPaths.Add($@"{User}\AppData\Roaming\Microsoft\Internet Explorer");
                    defaultPaths.Add($@"{User}\AppData\Roaming\Microsoft\Windows\Office\Recent");
                    defaultPaths.Add($@"{User}\AppData\Roaming\Microsoft\Windows\PowerShell\PSReadline");
                    defaultPaths.Add($@"{User}\AppData\Roaming\Microsoft\Windows\Recent");
                    defaultPaths.Add($@"{User}\AppData\Roaming\Microsoft\Windows\Start Menu");
                    defaultPaths.Add($@"{User}\AppData\Roaming\Mozilla\Firefox\Profiles");
                    defaultPaths.Add($@"{User}\AppData\Roaming\TeamViewer");
                }
            }

                if (Platform.IsUnixLike())
            {
                defaultPaths = new List<string> { };
                tempPaths = new List<string>
                {
                    "/root/.bash_history",
                    "/var/log",
                    "/private/var/log/",
                    "/.fseventsd",
                    "/etc/hosts.allow",
                    "/etc/hosts.deny",
                    "/etc/hosts",
                    "/System/Library/StartupItems",
                    "/System/Library/LaunchAgents",
                    "/System/Library/LaunchDaemons",
                    "/Library/LaunchAgents",
                    "/Library/LaunchDaemons",
                    "/Library/StartupItems",
                    "/etc/passwd",
                    "/etc/group",
                    "/etc/rc.d"
                };
                // Collect file listing
                AllFiles = new List<string> { };
                AllFiles.AddRange(RunCommand("/usr/bin/find", "/ -print"));

                // Find all *.plist files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("*.plist"))));
                // Find all .bash_history files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains(".bash_history"))));
                // Find all .sh_history files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains(".sh_history"))));
                // Find Chrome Preference files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/History"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Cookies"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Bookmarks"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Extensions"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Last"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Shortcuts"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Top"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("Support/Google/Chrome/Default/Visited"))));

                // Find FireFox Preference Files
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("places.sqlite"))));
                tempPaths.AddRange(AllFiles.Where((stringToCheck => stringToCheck.Contains("downloads.sqlite"))));

                // Fix any spaces to work with MacOS naming conventions
                defaultPaths = tempPaths.ConvertAll(stringToCheck => stringToCheck.Replace(" ", " "));
            }
            var paths = new List<string>(additionalPaths);

            if (arguments.CollectionFilePath != ".")
            {
                if (File.Exists(arguments.CollectionFilePath))
                {
                    paths.AddRange(File.ReadAllLines(arguments.CollectionFilePath).Select(Environment.ExpandEnvironmentVariables));
                }
                else
                {
                    Console.WriteLine("Error: Could not find file: {0}", arguments.CollectionFilePath);
                    Console.WriteLine("Exiting");
                    throw new ArgumentException();
                }
            }

            if (arguments.CollectionFiles != null)
            {
                paths.AddRange(arguments.CollectionFiles);
            }

            if (paths.Count == 1)
            {
                if (paths[0] == "")
                {
                    return defaultPaths;
                }
            }
            return paths.Any() ? paths : defaultPaths;
        }
        public static IEnumerable<UserProfile> FindUsers()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList");
            foreach (string name in key.GetSubKeyNames())
            {
                var path = $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\{name}";
                var profile = Registry.GetValue(path, "FullProfile", string.Empty);
                if (profile != null)
                {
                    var result = new UserProfile
                    {
                        UserKey = name,
                        Path = $@"{path}\ProfileImagePath",
                        ProfilePath = (string)Registry.GetValue(path, "ProfileImagePath", 0),
                        FullProfile = (int)Registry.GetValue(path, "FullProfile", 0)
                    };
                    if (result.FullProfile != -1) yield return result;
                }
            }

        }

        internal class UserProfile
        {
            public string UserKey { get; set; }
            public string Path { get; set; }
            public string ProfilePath { get; set; }
            public int FullProfile { get; set; }
        }
    }
}
