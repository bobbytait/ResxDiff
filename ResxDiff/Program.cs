using System;
using System.Data;


namespace ResxDiff
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings settings = new Settings(args);

            // Temp dev code
            string tempNewLoc = null, tempOldLoc = null;
            tempNewLoc = @"C:\source\git\PICO\TrimbleFieldLink-a\Foundation.SharedResources\Properties";
            tempOldLoc = @"C:\source\git\PICO\TrimbleFieldLink-b\Foundation.SharedResources\Properties";

            // Build a string resource table from the code base's new & old resx files
            StringResourceTable stringResourceTable = new StringResourceTable(tempNewLoc, tempOldLoc);

            if (!stringResourceTable.IsValid)
            {
                // TODO: Display an error? Or will we do that already?
                return;
            }

            stringResourceTable.TestStrings();


            stringResourceTable.OutputResults();


            Console.WriteLine("\nDone!");

            // Finish up
            if (settings.IsWaitForKeypressOnFinish)
            {
                Console.ReadLine();
            }
        }
    }
}
