using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetParser
{
    public static class AssetHelper
    {

        public static string GetPropertyName(this IUasset SourceFile, int Index)
        {
            if (SourceFile.NAMES_DIRECTORY.Count > Index && Index > 0)
            {
                return SourceFile.NAMES_DIRECTORY[Index];
            }
            return Index.ToString();
        }

        public static string GetExportPropertyName(this IUasset SourceFile, int Index)
        {
            if (SourceFile.IOFile)
            {
                return GetPropertyName(SourceFile, Index);
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
            if (fromStruct && SourceFile.UassetData.EngineVersion >= UEVersions.VER_UE4_NAME_HASHES_SERIALIZED)
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


            return ReplaceBreaklines(Stringvalue).TrimEnd('\0');
        }

        private static int GetRequiredUtf16Padding(uint NameData)
        {
            return (int)(NameData & 1u);
        }
        public static bool IsUtf16(byte NameData)
        {
            return (NameData & 0x80u) != 0;
        }

        public static string GetStringUES(this MemoryList memoryList)
        {
            string Stringvalue = "";
            byte[] Data = new byte[2];
            Data[0] = memoryList.GetByteValue();
            Data[1] = memoryList.GetByteValue();

            int len = (int)((Data[0] & 0x7Fu) << 8) + Data[1];

            if (IsUtf16(Data[0]))
            {
                if (memoryList.GetByteValue(false) == 0) //because "GetRequiredUtf16Padding" not work right :/
                {
                    memoryList.Skip(1);
                }
                Stringvalue = memoryList.GetStringValue(len * 2, true, -1, Encoding.Unicode);
            }
            else
            {
                Stringvalue = memoryList.GetStringValue(len);
            }


            return ReplaceBreaklines(Stringvalue).TrimEnd('\0');
        }

        public static string GetStringUES(this MemoryList memoryList, short namedata)
        {
            string Stringvalue;
            byte[] Data = new byte[2];
            Data[0] = (byte)(namedata & 0xff);
            Data[1] = (byte)(namedata >> 8);

            int len = (int)((Data[0] & 0x7Fu) << 8) + Data[1];

            if (IsUtf16(Data[0]))
            {
                Stringvalue = memoryList.GetStringValue(len * 2, true, -1, Encoding.Unicode);
            }
            else
            {
                Stringvalue = memoryList.GetStringValue(len);
            }


            return ReplaceBreaklines(Stringvalue).TrimEnd('\0');
        }

        public static string GetStringUE(this MemoryList memoryList, Encoding encoding, bool SavePosition = true, int SeekAndRead = -1)
        {
            string Stringvalue = ReplaceBreaklines(memoryList.GetStringValueN(SavePosition, SeekAndRead, encoding));
            return Stringvalue.TrimEnd('\0');
        }
        public static void SetStringUE(this MemoryList memoryList, string str, Encoding encoding, bool SavePosition = true, int SeekAndRead = -1)
        {
            memoryList.SetStringValueN(ReplaceBreaklines(str, true), SavePosition, SeekAndRead, encoding);
        }

        public static void SetStringUE(this MemoryList memoryList, string StringValue, bool UseUnicode = false,bool IgnoreNull=true)
        {

            StringValue = ReplaceBreaklines(StringValue, true);

            if (string.IsNullOrEmpty(StringValue)&&IgnoreNull)
            {
                memoryList.InsertIntValue(0);
                return;
            }


            StringValue += '\0';

            Encoding encoding = Encoding.Unicode;
            if (IsASCII(StringValue) && !UseUnicode)
            {
                encoding = Encoding.ASCII;
            }

            byte[] TextBytes = encoding.GetBytes(StringValue);

            if (encoding == Encoding.ASCII)
            {
                memoryList.InsertIntValue(TextBytes.Length);
                memoryList.InsertBytes(TextBytes);
            }
            else
            {
                memoryList.InsertIntValue(TextBytes.Length / -2);
                memoryList.InsertBytes(TextBytes);
            }
        }
        
        public static string GetStringUE(this MemoryList memoryList, int Lenght, bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            string Stringvalue = ReplaceBreaklines(memoryList.GetStringValue(Lenght, SavePosition, SeekAndRead, encoding));
            return Stringvalue.TrimEnd('\0');
        }





        public static int ReplaceStringUE_Func(this MemoryList memoryList, string StringValue)
        {
            int StringLength = 0;
            StringValue = ReplaceBreaklines(StringValue, true);

            memoryList.Skip(-1);
            ExprToken eExpr = (ExprToken)memoryList.GetByteValue();
            if (eExpr == ExprToken.EX_StringConst)
            {
                StringLength = memoryList.DeleteStringN(-1, Encoding.ASCII);
            }
            else if (eExpr == ExprToken.EX_UnicodeStringConst)
            {
                StringLength = memoryList.DeleteStringN(-1, Encoding.Unicode);
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
                memoryList.InsertStringValueN(StringValue, true, -1, encoding);
            }
            else
            {
                memoryList.SetByteValue((byte)ExprToken.EX_UnicodeStringConst);
                memoryList.InsertStringValueN(StringValue, true, -1, encoding);
            }
            return StringLength;
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

        public static string ReplaceBreaklines(string StringValue, bool Back = false)
        {
            if (!Back)
            {
                StringValue = StringValue.Replace("\r\n", "<cf>");
                StringValue = StringValue.Replace("\r", "<cr>");
                StringValue = StringValue.Replace("\n", "<lf>");
            }
            else
            {
                StringValue = StringValue.Replace("<cf>", "\r\n");
                StringValue = StringValue.Replace("<cr>", "\r");
                StringValue = StringValue.Replace("<lf>", "\n");
            }

            return StringValue;
        }

        public static void ReplaceStringUE(this MemoryList memoryList, string StringValue)
        {


            StringValue = ReplaceBreaklines(StringValue, true);

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

            byte[] TextBytes = encoding.GetBytes(StringValue);

            if (encoding == Encoding.ASCII)
            {
                memoryList.InsertIntValue(TextBytes.Length);
                memoryList.InsertBytes(TextBytes);
            }
            else
            {
                memoryList.InsertIntValue(TextBytes.Length / -2);
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

    public class FName
    {
        public FName(MemoryList memoryList, Uexp uexp, string PropertyName, bool Modify = false)
        {
            int NameIndex = memoryList.GetIntValue();
            memoryList.Skip(4);
            if (!Modify)
            {
                uexp.Strings.Add(new List<string>() { PropertyName, uexp.UassetData.GetPropertyName(NameIndex), !uexp.UassetData.IOFile ? "be careful with this value." : "Can't edit this value.", !uexp.UassetData.IOFile ? "#FFBFB2" : "#FF0000", "#000000" });
            }
            else
            {
                uexp.UassetData.EditName(uexp.Strings[uexp.CurrentIndex][1], NameIndex);
                uexp.CurrentIndex++;
            }
        }
    }


    public class TextHistory
    {
        public TextHistory(MemoryList memoryList, Uexp uexp, string PropertyName, bool Modify = false)
        {
            memoryList.Skip(4); //unkown
            TextHistoryType texthistorytype = (TextHistoryType)memoryList.GetUByteValue();
            switch (texthistorytype)
            {
                case TextHistoryType.None:
                    if (memoryList.GetIntValue() != 0)
                    {
                        new ReadStringProperty(memoryList, uexp, PropertyName, Modify);
                    }
                    break;

                case TextHistoryType.Base:
                    new ReadStringProperty(memoryList, uexp, PropertyName + "_1", Modify);
                    new ReadStringProperty(memoryList, uexp, PropertyName + "_2", Modify);
                    new ReadStringProperty(memoryList, uexp, PropertyName + "_3", Modify);

                    if (uexp.DumpNameSpaces)
                    {
                        uexp.StringNodes.Add(new StringNode() { 
                        NameSpace = uexp.Strings[uexp.Strings.Count - 3][1],
                        Key= uexp.Strings[uexp.Strings.Count - 2][1],
                        Value= uexp.Strings[uexp.Strings.Count - 1][1]
                        });
                    }

                    break;
                case TextHistoryType.NamedFormat:
                case TextHistoryType.OrderedFormat:
                    {
                        new TextHistory(memoryList, uexp, PropertyName, Modify);
                        int ArgumentsCount = memoryList.GetIntValue();
                        for (int i = 0; i < ArgumentsCount; i++)
                        {
                            GetArgumentValue(memoryList, uexp, PropertyName, Modify);
                        }
                    }
                    break;

                case TextHistoryType.AsNumber:
                case TextHistoryType.AsPercent:
                case TextHistoryType.AsCurrency:
                    {
                        GetArgumentValue(memoryList, uexp, PropertyName, Modify);
                        new ReadStringProperty(memoryList, uexp, PropertyName + "_1", Modify);
                        new ReadStringProperty(memoryList, uexp, PropertyName + "_2", Modify);
                    }
                    break;
                case TextHistoryType.StringTableEntry:
                    new FName(memoryList, uexp, PropertyName, Modify);
                    new ReadStringProperty(memoryList, uexp, PropertyName + "_1", Modify);

                    if (uexp.DumpNameSpaces)
                    {
                        uexp.StringNodes.Add(new StringNode()
                        {
                            Key = uexp.Strings[uexp.Strings.Count - 2][1],
                            Value = uexp.Strings[uexp.Strings.Count - 1][1]
                        });
                    }

                    break;

                default:
                    throw new Exception("UnKnown 'TextProperty' type: " + texthistorytype.ToString());
            }
        }

        private static void GetArgumentValue(MemoryList memoryList, Uexp uexp, string PropertyName, bool Modify)
        {
            FormatArgumentType Type = (FormatArgumentType)memoryList.GetUByteValue();

            switch (Type)
            {
                case FormatArgumentType.Text:
                    new TextHistory(memoryList, uexp, PropertyName, Modify);
                    break;
                case FormatArgumentType.Int:
                case FormatArgumentType.UInt:
                case FormatArgumentType.Double:
                    memoryList.Skip(8);
                    break;
                case FormatArgumentType.Float:
                    memoryList.Skip(4);
                    break;
                case FormatArgumentType.Gender:
                    memoryList.Skip(1);
                    break;
                default:
                    throw new Exception("Unkown argument type: " + Type.ToString());
            }
        }
    }



}
