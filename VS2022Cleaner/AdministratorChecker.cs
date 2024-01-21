using System;
using System.Diagnostics;
using System.Security.Principal;

namespace VS2022Cleaner
{
    internal static class AdministratorChecker
    {
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartWithAdminRights()
        {
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
            {
                Verb = "runas",
                UseShellExecute = true
            };
            try
            {
                Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Logger.Log(Logger.LogLevel.ERROR, "Administrator's privileges not granted.");
            }
            Environment.Exit(0);
        }
    }
}