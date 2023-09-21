using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace UE4localizationsTool.Helper
{
    public class CSVFile
    {
        public static CSVFile Instance { get; } = new CSVFile();

        public char Delimiter { get; set; } = ',';
        public bool HasHeader { get; set; } = true;

        public void Load(NDataGridView dataGrid, string filePath)
        {

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(Delimiter.ToString());
                
                int i = -1;
                if (!HasHeader) i++;
                
                while (!parser.EndOfData)
                {

                    string[] fields = parser.ReadFields();

                    if (HasHeader && i == -1)
                    {
                        i++;
                        continue;
                    }

                    if (fields.Length >= 3 && !string.IsNullOrEmpty(fields[2]))
                    {
                    
                        dataGrid.SetValue(dataGrid.Rows[i].Cells["Text value"], fields[2]);
                    }
                    i++;
                }
            }
        }

        public void Save(DataGridView dataGrid, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Key,Source,Translation");
                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    writer.WriteLine("\"" + FixedString(row.Cells["Name"].Value) + "\"" + Delimiter.ToString() + "\"" + FixedString(row.Cells["Text value"].Value) + "\"" + Delimiter.ToString() + "\"" + "\"");
                }
            }
        }

        public void Load(List<List<string>> Strings, string filePath)
        {
            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(Delimiter.ToString());

                int i = -1;
                if (!HasHeader) i++;

                while (!parser.EndOfData)
                {

                    string[] fields = parser.ReadFields();

                    if (HasHeader && i == -1)
                    {
                        i++;
                        continue;
                    }

                    if (fields.Length >= 3 && !string.IsNullOrEmpty(fields[2]))
                    {
                        Strings[i][1] = fields[2];                }
                    i++;
                }
            }
        }

        public void Save(List<List<string>> Strings,string filePath)
        {

            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Key,Source,Translation");
                foreach (var row in Strings)
                {
                    if (row[0] == "[~PATHFile~]")
                    {
                        writer.WriteLine("\"" + FixedString(row[1]) + "\"");
                        continue;
                    }

                    writer.WriteLine("\"" + FixedString(row[0]) + "\"" + Delimiter.ToString() + "\"" + FixedString(row[1]) + "\"" + Delimiter.ToString() + "\"" + "\"");
                }
            }

        }

        public static string FixedString(object str,bool returnValue =true)
        {
            if (returnValue ) return str.ToString();
            return str.ToString().Replace("\"", "\"\"");
        }
    }

}
