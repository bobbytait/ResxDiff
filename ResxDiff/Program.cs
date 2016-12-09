using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxDiff
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings settings = new Settings(args);

            // Temp dev code
            string tempNewLoc = null, tempOldLoc = null;
            if (Environment.MachineName == "BSANTOR-US-LE02")
            {
                tempNewLoc = @"C:\Source\PICO\TrimbleFieldLink\Foundation.SharedResources\Properties";
                tempOldLoc = @"C:\Source\PICO\TrimbleFieldLink-a\Foundation.SharedResources\Properties";
            }
            else
            {
                tempNewLoc = @"C:\source\git\PICO\TrimbleFieldLink-a\Foundation.SharedResources\Properties";
                tempOldLoc = @"";
            }

            // Build a string resource table from the code base's resx files
            StringResourceTable srtNew = new StringResourceTable(tempNewLoc, tempOldLoc);


            // --- compare ---

            // foreach cell in "new"

            //  // compare to corresponding cell in "old"

            //  // Report if string IDs don't match exactly

            //  // Report if default strings don't match exactly

            //  // Report if translated strings don't match exactly


            // --- output ---

            // Output report to file

            // Finish up
            if (settings.IsWaitForKeypressOnFinish)
            {
                Console.ReadLine();
            }
        }
    }
}
