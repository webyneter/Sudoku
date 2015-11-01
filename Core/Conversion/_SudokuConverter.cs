// Decompiled with JetBrains decompiler
// Type: SudokuCommons.Utilities.SudokuConverter
// Assembly: Sudoku Commons, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 17F10EC5-8832-4781-8EFB-00E639B60A13
// Assembly location: D:\Программирование\Практика\C#\Sudoku\CUI\Releases\Sudoku Commons.dll

using SudokuCommons;
using System;
using System.IO;
using System.Text;

using SudokuCommons.Conversion;
using SudokuCommons.Desks;


namespace SudokuCommons.Conversion
{
    internal static class SudokuConverter
    {
        private const byte LINEAR_HEADER_LENGTH = (byte)5;
        private const byte NONLINEAR_HEADER_LENGTH = (byte)8;

        public static byte[] ToBinary(string content, SudokuConvertionAlgorithm alg, SudokuDeskRestrictions kind, SudokuDeskMetrics metrics)
        {
            switch (alg)
            {
                case SudokuConvertionAlgorithm.Uniform:
                    byte[] numArray1 = SudokuConverter._textToBinaryLinear(content, metrics.MaximumNumber);
                    byte[] numArray2 = new byte[5 + numArray1.Length];
                    numArray2[0] = (byte)0;
                    numArray2[1] = (byte)kind;
                    numArray2[2] = metrics.MaximumNumber;
                    numArray2[3] = metrics.BlockWidth;
                    numArray2[4] = metrics.BlockHeight;
                    Array.Copy((Array)numArray1, 0, (Array)numArray2, 5, numArray1.Length);
                    return numArray2;
                case SudokuConvertionAlgorithm.Ununiform:
                    ushort zerosPairsPos;
                    byte bitsPerPos;
                    byte bitsPerZerosQuantity;
                    byte[] numArray3 = SudokuConverter._textToBinaryNonLinear(content, metrics.MaximumNumber, out zerosPairsPos, out bitsPerPos, out bitsPerZerosQuantity);
                    byte[] numArray4 = new byte[8 + numArray3.Length];
                    numArray4[0] = (byte)1;
                    numArray4[1] = (byte)((uint)zerosPairsPos >> 8);
                    numArray4[2] = (byte)zerosPairsPos;
                    numArray4[3] = (byte)((int)bitsPerPos - 1 << 4 | (int)bitsPerZerosQuantity - 1);
                    numArray4[4] = (byte)kind;
                    numArray4[5] = metrics.MaximumNumber;
                    numArray4[6] = metrics.BlockWidth;
                    numArray4[7] = metrics.BlockHeight;
                    Array.Copy((Array)numArray3, 0, (Array)numArray4, 8, numArray3.Length);
                    return numArray4;
                default:
                    return (byte[])null;
            }
        }

        [Obsolete]
        public static byte[] ToBinary(FileStream textFile, SudokuDesk desk, SudokuConvertionAlgorithm alg)
        {
            textFile.Position = 0L;
            string content;
            using (StreamReader streamReader = new StreamReader((Stream)textFile))
                content = streamReader.ReadToEnd();
            textFile.Position = 0L;
            return SudokuConverter.ToBinary(content, alg, desk.Restrictions, desk.Metrics);
        }

        public static byte[] ToBinary(SudokuDesk desk, SudokuConvertionAlgorithm alg)
        {
            StringBuilder sb = new StringBuilder((int)desk.Metrics.CellsTotal);
            desk.IterateLines((Action<byte, byte>)((i, j) => sb.Append(desk.Cells[(int)i, (int)j].Number)));
            return SudokuConverter.ToBinary(sb.ToString(), alg, desk.Restrictions, desk.Metrics);
        }

