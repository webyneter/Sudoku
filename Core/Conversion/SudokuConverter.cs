using System;
using System.IO;
using System.Text;

using Webyneter.Sudoku.Core.Grids;


namespace Webyneter.Sudoku.Core.Conversion
{
    // TODO: separate entities (class --> interface) for conversion algorithms depending on the existing classification
    /// <summary>
    /// Provides static methods for sudoku files conversion.
    /// </summary>
    internal static class SudokuConverter
    {
        private const byte HEADER_RESTRICTIONS_LENGTH = 3;
        private const byte HEADER_UNIFORM_LENGTH = 5;
        private const byte HEADER_UNIFORM_RESTRICTED_LENGTH = HEADER_UNIFORM_LENGTH + HEADER_RESTRICTIONS_LENGTH;
        private const byte HEADER_NONUNIFORM_LENGTH = 8;
        private const byte HEADER_NONUNIFORM_RESTRICTED_LENGTH = HEADER_NONUNIFORM_LENGTH + HEADER_RESTRICTIONS_LENGTH;
        
        public static byte[] ToBinary(string content, SudokuConvertionAlgorithm algorithm, 
            SudokuGridConstraints constraints, SudokuGridMetrics metrics)
        {
            byte[] compressedContent;
            byte[] result;
            switch (algorithm) 
            {
                case SudokuConvertionAlgorithm.Uniform:
                    compressedContent = TextToBinaryUniform(content, metrics.MaximumNumber);
                    result = new byte[(constraints == SudokuGridConstraints.Traditional) 
                        ? HEADER_UNIFORM_LENGTH 
                        : HEADER_UNIFORM_RESTRICTED_LENGTH 
                        + compressedContent.Length];
                    //if (constraints == SudokuGridConstraints.Traditional)
                    //{
                    //    result = new byte[HEADER_UNIFORM_LENGTH + compressedContent.Length];
                    //}
                    //else
                    //{
                    //    result = new byte[HEADER_UNIFORM_RESTRICTED_LENGTH + compressedContent.Length];    
                    //}
                    result[0] = (byte)SudokuConvertionAlgorithm.Uniform;
                    result[1] = (byte)constraints;
                    result[2] = metrics.MaximumNumber;
                    result[3] = metrics.BlockWidth;
                    result[4] = metrics.BlockHeight;
                    if (constraints != SudokuGridConstraints.Traditional) 
                    {
                        throw new NotImplementedException();
                        //var restrictionsPos = (ushort)();
                        //result[5] = (byte)();
                    }
                    Array.Copy(compressedContent, 0, result, (constraints == SudokuGridConstraints.Traditional) 
                        ? HEADER_UNIFORM_LENGTH 
                        : HEADER_UNIFORM_RESTRICTED_LENGTH, 
                        compressedContent.Length);
					return result;
                    
                case SudokuConvertionAlgorithm.NonUniform:
                    ushort zerosPairsPos;
                    byte bitsPerPos;
                    byte bitsPerZerosQuantity;
                    compressedContent = TextToBinaryNonUniform(content, metrics.MaximumNumber, out zerosPairsPos, 
                        out bitsPerPos, out bitsPerZerosQuantity);
                    result = new byte[(constraints == SudokuGridConstraints.Traditional)
                        ? HEADER_NONUNIFORM_LENGTH
                        : HEADER_NONUNIFORM_RESTRICTED_LENGTH
                        + compressedContent.Length];
                    result[0] = (byte)SudokuConvertionAlgorithm.NonUniform;
                    result[1] = (byte)(zerosPairsPos >> 8);
                    result[2] = (byte)zerosPairsPos;
                    result[3] = (byte)(((bitsPerPos - 1) << 4) | (bitsPerZerosQuantity - 1));
                    result[4] = (byte)constraints;
                    result[5] = metrics.MaximumNumber;
                    result[6] = metrics.BlockWidth;
                    result[7] = metrics.BlockHeight;
                    Array.Copy(compressedContent, 0, result, (constraints == SudokuGridConstraints.Traditional)
                        ? HEADER_NONUNIFORM_LENGTH
                        : HEADER_NONUNIFORM_RESTRICTED_LENGTH,
                        compressedContent.Length);
                    //Array.Copy(compressedContent, 0, result, NONLINEAR_HEADER_LENGTH, compressedContent.Length);
                    return result;

                default:
                    return null;
            }
        }
        
        public static byte[] ToBinary(FileStream textFile, SudokuGrid grid, 
            SudokuConvertionAlgorithm algorithm)
        {
            string content;
            textFile.Position = 0;
            using (var sr = new StreamReader(textFile))
            {
                content = sr.ReadToEnd();
            }
            textFile.Position = 0;
            return ToBinary(content, algorithm, grid.Constraints, grid.Metrics);
        }

