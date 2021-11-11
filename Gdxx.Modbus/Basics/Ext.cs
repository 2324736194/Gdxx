using EasyModbus;

using System;
using System.Collections;

namespace Gdxx.Modbus
{
    internal static class Ext
    {
     

        /// <summary>
        /// 更改寄存器数据
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static short RegisterChanged(this short register, bool value, int index, bool offset)
        {   
            if (index > 0)
            {
                var bytes = BitConverter.GetBytes(register);
                var bits = new BitArray(bytes);
                if (offset)
                {
                    index = bits.Length - index - 1;
                }

                bits[index] = value;
                bytes = Convert(bits);
                register = BitConverter.ToInt16(bytes, 0);
            }
            else
            {
                register = (short)(value ? 1 : 0);
            }

            return register;
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



        public static void SetValue(this ModbusServer.HoldingRegisters holdingRegisters, int start, float value, ModbusSingleFormat format)
        {
            var source = BitConverter.GetBytes(value);
            var shortArrary = new ushort[2];
            switch (format)
            {
                case ModbusSingleFormat.ABCD:
                    shortArrary[0] = GetUShort(source[0], source[1]);
                    shortArrary[1] = GetUShort(source[2], source[3]);
                    break;
                case ModbusSingleFormat.BADC:
                    shortArrary[0] = GetUShort(source[1], source[0]);
                    shortArrary[1] = GetUShort(source[3], source[2]);
                    break;
                case ModbusSingleFormat.CDAB:
                    shortArrary[0] = GetUShort(source[2], source[3]); 
                    shortArrary[1] = GetUShort(source[0], source[1]);
                    break;
                case ModbusSingleFormat.DCBA:
                    shortArrary[0] = GetUShort(source[3], source[2]);
                    shortArrary[1] = GetUShort(source[1], source[0]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
            for (int i = 0; i < shortArrary.Length; i++)
            {
                holdingRegisters[start + i] = (short)shortArrary[i];
            }
        }

        public static float GetFloat(this ModbusServer.HoldingRegisters holdingRegisters, int start, ModbusSingleFormat format)
        {
            var shortArrary = new ushort[2];
            for (int i = 0; i < shortArrary.Length; i++)
            {
                shortArrary[i] = (ushort) holdingRegisters[start + i];
            }

            var source = new byte[4];
            var bytes1 = BitConverter.GetBytes(holdingRegisters[start]);
            var bytes2 = BitConverter.GetBytes(holdingRegisters[start + 1]);
            switch (format)
            {
                case ModbusSingleFormat.ABCD:
                    source[0] = bytes1[0];
                    source[1] = bytes1[1];
                    source[2] = bytes2[0];
                    source[3] = bytes2[1];
                    break;
                case ModbusSingleFormat.BADC:
                    source[0] = bytes1[1];
                    source[1] = bytes1[0];
                    source[2] = bytes2[1];
                    source[3] = bytes2[0];
                    break;
                case ModbusSingleFormat.CDAB:
                    source[0] = bytes2[0];
                    source[1] = bytes2[1];
                    source[2] = bytes1[0];
                    source[3] = bytes1[1];
                    break;
                case ModbusSingleFormat.DCBA:
                    source[0] = bytes2[1];
                    source[1] = bytes2[0];
                    source[2] = bytes1[1];
                    source[3] = bytes1[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            var result = BitConverter.ToSingle(source, 0);
            return result;
        }

        private static ushort GetUShort(params byte[] source)
        {
            return BitConverter.ToUInt16(source, 0);
        }
    }
}