        public static string ToText(byte[] binary, out SudokuDeskRestrictions kind, out SudokuDeskMetrics metrics)
        {
            switch (binary[0])
            {
                case (byte)0:
                    kind = (SudokuDeskRestrictions)binary[1];
                    metrics = new SudokuDeskMetrics(binary[2], binary[3], binary[4]);
                    byte[] data1 = new byte[binary.Length - 5];
                    Array.Copy((Array)binary, 5, (Array)data1, 0, data1.Length);
                    return SudokuConverter._binaryLinearToText(data1, binary[2]);
                case (byte)1:
                    ushort zerosPairsPos = (ushort)((uint)binary[1] << 8 | (uint)binary[2]);
                    byte bitsPerPos = (byte)(((int)binary[3] >> 4) + 1);
                    byte bitsPerZerosQuantity = (byte)(((int)(byte)((uint)binary[3] << 4) >> 4) + 1);
                    kind = (SudokuDeskRestrictions)binary[4];
                    metrics = new SudokuDeskMetrics(binary[5], binary[6], binary[7]);
                    byte[] data2 = new byte[binary.Length - 8];
                    Array.Copy((Array)binary, 8, (Array)data2, 0, data2.Length);
                    return SudokuConverter._binaryNonLinearToText(data2, metrics, zerosPairsPos, bitsPerPos, bitsPerZerosQuantity);
                default:
                    kind = SudokuDeskRestrictions.Traditional;
                    metrics = SudokuDeskMetrics.Empty;
                    return (string)null;
            }
        }

        [Obsolete]
        public static string ToText(FileStream binaryFile, out SudokuDeskRestrictions kind, out SudokuDeskMetrics metrics)
        {
            byte[] numArray = new byte[binaryFile.Length];
            binaryFile.Position = 0L;
            binaryFile.Read(numArray, 0, (int)(ushort)binaryFile.Length);
            binaryFile.Position = 0L;
            return SudokuConverter.ToText(numArray, out kind, out metrics);
        }

        private static byte[] _textToBinaryLinear(string content, byte maxNumber)
        {
            byte[] data = new byte[content.Length];
            for (ushort index = (ushort)0; (int)index < content.Length; ++index)
                data[(int)index] = byte.Parse(content[(int)index].ToString());
            return SudokuConverter._compressNumbers(data, maxNumber);
        }

        private static byte[] _textToBinaryNonLinear(string content, byte maxNumber, out ushort zerosPairsPos, out byte bitsPerPos, out byte bitsPerZerosQuantity)
        {
            ushort num1 = (ushort)0;
            ushort num2 = (ushort)0;
            ushort num3 = (ushort)0;
            while ((int)num3 < content.Length)
            {
                if ((int)content[(int)num3] == 48)
                {
                    ++num2;
                    do
                    {
                        ++num3;
                    }
                    while ((int)num3 < content.Length && (int)content[(int)num3] == 48);
                }
                else
                {
                    ++num1;
                    ++num3;
                }
            }
            byte[] data1 = new byte[(int)num1];
            byte num4 = (byte)0;
            ushort[] data2 = new ushort[(int)num2 * 2];
            byte num5 = (byte)1;
            byte num6 = (byte)0;
            for (ushort index = (ushort)0; (int)index < content.Length; ++index)
            {
                char ch = content[(int)index];
                if ((int)ch == 48)
                {
                    if ((int)num6 == 0)
                    {
                        data2[(int)num5 - 1] = index;
                        data2[(int)num5] = (ushort)1;
                    }
                    data2[(int)num5] = (ushort)++num6;
                }
                else
                {
                    data1[(int)num4++] = byte.Parse(ch.ToString());
                    if ((int)num6 != 0)
                    {
                        num6 = (byte)0;
                        num5 += (byte)2;
                    }
                }
            }
            byte[] numArray1 = SudokuConverter._compressNumbers(data1, maxNumber);
            byte[] numArray2 = SudokuConverter._compressZerosPairs(data2, out bitsPerPos, out bitsPerZerosQuantity);
            byte[] numArray3 = new byte[numArray1.Length + numArray2.Length];
            zerosPairsPos = (ushort)numArray1.Length;
            Array.Copy((Array)numArray1, 0, (Array)numArray3, 0, numArray1.Length);
            Array.Copy((Array)numArray2, 0, (Array)numArray3, numArray1.Length, numArray2.Length);
            return numArray3;
        }

        private static string _binaryLinearToText(byte[] data, byte maxNumber)
        {
            byte[] numArray = SudokuConverter._decompressNumbers(data, maxNumber);
            StringBuilder stringBuilder = new StringBuilder(numArray.Length);
            for (ushort index = (ushort)0; (int)index < numArray.Length; ++index)
                stringBuilder.Append(numArray[(int)index]);
            return stringBuilder.ToString();
        }

