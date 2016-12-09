using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Resources;


namespace ResxDiff
{
    class StringResourceTable
    {
        private const string DEFAULT_RESX_FILENAME = "Resources.resx";
        private const string RESX_FILE_FILTER = "*.resx";

        private string _newResxFilePath = null;
        private string _oldResxFilePath = null;

        private string _newDefaultResxFile = null;
        private string _oldDefaultResxFile = null;

        private string _defaultResxFile = null;

        private DataTable _table = null;

        public StringResourceTable(string newResxFilePath, string oldResxFilePath)
        {
            if (newResxFilePath == null)
            {
                // TODO: Report error
                return;
            }

            if (oldResxFilePath == null)
            {
                // TODO: Report error
                return;
            }

            _newResxFilePath = newResxFilePath;
            _oldResxFilePath = oldResxFilePath;

            try
            {
                if (!Directory.Exists(_newResxFilePath))
                {
                    // TODO: Report error
                    return;
                }

                if (!Directory.Exists(_oldResxFilePath))
                {
                    // TODO: Report error
                    return;
                }

                _newDefaultResxFile = Path.Combine(_newDefaultResxFile, DEFAULT_RESX_FILENAME);
                if (!File.Exists(_newDefaultResxFile))
                {
                    // TODO: Report error
                    _newDefaultResxFile = null;
                    return;
                }

                _oldDefaultResxFile = Path.Combine(_oldDefaultResxFile, DEFAULT_RESX_FILENAME);
                if (!File.Exists(_oldDefaultResxFile))
                {
                    // TODO: Report error
                    _oldDefaultResxFile = null;
                    return;
                }

                Environment.CurrentDirectory = _newResxFilePath;
                string[] newResxFiles = Directory.GetFiles(_newResxFilePath, RESX_FILE_FILTER);
                if (newResxFiles.Length < 1)
                {
                    // TODO: Report error
                    return;
                }

                ArrayList tempList = new ArrayList();
                string[] oldResxFiles = Directory.GetFiles(_oldResxFilePath, DEFAULT_RESX_FILENAME);
                if (oldResxFiles.Length < 1)
                {
                    // TODO: Report error
                    return;
                }

                // Remove the default resx file from the new array
                newResxFiles = newResxFiles.Where((value, index) => index != Array.IndexOf(newResxFiles, _newDefaultResxFile)).ToArray();

                // Move the default resx file to the beginning of the old array
                oldResxFiles = oldResxFiles.Where((value, index) => index != Array.IndexOf(oldResxFiles, _oldDefaultResxFile)).ToArray();

                oldResxFiles += Directory.GetFiles(_oldResxFilePath, RESX_FILE_FILTER);

                IntializeTable();

                foreach (string resxFile in newResxFiles)
                {
                    AddResxFileToTable(resxFile);
                }

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

            int check = 0;
        }

        private void IntializeTable()
        {
            // Create data table
            _table = new DataTable();

            // Add our ID column
            DataColumn col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "ID";
            col.AutoIncrement = false;
            col.Caption = "ID";
            col.ReadOnly = false;
            col.Unique = false;
            _table.Columns.Add(col);

            // Add our default value column
            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "Default";
            col.AutoIncrement = false;
            col.Caption = "Default";
            col.ReadOnly = false;
            col.Unique = false;
            _table.Columns.Add(col);

            // Get data from default resx file
            ResXResourceReader reader = new ResXResourceReader(_defaultResxFile);
            DataRow row;
            foreach (DictionaryEntry entry in reader)
            {
                if (entry.Value is String)
                {
                    row = _table.NewRow();
                    row["ID"] = entry.Key.ToString();
                    row["Default"] = entry.Value.ToString();
                    _table.Rows.Add(row);
                }
            }

            reader.Close();
        }

        private void AddResxFileToTable(string resxFile, bool isOldData = false)
        {
            string columnName = (isOldData ? "old_" : "") + Path.GetFileName(resxFile).Split('.')[1];

            // Add a data table column for this file
            DataColumn col;
            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = columnName;
            col.AutoIncrement = false;
            col.Caption = columnName;
            col.ReadOnly = false;
            col.Unique = false;
            _table.Columns.Add(col);

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

                    // Look for string ID in _table
                    DataRow[] foundRows = _table.Select(String.Format("ID = '{0}'", key));
                    if (foundRows.Length < 1)
                    {
                        // If not found, add a new row, add this entry's string ID to its ID column and this
                        // entry's value to its new column
                        row = _table.NewRow();
                        row["ID"] = key;
                        row[columnName] = value;
                        _table.Rows.Add(row);
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
    }
}
