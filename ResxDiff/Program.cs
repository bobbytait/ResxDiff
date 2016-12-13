using System;


namespace ResxDiff
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!Settings.ProcessArgs(args))
            {
                // Error messages written inside above function
                return 1;
            }

            // Temp dev code; get the code from git
            Settings.OldResxDir = @"C:\source\git\PICO\TrimbleFieldLink-b\Foundation.SharedResources\Properties";

            // Build a string resource table from the code base's new & old resx files
            if (!StringResourceTable.Initialize())
            {
                // Error messages written inside above function
                return 1;
            }

            if (!StringResourceTable.ImportNewResxData())
            {
                // Error messages written inside above function
                return 1;
            }

            int testResults = StringResourceTable.ImportAndProcessOldResxData();
            switch (testResults)
            {
                case 0:
                    break;
                case 1:
                    break;
                default:
                    break;
            }

            if (!StringResourceTable.OutputResults())
            {
                // Error messages written inside above function
                return 1;
            }

            Console.WriteLine("\nDone!");

            // Finish up
            if (Settings.IsWaitForKeypressOnFinish)
            {
                Console.ReadLine();
            }

            return (Settings.IsReturnFailureOnDiscrepency) ? testResults : 0;
        }
    }
}