        private static string _binaryNonLinearToText(byte[] data, SudokuDeskMetrics metrics, ushort zerosPairsPos, byte bitsPerPos, byte bitsPerZerosQuantity)
        {
            byte[] data1 = new byte[(int)zerosPairsPos];
            Array.Copy((Array)data, 0, (Array)data1, 0, (int)zerosPairsPos);
            byte[] numArray1 = SudokuConverter._decompressNumbers(data1, metrics.MaximumNumber);
            if ((int)numArray1[numArray1.Length - 1] == 0)
            {
                byte[] numArray2 = new byte[numArray1.Length - 1];
                Array.Copy((Array)numArray1, 0, (Array)numArray2, 0, numArray2.Length);
                numArray1 = numArray2;
            }
            byte[] data2 = new byte[data.Length - (int)zerosPairsPos];
            Array.Copy((Array)data, (int)zerosPairsPos, (Array)data2, 0, data2.Length);
            ushort[] numArray3 = SudokuConverter._decompressZerosPairs(data2, bitsPerPos, bitsPerZerosQuantity);
            StringBuilder stringBuilder = new StringBuilder((int)metrics.CellsTotal);
            ushort num1 = (ushort)0;
            ushort num2 = (ushort)0;
            ushort num3 = (ushort)0;
            while ((int)num3 < (int)metrics.CellsTotal)
            {
                if ((int)num2 < numArray3.Length && (int)num3 < (int)numArray3[(int)num2])
                {
                    stringBuilder.Append(numArray1[(int)num1++]);
                    ++num3;
                }
                else if ((int)num2 < numArray3.Length && (int)num3 == (int)numArray3[(int)num2])
                {
                    ushort num4 = (ushort)((uint)num2 + 1U);
                    ushort num5 = (ushort)0;
                    while ((int)num5 < (int)numArray3[(int)num4])
                    {
                        stringBuilder.Append("0");
                        ++num5;
                        ++num3;
                    }
                    num2 = (ushort)((uint)num4 + 1U);
                }
                else
                {
                    ushort num4 = num3;
                    while ((int)num4 < (int)metrics.CellsTotal)
                    {
                        stringBuilder.Append(numArray1[(int)num1++]);
                        ++num4;
                        ++num3;
                    }
                }
            }
            return stringBuilder.ToString();
        }

        private static byte _getSignificantBitsOf(ushort number)
        {
            return (byte)(Math.Log((double)number, 2.0) + 1.0);
        }

        private static bool[] _representAsBits(byte[] array)
        {
            bool[] flagArray = new bool[8 * array.Length];
            int num1 = 0;
            for (ushort index1 = (ushort)0; (int)index1 < array.Length; ++index1)
            {
                sbyte num2 = sbyte.MinValue;
                for (ushort index2 = (ushort)0; (int)index2 < 8; ++index2)
                {
                    flagArray[num1++] = ((int)array[(int)index1] & (int)num2) != 0;
                    num2 >>= 1;
                }
            }
            return flagArray;
        }

        private static byte[] _compressNumbers(byte[] data, byte maxNumber)
        {
            ushort num1 = (ushort)SudokuConverter._getSignificantBitsOf((ushort)maxNumber);
            ushort num2 = (ushort)((uint)num1 * (uint)data.Length);
            ushort num3 = (ushort)((uint)num2 % 8U);
            bool[] flagArray = new bool[(int)num2 + ((int)num3 == 0 ? 0 : 8 - (int)num3)];
            ushort num4 = (ushort)0;
            ushort num5 = (ushort)(1 << (int)num1 - 1);
            for (ushort index = (ushort)0; (int)index < data.Length; ++index)
            {
                ushort num6 = num5;
                while ((int)num6 > 0)
                {
                    flagArray[(int)num4++] = ((int)data[(int)index] & (int)num6) == (int)num6;
                    num6 >>= 1;
                }
            }
            ushort num7 = (ushort)(flagArray.Length / 8);
            byte[] numArray = new byte[(int)num7];
            ushort num8 = (ushort)0;
            for (ushort index1 = (ushort)0; (int)index1 < (int)num7; ++index1)
            {
                for (ushort index2 = (ushort)0; (int)index2 < 8; ++index2)
                    numArray[(int)index1] |= flagArray[(int)num8++] ? (byte)(128 >> (int)index2) : (byte)0;
            }
            return numArray;
        }

