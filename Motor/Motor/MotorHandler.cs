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
        public StepSize FineCaurseSelected { get; set; } = StepSize.Caurse;
        public MovePreferences RelativeAbseluteSelected { get; set; } = MovePreferences.Relative;
        private ModbusClient _connection;

        private Dictionary<StepSize, int> stepSizeDictionary = new Dictionary<StepSize, int>();

        public ModbusClient Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public int ACCL
        {
            get
            {
                int[] arrACCL = Connection.ReadHoldingRegisters(0, 2);
                return (arrACCL[1] * 65536 + arrACCL[0]);
            }
            set
            {
                int[] arrACCL = new int[2] { value / 65536, value % 65536 };
                Connection.WriteMultipleRegisters(0, arrACCL);
            }
        }
        public int DECL
        {
            get
            {
                int[] arrDECL = Connection.ReadHoldingRegisters(24, 2);
                return (arrDECL[1] * 65536 + arrDECL[0]);
            }
            set
            {
                int[] arrDECL = new int[2] { value / 65536, value % 65536 };
                Connection.WriteMultipleRegisters(24, arrDECL);
            }
        }
        public int VI
        {
            get
            {
                int[] arrVI = Connection.ReadHoldingRegisters(137, 2);// VI[1]*16^4+VI[0]
                return (arrVI[1] * 65536 + arrVI[0]);
            }
            set
            {
                int[] arrVI = new int[2] { value / 65536, value % 65536 };
                Connection.WriteMultipleRegisters(24, arrVI);
            }
        }
        public int VM
        {
            get
            {
                int[] arrVM = Connection.ReadHoldingRegisters(139, 2);
                return (arrVM[1] * 65536 + arrVM[0]);
            }
            set
            {
                int[] arrVM = new int[2] { value / 65536, value % 65536 };
                Connection.WriteMultipleRegisters(24, arrVM);
            }
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
                return Connection.ReadDiscreteInputs(0, 4);
            }
            private set { }
        }
        public int Possition
        {
            get
            {
                int[] arrPossition = Connection.ReadHoldingRegisters(87, 2);
                return (arrPossition[1] * 65536 + arrPossition[0]);
            }
            private set
            {

            }
        }


        public int[] HomeAbselutePosition { get; set; }
        public string Error
        {
            get
            {
                int[] arrError = Connection.ReadHoldingRegisters(33, 2);
                return (Enum.GetName(typeof(DataErrorEnum), arrError[0]));
            }
            private set
            {

            }
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
                return true;
            return false;
        }

        public void Disconnect()
        {
            if (Connection != null && Connection.Connected)
                Connection.Disconnect();
        }

        public void SeekHome()
        {

            while (!DiscreteParameter_Gates[0] || !DiscreteParameter_Gates[2]) //search for home by moving up
            {
                Connection.WriteMultipleRegisters(70, new int[2] { 1, 0 });
            }
            if (!DiscreteParameter_Gates[0])// reached limit+ and didnt find home seeking down
            {
                while (!DiscreteParameter_Gates[2])
                    Connection.WriteMultipleRegisters(70, new int[2] { -1, 0 });
                int[] HomeAbselutePosition = Connection.ReadHoldingRegisters(87, 2);
            }
        }
        public void MoveHome()
        {
            Connection.WriteMultipleRegisters(67, new int[2] { HomeAbselutePosition[1]* 65536+ HomeAbselutePosition[0], 0 });
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
                                    Connection.WriteMultipleRegisters(70, new int[2] { stepSizeDictionary[StepSize.Fine], 0 });
                                    break;
                                }
                            case MovePreferences.Abselute:
                                {
                                    Connection.WriteMultipleRegisters(67, new int[2] { stepSizeDictionary[StepSize.Fine], 0 });
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
                                    Connection.WriteMultipleRegisters(70, new int[2] { stepSizeDictionary[StepSize.Caurse], 0 });
                                    break;
                                }
                            case MovePreferences.Abselute:
                                {
                                    Connection.WriteMultipleRegisters(67, new int[2] { stepSizeDictionary[StepSize.Caurse], 0 });
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
                                    Connection.WriteMultipleRegisters(70, new int[2] { -1*stepSizeDictionary[StepSize.Fine], 0 });
                                    break;
                                }
                            case MovePreferences.Abselute:
                                {
                                    Connection.WriteMultipleRegisters(67, new int[2] { -1*stepSizeDictionary[StepSize.Fine], 0 });
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
                                    Connection.WriteMultipleRegisters(70, new int[2] { -1*stepSizeDictionary[StepSize.Caurse], 0 });
                                    break;
                                }
                            case MovePreferences.Abselute:
                                {
                                    Connection.WriteMultipleRegisters(67, new int[2] { -1*stepSizeDictionary[StepSize.Caurse], 0 });
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