﻿using Helper.MemoryList;
using System;
using System.Collections.Generic;

namespace AssetParser
{
    public class StructProperty
    {

        public StructProperty(MemoryList memoryList, Uexp uexp, bool FromStruct = true, bool Modify = false)
        {

            while (memoryList.GetPosition() < memoryList.GetSize())
            {

                string PropertyName = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
                if (PropertyName == "None")
                {
                    break;
                }
                string Property = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
                int ThisPosition = memoryList.GetPosition();
                int PropertyLength = memoryList.GetIntValue();
                memoryList.Skip(4);//null

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PropertyName-> " + PropertyName);
                Console.WriteLine("Property-> " + Property);
                Console.WriteLine("PropertyLength-> " + PropertyLength);
                Console.ForegroundColor = ConsoleColor.White;

                if (Property == "MapProperty")
                {

                    string MapKey = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(4);//null or something
                    string MapValue = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(4);//null or something
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }
                    int MapDataPosition = memoryList.GetPosition();
                    MemoryList MapData = new MemoryList(memoryList.GetBytes(PropertyLength));
                    MapData.Skip(4);//null or something
                    int MapCount = MapData.GetIntValue();
                    //Console.WriteLine("MapCount-> " + MapCount);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("MapProperty");
                    Console.ForegroundColor = ConsoleColor.White;
                    try
                    {
                        for (int Mapindex = 0; Mapindex < MapCount; Mapindex++)
                        {
                            if (MapData.GetIntValue(false) == 0)
                            {
                                MapData.Skip(4);//null or something
                            }
                            Console.WriteLine("Mapindex-> " + Mapindex);
                            Console.WriteLine("MapKey-> " + MapKey);
                            Console.WriteLine("MapValue-> " + MapValue);
                            PropertyParser(PropertyName, MapKey, -1, MapData, uexp, Modify);
                            PropertyParser(PropertyName, MapValue, -1, MapData, uexp, Modify);
                        }
                    }
                    catch
                    {

                    }
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("EndMapProperty");
                    Console.ForegroundColor = ConsoleColor.White;
                    if (Modify)
                    {
                        memoryList.ReplaceBytes(PropertyLength, MapData.ToArray(), false, MapDataPosition);
                        memoryList.Seek(MapDataPosition + MapData.GetSize());
                        memoryList.SetIntValue(MapData.GetSize(), false, ThisPosition);
                    }
                }
                else if (Property == "ArrayProperty")
                {
                    string ArrayType = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(4);//null or something
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }
                    int ArrayPosition = memoryList.GetPosition();
                    MemoryList ArrayData = new MemoryList(memoryList.GetBytes(PropertyLength));
                    int ArrayCount = ArrayData.GetIntValue();
                    if (ArrayCount == 0)
                    {
                        continue;
                    }
                    try
                    {
                        if (ArrayType == "StructProperty")
                        {
                            string ArrayName /*?*/ = uexp.UassetData.GetPropertyName(ArrayData.GetIntValue());
                            ArrayData.Skip(12); //null bytes
                            int StructLength = ArrayData.GetIntValue();
                            ArrayData.Skip(4); //null or something
                            string StructType = uexp.UassetData.GetPropertyName(ArrayData.GetIntValue());
                            ArrayData.Skip(20); //Unkown bytes
                            if (FromStruct)
                            {
                                ArrayData.Skip(1);  //null For "Struct"
                            }
                            for (int Arrayindex = 0; Arrayindex < ArrayCount; Arrayindex++)
                            {
                                PropertyParser(PropertyName, StructType, -1, ArrayData, uexp, Modify);
                            }
                        }
                        else
                        {
                            for (int Arrayindex = 0; Arrayindex < ArrayCount; Arrayindex++)
                            {
                                PropertyParser(PropertyName, ArrayType, -1, ArrayData, uexp, Modify);
                            }
                        }
                    }
                    catch
                    {

                    }
                    if (Modify)
                    {
                        memoryList.ReplaceBytes(PropertyLength, ArrayData.ToArray(), false, ArrayPosition);
                        memoryList.Seek(ArrayPosition + ArrayData.GetSize());
                        memoryList.SetIntValue(ArrayData.GetSize(), false, ThisPosition);
                    }
                }
                else if (Property == "StructProperty")
                {
                    string StructType = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    Console.WriteLine("StructType-> " + StructType);
                    memoryList.Skip(4);  //null or something
                    memoryList.Skip(16); //null bytes
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }

