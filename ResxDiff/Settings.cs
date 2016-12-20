using System;
using System.IO;
using System.Net;


namespace ResxDiff
{
    public static class Settings
    {
        public const string DEFAULT_RESX_FILENAME = "Resources.resx";

        // Directory containing newer resx file
        public static string NewResxDir = null;

        // Directory containing older resx files we want to compare with
        public static string OldResxDir = null;

        // If true, output a report on this type of finding
        // These variables are set as options; maybe later we'll make them switchable
        public static bool IsReportDuplicateIds = true;
        public static bool IsReportMismatches = true;
        public static bool IsReportEmptyStrings = true;
        public static bool IsReportAdds = true;
        public static bool IsReportDeletes = true;
        public static bool IsReportMatches = false;

        // If true, after running, waits for a keypress to return to caller
        public static bool IsWaitForKeypressOnFinish = false;


        public static bool ProcessArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string argName = args[i].ToLower();

                // Path to the latest resources.resx file
                if (argName == "/newpath")
                {
                    NewResxDir = args[++i];
                    if (!Directory.Exists(NewResxDir))
                    {
                        Console.WriteLine(" [ERROR] Directory does not exist: {0}", NewResxDir);
                        return false;
                    }
                    if (!File.Exists(Path.Combine(NewResxDir, DEFAULT_RESX_FILENAME)))
                    {
                        Console.WriteLine(" [ERROR] {0} does not exist in: {1}", DEFAULT_RESX_FILENAME, NewResxDir);
                        return false;
                    }
                    continue;
                }

                // Path to the resources.resx file to compare to
                if (argName == "/oldpath")
                {
                    OldResxDir = args[++i];
                    if (OldResxDir.Equals(NewResxDir))
                    {
                        Console.WriteLine(" [ERROR] '/oldpath' cannot be the same as: {0}", NewResxDir);
                        return false;
                    }
                    if (!Directory.Exists(OldResxDir))
                    {
                        Console.WriteLine(" [ERROR] Directory does not exist: {0}", OldResxDir);
                        return false;
                    }
                    if (!File.Exists(Path.Combine(OldResxDir, DEFAULT_RESX_FILENAME)))
                    {
                        Console.WriteLine(" [ERROR] {0} does not exist in: {1}", DEFAULT_RESX_FILENAME, OldResxDir);
                        return false;
                    }
                    continue;
                }

                // After running, waits for a keypress to return to comnmand prompt
                else
                if (argName == "/wait")
                {
                    IsWaitForKeypressOnFinish = true;
                    continue;
                }
            }

            // If NewResxDir was not set above, assume "."
            if (NewResxDir == null)
            {
                NewResxDir = Environment.CurrentDirectory;
                if (!File.Exists(Path.Combine(NewResxDir, DEFAULT_RESX_FILENAME)))
                {
                    Console.WriteLine(" [ERROR] {0} does not exist in: {1}", DEFAULT_RESX_FILENAME, NewResxDir);
                    return false;
                }
            }

            if (OldResxDir == null)
            {
                Console.WriteLine(" [ERROR] '/oldpath' parameter is required");
                return false;
            }

            return true;
        }
    }
}
