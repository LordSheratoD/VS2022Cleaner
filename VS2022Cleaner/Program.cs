using System;

namespace VS2022Cleaner
{
    internal class Program
    {
        private static void Main()
        {
            if (!AdministratorChecker.IsAdministrator())
            {
                AdministratorChecker.RestartWithAdminRights();
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Restoring Visual Studio 2022 to its original state...");

            var devenvPath = VisualStudioManager.FindDevenvPath();
            if (string.IsNullOrEmpty(devenvPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find devenv.exe. Make sure Visual Studio 2022 is installed.");
                return;
            }

            VisualStudioManager.CloseVisualStudio();
            VisualStudioManager.WaitForVisualStudioToClose();
            VisualStudioManager.ClearLocalCache();
            VisualStudioManager.ResetVisualStudioSettings(devenvPath);
            VisualStudioManager.CloseVisualStudio();
            VisualStudioManager.WaitForVisualStudioToClose();
            
            VisualStudioManager.ClearLocalCache();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Process completed. Wait a moment.");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nYou can close the program now!");
            Console.ReadKey();
        }
    }
}