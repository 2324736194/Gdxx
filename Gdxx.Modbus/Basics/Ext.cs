using EasyModbus;

using System;

namespace Gdxx.Modbus
{
    internal static class Ext
    {
        public static void SetValue(this ModbusServer.HoldingRegisters holdingRegisters, int start, float value)
        {
            var assistant = new ModbusAssistant();
            var src = new ushort[2];
            assistant.SetReal(src, 0, value);
            //holdingRegisters[start] = (short)src[1];
            //holdingRegisters[start + 1] = (short) src[0];
            for (int i = 0; i < src.Length; i++)
            {
                holdingRegisters[start + i] = (short) src[i];
            }
        }
    }
}