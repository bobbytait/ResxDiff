using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Resources;
using System.Xml.Linq;


namespace ResxDiff
{
    public enum ResultType
    {
        StringMatch = 0,
        StringMismatch,
        StringAdded,
        StringDeleted,
        StringIdsDuplicated,
        StringsEmpty
    }

    public static class StringResourceTable
    {
        private const char FAKE_CR = '»';

        public static DataTable Table = null;

        private static string _newResxFile = null;

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
                return ErrorHandling.OutputError("Occurred initializing data table", e);
            }

            return true;
        }

        public static bool ImportNewResxData()
        {
            try
            {
                // Setting the new resx dir as the current directory, since file-based resources can cause exceptions if the source file is not found
                Environment.CurrentDirectory = Settings.NewResxDir;

                // Get data from new default resx file
                _newResxFile = Path.Combine(Settings.NewResxDir, Settings.DEFAULT_RESX_FILENAME);
                ResXResourceReader reader = new ResXResourceReader(_newResxFile);
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
                return ErrorHandling.OutputError("Occurred importing newer string resource data", e);
            }

            return true;
        }

        public static bool ImportOldResxData()
        {
            try
            {
                // Get data from old default resx file
                string resxFile = Path.Combine(Settings.OldResxDir, Settings.DEFAULT_RESX_FILENAME);
                ResXResourceReader reader = new ResXResourceReader(resxFile);
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
                        }
                        else if (foundRows.Length < 1)
                        {
                            // If not found, add a new row, add this entry's string ID to its ID column and this entry's value to its new column
                            DataRow row = Table.NewRow();
                            row["ID"] = key;
                            row["old"] = value;
                            Table.Rows.Add(row);
                        }
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
                return ErrorHandling.OutputError("Occurred importing older string resource data", e);
            }

            return true;
        }

        public static int CompareResxData()
        {
            try
            {
                foreach (DataRow row in Table.Rows)
                {
                    // Get entry's string ID & value
                    string key = row["ID"].ToString();
                    string newValue = row["new"].ToString();
                    string oldValue = row["old"].ToString();

                    if ((newValue == String.Empty) && (oldValue == String.Empty))
                    {
                        row["result"] = (int)ResultType.StringsEmpty;
                    }
                    else if (oldValue == String.Empty)
                    {
                        row["result"] = (int)ResultType.StringAdded;
                    }
                    else if (newValue == String.Empty)
                    {
                        row["result"] = (int)ResultType.StringDeleted;
                    }
                    else if (newValue == oldValue)
                    {
                        row["result"] = (int)ResultType.StringMatch;
                    }
                    else if (newValue != oldValue)
                    {
                        row["result"] = (int)ResultType.StringMismatch;
                    }
                }

                // A quick hack (for now) to find resource string ID duplicates; Needed since
                // ResXResourceReader simple merges rows with duplicate IDs
                IEnumerable<string> duplicateIds = XDocument.Load(_newResxFile)
                    .Descendants("data")
                    .GroupBy(g => (string)g.Attribute("name"))
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);
                DataRow duplicateRow;
                foreach (string duplicateId in duplicateIds)
                {
                    duplicateRow = Table.NewRow();
                    duplicateRow["ID"] = duplicateId;
                    duplicateRow["result"] = ResultType.StringIdsDuplicated;
                    Table.Rows.Add(duplicateRow);
                }
            }
            catch (Exception e)
            {
                ErrorHandling.OutputError("Occurred comparing ResX data", e);
                return 1;
            }

            return 0;
        }

        private static int CompareStrings(string newString, string oldString)
        {
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

            try
            {
                // Filter on duplicate string IDs & sort
                if (Settings.IsReportDuplicateIds)
                {
                    dataView = SetDataView(ResultType.StringIdsDuplicated);
                    Console.WriteLine("\nResX Duplicate String IDs: {0} ----------------------------------------\n", dataView.Count);
                    foreach (DataRowView item in dataView)
                    {
                        Console.WriteLine("  {0}", item["ID"].ToString());
                    }
                }

                // Filter on mismatched strings & sort
                if (Settings.IsReportMismatches)
                {
                    dataView = SetDataView(ResultType.StringMismatch);
                    Console.WriteLine("\n\nResX Default String Mismatches: {0} ----------------------------------------", dataView.Count);
                    foreach (DataRowView item in dataView)
                    {
                        id = item["ID"].ToString();
                        newVal = item["new"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                        oldVal = item["old"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                        Console.WriteLine("\n  {0}\n    New: {1}\n    Old: {2}", id, newVal, oldVal);
                    }
                }

                // Filter on empty strings & sort
                if (Settings.IsReportEmptyStrings)
                {
                    dataView = SetDataView(ResultType.StringsEmpty);
                    Console.WriteLine("\n\nResX Default Empty Strings: {0} ----------------------------------------\n", dataView.Count);
                    foreach (DataRowView item in dataView)
                    {
                        Console.WriteLine("  {0}", item["ID"].ToString());
                    }
                }

                // Filter on deleted strings & sort
                if (Settings.IsReportDeletes)
                {
                    dataView = SetDataView(ResultType.StringDeleted);
                    Console.WriteLine("\n\nResX Default String Deletions: {0} ----------------------------------------\n", dataView.Count);
                    foreach (DataRowView item in dataView)
                    {
                        id = item["ID"].ToString();
                        oldVal = item["old"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                        Console.WriteLine("  {0}  //  {1}", id, oldVal);
                    }
                }

                // Filter on added strings & sort
                if (Settings.IsReportAdds)
                {
                    dataView = SetDataView(ResultType.StringAdded);
                    Console.WriteLine("\n\nResX Default String Additions: {0} ----------------------------------------\n", dataView.Count);
                    foreach (DataRowView item in dataView)
                    {
                        id = item["ID"].ToString();
                        newVal = item["new"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                        Console.WriteLine("  {0}  //  {1}", id, newVal);
                    }
                }

                // Filter on matching strings & sort
                if (Settings.IsReportMatches)
                {
                    dataView = SetDataView(ResultType.StringMatch);
                    Console.WriteLine("\n\nResX String Matches: {0} ----------------------------------------\n", dataView.Count);
                    foreach (DataRowView item in dataView)
                    {
                        id = item["ID"].ToString();
                        newVal = item["new"].ToString().Replace('\n', FAKE_CR).Replace('\r', FAKE_CR);
                        Console.WriteLine("  {0}  //  {1}", id, newVal);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandling.OutputError("Occurred outputting results", e);
            }

            return true;
        }

        private static DataView SetDataView(ResultType resultType)
        {
            try
            {
                string filter = "result = " + ((int)resultType).ToString();
                string sort = "ID ASC";
                return new DataView(Table, filter, sort, DataViewRowState.CurrentRows);
            }
            catch (Exception e)
            {
                ErrorHandling.OutputError("Occurred setting data view", e);
                return null;
            }
        }
    }
}
