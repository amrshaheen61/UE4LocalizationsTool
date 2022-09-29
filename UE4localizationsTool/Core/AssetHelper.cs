using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetParser
{
    public static class AssetHelper
    {

        public static string GetPropertyName(this Uasset SourceFile, int Index)
        {
            if (SourceFile.NAMES_DIRECTORY.Count > Index && Index>0)
            {
                return SourceFile.NAMES_DIRECTORY[Index];
            }
            return Index.ToString();
        }

        public static string GetExportPropertyName(this Uasset SourceFile, int Index)
        {
            if (SourceFile.IOFile)
            {
               return  GetPropertyName(SourceFile,Index);
            }


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

        private static int GetRequiredUtf16Padding(uint NameData)
        {
            return (int)(NameData & 1u);
        }
        private static bool IsUtf16(byte NameData)
        {
            return (NameData & 0x80u) != 0;
        }
        public static string GetStringUES(this MemoryList memoryList)
        {
            string Stringvalue = "";
            byte[] Data = new byte[2];
            Data[0] = memoryList.GetByteValue();
            Data[1] = memoryList.GetByteValue();

            int len= (int)((Data[0] & 0x7Fu) << 8) + Data[1];

            if (IsUtf16(Data[0]))
            {
                if (memoryList.GetByteValue(false)==0) //because "GetRequiredUtf16Padding" not work right :/
                {
                    memoryList.Skip(1);
                }
                Stringvalue = memoryList.GetStringValue(len*2,true,-1,Encoding.Unicode);
            }
            else
            {
                Stringvalue= memoryList.GetStringValue(len);
            }
            Stringvalue = Stringvalue.Replace("\r\n", "<cf>");
            Stringvalue = Stringvalue.Replace("\r", "<cr>");
            Stringvalue = Stringvalue.Replace("\n", "<lf>");

            return Stringvalue.TrimEnd('\0');
        }
        public static string GetStringUE(this MemoryList memoryList,Encoding encoding)
        {
            string Stringvalue = memoryList.GetStringValueN(true,-1, encoding);
            Stringvalue = Stringvalue.Replace("\r\n", "<cf>");
            Stringvalue = Stringvalue.Replace("\r", "<cr>");
            Stringvalue = Stringvalue.Replace("\n", "<lf>");
            return Stringvalue.TrimEnd('\0');
        }

        public static string GetStringUE(this MemoryList memoryList,int Lenght, bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            string Stringvalue = memoryList.GetStringValue(Lenght, SavePosition, SeekAndRead, encoding);
            Stringvalue = Stringvalue.Replace("\r\n", "<cf>");
            Stringvalue = Stringvalue.Replace("\r", "<cr>");
            Stringvalue = Stringvalue.Replace("\n", "<lf>");
            return Stringvalue.TrimEnd('\0');
        }


        public static string ReplaceString(string Str)
        {
            Str = Str.Replace("<cf>", "\r\n");
            Str = Str.Replace("<cr>", "\r");
            return Str.Replace("<lf>", "\n");  
        }


        public static void ReplaceStringUE_Func(this MemoryList memoryList, string StringValue)
        {

            StringValue = StringValue.Replace("<cf>", "\r\n");
            StringValue = StringValue.Replace("<cr>", "\r");
            StringValue = StringValue.Replace("<lf>", "\n");

            memoryList.Skip(-1);
            ExprToken eExpr = (ExprToken)memoryList.GetByteValue();
            if (eExpr == ExprToken.EX_StringConst)
            {
                memoryList.DeleteStringN(-1, Encoding.ASCII);
            }
            else if (eExpr == ExprToken.EX_UnicodeStringConst)
            {
                memoryList.DeleteStringN(-1, Encoding.Unicode);
            }
            memoryList.Skip(-1);


            Encoding encoding = Encoding.Unicode;
            if (IsASCII(StringValue))
            {
                encoding = Encoding.ASCII;
            }

            if (encoding == Encoding.ASCII)
            {
                memoryList.SetByteValue((byte)ExprToken.EX_StringConst);
                memoryList.InsertStringValueN(StringValue,true,-1, encoding);
            }
            else
            {
                memoryList.SetByteValue((byte)ExprToken.EX_UnicodeStringConst);
                memoryList.InsertStringValueN(StringValue, true, -1, encoding);
            }
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


    public class ReadStringProperty
    {
        public ReadStringProperty(MemoryList memoryList, Uexp uexp, string PropertyName, bool Modify = false)
        {
            if (!Modify)
            {
                uexp.Strings.Add(new List<string>() { PropertyName, memoryList.GetStringUE() });
                ConsoleMode.Print(uexp.Strings[uexp.Strings.Count - 1][1], ConsoleColor.Magenta);
            }
            else
            {
                memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                uexp.CurrentIndex++;
            }
        }
    }


}
