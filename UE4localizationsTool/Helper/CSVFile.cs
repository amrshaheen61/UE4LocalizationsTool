using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Windows.Forms;

namespace UE4localizationsTool.Helper
{
    public class CSVFile
    {
        public static CSVFile Instance { get; } = new CSVFile();

        public char Delimiter { get; set; } = ',';
        public bool HasHeader { get; set; } = true;

        public void Load(DataGridView dataGrid, string filePath)
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
                        dataGrid.Rows[i].Cells["TextValue"].Value = fields[2];

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
                    writer.WriteLine("\"" + FixedString(row.Cells["TextName"].Value) + "\"" + Delimiter.ToString() + "\"" + FixedString(row.Cells["TextValue"].Value) + "\"" + Delimiter.ToString() + "\"" + "\"");
                }
            }
        }

        private string FixedString(object str)
        {
            return str.ToString().Replace("\"", "\"\"");
        }
    }

}
