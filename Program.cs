using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32;

#pragma warning disable CA1416 // Validate platform compatibility

namespace KingModTweak
{
    internal class Program
    {
        private static string cpuName = "N/A";
        private static string gpuName = "N/A";
        private static readonly string[] MonitoredProcesses = { "svchost.exe", "dwm.exe", "explorer.exe", "spoolsv.exe" }; // Add common background processes

        static void Main(string[] args)
        {
            Console.Title = "KingMod Tweak V1.2";
            if (Console.WindowWidth < 120 || Console.WindowHeight < 40) 
            {
                try { Console.SetWindowSize(120, 40); } catch {} // Set a larger, fixed window size for better layout
            }

            Console.ForegroundColor = ConsoleColor.White;

            if (!IsAdministrator())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                CenterWriteLine("ERROR: Please run this application as an administrator.");
                Console.WriteLine();
                CenterWriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            cpuName = GetManagementObjectProperty("Win32_Processor", "Name");
            gpuName = GetManagementObjectProperty("Win32_VideoController", "Name");

            while (true)
            {
                ShowMenu();
                Console.WriteLine();
                CenterWriteMultiColor(false, ("[ > ] ", ConsoleColor.Red, 0));
                string? choice = Console.ReadLine()?.ToUpper();
                HandleMenuChoice(choice);
            }
        }

        static void ShowMenu()
        {
            Console.Clear();
            string[] kingArt = {
                "______ ______              ",
                "___  //_/__(_)_____________",
                "__  ,<  __  /__  __ \\_  __ `",
                "_  /| | _  / _  / / /  /_/ /",
                "/_/ |_| /_/  /_/ /_/\\__, /",
                "                    /____/ "
            };
            string[] modArt = {
                " ______  ___     _________",
                "___   |/  /___________  /",
                "/_  /|_/ /_  __ \\  __  / ",
                "_  /  / / /_/ / /_/ /  ",
                "/_/  /_/  \\____/\\__,_/   ",
                "                         "
            };

            for (int i = 0; i < kingArt.Length; i++)
            {
                CenterWriteMultiColor(true, (kingArt[i], ConsoleColor.White, 0), (" ", ConsoleColor.White, 0), (modArt[i], ConsoleColor.Red, 0));
            }
            Console.WriteLine();

            // System Info
            CenterWriteMultiColor(true, ("Version: 1.2", ConsoleColor.Cyan, 25), ("User: " + Environment.UserName, ConsoleColor.Cyan, 30), ($"Date: {DateTime.Now:ddd MM/dd/yyyy}", ConsoleColor.Cyan, 30));
            Console.WriteLine();
            CenterWriteMultiColor(true, ($"GPU: {gpuName}", ConsoleColor.Magenta, 45), ($"CPU: {cpuName}", ConsoleColor.Magenta, 45));
            Console.WriteLine();

            // Description
            CenterWriteLine("KingMod Tweak is a powerful tool to optimize your system for the best performance.", ConsoleColor.DarkGray);
            Console.WriteLine();

            // Menu Options
            CenterWriteMultiColor(true, ("[1] Optimizations", ConsoleColor.Cyan, 25), ("[2] Privacy", ConsoleColor.Cyan, 25), ("[3] Windows", ConsoleColor.Cyan, 25), ("[4] Advanced", ConsoleColor.Cyan, 25));
            CenterWriteMultiColor(true, ("[5] Spoofing", ConsoleColor.Cyan, 25), ("[6] Utilities", ConsoleColor.Cyan, 25), ("[7] Hardware", ConsoleColor.Cyan, 25), ("[8] Backup", ConsoleColor.Cyan, 25));
            CenterWriteMultiColor(true, ("[9] Info", ConsoleColor.Cyan, 0));
            Console.WriteLine();
            CenterWriteMultiColor(true, ("[ X to close ]", ConsoleColor.DarkGray, 0));
        }

