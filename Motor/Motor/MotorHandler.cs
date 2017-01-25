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

    class MotorHandler
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
            set{}
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
        public int LastPosition { get; set; }
        public int HomeAbselutePosition { get; set; }
        public string Error
        {
            get
            {
                return (Enum.GetName(typeof(DataErrorEnum), EasyModbus.ModbusClient.ConvertRegistersToDouble(Connection.ReadHoldingRegisters(33, 2))));
            }
            private set { }
        }


        public MotorHandler()
        {
            Connection = new ModbusClient();
            stepSizeDictionary.Add(StepSize.Fine, int.Parse(ConfigurationManager.AppSettings["FineValue"]));
            stepSizeDictionary.Add(StepSize.Caurse, int.Parse(ConfigurationManager.AppSettings["CaurseValue"]));
        }
        public bool Connect(string ipAddress, int port)
        {
            Connection.Connect(ipAddress, port);
            if (_connection.Connected)
            {
                LastPosition = Possition;
                return true;
            }
            return false;
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
//int[] ACCL = _client.ReadHoldingRegisters(0, 2);// ACCL[1]*16^4+ACCL[0] - how fast move at start
//_client.WriteMultipleRegisters(0, new int[2] { 34464, 1 }); //100000= 16^4*1+34464
//int[] DECL = _client.ReadHoldingRegisters(24, 2);// DECL[1]*16^4+DECL[0] - how fast to move at end
//_client.WriteMultipleRegisters(24, new int[2] { 34464, 1 }); //100000= 16^4*1+34464
//int[] VI = _client.ReadHoldingRegisters(137, 2);// VI[1]*16^4+VI[0]
//int[] VM = _client.ReadHoldingRegisters(139, 2);//?
//int[] MV = _client.ReadHoldingRegisters(77, 1);
//int[] Position = _client.ReadHoldingRegisters(87, 2);// Position[1]*16^4+Position[0]
//int[] V = _client.ReadHoldingRegisters(133, 2);
//int[] Error = _client.ReadHoldingRegisters(33, 2);
//Enum.Parse(typeof(DataErrorEnum), Error[0].ToString());