        public static byte[] ToBinary(SudokuGrid grid, SudokuConvertionAlgorithm algorithm)
        {
            var sb = new StringBuilder(grid.Metrics.CellsTotal);
            grid.IterateLinesXY((x, y) => sb.Append(grid.Cells[y, x].Number));
            return ToBinary(sb.ToString(), algorithm, grid.Constraints, grid.Metrics);
        }

        public static string ToText(byte[] binary, out SudokuGridConstraints constraints, 
            out byte[] numbersKinds, out SudokuGridMetrics metrics)
        {
            // !!! temporary !!!
            numbersKinds = null;
            var alg = (SudokuConvertionAlgorithm)(binary[0]);
            byte[] data;
            switch (alg)
            {
                case SudokuConvertionAlgorithm.Uniform:
                    constraints = (SudokuGridConstraints)(binary[1]);
                    metrics = new SudokuGridMetrics(binary[2], binary[3], binary[4]);
                    data = new byte[binary.Length - ((constraints == SudokuGridConstraints.Traditional)
                        ? HEADER_UNIFORM_LENGTH
                        : HEADER_UNIFORM_RESTRICTED_LENGTH)];
                    Array.Copy(binary, (constraints == SudokuGridConstraints.Traditional)
                        ? HEADER_UNIFORM_LENGTH
                        : HEADER_UNIFORM_RESTRICTED_LENGTH, 
                        data, 0, data.Length);
                    return BinaryUniformToText(data, binary[2], constraints);
                    
                case SudokuConvertionAlgorithm.NonUniform:
                    var zerosPairsPos = (ushort)((binary[1] << 8) | binary[2]);
                    var bitsPerPos = (byte) ((binary[3] >> 4) + 1);
                    var bitsPerZerosQuantity = (byte) (((byte) (binary[3] << 4) >> 4) + 1);
                    constraints = (SudokuGridConstraints)(binary[4]);
                    metrics = new SudokuGridMetrics(binary[5], binary[6], binary[7]);
                    data = new byte[binary.Length - ((constraints == SudokuGridConstraints.Traditional)
                        ? HEADER_NONUNIFORM_LENGTH
                        : HEADER_NONUNIFORM_RESTRICTED_LENGTH)];
                    Array.Copy(binary, (constraints == SudokuGridConstraints.Traditional)
                        ? HEADER_NONUNIFORM_LENGTH
                        : HEADER_NONUNIFORM_RESTRICTED_LENGTH,
                        data, 0, data.Length);
                    return BinaryNonUniformToText(data, metrics, zerosPairsPos, bitsPerPos, 
                        bitsPerZerosQuantity, constraints);

                default:
                    // dummy code
                    constraints = SudokuGridConstraints.Traditional;
                    metrics = SudokuGridMetrics.Empty;
                    return null;
            }
        }

        public static string ToText(FileStream binaryFile, out SudokuGridConstraints constraints, 
            out byte[] numbersKinds, out SudokuGridMetrics metrics)
        {
            var binary = new byte[binaryFile.Length];
            binaryFile.Position = 0;
            binaryFile.Read(binary, 0, (int)binaryFile.Length);
            binaryFile.Position = 0;
            return ToText(binary, out constraints, out numbersKinds, out metrics);
        }


        private static SudokuConvertionAlgorithm ChooseOptimalAlgorithm(byte[] binary
            // send by reference any additional data
            //, out 
            )
        {
            throw new NotImplementedException();
        }

        private static byte[] TextToBinaryUniform(string content, byte maxNumber)
        {
            var data = new byte[content.Length];
            for (ushort i = 0; i < content.Length; ++i)
            {
                data[i] = Convert.ToByte(content[i]);    
            }
            return CompressNumbers(data, maxNumber);
        }

        private static byte[] TextToBinaryNonUniform(string content, byte maxNumber, out ushort zerosPairsPos, 
            out byte bitsPerPos, out byte bitsPerZerosQuantity)
        {
            // determining the number of zeros sequences
            ushort i;
            ushort numbersTotal = 0;
            ushort zerosSeqsTotal = 0;
            for (i = 0; i < content.Length;)
            {
                if (content[i] == '0')
                {
                    ++zerosSeqsTotal;
                    do
                    {
                        ++i;
                    } while ((i < content.Length) && (content[i] == '0'));
                }
                else
                {
                    ++numbersTotal;
                    ++i;
                }
            }
            
            // composing sequences of the form <zeros' sequence start position; zeros total>
            char buffer;
            var numbersUncompressed = new byte[numbersTotal];
            byte indCurrNumber = 0;
            var zerosPairsUncompressed = new ushort[zerosSeqsTotal * 2];
            byte indCurrCounter = 1;
            byte zerosCount = 0;
            for (i = 0; i < content.Length; ++i)
            {
                buffer = content[i];
                if (buffer == '0')
                {
                    if (zerosCount == 0)
                    {
                        zerosPairsUncompressed[indCurrCounter - 1] = i;
                        zerosPairsUncompressed[indCurrCounter] = 1;
                    }

                    zerosPairsUncompressed[indCurrCounter] = ++zerosCount;
                }
                else
                {
                    numbersUncompressed[indCurrNumber++] = Convert.ToByte(buffer);
                    if (zerosCount != 0)
                    {
                        zerosCount = 0;
                        indCurrCounter += 2;
                    }
                }
            }
            
            // compression and output
            byte[] numbers = CompressNumbers(numbersUncompressed, maxNumber);
            byte[] zerosPairs = CompressZerosPairs(zerosPairsUncompressed, out bitsPerPos,
                out bitsPerZerosQuantity);
            var result = new byte[numbers.Length + zerosPairs.Length];
            zerosPairsPos = (ushort)(numbers.Length);
            Array.Copy(numbers, 0, result, 0, numbers.Length);
            Array.Copy(zerosPairs, 0, result, numbers.Length, zerosPairs.Length);
            return result;
        }