                    int StructPosition = memoryList.GetPosition();
                    MemoryList StructData = new MemoryList(memoryList.GetBytes(PropertyLength));
                    if (StructData.GetSize() <= 4)
                    {
                        continue;
                    }

                    if (StructType != "Box2D"
                    && StructType != "Box"
                    && StructType != "ColorMaterialInput"
                    && StructType != "Color"
                    && StructType != "ComponentDelegateBinding"
                    && StructType != "DateTime"
                    && StructType != "ExpressionInput"
                    && StructType != "GameplayTagContainer"
                    && StructType != "Guid"
                    && StructType != "IntPoint"
                    && StructType != "ItemsBitArray"
                    && StructType != "LinearColor"
                    && StructType != "MovieSceneEvaluationKey"
                    && StructType != "MovieSceneFloatChannel"
                    && StructType != "MovieSceneFrameRange"
                    && StructType != "MovieSceneSegment"
                    && StructType != "MovieSceneSequenceID"
                    && StructType != "PerPlatformFloat"
                    && StructType != "PointerToUberGraphFrame"
                    && StructType != "Quat"
                    && StructType != "RichCurveKey"
                    && StructType != "Rotator"
                    && StructType != "ScalarMaterialInput"
                    && StructType != "SoftClassPath"
                    && StructType != "StringAssetReference"
                    && StructType != "Table"
                    && StructType != "Timespan"
                    && StructType != "Vector2D"
                    && StructType != "Vector4"
                    && StructType != "VectorMaterialInput"
                    && StructType != "Vector"
                    && StructType != "ViewTargetBlendParams"
                    && StructType != "MovieSceneEvaluationFieldEntityTree"
                    && StructType != "MovieSceneTrackImplementationPtr")
                    {
                        try
                        {
                            new StructProperty(StructData, uexp, true, Modify);
                        }
                        catch
                        {
                        }
                    }

