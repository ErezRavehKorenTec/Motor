using EasyModbus;
using Motor.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motor
{

    class MotorHandlerModbus
    {
        private ModbusClient _connection;
        private Dictionary<StepSize, int> stepSizeDictionary = new Dictionary<StepSize, int>();
        public Direction LastMovementDirection { get; set; } = Direction.Positive;
        public StepSize FineCaurseSelected { get; set; } = StepSize.Caurse;
        public MovePreferences RelativeAbseluteSelected { get; set; } = MovePreferences.Relative;
        public ModbusClient Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }
        public int ACCL
        {
            get
            {
                return (EasyModbus.ModbusClient.ConvertRegistersToDouble(Connection.ReadHoldingRegisters(0, 2)));
            }
            set
            {
                Connection.WriteMultipleRegisters(0, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(value));
            }
        }
        public int DECL
        {
            get
            {
                return (EasyModbus.ModbusClient.ConvertRegistersToDouble(Connection.ReadHoldingRegisters(24, 2)));
            }
            set
            {
                Connection.WriteMultipleRegisters(137, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(value));
            }
        }
        public int VI
        {
            get
            {
                return (EasyModbus.ModbusClient.ConvertRegistersToDouble(Connection.ReadHoldingRegisters(137, 2)));
            }
            set
            {
                Connection.WriteMultipleRegisters(24, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(value));
            }
        }
        public int VM
        {
            get
            {
                return (EasyModbus.ModbusClient.ConvertRegistersToDouble(Connection.ReadHoldingRegisters(139, 2)));
            }
            set
            {
                Connection.WriteMultipleRegisters(139, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(value));
            }
        }
        public int MV //is mottor in movment
        {
            get
            {
                return Connection.ReadHoldingRegisters(74, 1)[0];
            }
            set { }
        }
        //public int Slew
        //{
        //    get { }
        //    set { }
        //}
        public bool[] DiscreteParameter_Gates
        {
            get
            {
                /*
                 * Discrete input
                 * 0-Limit+
                 * 1-Limit-
                 * 2-Home
                 */
                return Connection.ReadDiscreteInputs(0, 3);
            }
            private set { }
        }
        public int Possition
        {
            get
            {
                return (EasyModbus.ModbusClient.ConvertRegistersToDouble(Connection.ReadHoldingRegisters(87, 2)));
            }
            private set { }
        }
        //public int LastPosition { get; set; }
        public int HomeAbselutePosition { get; set; }
        public string Error
        {
            get
            {
                return (Enum.GetName(typeof(DataErrorEnum), EasyModbus.ModbusClient.ConvertRegistersToDouble(Connection.ReadHoldingRegisters(33, 2))));
            }
            private set { }
        }
        public bool LimitPlusReached { get; private set; }
        public bool LimitMinusReached { get; private set; }

        public MotorHandlerModbus()
        {
            Connection = new ModbusClient();
            stepSizeDictionary.Add(StepSize.Fine, int.Parse(ConfigurationManager.AppSettings["FineValue"]));
            stepSizeDictionary.Add(StepSize.Caurse, int.Parse(ConfigurationManager.AppSettings["CaurseValue"]));
            Task aa = Task.Factory.StartNew(MonitorBoolArray);
        }
        public bool Connect(string ipAddress, int port)
        {
            Connection.Connect(ipAddress, port);
            if (_connection.Connected)
            {
                //Connection.receiveDataChanged += Connection_receiveDataChanged;
                //LastPosition = Possition;
                return true;
            }
            return false;
        }

        private void MonitorBoolArray()
        {
            //DiscreteParameter_Gates.Contains(true);
            while (true)
            {
                if (Connection.Connected == false) continue;
                bool[] x = DiscreteParameter_Gates;
                if (!x[0])
                {
                    LimitPlusReached = true;
                }
                else if(!x[1])
                {
                    LimitMinusReached = true;
                }
                else
                {
                    LimitMinusReached = LimitPlusReached = false;
                }
            }
        }
        public void Disconnect()
        {
            if (Connection != null && Connection.Connected)
                Connection.Disconnect();
        }

        public void SeekHome()
        {
            while (DiscreteParameter_Gates[0] != false && DiscreteParameter_Gates[2] != false) //search for home by moving up
            {
                Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(10));
                while (MV == 1)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
            if (DiscreteParameter_Gates[2] != false)// reached limit+ and didnt find home seeking down
            {
                while (DiscreteParameter_Gates[2] != false)
                {
                    Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(-10));
                    while (MV == 1)
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
            HomeAbselutePosition = EasyModbus.ModbusClient.ConvertRegistersToDouble(Connection.ReadHoldingRegisters(87, 2));
        }
        public void MoveHome()
        {
            Connection.WriteMultipleRegisters(67, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(HomeAbselutePosition));
        }
        public void MoveUP()
        {
            if (LimitPlusReached) return;
            switch (FineCaurseSelected)
            {
                case StepSize.Fine:
                    {
                        switch (RelativeAbseluteSelected)
                        {
                            case MovePreferences.Relative:
                                {
                                    Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(stepSizeDictionary[StepSize.Fine]));
                                    while (MV == 1)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    //if (LastMovementDirection == Direction.Negative)
                                    //{
                                    //    int antiBacklash = LastPosition + stepSizeDictionary[StepSize.Fine] - Possition;
                                    //    if (antiBacklash != 0)
                                    //        Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(antiBacklash));
                                    //    LastMovementDirection = Direction.Positive;
                                    //}
                                    //LastPosition = Possition;
                                    break;
                                }
                            case MovePreferences.Abselute:
                                {
                                    Connection.WriteMultipleRegisters(67, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(stepSizeDictionary[StepSize.Fine]));
                                    while (MV == 1)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    //if (LastMovementDirection == Direction.Negative)
                                    //{
                                    //    int antiBacklash = LastPosition + stepSizeDictionary[StepSize.Fine] - Possition;
                                    //    if (antiBacklash != 0)
                                    //        Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(antiBacklash));
                                    //    LastMovementDirection = Direction.Positive;
                                    //}
                                    //LastPosition = Possition;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case StepSize.Caurse:
                    {
                        switch (RelativeAbseluteSelected)
                        {
                            case MovePreferences.Relative:
                                {
                                    Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(stepSizeDictionary[StepSize.Caurse]));
                                    while (MV == 1)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    //if (LastMovementDirection == Direction.Negative)
                                    //{
                                    //    int antiBacklash = LastPosition +stepSizeDictionary[StepSize.Caurse] - Possition;
                                    //    if (antiBacklash != 0)
                                    //        Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(antiBacklash));
                                    //    LastMovementDirection = Direction.Positive;
                                    //}
                                    //LastPosition = Possition;
                                    break;
                                }
                            case MovePreferences.Abselute:
                                {
                                    Connection.WriteMultipleRegisters(67, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(stepSizeDictionary[StepSize.Caurse]));
                                    while (MV == 1)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    //if (LastMovementDirection == Direction.Negative)
                                    //{
                                    //    int antiBacklash = LastPosition + stepSizeDictionary[StepSize.Caurse] - Possition;
                                    //    if (antiBacklash != 0)
                                    //        Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(antiBacklash));
                                    //    LastMovementDirection = Direction.Positive;
                                    //}
                                    //LastPosition = Possition;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                default:
                    break;
            }
        }
        public void MoveDown()
        {
            if (LimitMinusReached) return;
            switch (FineCaurseSelected)
            {
                case StepSize.Fine:
                    {
                        switch (RelativeAbseluteSelected)
                        {
                            case MovePreferences.Relative:
                                {
                                    Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(-1 * stepSizeDictionary[StepSize.Fine]));
                                    while (MV == 1)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    //if (LastMovementDirection == Direction.Positive)
                                    //{
                                    //    int antiBacklash = LastPosition - stepSizeDictionary[StepSize.Fine] - Possition;
                                    //    if (antiBacklash != 0)
                                    //        Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(antiBacklash));
                                    //    LastMovementDirection = Direction.Negative;
                                    //}
                                    //LastPosition = Possition;
                                    break;
                                }
                            case MovePreferences.Abselute:
                                {
                                    Connection.WriteMultipleRegisters(67, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(-1 * stepSizeDictionary[StepSize.Fine]));
                                    while (MV == 1)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }

                                    //if (LastMovementDirection == Direction.Positive)
                                    //{
                                    //    int antiBacklash = LastPosition - stepSizeDictionary[StepSize.Fine] - Possition;
                                    //    if (antiBacklash != 0)
                                    //        Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(antiBacklash));
                                    //    LastMovementDirection = Direction.Negative;
                                    //}
                                    //LastPosition = Possition;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case StepSize.Caurse:
                    {
                        switch (RelativeAbseluteSelected)
                        {
                            case MovePreferences.Relative:
                                {
                                    Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(-1 * stepSizeDictionary[StepSize.Caurse]));
                                    while (MV == 1)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    //if (LastMovementDirection == Direction.Positive)
                                    //{
                                    //    int antiBacklash = LastPosition - stepSizeDictionary[StepSize.Caurse] - Possition;
                                    //    if (antiBacklash != 0)
                                    //        Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(antiBacklash));
                                    //    LastMovementDirection = Direction.Negative;
                                    //}
                                    //LastPosition = Possition;
                                    break;
                                }
                            case MovePreferences.Abselute:
                                {
                                    Connection.WriteMultipleRegisters(67, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(-1 * stepSizeDictionary[StepSize.Caurse]));
                                    while (MV == 1)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    //if (LastMovementDirection == Direction.Positive)
                                    //{
                                    //    int antiBacklash = LastPosition - stepSizeDictionary[StepSize.Caurse] - Possition;
                                    //    if (antiBacklash != 0)
                                    //        Connection.WriteMultipleRegisters(70, EasyModbus.ModbusClient.ConvertDoubleToTwoRegisters(antiBacklash));
                                    //    LastMovementDirection = Direction.Negative;
                                    //}
                                    //LastPosition = Possition;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }
}