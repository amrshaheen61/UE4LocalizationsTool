using Helper.MemoryList;
using System;


namespace AssetParser
{
    public class StructProperty
    {

        public StructProperty(MemoryList memoryList, Uexp uexp, bool FromStruct = true, bool FromProperty = false, bool Modify = false)
        {

            while (memoryList.GetPosition() < memoryList.GetSize())
            {
                ulong GetPropertyName;
                if (FromProperty)
                {
                    GetPropertyName = memoryList.GetUInt64Value();

                    ConsoleMode.Print($"PropertyNameMoving- {GetPropertyName} > " + memoryList.GetPosition(), ConsoleColor.DarkBlue);
                    if (GetPropertyName > (uint)uexp.UassetData.Number_of_Names && (int)GetPropertyName > 0)
                    {
                        memoryList.Skip(-4);
                        continue;
                    }
                }
                else
                {
                    GetPropertyName = (uint)memoryList.GetIntValue();
                    memoryList.Skip(4);
                }
                string PropertyName = uexp.UassetData.GetPropertyName((int)GetPropertyName);
                if (PropertyName == "None")
                {
                    break;
                }

                string Property = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
                int ThisPosition = memoryList.GetPosition();
                int PropertyLength = memoryList.GetIntValue();
                memoryList.Skip(4);//null

                ConsoleMode.Print("PropertyName-> " + PropertyName, ConsoleColor.Green);
                ConsoleMode.Print("Property-> " + Property, ConsoleColor.Green);
                ConsoleMode.Print("PropertyLength-> " + PropertyLength, ConsoleColor.Green);


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

                    ConsoleMode.Print("MapCount-> " + MapCount, ConsoleColor.Blue);
                    ConsoleMode.Print("MapProperty " + MapCount, ConsoleColor.Blue);

                    try
                    {
                        for (int Mapindex = 0; Mapindex < MapCount; Mapindex++)
                        {
                            if (MapData.GetIntValue(false) == 0)
                            {
                                MapData.Skip(4);//null or something
                            }

                            ConsoleMode.Print("Mapindex-> " + Mapindex, ConsoleColor.Blue);
                            ConsoleMode.Print("MapKey-> " + MapKey, ConsoleColor.Blue);
                            ConsoleMode.Print("MapValue-> " + MapValue, ConsoleColor.Blue);
                            PropertyParser(ref PropertyName, MapKey, -1, MapData, uexp, Modify);
                            PropertyParser(ref PropertyName, MapValue, -1, MapData, uexp, Modify);
                        }
                    }
                    catch (Exception ex)
                    {
                        uexp.IsGood = false;
                        ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
                    }

                    ConsoleMode.Print("EndMapProperty", ConsoleColor.Blue);

                    if (Modify)
                    {
                        memoryList.ReplaceBytes(PropertyLength, MapData.ToArray(), false, MapDataPosition);
                        memoryList.Seek(MapDataPosition + MapData.GetSize());
                        memoryList.SetIntValue(MapData.GetSize(), false, ThisPosition);
                    }
                }
                else if (Property == "ArrayProperty")
                {

                    ConsoleMode.Print("ArrayProperty", ConsoleColor.Yellow);
                    string ArrayType = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(4);//null or something
                    ConsoleMode.Print("ArrayType-> " + ArrayType, ConsoleColor.Yellow);
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
                            ConsoleMode.Print("ArrayName-> " + ArrayName, ConsoleColor.DarkYellow);
                            ArrayData.Skip(12); //null Databytes
                            int StructpositionEdit = ArrayData.GetPosition();
                            int StructLength = ArrayData.GetIntValue();
                            ArrayData.Skip(4); //null or something
                            string StructType = uexp.UassetData.GetPropertyName(ArrayData.GetIntValue());
                            ConsoleMode.Print("ArrayStructType-> " + StructType, ConsoleColor.DarkYellow);
                            ArrayData.Skip(20); //Unkown Databytes
                            if (FromStruct)
                            {
                                ArrayData.Skip(1);  //null For "Struct"
                            }

                            int StructPosition = ArrayData.GetPosition();

                            for (int Arrayindex = 0; Arrayindex < ArrayCount; Arrayindex++)
                            {
                                PropertyParser(ref PropertyName, StructType, -1, ArrayData, uexp, Modify);
                            }


                            if (Modify)
                            {
                                ArrayData.SetIntValue(ArrayData.GetSize() - StructPosition, false, StructpositionEdit);
                            }
                        }
                        else
                        {
                            PropertyName = ReadArrayData(uexp, Modify, PropertyName, ArrayType, ArrayData, ArrayCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        uexp.IsGood = false;
                        ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
                    }

                    if (Modify)
                    {
                        memoryList.ReplaceBytes(PropertyLength, ArrayData.ToArray(), false, ArrayPosition);
                        memoryList.Seek(ArrayPosition + ArrayData.GetSize());
                        memoryList.SetIntValue(ArrayData.GetSize(), false, ThisPosition);
                    }

                    ConsoleMode.Print("EndArrayProperty", ConsoleColor.Yellow);
                }
                else if (Property == "StructProperty")
                {
                    string StructType = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    ConsoleMode.Print("StructType-> " + StructType, ConsoleColor.Gray);
                    memoryList.Skip(4);  //null or something
                    memoryList.Skip(16); //null Databytes
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }


                    if (StructType == "MovieSceneEvalTemplatePtr" || StructType == "MovieSceneTrackImplementationPtr")
                    {
                        PropertyParser(ref PropertyName, StructType, PropertyLength, memoryList, uexp, Modify);
                        continue;
                    }

                    int StructPosition = memoryList.GetPosition();
                    MemoryList StructData = new MemoryList(memoryList.GetBytes(PropertyLength));
                    if (StructData.GetSize() <= 4)
                    {
                        continue;
                    }

                    if (StructType != "ActorReference"
                     && StructType != "Box2D"
                     && StructType != "Box"
                     && StructType != "BoxSphereBounds"
                     && StructType != "Color"
                     && StructType != "ColorMaterialInput"
                     && StructType != "ComponentDelegateBinding"
                     && StructType != "DateTime"
                     && StructType != "EngineVersion"
                     && StructType != "EngineVersionBase"
                     && StructType != "EvaluationTreeEntryHandle"
                     && StructType != "ExpressionInput"
                     && StructType != "Float16"
                     && StructType != "FrameNumber"
                     && StructType != "FrameRate"
                     && StructType != "GameplayTagContainer"
                     && StructType != "Guid"
                     && StructType != "IntPoint"
                     && StructType != "IntVector"
                     && StructType != "ItemsBitArray"
                     && StructType != "LevelSequenceObjectReferenceMap"
                     && StructType != "LinearColor"
                     && StructType != "MaterialAttributesInput"
                     && StructType != "MovieSceneEvaluationKey"
                     && StructType != "MovieSceneEvaluationTree"
                     && StructType != "MovieSceneEvaluationTreeNode"
                     && StructType != "MovieSceneEvaluationTreeNodeHandle"
                     && StructType != "MovieSceneFloatChannel"
                     && StructType != "MovieSceneFloatValue"
                     && StructType != "MovieSceneFrameRange"
                     && StructType != "MovieSceneSegment"
                     && StructType != "MovieSceneSegmentIdentifier"
                     && StructType != "MovieSceneSequenceID"
                     && StructType != "MovieSceneSubSequenceTree"
                     && StructType != "MovieSceneTangentData"
                     && StructType != "MovieSceneTrackIdentifier"
                     && StructType != "NavAgentSelector"
                     && StructType != "PerPlatformBool"
                     && StructType != "PerPlatformFloat"
                     && StructType != "PerPlatformInt"
                     && StructType != "PerQualityLevelInt"
                     && StructType != "Plane"
                     && StructType != "PointerToUberGraphFrame"
                     && StructType != "Quat"
                     && StructType != "RichCurveKey"
                     && StructType != "Rotator"
                     && StructType != "SHAHash"
                     && StructType != "ScalarMaterialInput"
                     && StructType != "SectionEvaluationDataTree"
                     && StructType != "ShadingModelMaterialInput"
                     && StructType != "SimpleCurveKey"
                     && StructType != "SkeletalMeshSamplingLODBuiltData"
                     && StructType != "SkeletalMeshSamplingRegionBuiltData"
                     && StructType != "SmartName"
                     && StructType != "SoftClassPath"
                     && StructType != "SoftObjectPath"
                     && StructType != "Sphere"
                     && StructType != "StringAssetReference"
                     && StructType != "StringClassReference"
                     && StructType != "Table"
                     && StructType != "Timespan"
                     && StructType != "Transform"
                     && StructType != "UInt128"
                     && StructType != "Vector2D"
                     && StructType != "Vector2MaterialInput"
                     && StructType != "Vector4"
                     && StructType != "Vector"
                     && StructType != "VectorMaterialInput"
                     && StructType != "Vector_NetQuantize10"
                     && StructType != "Vector_NetQuantize100"
                     && StructType != "Vector_NetQuantize"
                     && StructType != "Vector_NetQuantizeNormal"
                     && StructType != "ViewTargetBlendParams")
                    {
                        try
                        {

                            if (StructType == "MovieSceneEventParameters")
                            {
                                PropertyParser(ref PropertyName, StructType, PropertyLength, StructData, uexp, Modify);
                            }
                            else
                            {

                                new StructProperty(StructData, uexp, true, false, Modify);
                            }
                        }
                        catch (Exception ex)
                        {
                            uexp.IsGood = false;
                            ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
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
                    int TextDataPosition = memoryList.GetPosition();
                    MemoryList TextData = new MemoryList(memoryList.GetBytes(PropertyLength));

                    if (TextData.GetSize() == 5)
                    {
                        continue;
                    }

                    try
                    {
                        if (uexp.UassetData.EngineVersion < UEVersions.VER_UE4_FTEXT_HISTORY)
                        {

                            new ReadStringProperty(TextData, uexp, PropertyName + "_0", Modify);
                            if (uexp.UassetData.EngineVersion >= UEVersions.VER_UE4_ADDED_NAMESPACE_AND_KEY_DATA_TO_FTEXT)
                            {
                                new ReadStringProperty(TextData, uexp, PropertyName + "_1", Modify);
                                new ReadStringProperty(TextData, uexp, PropertyName + "_2", Modify);
                            }
                            else
                            {
                                new ReadStringProperty(TextData, uexp, PropertyName + "_2", Modify);
                            }

                        }

                        if (uexp.UassetData.EngineVersion > UEVersions.VER_UE4_FTEXT_HISTORY)
                        {
                            new TextHistory(TextData, uexp, PropertyName, Modify);
                        }

                        if (Modify)
                        {
                            memoryList.ReplaceBytes(PropertyLength, TextData.ToArray(), false, TextDataPosition);
                            memoryList.Seek(TextDataPosition + TextData.GetSize());
                            memoryList.SetIntValue(TextData.GetSize(), false, ThisPosition);
                        }
                    }
                    catch (Exception ex)
                    {
                        uexp.IsGood = false;
                        ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
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
                else if (Property == "SetProperty")
                {

                    string SetKey = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(4);//null or something
                    if (FromStruct)
                    {
                        memoryList.Skip(1);  //null For "Struct"
                    }
                    int StructPosition = memoryList.GetPosition();
                    MemoryList SetData = new MemoryList(memoryList.GetBytes(PropertyLength));

                    int SetCount = SetData.GetIntValue();
                    for (int n = 0; n < SetCount; n++)
                    {
                        PropertyParser(ref PropertyName, SetKey, -1, SetData, uexp, Modify);
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
                        PropertyParser(ref PropertyName, Property, PropertyLength, memoryList, uexp, Modify);
                        continue;
                    }

                    int TextDataPosition = memoryList.GetPosition();
                    MemoryList TextData = new MemoryList(memoryList.GetBytes(PropertyLength));
                    try
                    {
                        PropertyParser(ref PropertyName, Property, PropertyLength, TextData, uexp, Modify);
                        if (Modify)
                        {
                            memoryList.ReplaceBytes(PropertyLength, TextData.ToArray(), false, TextDataPosition);
                            memoryList.Seek(TextDataPosition + TextData.GetSize());
                            memoryList.SetIntValue(TextData.GetSize(), false, ThisPosition);
                        }
                    }
                    catch (Exception ex)
                    {
                        uexp.IsGood = false;
                        ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
                    }
                }

            }
        }



        private void PropertyParser(ref string PropertyName, string Property, int PropertyLength, MemoryList memoryList, Uexp uexp, bool Modify = false)
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
                new FName(memoryList, uexp, PropertyName, Modify);
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
                memoryList.Skip(16);// Databytes range
            }
            else if (Property == "InterfaceProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "EnumProperty")
            {
                PropertyName = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
            }
            else if (Property == "UserDefinedEnum")
            {
                memoryList.Skip(8);
                memoryList.Skip(memoryList.GetIntValue() * 9);//4 4 1
            }
            else if (Property == "ActorReference")
            {
                memoryList.Skip(4);
                new ReadStringProperty(memoryList, uexp, PropertyName, Modify);
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

                ConsoleMode.Print("MapCount-> " + MapCount, ConsoleColor.DarkBlue);
                ConsoleMode.Print("StartMap", ConsoleColor.DarkBlue);

                try
                {
                    for (int Mapindex = 0; Mapindex < MapCount; Mapindex++)
                    {
                        if (memoryList.GetIntValue(false) == 0)
                        {
                            memoryList.Skip(4);//null or something
                        }
                        ConsoleMode.Print("Mapindex2-> " + Mapindex, ConsoleColor.DarkBlue);
                        ConsoleMode.Print("MapKey2-> " + MapKey, ConsoleColor.DarkBlue);
                        ConsoleMode.Print("MapValue2-> " + MapValue, ConsoleColor.DarkBlue);
                        PropertyParser(ref PropertyName, MapKey, -1, memoryList, uexp, Modify);
                        PropertyParser(ref PropertyName, MapValue, -1, memoryList, uexp, Modify);

                    }
                }
                catch (Exception ex)
                {
                    uexp.IsGood = false;
                    ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
                }
                ConsoleMode.Print("EndMap", ConsoleColor.DarkBlue);
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
                new ReadStringProperty(memoryList, uexp, PropertyName, Modify);
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
                    memoryList.Skip(12); //null Databytes
                    int StructpositionEdit = memoryList.GetPosition();
                    int StructLength = memoryList.GetIntValue();
                    memoryList.Skip(4); //null or something
                    string StructType = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                    memoryList.Skip(20); //Unkown Databytes
                    int StructPosition = memoryList.GetPosition();
                    MemoryList StructData = new MemoryList(memoryList.GetBytes(StructLength));
                    try
                    {

                        for (int Arrayindex = 0; Arrayindex < ArrayCount; Arrayindex++)
                        {
                            PropertyParser(ref PropertyName, ArrayType, -1, StructData, uexp, Modify);
                        }
                    }
                    catch (Exception ex)
                    {
                        uexp.IsGood = false;
                        ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
                    }

                    if (Modify)
                    {
                        memoryList.ReplaceBytes(StructLength, StructData.ToArray(), false, StructPosition);
                        memoryList.Seek(StructPosition + memoryList.GetSize());
                        memoryList.SetIntValue(StructData.GetSize(), false, StructpositionEdit);
                    }
                }
                else
                {
                    PropertyName = ReadArrayData(uexp, Modify, PropertyName, ArrayType, memoryList, ArrayCount);
                }

            }
            else if (Property == "StrProperty")
            {
                new ReadStringProperty(memoryList, uexp, PropertyName, Modify);
            }
            else if (Property == "TextProperty")
            {
                if (uexp.UassetData.EngineVersion < UEVersions.VER_UE4_FTEXT_HISTORY)
                {

                    new ReadStringProperty(memoryList, uexp, PropertyName + "_0", Modify);
                    if (uexp.UassetData.EngineVersion >= UEVersions.VER_UE4_ADDED_NAMESPACE_AND_KEY_DATA_TO_FTEXT)
                    {
                        new ReadStringProperty(memoryList, uexp, PropertyName + "_1", Modify);
                        new ReadStringProperty(memoryList, uexp, PropertyName + "_2", Modify);
                    }
                    else
                    {
                        new ReadStringProperty(memoryList, uexp, PropertyName + "_2", Modify);
                    }

                }

                if (uexp.UassetData.EngineVersion > UEVersions.VER_UE4_FTEXT_HISTORY)
                {
                    new TextHistory(memoryList, uexp, PropertyName, Modify);
                }

            }
            else if (Property == "StructProperty")
            {



                ConsoleMode.Print("Struct->" + memoryList.GetPosition(), ConsoleColor.DarkCyan);
                new StructProperty(memoryList, uexp, uexp.UassetData.UseFromStruct, true, Modify);
                ConsoleMode.Print("EndStruct->" + memoryList.GetPosition(), ConsoleColor.DarkCyan);
            }
            else if (Property == "SetProperty")
            {
                string SetKey = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
                memoryList.Skip(4);//null or something
                int SetCount = memoryList.GetIntValue();
                for (int n = 0; n < SetCount; n++)
                {
                    PropertyParser(ref PropertyName, SetKey, -1, memoryList, uexp, Modify);
                }

            }
            else if (Property == "MulticastInlineDelegateProperty")
            {

            }
            //For StructProperty
            else if (Property == "FrameNumber")
            {
                memoryList.Skip(4);
            }
            else if (Property == "MovieSceneEvalTemplatePtr")
            {

                if (memoryList.GetStringUE().Length > 0)
                {

                    try
                    {
                        ConsoleMode.Print("MovieSceneEvalTemplatePtr--> StructProperty", ConsoleColor.Red);
                        new StructProperty(memoryList, uexp, uexp.UassetData.UseFromStruct, true, Modify);
                        ConsoleMode.Print("MovieSceneEvalTemplatePtr--> EndStructProperty", ConsoleColor.Red);
                    }
                    catch (Exception ex)
                    {
                        uexp.IsGood = false;
                        ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
                    }
                }
            }
            else if (Property == "MovieSceneTrackImplementationPtr")
            {
                ConsoleMode.Print("StartMovieSceneEvalTemplatePtrLength->" + PropertyLength);
                if (memoryList.GetStringUE().Length > 0 || PropertyLength > 0)
                {
                    ConsoleMode.Print("StartMovieSceneEvalTemplatePtr", ConsoleColor.Yellow);
                    try
                    {

                        ConsoleMode.Print("MovieSceneTrackImplementationPtr--> StructProperty", ConsoleColor.DarkYellow);
                        new StructProperty(memoryList, uexp, true, true, Modify);
                        ConsoleMode.Print("MovieSceneTrackImplementationPtr--> EndStructProperty", ConsoleColor.DarkYellow);
                    }
                    catch (Exception ex)
                    {
                        uexp.IsGood = false;
                        ConsoleMode.Print("Bug here: " + ex.Message, ConsoleColor.Red, ConsoleMode.ConsoleModeType.Error);
                    }
                    memoryList.Skip(4); //ImplementationPtr Index
                    ConsoleMode.Print("EndMovieSceneEvalTemplatePtr", ConsoleColor.Yellow);
                }
            }
            else if (Property == "MovieSceneEventParameters")
            {
                ConsoleMode.Print("MovieSceneEventParameters->" + PropertyLength);

                memoryList.GetInt64Value(); //unknown
                memoryList.GetStringUE();
                int ValuePosition = memoryList.GetPosition();
                int Value = memoryList.GetIntValue();
                memoryList.Skip(4);
                int EventBlockPosition = memoryList.GetPosition();
                int EventSize = memoryList.GetIntValue();
                MemoryList EventBlock = new MemoryList(memoryList.GetBytes(EventSize));


                while (EventBlock.GetPosition() < EventBlock.GetSize())
                {
                    string Name = EventBlock.GetStringUE();
                    if (Name == "None")
                    {
                        break;
                    }

                    string PropertyType = EventBlock.GetStringUE();
                    int SizePosition = EventBlock.GetPosition();
                    int PropertySize = EventBlock.GetIntValue();
                    EventBlock.Skip(4);
                    EventBlock.Skip(1);  //null
                    int BlockPosition = EventBlock.GetPosition();
                    MemoryList memory = new MemoryList(EventBlock.GetBytes(PropertySize));

                    PropertyParser(ref Name, PropertyType, PropertyLength, memory, uexp, Modify);
                    if (Modify)
                    {
                        EventBlock.SetIntValue(memory.GetSize(), false, SizePosition);
                        EventBlock.Seek(BlockPosition);
                        EventBlock.ReplaceBytes(PropertySize, memory.ToArray(), true);
                    }
                }


                if (Modify)
                {
                    memoryList.Seek(ValuePosition);
                    memoryList.SetIntValue(Value + (EventBlock.GetSize() - EventSize));
                    memoryList.Seek(EventBlockPosition);
                    memoryList.SetIntValue(EventBlock.GetSize());
                    memoryList.ReplaceBytes(EventSize, EventBlock.ToArray(), true);
                }
            }
            else
            {
                new StructProperty(memoryList, uexp, uexp.UassetData.UseFromStruct, true, Modify);
            }

        }

