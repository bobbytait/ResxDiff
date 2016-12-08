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

            string tempLoc = @"C:\source\git\PICO\TrimbleFieldLink-a\Foundation.SharedResources\Properties";

            StringResourceTable srtNew = new StringResourceTable(tempLoc);

            // --- new dir ---

            // in the "new" dir, determine how many language resx's are used

            // load the default resx (text file)

            // build a table to accomodate the default plus the additional resx's

            // populate the table with the default resx data

            // sort it by ID string

            // foreach additional resx

            //  // load the resx (text file)

            //  // foreach string ID

            //  //  // find default ID match

            //  //  // copy string to corresponding cell in table

            // in the "new" dir, determine how many language resx's are used

            // load the default resx (text file)

            // build a table to accomodate the default plus the additional resx's

            // populate the table with the default resx data

            // sort it by ID string

            // foreach additional resx

            //  // load the resx (text file)

            //  // foreach string ID

            //  //  // find default ID match

            //  //  // copy string to corresponding cell in table


            // --- old dir ---

            // (repeat steps from new dir)


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
