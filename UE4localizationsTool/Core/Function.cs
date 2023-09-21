using Helper.MemoryList;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetParser
{
    public class Function
    {
        MemoryList FuncBlock;
        Uexp uexp;
        bool Modify;
        int ScriptBytecodeSize;
        int scriptStorageSize;
        List<int> offsetList;
        /// <summary>
        /// [old] [new] [size] 
        /// </summary>
        List<List<int>> stringOffset;
        int NewSize;
        public Function(MemoryList memoryList, Uexp Uexp, bool modify = false)
        {
            uexp = Uexp;
            Modify = modify;
            offsetList = new List<int>();
            stringOffset = new List<List<int>>();
            NewSize = 0;
            if (uexp.UassetData.EngineVersion < UEVersions.VER_UE4_16)
            {
                return;
            }

            memoryList.Skip(4 * 2);

            int numIndexEntries = memoryList.GetIntValue();

            // if (numIndexEntries!=0) return;
            List<int> IndexEntries = new List<int>();
            for (int i = 0; i < numIndexEntries; i++)
            {
                int ExportIndex  = memoryList.GetIntValue();
                IndexEntries.Add(ExportIndex);
            }

            //TODO :(
            if (uexp.UassetData.Exports_Directory[Uexp.ExportIndex].Value >= 4 || uexp.UassetData.EngineVersion >= UEVersions.VER_UE4_ADDED_PACKAGE_OWNER)
            {
                if (uexp.UassetData.AutoVersion)
                {
                    uexp.UassetData.EngineVersion = UEVersions.VER_UE4_26;
                }

                int num = memoryList.GetIntValue();

                for (int i = 0; i < num; i++)
                {
                    ReadFProperty(memoryList);
                }

            }
            int StartPosition = memoryList.GetPosition();
            ScriptBytecodeSize = memoryList.GetIntValue();
            scriptStorageSize = memoryList.GetIntValue();
            ConsoleMode.Print("-\n" + ScriptBytecodeSize + "\n" + scriptStorageSize + "\n-", ConsoleColor.Gray);

            FuncBlock = new MemoryList(memoryList.GetBytes(scriptStorageSize));
            int index = 0;
            while (FuncBlock.GetPosition() < FuncBlock.GetSize())
            {
                ConsoleMode.Print("Start: "+(index++), ConsoleColor.Yellow);
                ExprToken ss = ReadExpression();
                ConsoleMode.Print(Convert.ToString(ss), ConsoleColor.Yellow);
            }


            for (int i = 0; i < offsetList.Count; i++)
            {
                ConsoleMode.Print("offset: " + offsetList[i] + " - " + FuncBlock.GetIntValue(false, offsetList[i]) + " - " + (FuncBlock.GetIntValue(false, offsetList[i]) - (ScriptBytecodeSize - scriptStorageSize)), ConsoleColor.Green);
            }

            for (int i = 0; i < stringOffset.Count; i++)
            {
                ConsoleMode.Print("stringOffset: " + stringOffset[i][0] + " , " + stringOffset[i][1] + " , " + stringOffset[i][2], ConsoleColor.Green);

            }



            if (Modify)
            {
                int[] list = new int[offsetList.Count];

                for (int n = 0; n < offsetList.Count; n++)
                {
                    list[n] = FuncBlock.GetIntValue(false, offsetList[n]);
                }
                int ExtraSize = 0;

                ConsoleMode.Print("stringOffset: " + stringOffset.Count, ConsoleColor.DarkYellow);
                ConsoleMode.Print("list: " + list.Length, ConsoleColor.DarkYellow);

                for (int x = 0; x < list.Length; x++)
                {
                    for (int i = 0; i < stringOffset.Count; i++)
                    {
                        //   [old] [new] [size] 


                        if (list[x] > stringOffset[i][0])
                        {

                            ConsoleMode.Print("OldVal: " + list[x], ConsoleColor.DarkYellow);
                            ExtraSize = stringOffset[i][2];
                            list[x] += ExtraSize;

                            FuncBlock.SetIntValue(list[x], false, offsetList[x]);
                            ConsoleMode.Print("NewVal: " + list[x], ConsoleColor.DarkYellow);
                            ConsoleMode.Print("TotalSize: " + ExtraSize, ConsoleColor.DarkYellow);
                            ConsoleMode.Print("- -  -  -  -  - - - ", ConsoleColor.Green);
                        }

                    }
                    ConsoleMode.Print("-------------------------", ConsoleColor.Red);
                }

                int NewSize = FuncBlock.GetSize() - scriptStorageSize;

                memoryList.Seek(StartPosition);

                memoryList.SetIntValue(memoryList.GetIntValue(false) + NewSize);
                memoryList.SetIntValue(memoryList.GetIntValue(false) + NewSize);

                memoryList.ReplaceBytes(scriptStorageSize, FuncBlock.ToArray());

            }




        }




        void ReadFProperty(MemoryList memoryList)
        {
            string Property = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
            memoryList.Skip(4);
            string NameProperty = uexp.UassetData.GetPropertyName(memoryList.GetIntValue());
            memoryList.Skip(4);
            int Flags = memoryList.GetIntValue();
            int ArrayDim = memoryList.GetIntValue();
            int ElementSize = memoryList.GetIntValue();
            long PropertyFlags = memoryList.GetInt64Value();
            ushort RepIndex = memoryList.GetUShortValue();
            long RepNotifyFunc = memoryList.GetInt64Value();
            byte BlueprintReplicationCondition = memoryList.GetByteValue();


            //ConsoleMode.Print(NameProperty + "=" + Property, ConsoleColor.Yellow);

            if (Property == "BoolProperty")
            {
                memoryList.Skip(6);
            }
            else if (Property == "ObjectProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "DelegateProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "StructProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "EnumProperty")
            {
                memoryList.Skip(4);
                ReadFProperty(memoryList);
            }
            else if (Property == "ByteProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "ArrayProperty")
            {
                ReadFProperty(memoryList);
            }
            else if (Property == "SetProperty")
            {
                ReadFProperty(memoryList);
            }
            else if (Property == "ClassProperty")
            {
                memoryList.Skip(8); //[+4]
            }
            else if (Property == "SoftClassProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "InterfaceProperty")
            {
                memoryList.Skip(4);
            }
            else if (Property == "MapProperty")
            {
                ReadFProperty(memoryList);
                ReadFProperty(memoryList);
            }
            else if (Property == "ByteProperty")
            {
                memoryList.Skip(4);
            }
        }





        void ReadPPOINTER()
        {
            if (uexp.UassetData.EngineVersion >= UEVersions.VER_UE4_25)
            {
                int num = FuncBlock.GetIntValue();
                for (int i = 0; i < num; i++)
                {
                    FuncBlock.Skip(4);//name
                    FuncBlock.Skip(4);
                }
                // offsetList.Add(FuncBlock.GetPosition());
                FuncBlock.Skip(4);
            }
            else
            {
                // offsetList.Add(FuncBlock.GetPosition());
                FuncBlock.Skip(4);
            }
        }



        void ReadExpressionArray(ExprToken eExpr)
        {

            while (true)
            {
                if (ReadExpression() == eExpr)
                {
                    break;
                }
            }

        }


        ExprToken ReadExpression()
        {
            ExprToken token = (ExprToken)FuncBlock.GetByteValue();
            ConsoleMode.Print(Convert.ToString(token), ConsoleColor.Blue);
            int offset;
            string NameProperty;
            switch (token)
            {

                case ExprToken.EX_LocalVariable:
                    ReadPPOINTER();
                    break;
                case ExprToken.EX_InstanceVariable:
                    ReadPPOINTER();
                    break;
                case ExprToken.EX_DefaultVariable:
                    ReadPPOINTER();
                    break;
                case ExprToken.EX_Return:
                    ReadExpression();
                    break;
                case ExprToken.EX_Jump:
                    offsetList.Add(FuncBlock.GetPosition());
                    offset = FuncBlock.GetIntValue();// Code offset.

                    break;
                case ExprToken.EX_JumpIfNot:
                    offsetList.Add(FuncBlock.GetPosition());
                    offset = FuncBlock.GetIntValue(); //CodeOffset
                    ReadExpression();// Boolean expr.
                    break;
                case ExprToken.EX_Assert:
                    FuncBlock.Skip(3);
                    ReadExpression();
                    break;
                case ExprToken.EX_Nothing:
                    break;

                case ExprToken.EX_Let:
                    ReadPPOINTER();
                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_ClassContext:
                    ReadExpression();
                    offsetList.Add(FuncBlock.GetPosition());
                    offset = FuncBlock.GetIntValue();
                    ReadPPOINTER();
                    ReadExpression();
                    break;

                case ExprToken.EX_MetaCast:
                    FuncBlock.Skip(4);
                    ReadExpression();
                    break;


                case ExprToken.EX_LetBool:
                    ReadExpression();
                    ReadExpression();
                    break;


                case ExprToken.EX_EndParmValue:
                    break;

                case ExprToken.EX_EndFunctionParms:
                    break;

                case ExprToken.EX_Self:
                    break;

                case ExprToken.EX_Skip:
                    offsetList.Add(FuncBlock.GetPosition());
                    offset = FuncBlock.GetIntValue(); //CodeOffset
                    ReadExpression();
                    break;

                case ExprToken.EX_Context:
                    ReadExpression();
                    // offsetList.Add(FuncBlock.GetPosition());
                    offset = FuncBlock.GetIntValue();
                    ReadPPOINTER();
                    ReadExpression();
                    break;

                case ExprToken.EX_Context_FailSilent:
                    ReadExpression(); // Object expression.
                                      // offsetList.Add(FuncBlock.GetPosition());
                    offset = FuncBlock.GetIntValue();// Code offset for NULL expressions.
                    ReadPPOINTER();// Property corresponding to the r-value data, in case the l-value needs to be mem-zero'd
                    ReadExpression();// Context expression.
                    break;

                case ExprToken.EX_VirtualFunction:
                    NameProperty = uexp.UassetData.GetPropertyName(FuncBlock.GetIntValue());
                    FuncBlock.Skip(4);
                    ReadExpressionArray(ExprToken.EX_EndFunctionParms);
                    break;

                case ExprToken.EX_FinalFunction:
                    FuncBlock.Skip(4); //pointer
                    ReadExpressionArray(ExprToken.EX_EndFunctionParms);
                    break;

                case ExprToken.EX_IntConst:
                    FuncBlock.Skip(4); //value
                    break;

                case ExprToken.EX_FloatConst:
                    FuncBlock.Skip(4); //value
                    break;

                case ExprToken.EX_StringConst:
                    int thisoffset = FuncBlock.GetPosition();
                    if (!Modify)
                    {
                        string StringConst = FuncBlock.GetStringUE(Encoding.ASCII);
                        ConsoleMode.Print(StringConst, ConsoleColor.Blue);
                        uexp.Strings.Add(new List<string>() { "FuncText" + uexp.ExportIndex, StringConst, "Changing this value will cause the game crash!", "#141412", "#FFFFFF" });
                    }
                    else
                    {
                        FuncBlock.ReplaceStringUE_Func(uexp.Strings[uexp.CurrentIndex][1]);
                        uexp.CurrentIndex++;
                    }
                    stringOffset.Add(new List<int>() { thisoffset + NewSize, thisoffset, (FuncBlock.GetSize() - scriptStorageSize) - NewSize });
                    NewSize = FuncBlock.GetSize() - scriptStorageSize;
                    break;

                case ExprToken.EX_ObjectConst:
                    FuncBlock.Skip(4);
                    break;

                case ExprToken.EX_NameConst:
                    NameProperty = uexp.UassetData.GetPropertyName(FuncBlock.GetIntValue());
                    FuncBlock.Skip(4);
                    break;

                case ExprToken.EX_RotationConst:
                    FuncBlock.Skip(4 * 3);
                    break;

                case ExprToken.EX_VectorConst:
                    FuncBlock.Skip(4 * 3); ;
                    break;

                case ExprToken.EX_ByteConst:
                    FuncBlock.Skip(1);
                    break;

                case ExprToken.EX_IntZero:
                    break;

                case ExprToken.EX_IntOne:
                    break;

                case ExprToken.EX_True:
                    break;

                case ExprToken.EX_False:
                    break;

                case ExprToken.EX_TextConst:
                    byte TextLiteralType = FuncBlock.GetByteValue();
                    switch (TextLiteralType)
                    {
                        case 0: //Empty
                            break;
                        case 1: //LocalizedText
                            ReadExpression();
                            ReadExpression();
                            ReadExpression();

                            if (uexp.DumpNameSpaces)
                            {
                                uexp.StringNodes.Add(new StringNode()
                                {
                                    NameSpace = uexp.Strings[uexp.Strings.Count - 3][1],
                                    Key = uexp.Strings[uexp.Strings.Count - 2][1],
                                    Value = uexp.Strings[uexp.Strings.Count - 1][1]
                                });
                            }
                            break;
                        case 2: // InvariantText IsCultureInvariant
                            ReadExpression();
                            break;
                        case 3: //LiteralString
                            ReadExpression();
                            break;
                        case 4: //StringTableEntry
                            FuncBlock.Skip(4);
                            ReadExpression();
                            ReadExpression();

                            if (uexp.DumpNameSpaces)
                            {
                                uexp.StringNodes.Add(new StringNode()
                                {
                            //        NameSpace = uexp.Strings[uexp.Strings.Count - 3][1],
                                    Key = uexp.Strings[uexp.Strings.Count - 2][1],
                                    Value = uexp.Strings[uexp.Strings.Count - 1][1]
                                });
                            }
                            break;
                    }
                    break;

                case ExprToken.EX_NoObject:
                    break;

                case ExprToken.EX_TransformConst:

                    FuncBlock.Skip(4 * 4);
                    FuncBlock.Skip(4 * 3);
                    FuncBlock.Skip(4 * 3);
                    break;

                case ExprToken.EX_IntConstByte:

                    FuncBlock.Skip(1);
                    break;

                case ExprToken.EX_NoInterface:
                    break;

                case ExprToken.EX_DynamicCast:

                    FuncBlock.Skip(4);
                    ReadExpression();
                    break;

                case ExprToken.EX_StructConst:
                    FuncBlock.Skip(4);
                    int StructSize = FuncBlock.GetIntValue();
                    ReadExpressionArray(ExprToken.EX_EndStructConst);
                    break;

                case ExprToken.EX_EndStructConst:
                    break;

                case ExprToken.EX_SetArray:

                    if (uexp.UassetData.EngineVersion >= UEVersions.VER_UE4_CHANGE_SETARRAY_BYTECODE)
                    {
                        ReadExpression();
                    }
                    else
                    {
                        FuncBlock.Skip(4);
                    };

                    ReadExpressionArray(ExprToken.EX_EndArray);
                    break;

                case ExprToken.EX_EndArray:
                    break;

                case ExprToken.EX_PropertyConst:

                    ReadPPOINTER();
                    break;

                case ExprToken.EX_UnicodeStringConst:
                    int ThisOffset = FuncBlock.GetPosition();
                    if (!Modify)
                    {
                        string UnicodeStringConst = FuncBlock.GetStringUE(Encoding.Unicode);
                        ConsoleMode.Print(UnicodeStringConst, ConsoleColor.Blue);
                        uexp.Strings.Add(new List<string>() { "FuncText" + uexp.ExportIndex, UnicodeStringConst, "Changing this value will cause the game crash!", "#141412", "#FFFFFF" });
                        // Console.WriteLine(UnicodeStringConst.Length);
                        //  Console.ReadLine();
                    }
                    else
                    {
                        FuncBlock.ReplaceStringUE_Func(uexp.Strings[uexp.CurrentIndex][1]);
                        uexp.CurrentIndex++;
                    }
                    stringOffset.Add(new List<int>() { ThisOffset + NewSize, ThisOffset, (FuncBlock.GetSize() - scriptStorageSize) - NewSize });
                    NewSize = FuncBlock.GetSize() - scriptStorageSize;
                    break;

                case ExprToken.EX_Int64Const:

                    FuncBlock.Skip(8); //value
                    break;

                case ExprToken.EX_UInt64Const:

                    FuncBlock.Skip(8); //value;
                    break;

                case ExprToken.EX_PrimitiveCast:
                    FuncBlock.Skip(1);//ConversionType
                    ReadExpression();
                    break;

                case ExprToken.EX_SetSet:

                    ReadExpression();
                    FuncBlock.Skip(4);//Entrie num
                    ReadExpressionArray(ExprToken.EX_EndSet);
                    break;

                case ExprToken.EX_EndSet:
                    break;

                case ExprToken.EX_SetMap:
                    ReadExpression();
                    FuncBlock.Skip(4);//Entrie num
                    ReadExpressionArray(ExprToken.EX_EndMap);
                    break;

                case ExprToken.EX_EndMap:
                    break;

                case ExprToken.EX_SetConst:

                    ReadPPOINTER();
                    FuncBlock.Skip(4);//Entrie num
                    ReadExpressionArray(ExprToken.EX_EndSetConst);
                    break;

                case ExprToken.EX_EndSetConst:
                    break;

                case ExprToken.EX_MapConst:

                    ReadPPOINTER();
                    ReadPPOINTER();
                    FuncBlock.Skip(4);//Entrie num
                    ReadExpressionArray(ExprToken.EX_EndMapConst);
                    break;

                case ExprToken.EX_EndMapConst:
                    break;


                case ExprToken.EX_StructMemberContext:

                    ReadPPOINTER();
                    ReadExpression();
                    break;

                case ExprToken.EX_LetMulticastDelegate:

                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_LetDelegate:

                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_LocalVirtualFunction:

                    NameProperty = uexp.UassetData.GetPropertyName(FuncBlock.GetIntValue());
                    FuncBlock.Skip(4);
                    ReadExpressionArray(ExprToken.EX_EndFunctionParms);
                    break;

                case ExprToken.EX_LocalFinalFunction:

                    FuncBlock.Skip(4); //pointer
                    ReadExpressionArray(ExprToken.EX_EndFunctionParms);
                    break;

                case ExprToken.EX_LocalOutVariable:

                    ReadPPOINTER();
                    break;

                case ExprToken.EX_DeprecatedOp4A:
                    break;

                case ExprToken.EX_InstanceDelegate:

                    NameProperty = uexp.UassetData.GetPropertyName(FuncBlock.GetIntValue());
                    FuncBlock.Skip(4);
                    break;

                case ExprToken.EX_PushExecutionFlow:
                    offsetList.Add(FuncBlock.GetPosition());
                    uint PushingAddress = FuncBlock.GetUIntValue();
                    break;


                case ExprToken.EX_PopExecutionFlow:
                    break;

                case ExprToken.EX_ComputedJump:

                    ReadExpression();// Integer expression, specifying code offset.
                    break;

                case ExprToken.EX_PopExecutionFlowIfNot:

                    ReadExpression();
                    break;

                case ExprToken.EX_Breakpoint:
                    break;

                case ExprToken.EX_InterfaceContext:

                    ReadExpression();
                    break;

                case ExprToken.EX_ObjToInterfaceCast:

                    FuncBlock.Skip(4);
                    ReadExpression();
                    break;

                case ExprToken.EX_EndOfScript:
                    break;

                case ExprToken.EX_CrossInterfaceCast:

                    FuncBlock.Skip(4);
                    ReadExpression();
                    break;

                case ExprToken.EX_InterfaceToObjCast:

                    FuncBlock.Skip(4);
                    ReadExpression();
                    break;

                case ExprToken.EX_WireTracepoint:
                    break;

                case ExprToken.EX_SkipOffsetConst:
                    offsetList.Add(FuncBlock.GetPosition());
                    FuncBlock.Skip(4); //value
                    break;

                case ExprToken.EX_AddMulticastDelegate:

                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_ClearMulticastDelegate:

                    ReadExpression();
                    break;

                case ExprToken.EX_Tracepoint:
                    break;

                case ExprToken.EX_LetObj:

                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_LetWeakObjPtr:

                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_BindDelegate:

                    NameProperty = uexp.UassetData.GetPropertyName(FuncBlock.GetIntValue());
                    FuncBlock.Skip(4);
                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_RemoveMulticastDelegate:

                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_CallMulticastDelegate:

                    FuncBlock.Skip(4); //pointer
                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_LetValueOnPersistentFrame:

                    ReadPPOINTER();
                    ReadExpression();
                    break;

                case ExprToken.EX_ArrayConst:

                    ReadPPOINTER();
                    FuncBlock.Skip(4);//Entrie num
                    ReadExpression();
                    break;

                case ExprToken.EX_EndArrayConst:
                    break;

                case ExprToken.EX_SoftObjectConst:

                    ReadExpression();
                    break;

                case ExprToken.EX_CallMath:

                    FuncBlock.Skip(4); //pointer
                    ReadExpressionArray(ExprToken.EX_EndFunctionParms);
                    break;

                case ExprToken.EX_SwitchValue:


                    ushort numCases = FuncBlock.GetUShortValue();
                    offsetList.Add(FuncBlock.GetPosition());
                    int EndGotoOffset = FuncBlock.GetIntValue();
                    ReadExpression();
                    for (int i = 0; i < numCases; i++)
                    {
                        ReadExpression();   // case index value term
                        offsetList.Add(FuncBlock.GetPosition());
                        FuncBlock.Skip(4);  // offset to the next case
                        ReadExpression();   // case term
                    }
                    ReadExpression();       //default term
                    break;


                case ExprToken.EX_InstrumentationEvent:

                    byte EventType = FuncBlock.GetByteValue();

                    if (EventType == 4)
                    {
                        NameProperty = uexp.UassetData.GetPropertyName(FuncBlock.GetIntValue());
                        FuncBlock.Skip(4);
                    }

                    break;

                case ExprToken.EX_ArrayGetByRef:

                    ReadExpression();
                    ReadExpression();
                    break;

                case ExprToken.EX_ClassSparseDataVariable:

                    ReadPPOINTER();
                    break;

                case ExprToken.EX_FieldPathConst:

                    ReadExpression();
                    break;
                default:
                    throw new Exception("Invalid token type: " + token.ToString("X2"));

            }
            return token;
        }


    }



}

