using System;
using System.IO;


namespace ResxDiff
{
    public static class Settings
    {
        public const string DEFAULT_RESX_FILENAME = "Resources.resx";

        // Directory containing newer resx file
        public static string NewResxDir = null;

        // --- Undeveloped switches, but variables in use ---

        // If true, output a report on this type of finding
        public static bool IsReportDuplicateIds = true;
        public static bool IsReportMismatches = true;
        public static bool IsReportEmptyStrings = true;
        public static bool IsReportAdds = true;
        public static bool IsReportDeletes = true;
        public static bool IsReportMatches = false;

        // If true, return an error code on this type of finding
        public static bool IsErrorOnDuplicateIds = true;
        public static bool IsErrorOnMismatches = true;

        // If true, after running, waits for a keypress to return to caller
        public static bool IsWaitForKeypressOnFinish = false;

        // --- As yet undeveloped switches ---

        // Directory of this repository's local root
        public static string GitRootPath = null;

        // Git branch to compare with
        public static string GitBranch = null;

        // Directory containing older resx files we want to compare with
        public static string OldResxDir = null;

        // Write report to screen?
        public static bool IsReportToScreen = true;

        // Write report to text file?
        public static bool IsReportToFile = false;

        // Path to report file
        public static string ReportPath = null;

        // If a default language string has changed, clear its translations from the translation
        // resx files. Will require writing to resx files.
        public static bool IsClearObsoleteTranslations = false;

        // For scripting, return a non-zero code if any discrepencies found
        public static bool IsReturnFailureOnDiscrepency = true;

        public static bool IsShowProgress = false;

        public static int Verbosity = 0;

        // Need a value to determine when/not to return 0, depending on results

        //static Settings()
        //{
        //}

        public static bool ProcessArgs(string[] args)
        {
            Console.WriteLine("Arguments: {0}", args);

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

                // After running, waits for a keypress to return to comnmand prompt
                else
                if (argName == "/wait")
                {
                    IsWaitForKeypressOnFinish = true;
                    continue;
                }
            }

            // TODO: Set up any default values, if we need to

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

            return true;
        }
    }
}
