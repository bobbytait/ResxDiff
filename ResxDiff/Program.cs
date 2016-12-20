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

            // Build a string resource table from the code base's new & old resx files
            if (!StringResourceTable.Initialize())
            {
                // Error messages written inside above function
                return 1;
            }

            Console.WriteLine("Loading & processing resx data...");

            if (!StringResourceTable.ImportNewResxData())
            {
                // Error messages written inside above function
                return 1;
            }

            if (!StringResourceTable.ImportOldResxData())
            {
                // Error messages written inside above function
                return 1;
            }

            int testResults = StringResourceTable.CompareResxData();

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
