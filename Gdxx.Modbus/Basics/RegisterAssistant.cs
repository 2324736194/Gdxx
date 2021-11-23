using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gdxx.Modbus
{
    /// <summary>
    /// modbus 寄存器助手
    /// </summary>
    internal class RegisterAssistant
    {
        private readonly short[] registers;
        private readonly uint start;

        public RegisterAssistant(IEnumerable<short> registers, int start)
        {
            if (null == registers)
            {
                throw new ArgumentNullException(nameof(registers));
            }
            this.registers = registers.ToArray();
            this.start = (uint)start;
        }

        public object GetValue(IModbusData data)
        {
            switch (data)
            {
                case ModbusBoolean boolean:
                    return GetValue(boolean);
                case ModbusSingle single:
                    return GetValue(single);
                case ModbusInt32 int32:
                    return GetValue(int32);
                default:
                    throw new ArgumentOutOfRangeException(nameof(data));
            }
        }

        public int GetValue(ModbusInt32 data)
        {
            var address = data.DataAddress - start;
            var register = registers[address];
            return register;
        }

        public bool GetValue(ModbusBoolean data)
        {
            var address = data.DataAddress - start;
            var register = registers[address];
            if (data.IsEnabledSplit)
            {
                var source = ToBinary(register);
                Sort(ref source);
                
                if (data.IsOffset)
                {
                    Inversion(ref source);
                }
                return source[data.Index];
            }

            switch (register)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 倒序
        /// </summary>
        /// <param name="source"></param>
        private static void Sort(ref bool[] source)
        {
            var result = new bool[source.Length];
            // 倒置顺序
            for (int i = 0; i < source.Length; i++)
            {
                var index = source.Length - i - 1;
                result[i] = source[index];
            }
            source = result;
        }

        /// <summary>
        /// 倒置数据
        /// </summary>
        /// <param name="source"></param>
        private static void Inversion(ref bool[] source)
        {
            var result = new bool[source.Length];
            for (int i = 0; i < 8; i++)
            {
                result[i] = source[i + 8];
            }

            for (int i = 0; i < 8; i++)
            {
                result[i + 8] = source[i];
            }

            source = result;
        }

        private static bool[] ToBinary(short register)
        {
            return System.Convert.ToString(register, 2)
                .PadLeft(16, '0')
                .ToCharArray()
                .Select(p =>
                {
                    var str = p.ToString();
                    var i = int.Parse(str);
                    var b = System.Convert.ToBoolean(i);
                    return b;
                })
                .ToArray();
        }

        public float GetValue(ModbusSingle data)
        {
            var address = data.DataAddress - start;
            var arrary = new short[2];
            for (var i = 0; i < arrary.Length; i++)
            {
                arrary[i] = registers[address + i];
            }
            var source = arrary.Select(p => BitConverter.GetBytes(p)).ToArray();
            var bytes = new byte[4];
            switch (data.SingleFormat)
            {
                case ModbusSingleFormat.ABCD:
                    bytes[0] = source[0][0];
                    bytes[1] = source[0][1];
                    bytes[2] = source[1][0];
                    bytes[3] = source[1][1];
                    break;
                case ModbusSingleFormat.BADC:
                    bytes[0] = source[0][1];
                    bytes[1] = source[0][0];
                    bytes[2] = source[1][1];
                    bytes[3] = source[1][0];
                    break;
                case ModbusSingleFormat.CDAB:
                    bytes[0] = source[1][0];
                    bytes[1] = source[1][1];
                    bytes[2] = source[0][0];
                    bytes[3] = source[0][1];
                    break;
                case ModbusSingleFormat.DCBA:
                    bytes[0] = source[1][1];
                    bytes[1] = source[1][0];
                    bytes[2] = source[0][1];
                    bytes[3] = source[0][0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // 高低位对调
            var result = new byte[4];
            result[0] = bytes[2];
            result[1] = bytes[3];
            result[2] = bytes[0];
            result[3] = bytes[1];
            return BitConverter.ToSingle(result, 0);
        }

        public static void Changed(ref short register, ModbusBoolean data, bool value)
        {
            if (data.IsEnabledSplit)
            {
                var source = ToBinary(register);
                // 倒置
                if (data.IsOffset)
                {
                    Inversion(ref source);
                }

                Sort(ref source);
                source[data.Index] = value;
                // 还原
                if (data.IsOffset)
                {
                    Sort(ref source);
                    Inversion(ref source);
                }

                var binary = source
                    .Select(System.Convert.ToInt16)
                    .ToArray();
                var str = string.Join(string.Empty, binary);
                register = System.Convert.ToInt16(str, 2);
                //var 
                //var 
                //if (data.IsOffset)
                //{
                //    index = bits.Length - index - 1;
                //}   

                //bits[index] = value;
                //bytes = Convert(bits);
                //register = BitConverter.ToInt16(bytes, 0);

            }
            else
            {
                register = (short)(value ? 1 : 0);
            }
        }

        public static void Changed(ref int[] registers, ModbusSingle data, float value)
        {
            var source = registers.Select(p => (short)p).ToArray();
            Changed(ref source, data, value);
            for (int i = 0; i < registers.Length; i++)
            {
                registers[i] = source[i];
            }
        }

        public static void Changed(ref short[] registers, ModbusSingle data, float value)
        {
            Changed(ref registers, data.SingleFormat, value);
        }

        public static void Changed(ref short[] registers, ModbusSingleFormat format, float value)
        {
            var bytes = BitConverter.GetBytes(value);
            var write = new byte[4];
            // 高低位对调
            write[0] = bytes[2];
            write[1] = bytes[3];
            write[2] = bytes[0];
            write[3] = bytes[1];
            var result = new Dictionary<int, byte[]>();
            for (var i = 0; i < 2; i++)
            {
                result.Add(i, new byte[2]);
            }

            // 格式化数据
            switch (format)
            {
                case ModbusSingleFormat.ABCD:
                    result[0][0] = write[0];
                    result[0][1] = write[1];
                    result[1][0] = write[2];
                    result[1][1] = write[3];
                    break;
                case ModbusSingleFormat.BADC:
                    result[0][0] = write[1];
                    result[0][1] = write[0];
                    result[1][0] = write[3];
                    result[1][1] = write[2];
                    break;
                case ModbusSingleFormat.CDAB:
                    result[0][0] = write[2];
                    result[0][1] = write[3];
                    result[1][0] = write[0];
                    result[1][1] = write[1];
                    break;
                case ModbusSingleFormat.DCBA:
                    result[0][0] = write[3];
                    result[0][1] = write[2];
                    result[1][0] = write[1];
                    result[1][1] = write[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //
            for (var i = 0; i < registers.Length; i++)
            {
                registers[i] = BitConverter.ToInt16(result[i], 0);
            }
        }

        private static byte[] Convert(BitArray bits)
        {
            const int BYTE = 8;
            var length = (bits.Count / BYTE) + ((bits.Count % BYTE == 0) ? 0 : 1);
            var bytes = new byte[length];
            for (var i = 0; i < bits.Length; i++)
            {
                var bitIndex = i % BYTE;
                var byteIndex = i / BYTE;
                var mask = (bits[i] ? 1 : 0) << bitIndex;
                bytes[byteIndex] |= (byte)mask;
            }

            return bytes;
        }
    }
}