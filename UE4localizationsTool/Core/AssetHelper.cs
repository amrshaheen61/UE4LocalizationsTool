using Helper.MemoryList;
using System.Text;

namespace AssetParser
{
    public static class AssetHelper
    {

        public static string GetPropertyName(this Uasset SourceFile, int Index)
        {
            if (SourceFile.NAMES_DIRECTORY.Count > Index)
            {
                return SourceFile.NAMES_DIRECTORY[Index];
            }
            return Index.ToString();
        }

        public static string GetExportPropertyName(this Uasset SourceFile, int Index)
        {
            if (Index > 0)
            {
                return GetPropertyName(SourceFile, SourceFile.Imports_Directory[Index].NameID);
            }
            else if (Index == 0)
            {
                return "0";
            }
            return GetPropertyName(SourceFile, SourceFile.Imports_Directory[(Index * -1) - 1].NameID);
        }

        public static void ReadExtraByte(this MemoryList memoryList, Uexp SourceFile, bool fromStruct)
        {
            if (fromStruct && SourceFile.UassetData.EngineVersion >= UE4Version.VER_UE4_NAME_HASHES_SERIALIZED)
            {
                memoryList.Skip(1);
            }
        }


        public static string GetStringUE(this MemoryList memoryList)
        {
            int StringLength = memoryList.GetIntValue();
            string Stringvalue = "";
            if (StringLength != 0)
            {
                if (StringLength < 0)
                {
                    Stringvalue = memoryList.GetStringValue((StringLength * -2), true, -1, Encoding.Unicode);
                }
                else
                {
                    Stringvalue = memoryList.GetStringValue(StringLength);
                }
            }
            Stringvalue = Stringvalue.Replace("\r\n", "<cf>");
            Stringvalue = Stringvalue.Replace("\r", "<cr>");
            Stringvalue = Stringvalue.Replace("\n", "<lf>");

            return Stringvalue.TrimEnd('\0');
        }

        public static bool IsASCII(string StringValue)
        {
            for (int n = 0; n < StringValue.Length; n++)
            {
                if (StringValue[n] > 127)
                {
                    return false;
                }
            }
            return true;
        }

        public static void DeleteStringUE(this MemoryList memoryList)
        {
            int StringLength = memoryList.GetIntValue(false);

            if (StringLength != 0)
            {
                if (StringLength < 0)
                {
                    StringLength = (StringLength * -2);
                }
            }
            memoryList.DeleteBytes(4 + StringLength);
        }

        public static void ReplaceStringUE(this MemoryList memoryList, string StringValue)
        {


            StringValue = StringValue.Replace("<cf>", "\r\n");
            StringValue = StringValue.Replace("<cr>", "\r");
            StringValue = StringValue.Replace("<lf>", "\n");

            //To save time
            int ThisPosition = memoryList.GetPosition();
            string TempString = memoryList.GetStringUE();
            if (StringValue == TempString)
            {
              return;
            }

            memoryList.Seek(ThisPosition);
            memoryList.DeleteStringUE();


            if (string.IsNullOrEmpty(StringValue))
            {
                memoryList.InsertIntValue(0);
                return;
            }


            StringValue += '\0';

            Encoding encoding = Encoding.Unicode;
            if (IsASCII(StringValue))
            {
                encoding = Encoding.ASCII;
            }

           byte[] TextBytes= encoding.GetBytes(StringValue);

            if (encoding == Encoding.ASCII)
            {
                memoryList.InsertIntValue(TextBytes.Length);
                memoryList.InsertBytes(TextBytes);
            }
            else
            {
                memoryList.InsertIntValue(TextBytes.Length/-2);
                memoryList.InsertBytes(TextBytes);
            }
        }

    }
}
