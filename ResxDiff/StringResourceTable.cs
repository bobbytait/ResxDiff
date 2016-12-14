using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Resources;


namespace ResxDiff
{
    public enum ResultType
    {
        StringMatch = 0,
        StringMismatch,
        StringAdded,
        StringDeleted,
        StringsDuplicated,
        StringsEmpty
    }

    public static class StringResourceTable
    {
        private const char FAKE_CR = '»';

        public static DataTable Table = null;

        //private static string _oldResxFilePath = null;
        //private static string _oldDefaultResxFile = null;
        //private static int _oldDefaultColumnIndex = -1;

        public static bool Initialize()
        {
            try
            {
                // Create data table
                Table = new DataTable();
                Table.Columns.Add("ID", typeof(string));
                Table.Columns.Add("result", typeof(Int32));
                Table.Columns.Add("new", typeof(string));
                Table.Columns.Add("old", typeof(string));
            }
            catch (Exception e)
            {
                // TODO: Error handling
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public static bool ImportNewResxData()
        {
            try
            {
                // Get data from new default resx file
                Console.WriteLine("Loading new default resx file...");
                string resxFile = Path.Combine(Settings.NewResxDir, Settings.DEFAULT_RESX_FILENAME);
                ResXResourceReader reader = new ResXResourceReader(resxFile);

                Environment.CurrentDirectory = Settings.NewResxDir;

                Console.WriteLine("Populating table with new default resx entries...");
                DataRow row;
                foreach (DictionaryEntry entry in reader)
                {
                    if (entry.Value is String)
                    {
                        row = Table.NewRow();
                        row["ID"] = entry.Key.ToString();
                        row["result"] = -1;
                        row["new"] = entry.Value.ToString();
                        Table.Rows.Add(row);
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
                // TODO: Error handling
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public static int ImportAndProcessOldResxData()
        {
            try
            {
                // Get data from old default resx file
                Console.WriteLine("Loading old default resx file...");
                string resxFile = Path.Combine(Settings.OldResxDir, Settings.DEFAULT_RESX_FILENAME);
                ResXResourceReader reader = new ResXResourceReader(resxFile);

                Console.WriteLine("Populating table with old default resx entries...");
                foreach (DictionaryEntry entry in reader)
                {
                    if (entry.Value is String)
                    {
                        // Get entry's string ID & value
                        string key = entry.Key.ToString();
                        string value = entry.Value.ToString();

                        // Look for string ID in Table
                        DataRow[] foundRows = Table.Select(String.Format("ID = '{0}'", key));
                        if (foundRows.Length == 1)
                        {
                            // If this entry is found in the "new" column, add the value to the row
                            foundRows[0]["old"] = value;

                            // Compare its new & old strings and store the result
                            foundRows[0]["result"] = CompareStrings(foundRows[0]["new"].ToString(), value);
                        }
                        else if (foundRows.Length < 1)
                        {
                            // If not found, add a new row, add this entry's string ID to its ID column and this entry's value to its new column
                            DataRow row = Table.NewRow();
                            row["ID"] = key;
                            row["result"] = (int)ResultType.StringAdded;
                            row["old"] = value;
                            Table.Rows.Add(row);
                        }
                        else // (foundRows.Length > 1)
                        {
                            foreach (DataRow row in foundRows)
                            {
                                // If we found more than one row with this ID, we have duplicate string IDs; This should be fixed ASAP
                                foundRows[0]["result"] = (int)ResultType.StringsDuplicated;
                            }
                        }
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
                // TODO: Error handling
                Console.WriteLine(e);
                return 1;
            }

            return 0;
        }

        private static int CompareStrings(string newString, string oldString)
        {
            // TODO: Create some duplicate IDs to test with

            // TODO: Create some empty strings to test with

            // TODO: Debug why we lost additions


            if ((newString == String.Empty) && (oldString== String.Empty))
            {
                return (int)ResultType.StringsEmpty;
            }

            if (oldString == String.Empty)
            {
                return (int)ResultType.StringAdded;
            }

            if (newString == String.Empty)
            {
                return (int)ResultType.StringDeleted;
            }

            if (newString != oldString)
            {
                return (int)ResultType.StringMismatch;
            }

            return (int)ResultType.StringMatch;
        }

        public static bool OutputResults()
        {
            DataView dataView;
            string id, newVal, oldVal;

            // Filter on duplicate string IDs & sort
            if (Settings.IsReportDuplicateIds)
            {
                dataView = SetDataView(ResultType.StringsDuplicated);
                Console.WriteLine("\nResX Duplicate String IDs: {0} ----------------------------------------\n", dataView.Count);
                foreach (DataRowView item in dataView)
                {
                    id = item["ID"].ToString();
                    newVal = item["new"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                    oldVal = item["old"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                    Console.WriteLine("  {0}\n    New: {1}\n    Old: {2}\n", id, newVal, oldVal);
                }
            }

            // Filter on mismatched strings & sort
            if (Settings.IsReportMismatches)
            {
                dataView = SetDataView(ResultType.StringMismatch);
                Console.WriteLine("\nResX Default String Mismatches: {0} ----------------------------------------\n", dataView.Count);
                foreach (DataRowView item in dataView)
                {
                    id = item["ID"].ToString();
                    newVal = item["new"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                    oldVal = item["old"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                    Console.WriteLine("  {0}\n    New: {1}\n    Old: {2}\n", id, newVal, oldVal);
                }
            }

            // Filter on empty strings & sort
            if (Settings.IsReportEmptyStrings)
            {
                dataView = SetDataView(ResultType.StringsEmpty);
                Console.WriteLine("\nResX Default Empty Strings: {0} ----------------------------------------\n", dataView.Count);
                foreach (DataRowView item in dataView)
                {
                    id = item["ID"].ToString();
                    Console.WriteLine("  {0}\n", id);
                }
            }

            // Filter on deleted strings & sort
            if (Settings.IsReportDeletes)
            {
                dataView = SetDataView(ResultType.StringDeleted);
                Console.WriteLine("\nResX Default String Deletions: {0} ----------------------------------------\n", dataView.Count);
                foreach (DataRowView item in dataView)
                {
                    id = item["ID"].ToString();
                    oldVal = item["old"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                    Console.WriteLine("  {0}  //  {1}\n", id, oldVal);
                }
            }

            // Filter on added strings & sort
            if (Settings.IsReportAdds)
            {
                dataView = SetDataView(ResultType.StringAdded);
                Console.WriteLine("\nResX Default String Additions: {0} ----------------------------------------\n", dataView.Count);
                foreach (DataRowView item in dataView)
                {
                    id = item["ID"].ToString();
                    newVal = item["new"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                    Console.WriteLine("  {0}  //  {1}\n", id, newVal);
                }
            }

            // Filter on matching strings & sort
            if (Settings.IsReportMatches)
            {
                dataView = SetDataView(ResultType.StringMatch);
                Console.WriteLine("\nResX String Matches: {0} ----------------------------------------\n", dataView.Count);
                foreach (DataRowView item in dataView)
                {
                    id = item["ID"].ToString();
                    newVal = item["new"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                    Console.WriteLine("  {0}  //  {1}\n", id, newVal);
                }
            }

            return true; // TODO: Fix me!
        }

        private static DataView SetDataView(ResultType resultType)
        {
            string filter = "result = " + ((int)resultType).ToString();
            string sort = "ID ASC";
            return new DataView(Table, filter, sort, DataViewRowState.CurrentRows);
        }
    }
}
