using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Helper.MemoryList
{
    public enum Endian
    {
        Little,
        Big
    }


    public class MemoryList : IDisposable
    {

        public List<byte> MemoryListData;
        //private bool CutFromMemoryListData = false;
        //private int CutSize;
        //private int CutOffset;


        public int MemoryListSize
        {
            get
            {
                return MemoryListData.Count;
            }
            set
            {
                if (MemoryListSize > value)
                {
                    MemoryListData.RemoveRange(value, MemoryListData.Count - value);
                }

                if (MemoryListSize < value)
                {
                    MemoryListData.AddRange(Enumerable.Repeat((byte)0, value - MemoryListSize));
                }

            }
        }


        private int MemoryListIndex;
        public int MemoryListPosition
        {
            get
            {
                if (this.MemoryListIndex - 1 > MemoryListData.Count - 1)
                {

                    throw new IndexOutOfRangeException("out of range");
                }


                return this.MemoryListIndex;
            }
            set
            {
                if (MemoryListSize < value)
                {
                    throw new IndexOutOfRangeException($"This value \'{value}\' is out of range");
                }
                if (value < 0 && MemoryListSize != 0)
                {
                    throw new IndexOutOfRangeException($"This value \'{value}\' is non number and out of range");
                }
                if (value < 0 && MemoryListSize == 0)
                {
                    this.MemoryListIndex = 0;

                    return;
                }

                this.MemoryListIndex = value;

            }
        }


        public MemoryList()
        {
            MemoryListData = new List<byte>();
        }


        public MemoryList(string FilePath)
        {
            if (new FileInfo(FilePath).Length > int.MaxValue)
            {
                throw new Exception("Can't read this file: " + FilePath);
            }
            MemoryListData = File.ReadAllBytes(FilePath).ToList();

        }


        public MemoryList(List<byte> FileData)
        {
            MemoryListData = FileData;
        }


        public MemoryList(byte[] FileData)
        {
            MemoryListData = FileData.ToList();
        }

        //no plan for now
        private MemoryList(MemoryList FileData, int offset, int Length)
        {
            //CutFromMemoryListData = true;
            //CutOffset = offset;
            //CutSize = Length;
            //MemoryListData = FileData.MemoryListData;
        }

        #region MemoryList Controls
        public void Seek(int Value, SeekOrigin SeekOrigin = SeekOrigin.Begin)
        {
            if (SeekOrigin == SeekOrigin.Begin)
            {
                MemoryListPosition = Value;
            }
            if (SeekOrigin == SeekOrigin.Current)
            {
                MemoryListPosition += Value;
            }
            if (SeekOrigin == SeekOrigin.End)
            {
                MemoryListPosition = MemoryListSize - Value;
            }
        }

        public void Skip(int Value)
        {
            MemoryListPosition += Value;
        }

        public int GetPosition()
        {
            return MemoryListPosition;
        }

        public int GetSize()
        {
            return MemoryListSize;
        }

        public void SetPosition(int Value)
        {
            MemoryListPosition = Value;
        }

        public void SetSize(int Value)
        {
            if (MemoryListPosition> Value)
            {
                MemoryListPosition = Value;
            }


            MemoryListSize = Value;


        }

        public void WriteFile(string FilePath)
        {
            File.WriteAllBytes(FilePath, MemoryListData.ToArray());
        }

        public byte[] ToArray()
        {
            return MemoryListData.ToArray();
        }

        public List<byte> ToList()
        {
            return MemoryListData;
        }

        public bool EndofFile()
        {
            return MemoryListPosition == MemoryListSize;
        }

        public bool isEmpty()
        {
            return MemoryListSize == 0;
        }

        public void Clear()
        {

            MemoryListData.Clear();
            MemoryListPosition = 0;
        }

        public void Add(object Value)
        {
            if (Value is byte)
            {
                MemoryListData.Add((byte)Value);
            }
            else if (Value is int)
            {
                MemoryListData.AddRange(BitConverter.GetBytes((int)Value));
            }
            else if (Value is uint)
            {
                MemoryListData.AddRange(BitConverter.GetBytes((uint)Value));
            }
            else if (Value is short)
            {
                MemoryListData.AddRange(BitConverter.GetBytes((short)Value));
            }
            else if (Value is ushort)
            {
                MemoryListData.AddRange(BitConverter.GetBytes((ushort)Value));
            }
            else if (Value is long)
            {
                MemoryListData.AddRange(BitConverter.GetBytes((long)Value));
            }
            else if (Value is ulong)
            {
                MemoryListData.AddRange(BitConverter.GetBytes((ulong)Value));
            }
            else if (Value is float)
            {
                MemoryListData.AddRange(BitConverter.GetBytes((float)Value));
            }
            else if (Value is double)
            {
                MemoryListData.AddRange(BitConverter.GetBytes((double)Value));
            }
            else if (Value is string)
            {
                MemoryListData.AddRange(Encoding.ASCII.GetBytes((string)Value));
            }
            else if (Value is bool)
            {
                MemoryListData.Add(Convert.ToByte(Value));
            }
            else if (Value.GetType().IsArray)
            {
                MemoryListData.AddRange((byte[])Value);
            }
            else
            {
                throw new Exception("Unknown type");
            }
        }

        public void Append(object Value)
        {
            Add(Value);
        }

        public void Write(object Value, bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            if (Value is byte)
            {
                SetByteValue((byte)Value, SavePosition, SeekAndRead);
            }
            else if (Value is int)
            {
                SetIntValue((int)Value, SavePosition, SeekAndRead);
            }
            else if (Value is uint)
            {
                SetUIntValue((uint)Value, SavePosition, SeekAndRead);
            }
            else if (Value is short)
            {
                SetShortValue((short)Value, SavePosition, SeekAndRead);
            }
            else if (Value is ushort)
            {
                SetUShortValue((ushort)Value, SavePosition, SeekAndRead);
            }
            else if (Value is long)
            {
                SetInt64Value((long)Value, SavePosition, SeekAndRead);
            }
            else if (Value is ulong)
            {
                SetUInt64Value((ulong)Value, SavePosition, SeekAndRead);
            }
            else if (Value is float)
            {
                SetFloatValue((float)Value, SavePosition, SeekAndRead);
            }
            else if (Value is double)
            {
                SetDoubleValue((double)Value, SavePosition, SeekAndRead);
            }
            else if (Value is string)
            {
                SetStringValue((string)Value, SavePosition, SeekAndRead, encoding);
            }
            else if (Value is bool)
            {
                SetBoolValue((bool)Value, SavePosition, SeekAndRead);
            }
            else if (Value.GetType().IsArray)
            {
                SetBytes((byte[])Value, SavePosition, SeekAndRead);
            }
            else
            {
                throw new Exception("Unknown type");
            }
        }

  
        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Boolean
        public bool GetBoolValue(bool SavePosition = true, int SeekAndRead = -1)
        {
            return Convert.ToBoolean(GetByteValue(SavePosition, SeekAndRead));
        }
        public void SetBoolValue(bool Value, bool SavePosition = true, int SeekAndWrite = -1)
        {
            SetByteValue(Convert.ToByte(Value), SavePosition, SeekAndWrite);
        }

        public void InsertBoolValue(bool Value, bool SavePosition = true, int SeekAndWrite = -1)
        {
            InsertByteValue(Convert.ToByte(Value), SavePosition, SeekAndWrite);
        }

        public void DeleteBoolValue(int SeekAndWrite = -1)
        {
            DeleteByteValue(SeekAndWrite);
        }

        #endregion


        #region int8,uint8
        public byte GetByteValue(bool SavePosition = true, int SeekAndRead = -1)
        {

            if (SeekAndRead != -1)
            {
                return MemoryListData[SeekAndRead];
            }

            if (SavePosition)
            {
                byte value = MemoryListData[MemoryListPosition];
                MemoryListPosition++;
                return value;
            }

            return MemoryListData[MemoryListPosition];

        }
        public sbyte GetUByteValue(bool SavePosition = true, int SeekAndRead = -1)
        {
            return (sbyte)GetByteValue(SavePosition, SeekAndRead);

        }


        public void SetByteValue(byte value, bool SavePosition = true, int SeekAndRead = -1)
        {
            if (SeekAndRead != -1)
            {
                if (SeekAndRead == MemoryListSize)
                {
                    MemoryListData.Add(value);
                    return;
                }

                MemoryListData[SeekAndRead] = value;
                return;
            }

            if (SavePosition)
            {
                if (MemoryListPosition == MemoryListSize)
                {
                    MemoryListData.Add(value);
                    MemoryListPosition++;
                    return;
                }

                MemoryListData[MemoryListPosition] = value;
                MemoryListPosition++;
                return;
            }

            if (MemoryListPosition == MemoryListSize)
            {
                MemoryListData.Add(value);
                return;
            }
            MemoryListData[MemoryListPosition] = value;
        }

        public void SetUByteValue(sbyte value, bool SavePosition = true, int SeekAndRead = -1)
        {
            SetByteValue((byte)value, SavePosition, SeekAndRead);
        }

        public void InsertByteValue(byte value, bool SavePosition = true, int SeekAndRead = -1)
        {
            if (SeekAndRead != -1)
            {
                MemoryListData.Insert(SeekAndRead, value);
                return;
            }


            if (SavePosition)
            {
                MemoryListData.Insert(MemoryListPosition, value);
                MemoryListPosition++;
                return;
            }

            MemoryListData.Insert(MemoryListPosition, value);
        }
        public void InsertUByteValue(sbyte value, bool SavePosition = true, int SeekAndRead = -1)
        {
            InsertByteValue((byte)value, SavePosition, SeekAndRead);
        }




        public void DeleteByteValue(int SeekAndRead = -1)
        {
            if (MemoryListSize < 1)
            {
                throw new Exception("Not enought data");
            }

            if (SeekAndRead != -1)
            {
                if (SeekAndRead > MemoryListSize)
                {
                    throw new Exception("Out of range");
                }

                MemoryListData.RemoveAt(SeekAndRead);
                return;
            }
            if (MemoryListPosition > MemoryListSize - 1)
            {
                throw new Exception("Out of range");
            }
            MemoryListData.RemoveAt(MemoryListPosition);
        }

        public void DeleteUByteValue(int SeekAndRead = -1)
        {
            DeleteByteValue(SeekAndRead);

        }


        public byte[] GetBytes(int Count, bool SavePosition = true, int SeekAndRead = -1)
        {

            if (MemoryListSize < Count)
            {
                throw new Exception("Not enought data");
            }

            byte[] array;
            if (SeekAndRead != -1)
            {
                array = MemoryListData.GetRange(SeekAndRead, Count).ToArray();
                return array;
            }


            if (SavePosition)
            {
                array = MemoryListData.GetRange(MemoryListIndex, Count).ToArray();
                MemoryListIndex += Count;
                return array;



            }
            array = MemoryListData.GetRange(MemoryListIndex, Count).ToArray();
            return array;
        }

        public void SetBytes(byte[] BytesArray, bool SavePosition = true, int SeekAndRead = -1)
        {

            if (SeekAndRead != -1)
            {
                for (int i = 0; i < BytesArray.Length; i++)
                {
                    SetByteValue(BytesArray[i], false, SeekAndRead + i);
                }
                return;
            }


            if (SavePosition)
            {
                for (int i = 0; i < BytesArray.Length; i++)
                {
                    SetByteValue(BytesArray[i]);
                }
                return;
            }
            for (int i = 0; i < BytesArray.Length; i++)
            {
                SetByteValue(BytesArray[i], false, MemoryListIndex + i);
            }
        }

        public void InsertBytes(byte[] BytesArray, bool SavePosition = true, int SeekAndRead = -1)
        {
            if (SeekAndRead != -1)
            {
                MemoryListData.InsertRange(SeekAndRead, BytesArray);
                return;
            }

            if (SavePosition)
            {
                MemoryListData.InsertRange(MemoryListPosition, BytesArray);
                MemoryListPosition += BytesArray.Length;
                return;
            }
            MemoryListData.InsertRange(MemoryListPosition, BytesArray);
        }
        public void DeleteBytes(int count, int SeekAndRead = -1)
        {

            if (MemoryListSize < count)
            {
                throw new Exception("Not enought data");
            }
            if (SeekAndRead != -1)
            {
                if (SeekAndRead > MemoryListSize)
                {
                    throw new Exception("Out of range");
                }
                MemoryListData.RemoveRange(SeekAndRead, count);
                return;
            }
            if (MemoryListPosition > MemoryListSize - 1)
            {
                throw new Exception("Out of range");
            }
            MemoryListData.RemoveRange(MemoryListPosition, count);
        }




        public void ReplaceBytes(int OldBytesLenght, byte[] NewBytes, bool SavePosition = true, int SeekAndRead = -1)
        {

            if (OldBytesLenght== NewBytes.Length)
            {
                SetBytes(NewBytes, SavePosition, SeekAndRead);              
                return;
            }

            if (SeekAndRead != -1)
            {
                MemoryListData.RemoveRange(SeekAndRead, OldBytesLenght);
                MemoryListData.InsertRange(SeekAndRead, NewBytes);
                return;
            }


            if (SavePosition)
            {
                MemoryListData.RemoveRange(MemoryListPosition, OldBytesLenght);
                MemoryListData.InsertRange(MemoryListPosition, NewBytes);
                MemoryListPosition += NewBytes.Length;
                return;
            }

            MemoryListData.RemoveRange(MemoryListPosition, OldBytesLenght);
            MemoryListData.InsertRange(MemoryListPosition, NewBytes);
        }

        #endregion


        #region int16,uint16
        public short GetShortValue(bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            byte[] array = GetBytes(2, SavePosition, SeekAndRead);
            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            return (short)(array[0] | (array[1] << 8));
        }

        public ushort GetUShortValue(bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            return (ushort)(GetShortValue(SavePosition, SeekAndRead, _Endian));
        }


        public void SetShortValue(short value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = new byte[2] { (byte)(value), (byte)(value >> 8) };

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            SetBytes(array, SavePosition, SeekAndRead);
        }

        public void SetUShortValue(ushort value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            SetShortValue((short)value, SavePosition, SeekAndRead, _Endian);
        }

        public void InsertShortValue(short value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = new byte[2] { (byte)(value), (byte)(value >> 8) };

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            InsertBytes(array, SavePosition, SeekAndRead);

        }
        public void InsertUShortValue(ushort value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            InsertShortValue((short)value, SavePosition, SeekAndRead, _Endian);
        }

        public void DeleteShortValue(int SeekAndRead = -1)
        {
            if (MemoryListSize < 2)
            {
                throw new Exception("Not enought data");
            }
            DeleteBytes(2, SeekAndRead);
        }

        public void DeleteUShortValue(int SeekAndRead = -1)
        {
            DeleteShortValue(SeekAndRead);
        }


        public short[] GetShorts(int Count, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            if (MemoryListSize < Count * 2)
            {
                throw new Exception("Not enought data");
            }
            if (Count == 0)
            {
                throw new Exception("Can't get 0 short values");
            }
            short[] array = new short[Count];
            if (SeekAndRead != -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetShortValue(SavePosition, SeekAndRead + i * 2, _Endian);
                }
                return array;
            }


            if (SavePosition)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetShortValue(true, -1, _Endian);
                }
                return array;



            }
            for (int i = 0; i < Count; i++)
            {
                array[i] = GetShortValue(SavePosition, MemoryListPosition + i * 2, _Endian);
            }
            return array;
        }
        public ushort[] GetUShorts(int Count, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            if (MemoryListSize < Count * 2)
            {
                throw new Exception("Not enought data");
            }
            if (Count == 0)
            {
                throw new Exception("Can't get 0 ushort values");
            }
            ushort[] array = new ushort[Count];
            if (SeekAndRead != -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetUShortValue(SavePosition, SeekAndRead + i * 2, _Endian);
                }
                return array;
            }


            if (SavePosition)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetUShortValue(true, -1, _Endian);
                }
                return array;



            }
            for (int i = 0; i < Count; i++)
            {
                array[i] = GetUShortValue(SavePosition, MemoryListPosition + i * 2, _Endian);
            }
            return array;
        }



        #endregion


        #region int32,uint32
        public int GetIntValue(bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            byte[] array = GetBytes(4, SavePosition, SeekAndRead);
            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            return array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
        }

        public uint GetUIntValue(bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            return (uint)(GetIntValue(SavePosition, SeekAndRead, _Endian));
        }


        public void SetIntValue(int value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = new byte[4] { (byte)(value), (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) };

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            SetBytes(array, SavePosition, SeekAndRead);
        }

        public void SetUIntValue(uint value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            SetIntValue((int)value, SavePosition, SeekAndRead, _Endian);
        }

        public void InsertIntValue(int value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = new byte[4] { (byte)(value), (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) };

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            InsertBytes(array, SavePosition, SeekAndRead);

        }
        public void InsertUIntValue(uint value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            InsertIntValue((int)value, SavePosition, SeekAndRead, _Endian);
        }

        public void DeleteIntValue(int SeekAndRead = -1)
        {
            if (MemoryListSize < 4)
            {
                throw new Exception("Not enought data");
            }
            DeleteBytes(4, SeekAndRead);
        }

        public void DeleteUIntValue(int SeekAndRead = -1)
        {
            DeleteIntValue(SeekAndRead);
        }


        public int[] GetInts(int Count, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            if (MemoryListSize < Count * 4)
            {
                throw new Exception("Not enought data");
            }
            if (Count == 0)
            {
                throw new Exception("Can't get 0 int values");
            }
            int[] array = new int[Count];
            if (SeekAndRead != -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetIntValue(SavePosition, SeekAndRead + i * 4, _Endian);
                }
                return array;
            }


            if (SavePosition)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetIntValue(true, -1, _Endian);
                }
                return array;



            }
            for (int i = 0; i < Count; i++)
            {
                array[i] = GetIntValue(SavePosition, MemoryListPosition + i * 4, _Endian);
            }
            return array;
        }


        public uint[] GetUInts(int Count, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            if (MemoryListSize < Count * 4)
            {
                throw new Exception("Not enought data");
            }
            if (Count == 0)
            {
                throw new Exception("Can't get 0 uint values");
            }
            uint[] array = new uint[Count];
            if (SeekAndRead != -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetUIntValue(SavePosition, SeekAndRead + i * 4, _Endian);
                }
                return array;
            }


            if (SavePosition)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetUIntValue(true, -1, _Endian);
                }
                return array;



            }
            for (int i = 0; i < Count; i++)
            {
                array[i] = GetUIntValue(SavePosition, MemoryListPosition + i * 4, _Endian);
            }
            return array;
        }

        #endregion


        #region float
        public float GetFloatValue(bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            byte[] array = GetBytes(4, SavePosition, SeekAndRead);
            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            return BitConverter.ToSingle(array, 0);
        }



        public void SetFloatValue(float value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = BitConverter.GetBytes(value);

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            SetBytes(array, SavePosition, SeekAndRead);
        }



        public void InsertFloatValue(float value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = BitConverter.GetBytes(value);

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            InsertBytes(array, SavePosition, SeekAndRead);

        }

        public void DeleteFloatValue(int SeekAndRead = -1)
        {
            if (MemoryListSize < 4)
            {
                throw new Exception("Not enought data");
            }
            DeleteBytes(4, SeekAndRead);
        }


        public float[] GetFloats(int Count, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            if (MemoryListSize < Count * 4)
            {
                throw new Exception("Not enought data");
            }
            if (Count == 0)
            {
                throw new Exception("Can't get 0 float values");
            }
            float[] array = new float[Count];
            if (SeekAndRead != -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetFloatValue(SavePosition, SeekAndRead + i * 4, _Endian);
                }
                return array;
            }


            if (SavePosition)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetFloatValue(true, -1, _Endian);
                }
                return array;



            }
            for (int i = 0; i < Count; i++)
            {
                array[i] = GetFloatValue(SavePosition, MemoryListPosition + i * 4, _Endian);
            }
            return array;
        }

        #endregion


        #region int64,uint64
        public long GetInt64Value(bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            byte[] array = GetBytes(8, SavePosition, SeekAndRead);
            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            return BitConverter.ToInt64(array, 0);
        }

        public ulong GetUInt64Value(bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            return (ulong)(GetInt64Value(SavePosition, SeekAndRead, _Endian));
        }


        public void SetInt64Value(long value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = new byte[8] { (byte)(value), (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24), (byte)(value >> 32), (byte)(value >> 40), (byte)(value >> 48), (byte)(value >> 56) };

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            SetBytes(array, SavePosition, SeekAndRead);
        }

        public void SetUInt64Value(ulong value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            SetInt64Value((long)value, SavePosition, SeekAndRead, _Endian);
        }

        public void InsertInt64Value(long value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = new byte[8] { (byte)(value), (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24), (byte)(value >> 32), (byte)(value >> 40), (byte)(value >> 48), (byte)(value >> 56) };

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            InsertBytes(array, SavePosition, SeekAndRead);

        }
        public void InsertUInt64Value(ulong value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            InsertInt64Value((long)value, SavePosition, SeekAndRead, _Endian);
        }

        public void DeleteInt64Value(int SeekAndRead = -1)
        {
            if (MemoryListSize < 8)
            {
                throw new Exception("Not enought data");
            }
            DeleteBytes(8, SeekAndRead);
        }

        public void DeleteUInt64Value(int SeekAndRead = -1)
        {
            DeleteInt64Value(SeekAndRead);
        }


        public long[] GetInt64s(int Count, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            if (MemoryListSize < Count * 8)
            {
                throw new Exception("Not enought data");
            }
            if (Count == 0)
            {
                throw new Exception("Can't get 0 int64 values");
            }
            long[] array = new long[Count];
            if (SeekAndRead != -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetInt64Value(SavePosition, SeekAndRead + i * 4, _Endian);
                }
                return array;
            }


            if (SavePosition)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetInt64Value(true, -1, _Endian);
                }
                return array;



            }
            for (int i = 0; i < Count; i++)
            {
                array[i] = GetInt64Value(SavePosition, MemoryListPosition + i * 4, _Endian);
            }
            return array;
        }
        public ulong[] GetUInt64s(int Count, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            if (MemoryListSize < Count * 8)
            {
                throw new Exception("Not enought data");
            }
            if (Count == 0)
            {
                throw new Exception("Can't get 0 uint64 values");
            }
            ulong[] array = new ulong[Count];
            if (SeekAndRead != -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetUInt64Value(SavePosition, SeekAndRead + i * 4, _Endian);
                }
                return array;
            }


            if (SavePosition)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetUInt64Value(true, -1, _Endian);
                }
                return array;



            }
            for (int i = 0; i < Count; i++)
            {
                array[i] = GetUInt64Value(SavePosition, MemoryListPosition + i * 4, _Endian);
            }
            return array;
        }



        #endregion


        #region Double
        public double GetDoubleValue(bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            byte[] array = GetBytes(8, SavePosition, SeekAndRead);
            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            return BitConverter.ToDouble(array, 0);
        }

        public void SetDoubleValue(double value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = BitConverter.GetBytes(value);

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            SetBytes(array, SavePosition, SeekAndRead);
        }

        public void InsertDoubleValue(double value, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {

            byte[] array = BitConverter.GetBytes(value);

            if (_Endian == Endian.Big)
            {
                Array.Reverse(array);
            }
            InsertBytes(array, SavePosition, SeekAndRead);
        }

        public void DeleteDoubleValue(int SeekAndRead = -1)
        {
            if (MemoryListSize < 8)
            {
                throw new Exception("Not enought data");
            }
            DeleteBytes(8, SeekAndRead);
        }


        public double[] GetDoubles(int Count, bool SavePosition = true, int SeekAndRead = -1, Endian _Endian = Endian.Little)
        {
            if (MemoryListSize < Count * 8)
            {
                throw new Exception("Not enought data");
            }
            if (Count == 0)
            {
                throw new Exception("Can't get 0 double values");
            }
            double[] array = new double[Count];
            if (SeekAndRead != -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetDoubleValue(SavePosition, SeekAndRead + i * 4, _Endian);
                }
                return array;
            }


            if (SavePosition)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i] = GetDoubleValue(true, -1, _Endian);
                }
                return array;



            }
            for (int i = 0; i < Count; i++)
            {
                array[i] = GetDoubleValue(SavePosition, MemoryListPosition + i * 4, _Endian);
            }
            return array;
        }

        #endregion


        #region String
        public string GetStringValue(int StringLenght, bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }


            if (MemoryListSize < StringLenght)
            {
                throw new Exception("Not enought data");
            }

            byte[] array = GetBytes(StringLenght, SavePosition, SeekAndRead);
            return encoding.GetString(array);
        }


        public void SetStringValue(string String, bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            SetBytes(encoding.GetBytes(String), SavePosition, SeekAndRead);
        }

        public void InsertStringValue(string String, bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            InsertBytes(encoding.GetBytes(String), SavePosition, SeekAndRead);
        }


        private byte[] GetString(Encoding encoding, bool SavePosition = true)
        {
            int ThisPosition = GetPosition();
            List<byte> StringValues = new List<byte>();
            if (encoding != Encoding.Unicode)
            {
                while (true)
                {
                    StringValues.Add(GetByteValue());
                    if (StringValues[StringValues.Count - 1] == 0)
                    {
                        break;
                    }

                }
            }
            else
            {
                while (true)
                {
                    StringValues.Add(GetByteValue());
                    StringValues.Add(GetByteValue());
                    if (StringValues[StringValues.Count - 1] == 0 && StringValues[StringValues.Count - 2] == 0)
                    {
                        break;
                    }
                }

            }

            if (!SavePosition)
            {
                Seek(ThisPosition);
            }

            return StringValues.ToArray();
        }

        public string GetStringValueN(bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }

            int ThisPosition = GetPosition();
            string Value;
            if (SeekAndRead != -1)
            {

                Seek(SeekAndRead);
                Value = encoding.GetString(GetString(encoding)).TrimEnd('\0');
                Seek(ThisPosition);
                return Value;
            }

            if (SavePosition)
            {
                return encoding.GetString(GetString(encoding)).TrimEnd('\0');
            }

            Seek(MemoryListIndex);
            Value = encoding.GetString(GetString(encoding)).TrimEnd('\0');
            Seek(ThisPosition);
            return Value;
        }


        public int DeleteStringN(int SeekAndRead = -1, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }

            int ThisPosition = GetPosition();
            int StringLength;
            if (SeekAndRead != -1)
            {
                Seek(SeekAndRead);
                StringLength = GetString(encoding, false).Length;
                DeleteBytes(StringLength);
                Seek(ThisPosition);
                return StringLength;
            }
           
            StringLength = GetString(encoding, false).Length;
            DeleteBytes(GetString(encoding, false).Length);
            return StringLength;
        }


        public void SetStringValueN(string String, bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            SetBytes(encoding.GetBytes(String + '\0'), SavePosition, SeekAndRead);
        }

        public void InsertStringValueN(string String, bool SavePosition = true, int SeekAndRead = -1, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            InsertBytes(encoding.GetBytes(String + '\0'), SavePosition, SeekAndRead);
        }

        public int GetStringLenght(string String, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            return encoding.GetBytes(String).Length;
        }
        public int GetStringLenghtN(string String, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            return encoding.GetBytes(String + '\0').Length;
        }
        #endregion


        #region Struct

        public T GetStructureValues<T>(bool SavePosition = true, int SeekAndRead = -1)
        {
            var structureSize = Marshal.SizeOf(typeof(T));
            var buffer = GetBytes(structureSize, SavePosition, SeekAndRead);

            if (buffer.Length != structureSize)
            {
                throw new Exception("could not read all of data for structure");
            }

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return structure;
        }


        public void SetStructureValus<T>(T structure, bool SavePosition = true, int SeekAndRead = -1)
        {
            var structureSize = Marshal.SizeOf(typeof(T));
            var buffer = new byte[structureSize];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
            handle.Free();
            SetBytes(buffer, SavePosition, SeekAndRead);
        }
        #endregion

    }
}