                    if (Modify)
                    {
                        memoryList.ReplaceBytes(PropertyLength, StructData.ToArray(), false, StructPosition);
                        memoryList.Seek(StructPosition + StructData.GetSize());
                        memoryList.SetIntValue(StructData.GetSize(), false, ThisPosition);
                    }


                }
                else if (Property == "BoolProperty")
                {
                    memoryList.Skip(1);
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }
                    //1-> bool value
                    //1-> null For "Struct"
                }
                else if (Property == "EnumProperty")
                {
                    memoryList.Skip(8);//val 1
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }
                    memoryList.Skip(8);//val 2
                }
                else if (Property == "TextProperty")
                {
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }
                    memoryList.Skip(4); //unkown
                    byte ContainText = memoryList.GetByteValue();
                    MemoryList TextData;
                    int TextDataPosition = memoryList.GetPosition();
                    if (ContainText == 0xff)
                    {
                        TextData = new MemoryList(memoryList.GetBytes(PropertyLength - 5));
                        if (TextData.GetSize() == 0)
                        {
                            continue;
                        }
                        int TextLinesCount = TextData.GetIntValue();
                        for (int i = 0; i < TextLinesCount; i++)
                        {
                            if (!Modify)
                            {
                                uexp.Strings.Add(new List<string>() { PropertyName, TextData.GetStringUE() });

                            }
                            else
                            {
                                TextData.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                                uexp.CurrentIndex++;
                                memoryList.ReplaceBytes(PropertyLength - 5, TextData.ToArray(), false, TextDataPosition);
                                memoryList.Seek(TextDataPosition + TextData.GetSize());
                                memoryList.SetIntValue(TextData.GetSize() + 5, false, ThisPosition);
                            }
                        }
                        continue;
                    }

                    TextData = new MemoryList(memoryList.GetBytes(PropertyLength - 5));
                    if (!Modify)
                    {
                        uexp.Strings.Add(new List<string>() { PropertyName, TextData.GetStringUE() });
                        uexp.Strings.Add(new List<string>() { PropertyName, TextData.GetStringUE() });
                        uexp.Strings.Add(new List<string>() { PropertyName, TextData.GetStringUE() });
                    }
                    else
                    {
                        TextData.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                        uexp.CurrentIndex++;
                        TextData.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                        uexp.CurrentIndex++;
                        TextData.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                        uexp.CurrentIndex++;
                        memoryList.ReplaceBytes(PropertyLength - 5, TextData.ToArray(), false, TextDataPosition);
                        memoryList.Seek(TextDataPosition + TextData.GetSize());
                        memoryList.SetIntValue(TextData.GetSize() + 5, false, ThisPosition);
                    }
                }
                else if (Property == "ByteProperty")
                {
                    if (PropertyLength == 1)
                    {
                        memoryList.Skip(8); //Val
                        if (FromStruct)
                        {
                            memoryList.Skip(1);  //null For "Struct"
                        }
                    }
                    else
                    {
                        memoryList.Skip(8); //Val1
                        if (FromStruct)
                        {
                            memoryList.Skip(1);  //null For "Struct"
                        }
                        memoryList.Skip(8); //Val2
                    }
                }
                else
                {
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }
                    if (PropertyLength == -1)
                    {
                        PropertyParser(PropertyName, Property, PropertyLength, memoryList, uexp, Modify);
                        continue;
                    }

                    int TextDataPosition = memoryList.GetPosition();
                    MemoryList TextData = new MemoryList(memoryList.GetBytes(PropertyLength));
                    PropertyParser(PropertyName, Property, PropertyLength, TextData, uexp, Modify);

                    if (Modify)
                    {
                        memoryList.ReplaceBytes(PropertyLength, TextData.ToArray(), false, TextDataPosition);
                        memoryList.Seek(TextDataPosition + TextData.GetSize());
                        memoryList.SetIntValue(TextData.GetSize(), false, ThisPosition);
                    }
                }

            }
        }


        private void PropertyParser(string PropertyName, string Property, int PropertyLength, MemoryList memoryList, Uexp uexp, bool Modify = false)
        {
            if (Property == "Int8Property")
            {
                memoryList.Skip(1);
            }
            else if (Property == "Int16Property")
            {
                memoryList.Skip(2);
            }
            else if (Property == "IntProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "Int64Property")
            {
                memoryList.Skip(8);
            }
            else if (Property == "UInt8Property")
            {
                memoryList.Skip(1);
            }
            else if (Property == "UInt16Property")
            {
                memoryList.Skip(2);
            }
            else if (Property == "UInt32Property")
            {
                memoryList.Skip(4);
            }
            else if (Property == "UInt64Property")
            {
                memoryList.Skip(8);
            }
            else if (Property == "FloatProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "DoubleProperty")
            {
                memoryList.Skip(8);
            }
            else if (Property == "ObjectProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "SoftObjectProperty")
            {
                memoryList.Skip(12);//  12 / 4
            }
            else if (Property == "NameProperty")
            {
                memoryList.Skip(8);//  8 / 4
            }
            else if (Property == "MulticastSparseDelegateProperty")
            {
                memoryList.Skip(16);// 16 / 4
            }
            else if (Property == "MulticastDelegateProperty")
            {
                memoryList.Skip(16);// 16 / 4
            }
            else if (Property == "LazyObjectProperty")
            {
                memoryList.Skip(16);// bytes range
            }
            else if (Property == "InterfaceProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "EnumProperty")
            {
                memoryList.Skip(8);
            }
            else if (Property == "UserDefinedEnum")
            {
                memoryList.Skip(8);
                memoryList.Skip(memoryList.GetIntValue() * 9);//4 4 1
            }
            else if (Property == "ActorReference")
            {
                memoryList.Skip(4);
                if (!Modify)
                {
                    uexp.Strings.Add(new List<string>() { PropertyName, memoryList.GetStringUE() });
                }
                else
                {
                    memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                    uexp.CurrentIndex++;
                }
                memoryList.Skip(2);
            }
            else if (Property == "MapProperty")
            {
                string MapKey = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
                string MapValue = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
                memoryList.Skip(4);//null or something
                int MapCount = memoryList.GetIntValue();
                //Console.WriteLine("MapCount-> " + MapCount);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("MapProperty");
                Console.ForegroundColor = ConsoleColor.White;
                try
                {
                    for (int Mapindex = 0; Mapindex < MapCount; Mapindex++)
                    {
                        if (memoryList.GetIntValue(false) == 0)
                        {
                            memoryList.Skip(4);//null or something
                        }
                        Console.WriteLine("Mapindex-> " + Mapindex);
                        Console.WriteLine("MapKey-> " + MapKey);
                        Console.WriteLine("MapValue-> " + MapValue);
                        PropertyParser(PropertyName, MapKey, -1, memoryList, uexp, Modify);
                        PropertyParser(PropertyName, MapValue, -1, memoryList, uexp, Modify);
                    }
                }
                catch
                {

                }
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("EndMapProperty");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (Property == "FieldPathProperty")
            {
                int FieldPathCount = memoryList.GetIntValue();
                memoryList.Skip(4);
                if (FieldPathCount == 1)
                {
                    memoryList.Skip(8); //8 / 4
                }
            }
            else if (Property == "AssetObjectProperty")
            {
                if (!Modify)
                {
                    uexp.Strings.Add(new List<string>() { PropertyName, memoryList.GetStringUE() });
                }
                else
                {
                    memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                    uexp.CurrentIndex++;
                }
            }
            else if (Property == "BoolProperty")
            {
                memoryList.Skip(1);
            }
            else if (Property == "ByteProperty")
            {
                if (PropertyLength == 1)
                {
                    memoryList.Skip(1);
                }
                else
                {
                    memoryList.Skip(8);
                }
            }
            else if (Property == "ArrayProperty")
            {
                string ArrayType = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
                int ArrayCount = memoryList.GetIntValue();
                if (ArrayCount == 0)
                {
                    return;
                }
                if (ArrayType == "StructProperty")
                {
                    string ArrayName /*?*/ = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(12); //null bytes
                    int StructLength = memoryList.GetIntValue();
                    memoryList.Skip(4); //null or something
                    string StructType = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(20); //Unkown bytes
                    for (int Arrayindex = 0; Arrayindex < ArrayCount; Arrayindex++)
                    {
                        PropertyParser(PropertyName, StructType, -1, memoryList, uexp, Modify);
                    }
                }
                else
                {
                    for (int Arrayindex = 0; Arrayindex < ArrayCount; Arrayindex++)
                    {
                        PropertyParser(PropertyName, ArrayType, -1, memoryList, uexp, Modify);
                    }
                }

            }
            else if (Property == "StrProperty")
            {
                if (!Modify)
                {
                    uexp.Strings.Add(new List<string>() { PropertyName, memoryList.GetStringUE() });
                    Console.WriteLine(uexp.Strings[uexp.Strings.Count - 1][1]);
                }
                else
                {
                    //memoryList.GetStringUE();
                    memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                    uexp.CurrentIndex++;
                }
            }
            else if (Property == "TextProperty")
            {
                memoryList.Skip(4); //unkown
                byte ContainText = memoryList.GetByteValue();
                if (ContainText == 0xff)
                {
                    int TextLinesCount = memoryList.GetIntValue();
                    for (int i = 0; i < TextLinesCount; i++)
                    {
                        if (!Modify)
                        {
                            uexp.Strings.Add(new List<string>() { PropertyName, memoryList.GetStringUE() });

                        }
                        else
                        {
                            memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                            uexp.CurrentIndex++;
                        }
                    }
                    return;
                }

                if (!Modify)
                {
                    uexp.Strings.Add(new List<string>() { PropertyName, memoryList.GetStringUE() });
                    uexp.Strings.Add(new List<string>() { PropertyName, memoryList.GetStringUE() });
                    uexp.Strings.Add(new List<string>() { PropertyName, memoryList.GetStringUE() });
                }
                else
                {
                    memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                    uexp.CurrentIndex++;
                    memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                    uexp.CurrentIndex++;
                    memoryList.ReplaceStringUE(uexp.Strings[uexp.CurrentIndex][1]);
                    uexp.CurrentIndex++;
                }

            }
            else if (Property == "StructProperty")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("StructProperty");
                Console.ForegroundColor = ConsoleColor.White;

                new StructProperty(memoryList, uexp, true, Modify);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("EndStructProperty");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                new StructProperty(memoryList, uexp, true, Modify);
            }

        }
    }
}
