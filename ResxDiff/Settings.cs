using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxDiff
{
    class Settings
    {
        // Directory of this repository's local root
        public string GitRootPath = null;

        // Git branch to compare with
        public string GitBranch = null;

        // Directory containing newer resx files
        public string NewResxDir = null;

        // Directory containing older resx files we want to compare with
        public string OldResxDir = null;

        // Write report to screen?
        public bool IsReportToScreen = false;

        // Write report to text file?
        public bool IsReportToFile = false;

        // Path to report file
        public string ReportPath = null;

        // If a default language string has changed, clear its translations from the translation
        // resx files. Will require writing to resx files
        public bool IsClearObsoleteTranslations = false;

        // For scripting, return a non-zero code is any discrepencies found
        public bool IsReturnFailureOnDiscrepency = false;

        public bool IsWaitForKeypressOnFinish = false;

        public int Verbosity = 0;

        // Need a value to determine when/not to return 0, depending on results

        public Settings(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Arguments: <none>");

                // TODO: Set up any default values, if we need to

                return;
            }

            Console.WriteLine("Arguments: {0}", args);

            foreach (string arg in args)
            {
                string argl = arg.ToLower();
                if (argl == "/w")
                {
                    // W: After running, waits for a keypress to return to comnmand prompt
                    IsWaitForKeypressOnFinish = true;
                    continue;
                }
                else if (argl == "/b")
                {
                    // Set property
                    continue;
                }
                else if (argl == "/c")
                {
                    // Set property
                    continue;
                }
            }
        }
    }
}
