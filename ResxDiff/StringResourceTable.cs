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
        StringDeleted
    }

    public class StringResourceTable
    {
        private const string DEFAULT_RESX_FILENAME = "Resources.resx";
        private const string RESX_FILE_FILTER = "*.resx";
        private const string FAKE_CR = "»";

        private string _newResxFilePath = null;
        private string _oldResxFilePath = null;

        private string _newDefaultResxFile = null;
        private string _oldDefaultResxFile = null;

        public DataTable Table = null;

        public bool IsValid = false;

        private int _oldDefaultColumnIndex = -1;

        public StringResourceTable(string newResxFilePath, string oldResxFilePath)
        {
            IsValid = ValidatePaths(newResxFilePath, oldResxFilePath);
            if (!IsValid) { return; }

            try
            {
                IntializeTable();

                // Make a list of new resx files, not including the default one
                ArrayList newResxFiles = new ArrayList();
                foreach (string file in Directory.GetFiles(_newResxFilePath, RESX_FILE_FILTER))
                {
                    if (file != _newDefaultResxFile)
                    {
                        newResxFiles.Add(file);
                    }
                }

                // Make a list of old resx files, including the default one in the first position
                ArrayList oldResxFiles = new ArrayList();
                oldResxFiles.Add(_oldDefaultResxFile);
                foreach (string file in Directory.GetFiles(_oldResxFilePath, RESX_FILE_FILTER))
                {
                    if (file != _oldDefaultResxFile)
                    {
                        oldResxFiles.Add(file);
                    }
                }

                // TODO: We don't need all of these damn tables!

                foreach (string resxFile in newResxFiles)
                {
                    AddResxFileToTable(resxFile);
                }

                _oldDefaultColumnIndex = Table.Columns.Count;

                foreach (string resxFile in oldResxFiles)
                {
                    AddResxFileToTable(resxFile, true);
                }
            }
            catch (Exception e)
            {
                // TODO: Error handling
                Console.WriteLine(e);
            }
        }

        private bool ValidatePaths(string newResxFilePath, string oldResxFilePath)
        {
            try
            {
                _newResxFilePath = newResxFilePath;
                _oldResxFilePath = oldResxFilePath;

                if ((_newResxFilePath == null) || (_newResxFilePath == String.Empty))
                {
                    // TODO: Report error
                    return false;
                }

                if ((_oldResxFilePath == null) || (_oldResxFilePath == String.Empty))
                {
                    // TODO: Report error
                    return false;
                }

                if (!Directory.Exists(_newResxFilePath))
                {
                    // TODO: Report error
                    return false;
                }

                if (!Directory.Exists(_oldResxFilePath))
                {
                    // TODO: Report error
                    return false;
                }

                _newDefaultResxFile = Path.Combine(newResxFilePath, DEFAULT_RESX_FILENAME);
                if (!File.Exists(_newDefaultResxFile))
                {
                    // TODO: Report error
                    return false;
                }
                _oldDefaultResxFile = Path.Combine(_oldResxFilePath, DEFAULT_RESX_FILENAME);
                if (!File.Exists(_oldDefaultResxFile))
                {
                    // TODO: Report error
                    return false;
                }
            }
            catch (Exception e)
            {
                // TODO: Error handling
                Console.WriteLine(e);
            }

            return true;
        }

        private void IntializeTable()
        {
            try
            {
                // Create data table
                Table = new DataTable();
                Table.Columns.Add("ID", typeof(string));
                Table.Columns.Add("result", typeof(Int32));
                Table.Columns.Add("default", typeof(string));

                // Change the current directory to our new ResX file path, so the files will be
                // processed correctly
                Environment.CurrentDirectory = _newResxFilePath;

                // Get data from default resx file
                Console.WriteLine("Loading new default resx file...");
                ResXResourceReader reader = new ResXResourceReader(_newDefaultResxFile);
                DataRow row;

                Console.WriteLine("Populating table with new default resx entries...");
                foreach (DictionaryEntry entry in reader)
                {
                    if (entry.Value is String)
                    {
                        row = Table.NewRow();
                        row["ID"] = entry.Key.ToString();
                        row["result"] = -1;
                        row["default"] = entry.Value.ToString();
                        Table.Rows.Add(row);
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
                // TODO: Error handling
                Console.WriteLine(e);
            }
        }

        private void AddResxFileToTable(string resxFile, bool isOldData = false)
        {
            string columnName = (isOldData ? "old_" : "") + Path.GetFileName(resxFile).Split('.')[1];

            Console.WriteLine("Populating table with {0} resx entries...", columnName);

            // Add a data table column for this file
            Table.Columns.Add(columnName, typeof(string));

            // TODO: Add exception handling

            ResXResourceReader reader = new ResXResourceReader(resxFile);
            DataRow row;
            foreach (DictionaryEntry entry in reader)
            {
                if (entry.Value is String)
                {
                    // Get entry's string ID & value
                    string key = entry.Key.ToString();
                    string value = entry.Value.ToString();

                    // Look for string ID in Table
                    DataRow[] foundRows = Table.Select(String.Format("ID = '{0}'", key));
                    if (foundRows.Length < 1)
                    {
                        // If not found, add a new row, add this entry's string ID to its ID column and this
                        // entry's value to its new column
                        row = Table.NewRow();
                        row["ID"] = key;
                        row[columnName] = value;
                        Table.Rows.Add(row);
                    }
                    else if (foundRows.Length == 1)
                    {
                        // If found, add this entry's value to the found-in row in the new column
                        foundRows[0][columnName] = value;
                    }
                    else // (foundRows.Length > 1)
                    {
                        // Duplicated string IDs -- TODO: flag it but don't touch
                    }

                    //Console.WriteLine("{0} | {1}", entry.Key.ToString(), entry.Value.ToString());
                }
            }

            reader.Close();
        }

        public int TestStrings()
        {
            Console.WriteLine("Checking resource strings...");

            int columns = Table.Columns.Count;
            int rows = Table.Rows.Count;

            int columnPairings = (columns - 1) / 2;
            int newColumnIndex = 2;
            int oldColumnIndex = columnPairings + newColumnIndex;
            //int resultColumn = 1;

            int returnValue = 0;

            //Results results = new Results();

            //for (int i = newColumnIndex; i < oldColumnIndex; i++)
            //{
                for (int j = 0; j < rows; j++)
                {
                    DataRow row = Table.Rows[j];

                    int type;
                    string id = row.ItemArray[0].ToString();
                    string newVal = row.ItemArray[newColumnIndex].ToString();
                    string oldVal = row.ItemArray[oldColumnIndex].ToString();

                    if ((oldVal == String.Empty) && (newVal == String.Empty))
                    {
                        // TODO: This is usually the translations for an added string
                        // For now, just count it as added, but we might want to alter that later
                        type = (int)ResultType.StringAdded;
                        if (Settings.IsErrorOnAdds) { returnValue = 1; }
                    }
                    else if (oldVal == String.Empty)
                    {
                        type = (int)ResultType.StringAdded;
                        if (Settings.IsErrorOnAdds) { returnValue = 1; }
                    }
                    else if (newVal == String.Empty)
                    {
                        type = (int)ResultType.StringDeleted;
                        if (Settings.IsErrorOnDeletes) { returnValue = 1; }
                    }
                    else if (newVal != oldVal)
                    {
                        type = (int)ResultType.StringMismatch;
                        if (Settings.IsErrorOnMismatches) { returnValue = 1; }
                    }
                    else
                    {
                        type = (int)ResultType.StringMatch;
                    }

                    if (j == 0)
                    {
                        int z = 0;
                    }

                    row["result"] = (int)type;
                }

                //newColumnIndex++;
                //oldColumnIndex++;
            //}

            return returnValue;
        }

        public void OutputResults()
        {
            DataView dataView;
            string id;
            string newVal;
            string oldVal;

            // Filter on mismatched strings & sort
            if (Settings.IsReportMismatches)
            {
                dataView = SetDataView(ResultType.StringMismatch);
                Console.WriteLine("\nResX Default String Mismatches: {0} ----------------------------------------\n", dataView.Count);
                foreach (DataRowView item in dataView)
                {
                    id = item["ID"].ToString();
                    newVal = item["default"].ToString().Replace("\n", FAKE_CR);
                    oldVal = item[_oldDefaultColumnIndex].ToString().Replace("\n", FAKE_CR);
                    Console.WriteLine("  {0}\n    New: {1}\n    Old: {2}\n", id, newVal, oldVal);
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
                    oldVal = item[_oldDefaultColumnIndex].ToString().Replace("\n", FAKE_CR);
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
                    newVal = item["default"].ToString().Replace("\n", FAKE_CR);
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
                    newVal = item["default"].ToString().Replace("\n", FAKE_CR);
                    Console.WriteLine("  {0}  //  {1}\n", id, newVal);
                }
            }
        }

        public DataView SetDataView(ResultType resultType)
        {
            string filter = "result = " + ((int)resultType).ToString();
            string sort = "ID ASC";
            return new DataView(Table, filter, sort, DataViewRowState.CurrentRows);
        }

    }
/*
    public class Results
        {
            private const string SORT_ORDER = "ID ASC";
            public const string HEADER_LINE = "\n--------------------------------------------------------------------------------";

            public DataTable ResultsTable = null;

            public Results()
            {
                ResultsTable = new DataTable();
                DataColumn col = new DataColumn();
                col.DataType = Type.GetType("System.Int32");
                col.ColumnName = "Type";
                col.AutoIncrement = false;
                col.Caption = "Type";
                col.ReadOnly = false;
                col.Unique = false;
                ResultsTable.Columns.Add(col);

                string[] additionalColumns = { "ID", "New", "Old" };
                foreach (string column in additionalColumns)
                {
                    col = new DataColumn();
                    col.DataType = Type.GetType("System.String");
                    col.ColumnName = column;
                    col.AutoIncrement = false;
                    col.Caption = column;
                    col.ReadOnly = false;
                    col.Unique = false;
                    ResultsTable.Columns.Add(col);
                }
            }

            public void Add(int type, string id, string newVal, string oldVal)
            {
                DataRow row = ResultsTable.NewRow();
                row["Type"] = type;
                row["ID"] = id;
                row["New"] = newVal;
                row["Old"] = oldVal;
                ResultsTable.Rows.Add(row);
            }

            public void DisplayResultType(int type)
            {
                string filter = String.Format("Type = {0}", (int)type);

                DataRow[] foundRows = ResultsTable.Select(filter, SORT_ORDER);
                foreach (DataRow row in foundRows)
                {
                    Console.WriteLine("MISMATCH: id[{0}], newVal[{1}], oldVal[{2}]",
                        row.ItemArray[1].ToString(),
                        row.ItemArray[2].ToString(),
                        row.ItemArray[3].ToString());
                }
            }

            public void DisplayMatches() { }

            public void DisplayMismatches()
            {
                Console.WriteLine(HEADER_LINE);
                Console.WriteLine("String resource mismatches\n ");

                DataRow[] foundRows = ResultsTable.Select("Type = 1", SORT_ORDER);
                foreach (DataRow row in foundRows)
                {
                    Console.WriteLine("* id[{0}], newVal[{1}], oldVal[{2}]", row.ItemArray[1].ToString(),
                        row.ItemArray[2].ToString(), row.ItemArray[3].ToString());
                }
            }

            public void DisplayAdditions()
            {
                Console.WriteLine(HEADER_LINE);
                Console.WriteLine("String resource additions\n ");

                DataRow[] foundRows = ResultsTable.Select("Type = 2", SORT_ORDER);
                foreach (DataRow row in foundRows)
                {
                    Console.WriteLine("* id[{0}], newVal[{1}], oldVal[{2}]", row.ItemArray[1].ToString(),
                        row.ItemArray[2].ToString(), row.ItemArray[3].ToString());
                }
            }

            public void DisplayDeletions()
            {
                Console.WriteLine(HEADER_LINE);
                Console.WriteLine("String resource deletions\n ");

                DataRow[] foundRows = ResultsTable.Select("Type = 3", SORT_ORDER);
                foreach (DataRow row in foundRows)
                {
                    Console.WriteLine("* id[{0}], newVal[{1}], oldVal[{2}]", row.ItemArray[1].ToString(),
                        row.ItemArray[2].ToString(), row.ItemArray[3].ToString());
                }
            }
        }
*/

}