        private string ReadArrayData(Uexp uexp, bool Modify, string PropertyName, string ArrayType, MemoryList ArrayData, int ArrayCount)
        {
            if (ArrayType == "ByteProperty" && PropertyName == "Databytes" ||
                ArrayType == "ByteProperty" && PropertyName == "BinaryData")
            {

                int StartPosition = ArrayData.GetPosition() - 4; //for get ArrayCount
                var data = new MemoryList(ArrayData.GetBytes(ArrayCount));

                if (ArrayType == "ByteProperty" && PropertyName == "Databytes")
                    new TheLastOricru(data, PropertyName, uexp, Modify);

                if (ArrayType == "ByteProperty" && PropertyName == "BinaryData")
                    new OctopathTraveler(data, PropertyName, uexp, Modify);

                if (Modify)
                {
                    ArrayData.Seek(StartPosition);
                    ArrayData.SetIntValue(data.GetSize());
                    ArrayData.ReplaceBytes(ArrayCount, data.ToArray());
                }
            }
            else
            {
                for (int Arrayindex = 0; Arrayindex < ArrayCount; Arrayindex++)
                {
                    PropertyParser(ref PropertyName, ArrayType, -1, ArrayData, uexp, Modify);
                }
            }

            return PropertyName;
        }
    }

}
