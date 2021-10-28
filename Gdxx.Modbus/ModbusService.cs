using System;
using EasyModbus;

namespace Gdxx.Modbus
{
    public sealed class ModbusService : IDisposable
    {
        private readonly ModbusServer server;
        private readonly ModbusServer.HoldingRegisters holdingRegisters;

        public ModbusService(int port = 168)
        {
            server = new ModbusServer();
            server.Port = port;
            holdingRegisters = server.holdingRegisters;
        }

        public void Start()
        {
            server.Listen();
        }

        public void Stop()
        {
            server.StopListening();
        }

        public void WriteHoldingRegisters(int index, float value)
        {
            holdingRegisters.SetValue(index, value);
        }

        public void Dispose()
        {
            server.StopListening();
        }
    }
}