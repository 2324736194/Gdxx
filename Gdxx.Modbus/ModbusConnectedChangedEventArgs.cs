using System;

namespace Gdxx.Modbus
{
    public class ModbusConnectedChangedEventArgs : EventArgs
    {
        public ModbusConnectedChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }

        public bool IsConnected { get; }
    }
}