using System;
using System.IO;


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
        // (Not fully implemented)
        public static bool IsReportDuplicateIds = true;
        public static bool IsReportMismatches = true;
        public static bool IsReportEmptyStrings = true;
        public static bool IsReportAdds = true;
        public static bool IsReportDeletes = true;
        public static bool IsReportMatches = false;

        // If true, after running, waits for a keypress to return to caller
        public static bool IsWaitForKeypressOnFinish = false;

        // If true, return a non-zero error code to the caller when a discrepency is encountered
        // (Not fully implemented)
        public static bool IsReturnFailureOnDiscrepency = false;


        public static bool ProcessArgs(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string argName = args[i].ToLower();

                    // Path to the latest resources.resx file
                    if (argName == "/newpath")
                    {
                        NewResxDir = args[++i];

                        if ((NewResxDir == null) || (NewResxDir == String.Empty) || NewResxDir.StartsWith("/"))
                        {
                            return ErrorHandling.OutputError(String.Format("Invalid '/newpath': {0}", NewResxDir));
                        }

                        if (!Directory.Exists(NewResxDir))
                        {
                            return ErrorHandling.OutputError(String.Format("Directory does not exist: {0}", NewResxDir));
                        }

                        if (!File.Exists(Path.Combine(NewResxDir, DEFAULT_RESX_FILENAME)))
                        {
                            return ErrorHandling.OutputError(String.Format("{0} does not exist in: {1}", DEFAULT_RESX_FILENAME, NewResxDir));
                        }

                        continue;
                    }

                    // Path to the resources.resx file to compare to
                    if (argName == "/oldpath")
                    {
                        OldResxDir = args[++i];

                        if ((OldResxDir == null) || (OldResxDir == String.Empty) || OldResxDir.StartsWith("/"))
                        {
                            return ErrorHandling.OutputError(String.Format("Invalid '/oldpath': {0}", OldResxDir));
                        }

                        if (OldResxDir.Equals(NewResxDir))
                        {
                            return ErrorHandling.OutputError(String.Format("'/oldpath' cannot be the same as: {0}", NewResxDir));
                        }

                        if (!Directory.Exists(OldResxDir))
                        {
                            return ErrorHandling.OutputError(String.Format("Directory does not exist: {0}", OldResxDir));
                        }

                        if (!File.Exists(Path.Combine(OldResxDir, DEFAULT_RESX_FILENAME)))
                        {
                            return ErrorHandling.OutputError(String.Format("{0} does not exist in: {1}", DEFAULT_RESX_FILENAME, OldResxDir));
                        }

                        continue;
                    }

                    // After running, waits for a keypress to return to comnmand prompt
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
                        return ErrorHandling.OutputError(String.Format("{0} does not exist in: {1}", DEFAULT_RESX_FILENAME, NewResxDir));
                    }
                }

                if (OldResxDir == null)
                {
                    return ErrorHandling.OutputError("'/oldpath' parameter is required");
                }
            }
            catch (Exception e)
            {
                return ErrorHandling.OutputError("Occurred processing command line parameters", e);
            }

            return true;
        }
    }
}