        private static string BinaryUniformToText(byte[] data, byte maxNumber, SudokuGridConstraints constraints) 
        {
            var result = DecompressNumbers(data, maxNumber);
            var sb = new StringBuilder(result.Length);
            for (ushort i = 0; i < result.Length; ++i)
            {
                sb.Append(result[i]);
            }
            return sb.ToString();
        }

        private static string BinaryNonUniformToText(byte[] data, SudokuGridMetrics metrics,
            ushort zerosPairsPos, byte bitsPerPos, byte bitsPerZerosQuantity, SudokuGridConstraints constraints)
        {
            // extraction
            var numbers = new byte[zerosPairsPos];
            Array.Copy(data, 0, numbers, 0, zerosPairsPos);
            numbers = DecompressNumbers(numbers, metrics.MaximumNumber);
            if (numbers[numbers.Length - 1] == 0)
            {
                var nums = new byte[numbers.Length - 1];
                Array.Copy(numbers, 0, nums, 0, nums.Length);
                numbers = nums;
            }
            var zerosPairsCompressed = new byte[data.Length - zerosPairsPos];
            Array.Copy(data, zerosPairsPos, zerosPairsCompressed, 0, zerosPairsCompressed.Length);
            ushort[] zerosPairs = DecompressZerosPairs(zerosPairsCompressed, bitsPerPos,
                bitsPerZerosQuantity);
            
            // combining decompressed sequences
            var sb = new StringBuilder(metrics.CellsTotal);
            ushort j;
            ushort indNumbers = 0;
            ushort indZerosPairs = 0;
            for (ushort i = 0; i < metrics.CellsTotal; )
            {
                if ((indZerosPairs < zerosPairs.Length) && 
                    (i < zerosPairs[indZerosPairs])) // possible numbers are at the beginning
                {
                    sb.Append(numbers[indNumbers++]);
                    ++i;
                }
                else if ((indZerosPairs < zerosPairs.Length) && 
                    (i == zerosPairs[indZerosPairs])) // zeros (either at the beginning or later)
                {
                    indZerosPairs++;
                    for (j = 0; j < zerosPairs[indZerosPairs]; ++j, ++i)
                    {
                        sb.Append("0");
                    }
                    indZerosPairs++;
                }
                else // numbers are at the end
                {
                    for (j = i; j < metrics.CellsTotal; ++j, ++i)
                    {
                        sb.Append(numbers[indNumbers++]);
                    }
                }
            }

            return sb.ToString();
        }



        private static byte GetSignificantBitsOf(ushort number)
        {
            return (byte)(Math.Log(number, 2) + 1);
        }

        private static bool[] RepresentAsBits(byte[] array) 
        {
            var result = new bool[8 * array.Length];
            int indResult = 0;
            ushort i;
            ushort j;
            for (i = 0; i < array.Length; ++i)
            {
                byte numWithSingleTrueBit = 128;
                for (j = 0; j < 8; ++j)
                {
                    if ((array[i] & numWithSingleTrueBit) == 0)
                    {
                        result[indResult++] = false;
                    }
                    else
                    {
                        result[indResult++] = true;
                    }
                    numWithSingleTrueBit >>= 1;
                }
            }
            return result;
        }