        static void HandleMenuChoice(string? choice)
        {
            switch (choice)
            {
                case "1": Optimizations(); break;
                case "2": Privacy(); break;
                case "3": Windows(); break;
                case "4": Advanced(); break;
                case "5": Spoofing(); break;
                case "6": Utilities(); break;
                case "7": Hardware(); break;
                case "8": Backup(); break;
                case "9": Info(); break;
                case "X": Environment.Exit(0); break;
                default:
                    CenterWriteLine("Invalid choice. Please try again.", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    break;
            }
        }

        static void Optimizations()
        {
            while (true)
            {
                Console.Clear();
                CenterWriteLine("--- Optimizations ---", ConsoleColor.Red);
                Console.WriteLine();
                CenterWriteMultiColor(true, ("[1] ", ConsoleColor.Red, 0), ("Clean Temporary Files", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[2] ", ConsoleColor.Red, 0), ("Disable Startup Programs", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[3] ", ConsoleColor.Red, 0), ("Defragment All Drives", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[B] ", ConsoleColor.Red, 0), ("Back to Main Menu", ConsoleColor.DarkGray, 0));
                Console.WriteLine();
                CenterWriteMultiColor(false, ("[ > ] ", ConsoleColor.Red, 0));
                string? choice = Console.ReadLine()?.ToUpper();

                switch (choice)
                {
                    case "1":
                        CleanTemporaryFiles();
                        break;
                    case "2":
                        RunCommand("explorer.exe", "ms-settings:startupapps");
                        break;
                    case "3":
                        RunCommand("defrag", "/C");
                        break;
                    case "B":
                        return;
                    default:
                        CenterWriteLine("Invalid choice.", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        break;
                }
                CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
                Console.ReadKey();
            }
        }

        static void Hardware()
        {
            Console.Clear();
            CenterWriteLine("--- Hardware Information ---", ConsoleColor.Red);
            Console.WriteLine();

            try
            {
                ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                ManagementObjectCollection results = searcher.Get();
                long totalRamBytes = 0;
                foreach (ManagementObject result in results)
                {
                    totalRamBytes = Convert.ToInt64(result["TotalPhysicalMemory"]);
                }
                double totalRamGb = Math.Round(totalRamBytes / (1024.0 * 1024.0 * 1024.0), 2);

                PerformanceCounter pc = new PerformanceCounter("Memory", "Available MBytes");
                pc.NextValue();
                Thread.Sleep(500);
                float availableRamMb = pc.NextValue();
                double availableRamGb = Math.Round(availableRamMb / 1024.0, 2);

                CenterWriteMultiColor(true, ("         CPU: ", ConsoleColor.Cyan, 0), (cpuName, ConsoleColor.White, 0));
                CenterWriteMultiColor(true, ("         GPU: ", ConsoleColor.Cyan, 0), (gpuName, ConsoleColor.White, 0));
                CenterWriteMultiColor(true, ("   Total RAM: ", ConsoleColor.Cyan, 0), ($"{totalRamGb} GB", ConsoleColor.White, 0));
                CenterWriteMultiColor(true, (" Available RAM: ", ConsoleColor.Cyan, 0), ($"{availableRamGb} GB ({Math.Round((availableRamMb / (totalRamGb * 1024)) * 100, 0)}% free)", ConsoleColor.White, 0));
            }
            catch (Exception ex)
            {
                CenterWriteLine("Error retrieving hardware information.", ConsoleColor.Red);
                CenterWriteLine(ex.Message, ConsoleColor.DarkRed);
            }

            Console.WriteLine();
            CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
            Console.ReadKey();
        }

        private static bool IsVisualEffectsDisabled()
        {
            try
            {
                var value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects", "VisualFxSetting", 1);
                return value != null && (int)value == 2;
            }
            catch { return false; }
        }

        private static bool IsHighPerformancePowerPlanActive()
        {
            const string highPerfGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
            try
            {
                using (var p = new Process
                {
                    StartInfo =
                    {
                        FileName = "powercfg",
                        Arguments = "/getactivescheme",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                })
                {
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return output.Contains(highPerfGuid);
                }
            }
            catch { return false; }
        }

        static void Windows()
        {
            while (true)
            {
                Console.Clear();
                CenterWriteLine("--- Windows Tweaks ---", ConsoleColor.Red);
                Console.WriteLine();

                bool visualEffectsDisabled = IsVisualEffectsDisabled();
                string visualMenuText = visualEffectsDisabled ? "Enable Visual Effects (Default)" : "Disable Visual Effects (Performance)";
                ConsoleColor visualMenuColor = visualEffectsDisabled ? ConsoleColor.Green : ConsoleColor.Cyan;

                bool highPerfActive = IsHighPerformancePowerPlanActive();
                string powerMenuText = highPerfActive ? "Set Balanced Power Plan (Default)" : "Set High-Performance Power Plan";
                ConsoleColor powerMenuColor = highPerfActive ? ConsoleColor.Green : ConsoleColor.Cyan;

                CenterWriteMultiColor(true, ("[1] ", ConsoleColor.Red, 0), (visualMenuText, visualMenuColor, 45));
                CenterWriteMultiColor(true, ("[2] ", ConsoleColor.Red, 0), (powerMenuText, powerMenuColor, 45));
                CenterWriteMultiColor(true, ("[3] ", ConsoleColor.Red, 0), ("Disable Game Bar", ConsoleColor.Cyan, 45));
                CenterWriteMultiColor(true, ("[4] ", ConsoleColor.Red, 0), ("Enable Game Mode", ConsoleColor.Cyan, 45));
                CenterWriteMultiColor(true, ("[B] ", ConsoleColor.Red, 0), ("Back to Main Menu", ConsoleColor.DarkGray, 0));
                Console.WriteLine();
                CenterWriteMultiColor(false, ("[ > ] ", ConsoleColor.Red, 0));
                string? choice = Console.ReadLine()?.ToUpper();

                switch (choice)
                {
                    case "1":
                        if (visualEffectsDisabled) EnableVisualEffects(); else DisableVisualEffects();
                        break;
                    case "2":
                        if (highPerfActive) SetBalancedPowerPlan(); else SetHighPerformancePowerPlan();
                        break;
                    case "3":
                        RunCommand("reg", "add HKLM\\SOFTWARE\\Microsoft\\PolicyManager\\default\\ApplicationManagement\\AllowGameDVR /v value /t REG_DWORD /d 0 /f");
                        break;
                    case "4":
                        RunCommand("reg", "add HKEY_CURRENT_USER\\Software\\Microsoft\\GameBar /v AllowAutoGameMode /t REG_DWORD /d 1 /f");
                        break;
                    case "B":
                        return;
                    default:
                        CenterWriteLine("Invalid choice.", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        continue;
                }
                CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
                Console.ReadKey();
            }
        }

        static void DisableVisualEffects()
        {
            Console.Clear();
            CenterWriteLine("--- Disabling Visual Effects ---", ConsoleColor.Red);
            Console.WriteLine();
            RunCommand("reg", "add \"HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects\" /v VisualFxSetting /t REG_DWORD /d 2 /f");
            RunCommand("reg", "add \"HKEY_CURRENT_USER\\Control Panel\\Desktop\" /v UserPreferencesMask /t REG_BINARY /d 9012038010000000 /f");
            Console.WriteLine();
            CenterWriteLine("Visual effects disabled for best performance.", ConsoleColor.Green);
            CenterWriteLine("A restart may be required for all changes to take effect.", ConsoleColor.Yellow);
        }

        static void EnableVisualEffects()
        {
            Console.Clear();
            CenterWriteLine("--- Enabling Visual Effects ---", ConsoleColor.Red);
            Console.WriteLine();
            RunCommand("reg", "add \"HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects\" /v VisualFxSetting /t REG_DWORD /d 1 /f");
            RunCommand("reg", "add \"HKEY_CURRENT_USER\\Control Panel\\Desktop\" /v UserPreferencesMask /t REG_BINARY /d 9E2E078012000000 /f");
            Console.WriteLine();
            CenterWriteLine("Visual effects restored to default.", ConsoleColor.Green);
            CenterWriteLine("A restart may be required for all changes to take effect.", ConsoleColor.Yellow);
        }

        static void SetHighPerformancePowerPlan()
        {
            Console.Clear();
            CenterWriteLine("--- Setting High-Performance Power Plan ---", ConsoleColor.Red);
            Console.WriteLine();
            RunCommand("powercfg", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            Console.WriteLine();
            CenterWriteLine("High-Performance power plan has been activated.", ConsoleColor.Green);
        }

        static void SetBalancedPowerPlan()
        {
            Console.Clear();
            CenterWriteLine("--- Setting Balanced Power Plan ---", ConsoleColor.Red);
            Console.WriteLine();
            RunCommand("powercfg", "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e");
            Console.WriteLine();
            CenterWriteLine("Balanced (default) power plan has been activated.", ConsoleColor.Green);
        }

        private static bool IsTelemetryDisabled()
        {
            try
            {
                using (var p = new Process
                {
                    StartInfo =
                    {
                        FileName = "reg",
                        Arguments = "query \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v \"AllowTelemetry\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                })
                {
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return p.ExitCode == 0 && output.Contains("REG_DWORD") && output.Trim().EndsWith("0x0");
                }
            }
            catch
            {
                return false;
            }
        }

        static void Privacy()
        {
            while (true)
            {
                Console.Clear();
                CenterWriteLine("--- Privacy Tweaks ---", ConsoleColor.Red);
                Console.WriteLine();

                bool isCurrentlyDisabled = IsTelemetryDisabled();
                string menuText = isCurrentlyDisabled ? "Enable Telemetry (Default)" : "Disable Telemetry";
                ConsoleColor menuColor = isCurrentlyDisabled ? ConsoleColor.Green : ConsoleColor.Cyan;

                CenterWriteMultiColor(true, ("[1] ", ConsoleColor.Red, 0), (menuText, menuColor, 40));
                CenterWriteMultiColor(true, ("[2] ", ConsoleColor.Red, 0), ("Disable Advertising ID", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[3] ", ConsoleColor.Red, 0), ("Clear Clipboard History", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[B] ", ConsoleColor.Red, 0), ("Back to Main Menu", ConsoleColor.DarkGray, 0));
                Console.WriteLine();
                CenterWriteMultiColor(false, ("[ > ] ", ConsoleColor.Red, 0));
                string? choice = Console.ReadLine()?.ToUpper();

                switch (choice)
                {
                    case "1":
                        if (isCurrentlyDisabled) EnableTelemetry(); else DisableTelemetry();
                        break;
                    case "2":
                        RunCommand("reg", "add HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo /v Enabled /t REG_DWORD /d 0 /f");
                        break;
                    case "3":
                        RunCommand("powershell", "-Command \"Clear-Clipboard -Confirm:$false\"");
                        break;
                    case "B":
                        return;
                    default:
                        CenterWriteLine("Invalid choice.", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        continue;
                }
                CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
                Console.ReadKey();
            }
        }

        static void DisableTelemetry()
        {
            Console.Clear();
            CenterWriteLine("--- Disabling Telemetry ---", ConsoleColor.Red);
            Console.WriteLine();

            var commands = new[]
            {
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v \"AllowTelemetry\" /t REG_DWORD /d 0 /f",
                "sc config DiagTrack start= disabled",
                "sc stop DiagTrack",
                "sc config dmwappushservice start= disabled",
                "sc stop dmwappushservice"
            };

            foreach (var command in commands)
            {
                RunCommand("cmd.exe", $"/C {command}");
            }

            Console.WriteLine();
            CenterWriteLine("Telemetry services have been disabled.", ConsoleColor.Green);
        }

        static void EnableTelemetry()
        {
            Console.Clear();
            CenterWriteLine("--- Enabling Telemetry ---", ConsoleColor.Red);
            Console.WriteLine();

            var commands = new[]
            {
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v \"AllowTelemetry\" /f",
                "sc config DiagTrack start= auto",
                "sc start DiagTrack",
                "sc config dmwappushservice start= auto",
                "sc start dmwappushservice"
            };

            foreach (var command in commands)
            {
                RunCommand("cmd.exe", $"/C {command}");
            }

            Console.WriteLine();
            CenterWriteLine("Telemetry services have been restored to default.", ConsoleColor.Green);
        }

        static void Advanced()
        {
            while (true)
            {
                Console.Clear();
                CenterWriteLine("--- Advanced Tweaks ---", ConsoleColor.Red);
                Console.WriteLine();
                CenterWriteMultiColor(true, ("[1] ", ConsoleColor.Red, 0), ("Optimize Process Priorities (Low)", ConsoleColor.Cyan, 0));
                CenterWriteMultiColor(true, ("[2] ", ConsoleColor.Red, 0), ("Aggressive Optimization (Idle)", ConsoleColor.DarkCyan, 0));
                CenterWriteMultiColor(true, ("[3] ", ConsoleColor.Red, 0), ("Restore Process Priorities (Normal)", ConsoleColor.Green, 0));
                CenterWriteMultiColor(true, ("[B] ", ConsoleColor.Red, 0), ("Back to Main Menu", ConsoleColor.DarkGray, 0));
                Console.WriteLine();
                CenterWriteMultiColor(false, ("[ > ] ", ConsoleColor.Red, 0));
                string? choice = Console.ReadLine()?.ToUpper();

                switch (choice)
                {
                    case "1":
                        SetProcessPriority("low", "Low");
                        CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
                        Console.ReadKey();
                        break;
                    case "2":
                        SetProcessPriority("idle", "Idle");
                        CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
                        Console.ReadKey();
                        break;
                    case "3":
                        SetProcessPriority("normal", "Normal");
                        CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
                        Console.ReadKey();
                        break;
                    case "B":
                        return;
                    default:
                        CenterWriteLine("Invalid choice.", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        static void SetProcessPriority(string priorityLevel, string friendlyName)
        {
            Console.Clear();
            CenterWriteLine($"--- Setting Process Priority to {friendlyName} ---", ConsoleColor.Red);
            Console.WriteLine();

            int count = 0;
            foreach (var procName in MonitoredProcesses)
            {
                Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(procName));
                if (processes.Length > 0)
                {
                    RunCommand("wmic", $"process where name='{procName}' CALL setpriority \"{priorityLevel}\"");
                    count++;
                }
            }

            Console.WriteLine();
            if (count > 0)
            {
                CenterWriteLine($"Set {count} background processes to {friendlyName} priority.", ConsoleColor.Green);
            }
            else
            {
                CenterWriteLine($"No running background processes from the list were found to set to {friendlyName}.", ConsoleColor.Yellow);
            }
        }
        static void Info()
        {
            Console.Clear();
            CenterWriteLine("--- KingMod Tweak Info ---", ConsoleColor.Red);
            Console.WriteLine();
            CenterWriteMultiColor(true, ("     App Name: ", ConsoleColor.Cyan, 0), ("KingMod Tweak", ConsoleColor.White, 0));
            CenterWriteMultiColor(true, ("      Version: ", ConsoleColor.Cyan, 0), ("1.2", ConsoleColor.White, 0));
            CenterWriteMultiColor(true, ("    Created By: ", ConsoleColor.Cyan, 0), ("KingMod", ConsoleColor.White, 0));
            Console.WriteLine();
            CenterWriteLine("Thank you for using the tool!", ConsoleColor.Yellow);
            Console.WriteLine();
            CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
            Console.ReadKey();
        }

        static void Backup()
        {
            CenterWriteLine("Creating a system restore point...", ConsoleColor.Yellow);
            CenterWriteLine("This may take a few moments.", ConsoleColor.DarkGray);
            try
            {
                var process = new Process { StartInfo = new ProcessStartInfo { FileName = "powershell.exe", Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"& { Checkpoint-Computer -Description 'KingModTweak Backup' -RestorePointType 'MODIFY_SETTINGS' }\"", UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true } };
                process.Start();
                
                string[] spinner = { "|", "/", "-", "\\" };
                int i = 0;
                while (!process.HasExited)
                {
                    int currentLeft = Console.CursorLeft;
                    int currentTop = Console.CursorTop;
                    Console.Write(spinner[i % spinner.Length]);
                    Console.SetCursorPosition(currentLeft, currentTop);
                    i++;
                    Thread.Sleep(100);
                }

                if (process.ExitCode == 0)
                {
                    CenterWriteLine("System restore point created successfully!", ConsoleColor.Green);
                }
                else
                {
                    CenterWriteLine("Failed to create system restore point.", ConsoleColor.Red);
                    CenterWriteLine("Make sure System Protection is enabled on your system drive.", ConsoleColor.DarkRed);
                }
            }
            catch (Exception ex)
            {
                CenterWriteLine($"An error occurred: {ex.Message}", ConsoleColor.Red);
            }
        }
        
        static void CleanTemporaryFiles()
        {
            CenterWriteLine("--- Cleaning Temporary Files ---", ConsoleColor.Red);
            string[] tempPaths = { Path.GetTempPath(), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp") };
            long totalDeletedFiles = 0;
            long totalDeletedSize = 0;

            foreach (string path in tempPaths)
            {
                CenterWriteLine($"\nScanning folder: {path}", ConsoleColor.Yellow);
                var dir = new DirectoryInfo(path);
                if (!dir.Exists) continue;

                try
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        try
                        {
                            long fileSize = file.Length;
                            file.Delete();
                            totalDeletedFiles++;
                            totalDeletedSize += fileSize;
                            CenterWriteLine($"Deleted: {file.Name}", ConsoleColor.DarkGray);
                        } catch { CenterWriteLine($"Skipped (in use): {file.Name}", ConsoleColor.DarkRed); }
                    }

                    foreach (DirectoryInfo subDir in dir.GetDirectories())
                    {
                        try
                        {
                            long dirSize = GetDirectorySize(subDir);
                            subDir.Delete(true);
                            totalDeletedFiles++; // Counting directory as one item
                            totalDeletedSize += dirSize;
                            CenterWriteLine($"Deleted folder: {subDir.Name}", ConsoleColor.DarkGray);
                        } catch { CenterWriteLine($"Skipped (in use): {subDir.Name}", ConsoleColor.DarkRed); }
                    }
                }
                catch (Exception ex)
                {
                    CenterWriteLine($"Error accessing {path}: {ex.Message}", ConsoleColor.Red);
                }
            }

            Console.WriteLine();
            CenterWriteLine("--- Cleanup Complete ---", ConsoleColor.Green);
            CenterWriteLine($"Deleted {totalDeletedFiles} files and folders.", ConsoleColor.Cyan);
            CenterWriteLine($"Freed up {totalDeletedSize / 1024 / 1024} MB of space.", ConsoleColor.Cyan);
        }

        // --- System Info & Helpers ---
        static bool IsAdministrator() => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        static string GetManagementObjectProperty(string wmiClass, string property)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"select {property} from {wmiClass}"))
                {
                    return searcher.Get().Cast<ManagementObject>().First()[property]?.ToString()?.Trim() ?? "N/A";
                }
            }
            catch { return "N/A"; }
        }

        static void RunCommand(string command, string args)
        {
            CenterWriteMultiColor(false, ("[CMD] ", ConsoleColor.DarkYellow, 0), ($"{command} {args}", ConsoleColor.DarkGray, 0));
            Console.WriteLine();
            try
            {
                using (var p = new Process
                {
                    StartInfo =
                    {
                        FileName = command,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                })
                {
                    p.Start();
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                CenterWriteLine($"Error executing command: {ex.Message}", ConsoleColor.Red);
            }
        }
        
        static long GetDirectorySize(DirectoryInfo dir) => dir.GetFiles().Sum(fi => fi.Length) + dir.GetDirectories().Sum(di => GetDirectorySize(di));

        static void Spoofing()
        {
            while (true)
            {
                Console.Clear();
                CenterWriteLine("--- Spoofing & Unban ---", ConsoleColor.Red);
                Console.WriteLine();
                CenterWriteMultiColor(true, ("[1] ", ConsoleColor.Red, 0), ("IP & DNS Flush", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[2] ", ConsoleColor.Red, 0), ("Spoof MAC Address", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[3] ", ConsoleColor.Red, 0), ("Spoof HWID (MachineGuid)", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[B] ", ConsoleColor.Red, 0), ("Back to Main Menu", ConsoleColor.DarkGray, 0));
                Console.WriteLine();
                CenterWriteMultiColor(false, ("[ > ] ", ConsoleColor.Red, 0));
                string? choice = Console.ReadLine()?.ToUpper();

                switch (choice)
                {
                    case "1":
                        RunCommand("ipconfig", "/release");
                        RunCommand("ipconfig", "/renew");
                        RunCommand("ipconfig", "/flushdns");
                        break;
                    case "2":
                        SpoofMacAddress();
                        break;
                    case "3":
                        RunCommand("reg", $"add HKLM\\SOFTWARE\\Microsoft\\Cryptography /v MachineGuid /t REG_SZ /d {Guid.NewGuid()} /f");
                        break;
                    case "B":
                        return;
                    default:
                        CenterWriteLine("Invalid choice.", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        continue;
                }
                CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
                Console.ReadKey();
            }
        }

        static void SpoofMacAddress()
        {
            CenterWriteLine("Spoofing MAC address...", ConsoleColor.Yellow);
            string newMac = string.Join("", Enumerable.Range(0, 6).Select(i => new Random().Next(0, 255).ToString("X2")));
            RunCommand("reg", $"add HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Class\\{{4d36e972-e325-11ce-bfc1-08002be10318}}\\0001 /v NetworkAddress /t REG_SZ /d {newMac} /f");
            CenterWriteLine($"MAC address spoofed to {newMac}. A restart is required.", ConsoleColor.Green);
        }

        static void Utilities()
        {
            while (true)
            {
                Console.Clear();
                CenterWriteLine("--- System Utilities ---", ConsoleColor.Red);
                Console.WriteLine();
                CenterWriteMultiColor(true, ("[1] ", ConsoleColor.Red, 0), ("Disk Cleanup", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[2] ", ConsoleColor.Red, 0), ("System File Checker", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[3] ", ConsoleColor.Red, 0), ("Show IP Configuration", ConsoleColor.Cyan, 40));
                CenterWriteMultiColor(true, ("[B] ", ConsoleColor.Red, 0), ("Back to Main Menu", ConsoleColor.DarkGray, 0));
                Console.WriteLine();
                CenterWriteMultiColor(false, ("[ > ] ", ConsoleColor.Red, 0));
                string? choice = Console.ReadLine()?.ToUpper();

                switch (choice)
                {
                    case "1":
                        RunCommand("cleanmgr.exe", "");
                        break;
                    case "2":
                        RunCommand("sfc", "/scannow");
                        break;
                    case "3":
                        RunCommand("ipconfig", "/all");
                        break;
                    case "B":
                        return;
                    default:
                        CenterWriteLine("Invalid choice.", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        continue;
                }
                CenterWriteLine("Press any key to return.", ConsoleColor.DarkGray);
                Console.ReadKey();
            }
        }

        static void CenterWriteLine(string text, ConsoleColor? color = null)
        {
            if (color.HasValue) Console.ForegroundColor = color.Value;
            if (Console.WindowWidth < text.Length) { Console.WriteLine(text); } else { Console.SetCursorPosition((Console.WindowWidth - text.Length) / 2, Console.CursorTop); Console.WriteLine(text); }
            if (color.HasValue) Console.ResetColor();
        }

        static void CenterWriteMultiColor(bool newLine, params (string text, ConsoleColor color, int padRight)[] segments)
        {
            var segmentList = segments.Select(s => (s.text.PadRight(s.padRight), s.color)).ToList();
            string fullText = string.Concat(segmentList.Select(s => s.Item1));
            int padding = (Console.WindowWidth > fullText.Length) ? (Console.WindowWidth - fullText.Length) / 2 : 0;
            Console.SetCursorPosition(padding, Console.CursorTop);
            foreach (var segment in segmentList)
            {
                Console.ForegroundColor = segment.color;
                Console.Write(segment.Item1);
            }
            Console.ResetColor();
            if(newLine) Console.WriteLine();
        }
    }
}
