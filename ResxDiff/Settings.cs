using System;


namespace ResxDiff
{
    public static class Settings
    {
        //
        public static string[] args = null;

        // Directory of this repository's local root
        public static string GitRootPath = null;

        // Git branch to compare with
        public static string GitBranch = null;

        // Directory containing newer resx files
        public static string NewResxDir = null;

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

        public static bool IsWaitForKeypressOnFinish = false;

        public static bool IsShowProgress = false;

        public static bool IsReportMismatches = true;
        public static bool IsReportAdds= true;
        public static bool IsReportDeletes = true;
        public static bool IsReportMatches = false;

        public static bool IsErrorOnMismatches = true;
        public static bool IsErrorOnAdds = false;
        public static bool IsErrorOnDeletes = false;

        public static int Verbosity = 0;

        // Need a value to determine when/not to return 0, depending on results

        //static Settings()
        //{
        //    if (args.Length < 1)
        //    {
        //        Console.WriteLine("Arguments: <none>");

        //        // TODO: Set up any default values, if we need to

        //        return;
        //    }

        //    Console.WriteLine("Arguments: {0}", args);

        //    foreach (string arg in args)
        //    {
        //        string argl = arg.ToLower();
        //        if (argl == "/w")
        //        {
        //            // W: After running, waits for a keypress to return to comnmand prompt
        //            IsWaitForKeypressOnFinish = true;
        //            continue;
        //        }
        //        else if (argl == "/b")
        //        {
        //            // Set property
        //            continue;
        //        }
        //        else if (argl == "/c")
        //        {
        //            // Set property
        //            continue;
        //        }
        //    }
        //}
    }
}
