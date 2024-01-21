using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace VS2022Cleaner
{
    internal static class VisualStudioManager
    {
        public static void CloseVisualStudio()
        {
            Logger.Log(Logger.LogLevel.INFO, "Closing all instances of Visual Studio...");
            KillAllVisualStudioInstances();
            WaitForVisualStudioToClose();
        }

        private static void KillAllVisualStudioInstances()
        {
            foreach (var process in Process.GetProcessesByName("devenv"))
            {
                try
                {
                    process.Kill();
                    Logger.Log(Logger.LogLevel.DEBUG, $"Killed Visual Studio process with ID {process.Id}");
                }
                catch (Exception ex)
                {
                    Logger.Log(Logger.LogLevel.ERROR, $"Failed to kill process {process.Id}: {ex.Message}");
                }
            }
        }

        public static string FindDevenvPath()
        {
            string[] possiblePaths = {
                @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE",
                @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE",
                @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE"
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.Combine(path, "devenv.exe");
                if (File.Exists(fullPath))
                {
                    Logger.Log(Logger.LogLevel.DEBUG, $"Found devenv.exe at {fullPath}");
                    return fullPath;
                }
            }

            Logger.Log(Logger.LogLevel.ERROR, "devenv.exe not found in standard locations.");
            return null;
        }

        public static void ResetVisualStudioSettings(string devenvPath)
        {
            Logger.Log(Logger.LogLevel.INFO, "Resetting the Visual Studio configuration...");
            ExecuteCommand(devenvPath, "/ResetSettings General /Command Exit");
        }
        
        public static void ClearLocalCache()
        {
            Logger.Log(Logger.LogLevel.INFO, "Clearing the local cache...");
            DeleteFilesInDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\VisualStudio");
            DeleteFilesInDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\VisualStudio");
        }

        private static void ExecuteCommand(string command, string arguments, int timeoutMilliseconds = 15000)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    if (!process.WaitForExit(timeoutMilliseconds))
                    {
                        Logger.Log(Logger.LogLevel.WARN, "Command is taking too long, attempting to kill the process...");
                        process.Kill();
                    }
                }
                else
                {
                    Logger.Log(Logger.LogLevel.ERROR, "Failed to start the process.");
                }
            }
        }


        private static void DeleteFilesInDirectory(string path)
        {
            Logger.Log(Logger.LogLevel.DEBUG, $"Searching Path: {path}");
            if (Directory.Exists(path))
            {
                Logger.Log(Logger.LogLevel.DEBUG, "Path Found!");
                var di = new DirectoryInfo(path);
                foreach (var file in di.GetFiles())
                {
                    Logger.Log(Logger.LogLevel.TRACE, $"Deleting File: {file.FullName}");
                    try
                    {
                        file.Delete();
                        Logger.Log(Logger.LogLevel.INFO, $"File Deleted: {file.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(Logger.LogLevel.ERROR, $"Failed to Delete File: {file.FullName}. Error: {ex.Message}");
                    }
                }

                foreach (var dir in di.GetDirectories())
                {
                    Logger.Log(Logger.LogLevel.TRACE, $"Deleting Directory: {dir.FullName}");
                    try
                    {
                        dir.Delete(true);
                        Logger.Log(Logger.LogLevel.INFO, $"Directory Deleted: {dir.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(Logger.LogLevel.ERROR, $"Failed to Delete Directory: {dir.FullName}. Error: {ex.Message}");
                    }
                }
            }
            else
            {
                Logger.Log(Logger.LogLevel.WARN, "Path Not Found!");
            }
        }
        
        public static void WaitForVisualStudioToClose()
        {
            Logger.Log(Logger.LogLevel.TRACE,"Waiting for all Visual Studio instances to close...");

            int maxWaitTime = 30000; // Tiempo máximo de espera en milisegundos (30 segundos)
            int waitInterval = 1000; // Intervalo de espera en milisegundos (1 segundo)
            int totalTimeWaited = 0;

            while (IsVisualStudioRunning())
            {
                if (totalTimeWaited >= maxWaitTime)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.Log(Logger.LogLevel.ERROR, "Visual Studio could not be closed after 30 seconds.");
                    break;
                }

                Thread.Sleep(waitInterval);
                totalTimeWaited += waitInterval;
            }
        }

        private static bool IsVisualStudioRunning()
        {
            return Process.GetProcessesByName("devenv").Length > 0;
        }

        private static bool IsChildProcessOfDevenv(Process process)
        {
            try
            {
                var parent = GetParentProcess(process.Id);
                return parent != null && parent.ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static Process GetParentProcess(int id)
        {
            var process = Process.GetProcessById(id);
            var processHandle = ProcessNativeMethods.OpenProcess(ProcessNativeMethods.PROCESS_QUERY_INFORMATION, false, process.Id);

            if (processHandle == IntPtr.Zero)
                return null;

            try
            {
                ProcessNativeMethods.PROCESS_BASIC_INFORMATION pbi = new ProcessNativeMethods.PROCESS_BASIC_INFORMATION();
                int returnLength;
                int status = ProcessNativeMethods.NtQueryInformationProcess(processHandle, 0, ref pbi, pbi.Size, out returnLength);
                if (status != 0)
                    return null;

                return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
            }
            catch
            {
                return null;
            }
            finally
            {
                ProcessNativeMethods.CloseHandle(processHandle);
            }
        }
    }
}