        private static byte[] CompressNumbers(byte[] data, byte maxNumber)
        {
            // determining maximum number of bits needed
            // to compress the maxNumber
			
            ushort bitsMaximum = GetSignificantBitsOf(maxNumber);
            
            // compression

            var bitsTotal = (ushort) (bitsMaximum * data.Length);
            var bitsRemainded = (ushort) (bitsTotal % 8);
            var bits = new bool[bitsTotal + ((bitsRemainded == 0) ? 0 : (8 - bitsRemainded))];
            ushort indBits = 0;
            ushort i;
            ushort maskCurr;
            var maskDefault = (ushort) (1 << (bitsMaximum - 1));
            for (i = 0; i < data.Length; ++i)
            {
                for (maskCurr = maskDefault; maskCurr > 0; maskCurr >>= 1)
                {
                    bits[indBits++] = ((data[i] & maskCurr) == maskCurr);
                }    
            }

            var bytesTotal = (ushort)(bits.Length / 8);
            var result = new byte[bytesTotal];
            ushort j;
            indBits = 0;
            for (i = 0; i < bytesTotal; ++i)
            {
                for (j = 0; j < 8; ++j)
                {
                    result[i] |= (byte) ((bits[indBits++]) ? (128 >> j) : 0);
                } 
            }

            return result;
        }

        private static byte[] DecompressNumbers(byte[] data, byte maxNumber)
        {
            byte bitsNeeded = GetSignificantBitsOf(maxNumber);
            var result = new byte[(8 * data.Length) / bitsNeeded];

            bool[] bits = RepresentAsBits(data);
            int indBits = 0;
            byte j;
            for (ushort i = 0; i < result.Length; ++i)
            {
                for (j = bitsNeeded; j > 0; --j)
                {
                    if (bits[indBits++])
                    {
                        result[i] |= (byte) (1 << (j - 1));
                    }
                }
            }
            
            return result;
        }

        private static byte[] CompressZerosPairs(ushort[] data, out byte bitsPerPos,
            out byte bitsPerZerosQuotient)
        {
            // looking up for the max. position orderal number
            ushort i;
            ushort maxPosNumber = 0;
            ushort maxZerosQuotient = 0;
            for (i = 0; i < data.Length; )
            {
                if (data[i] > maxPosNumber)
                {
                    maxPosNumber = data[i];   
                }
                ++i;
                if (data[i] > maxZerosQuotient)
                {
                    maxZerosQuotient = data[i];   
                }
                ++i;
            }
            bitsPerPos = GetSignificantBitsOf(maxPosNumber);
            bitsPerZerosQuotient = GetSignificantBitsOf(maxZerosQuotient);
			
            var bitsNeeded =  (byte)((bitsPerPos + bitsPerZerosQuotient) * (data.Length / 2));
            i = (ushort)(bitsNeeded % 8);
            var bits = new byte[bitsNeeded + ((i == 0) ? 0 : (8 - i))];
            ushort indBits = 0;
            for (i = 0; i < data.Length; ++i)
            {
                byte numWithSingleTrueBit;
                if (i % 2 == 0) // data[i] contains position
                {
                    numWithSingleTrueBit = (byte)(1 << (bitsPerPos - 1)); 
                }
                else // data[i] contains zeros quotient
                {
                    numWithSingleTrueBit = (byte)(1 << (bitsPerZerosQuotient - 1));
                }
                // iterating left to right bitwise
                for (; numWithSingleTrueBit > 0; numWithSingleTrueBit >>= 1)
                {
                    if ((data[i] & numWithSingleTrueBit) == 0)
                    {
                        bits[indBits++] = 0;
                    }
                    else
                    {
                        bits[indBits++] = 1;
                    }
                }
            }
            
            // output
            var result = new byte[bits.Length / 8];
            ushort j = 0;
            for (i = 0; i < result.Length; ++i)
            {
                result[i] = (byte)(128 * bits[j] 
                    + 64 * bits[j + 1] 
                    + 32 * bits[j + 2] 
                    + 16 * bits[j + 3] 
                    + 8 * bits[j + 4] 
                    + 4 * bits[j + 5] 
                    + 2 * bits[j + 6] 
                    + bits[j + 7]);
                j += 8;
            }

            return result;
        }

        private static ushort[] DecompressZerosPairs(byte[] data, byte bitsPerPos,
                                                      byte bitsPerZerosQuantity)
        {
            // getting the bitwise representation of input data
            bool[] bits = RepresentAsBits(data);
            
            // decompression of pairs to two bytes where
            // the first one contains the position of zeros sequence, 
            // and the second one contains the quantity of zeros
            var pairsTotal = (ushort)((8 * data.Length) / (bitsPerPos + bitsPerZerosQuantity));
            var result = new ushort[2 * pairsTotal];
            ushort i;
            ushort j;
            ushort indBits = 0;
            for (i = 0; i < result.Length; )
            {
                for (j = bitsPerPos; j > 0; --j)
                {
                    if (bits[indBits++])
                    {
                        result[i] |= (ushort) (1 << (j - 1));
                    }
                }
                ++i;
                for (j = bitsPerZerosQuantity; j > 0; --j)
                {
                    if (bits[indBits++])
                    {
                        result[i] |= (ushort) (1 << (j - 1));
                    }
                }
                ++i;
            }

            return result;
        }
    }
}