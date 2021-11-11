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
            if (data.Index > 0)
            {   
                var bytes = BitConverter.GetBytes(register);
                var bits = new BitArray(bytes);
                var index = data.Index;
                if (data.IsOffset)
                {
                    index = bits.Length - index - 1;
                }

                return bits[index];
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
            var index = data.Index;
            if (index > 0)
            {
                var bytes = BitConverter.GetBytes(register);
                var bits = new BitArray(bytes);
                if (data.IsOffset)
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