        private static byte[] _decompressNumbers(byte[] data, byte maxNumber)
        {
            byte significantBitsOf = SudokuConverter._getSignificantBitsOf((ushort)maxNumber);
            byte[] numArray = new byte[8 * data.Length / (int)significantBitsOf];
            bool[] flagArray = SudokuConverter._representAsBits(data);
            int num = 0;
            for (ushort index1 = (ushort)0; (int)index1 < numArray.Length; ++index1)
            {
                for (byte index2 = significantBitsOf; (int)index2 > 0; --index2)
                {
                    if (flagArray[num++])
                        numArray[(int)index1] |= (byte)(1 << (int)index2 - 1);
                }
            }
            return numArray;
        }

        private static byte[] _compressZerosPairs(ushort[] data, out byte bitsPerPos, out byte bitsPerZerosQuotient)
        {
            ushort number1 = (ushort)0;
            ushort number2 = (ushort)0;
            ushort num1;
            for (ushort index = (ushort)0; (int)index < data.Length; index = (ushort)((uint)num1 + 1U))
            {
                if ((int)data[(int)index] > (int)number1)
                    number1 = data[(int)index];
                num1 = (ushort)((uint)index + 1U);
                if ((int)data[(int)num1] > (int)number2)
                    number2 = data[(int)num1];
            }
            bitsPerPos = SudokuConverter._getSignificantBitsOf(number1);
            bitsPerZerosQuotient = SudokuConverter._getSignificantBitsOf(number2);
            byte num2 = (byte)(((int)bitsPerPos + (int)bitsPerZerosQuotient) * (data.Length / 2));
            ushort num3 = (ushort)((uint)num2 % 8U);
            byte[] numArray1 = new byte[(int)num2 + ((int)num3 == 0 ? 0 : 8 - (int)num3)];
            ushort num4 = (ushort)0;
            for (ushort index = (ushort)0; (int)index < data.Length; ++index)
            {
                byte num5 = (int)index % 2 != 0 ? (byte)(1 << (int)bitsPerZerosQuotient - 1) : (byte)(1 << (int)bitsPerPos - 1);
                while ((int)num5 > 0)
                {
                    numArray1[(int)num4++] = ((int)data[(int)index] & (int)num5) != 0 ? (byte)1 : (byte)0;
                    num5 >>= 1;
                }
            }
            byte[] numArray2 = new byte[numArray1.Length / 8];
            ushort num6 = (ushort)0;
            for (ushort index = (ushort)0; (int)index < numArray2.Length; ++index)
            {
                numArray2[(int)index] = (byte)((uint)(128 * (int)numArray1[(int)num6] + 64 * (int)numArray1[(int)num6 + 1] + 32 * (int)numArray1[(int)num6 + 2] + 16 * (int)numArray1[(int)num6 + 3] + 8 * (int)numArray1[(int)num6 + 4] + 4 * (int)numArray1[(int)num6 + 5] + 2 * (int)numArray1[(int)num6 + 6]) + (uint)numArray1[(int)num6 + 7]);
                num6 += (ushort)8;
            }
            return numArray2;
        }

        private static ushort[] _decompressZerosPairs(byte[] data, byte bitsPerPos, byte bitsPerZerosQuantity)
        {
            bool[] flagArray = SudokuConverter._representAsBits(data);
            ushort[] numArray = new ushort[2 * (int)(ushort)(8 * data.Length / ((int)bitsPerPos + (int)bitsPerZerosQuantity))];
            ushort num1 = (ushort)0;
            ushort num2;
            for (ushort index1 = (ushort)0; (int)index1 < numArray.Length; index1 = (ushort)((uint)num2 + 1U))
            {
                for (ushort index2 = (ushort)bitsPerPos; (int)index2 > 0; --index2)
                {
                    if (flagArray[(int)num1++])
                        numArray[(int)index1] |= (ushort)(1 << (int)index2 - 1);
                }
                num2 = (ushort)((uint)index1 + 1U);
                for (ushort index2 = (ushort)bitsPerZerosQuantity; (int)index2 > 0; --index2)
                {
                    if (flagArray[(int)num1++])
                        numArray[(int)num2] |= (ushort)(1 << (int)index2 - 1);
                }
            }
            return numArray;
        }
    }
}
