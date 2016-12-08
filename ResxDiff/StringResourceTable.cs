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
        private string _defaultResxFile = null;

        private DataTable _table = null;


        public StringResourceTable(string resxFilePath)
        {
            try
            {
                if (!Directory.Exists(resxFilePath))
                {
                    // TODO: Report error
                    return;
                }

                _defaultResxFile = Path.Combine(resxFilePath, "Resources.resx");
                if (!File.Exists(_defaultResxFile))
                {
                    // TODO: Report error
                    _defaultResxFile = null;
                    return;
                }

                Environment.CurrentDirectory = resxFilePath;

                string[] resxFiles = Directory.GetFiles(resxFilePath, "*.resx");
                if (resxFiles.Length < 1)
                {
                    // TODO: Report error
                    return;
                }

                // Remove the default resx file from the array
                resxFiles = resxFiles.Where((val, idx) => idx != Array.IndexOf(resxFiles, _defaultResxFile)).ToArray();

                IntializeTable();

                //?

                foreach (string resxFile in resxFiles)
                {
                    AddResxFileToTable(resxFile);
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

        private void AddResxFileToTable(string resxFile)
        {
            string columnName = Path.GetFileName(resxFile).Split('.')[1];

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
