using System.Collections.Generic;

namespace Gdxx.Modbus
{
    public delegate IEnumerable<T> ModbusReadHandler<T>(int start, int quantity);
}