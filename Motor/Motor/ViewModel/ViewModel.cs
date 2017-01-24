using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Motor.RelayCommands;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using EasyModbus;
using Motor.Enums;

namespace Motor.ViewModel
{
    public class ViewModel : INotifyPropertyChanged
    {
        Timer getPositionAndError;
        private MotorHandler mh;
        public event PropertyChangedEventHandler PropertyChanged;
        private String _logString = string.Empty;

        private bool _isConnectionSucceded = false;
        public bool IsConnectionSucceded
        {
            get
            {
                return _isConnectionSucceded;
            }
            set
            {
                _isConnectionSucceded = value;
            }
        }

        private bool _fineCaurseState;
        public bool FineCaurseToggleButtonState
        {
            get
            {
                return _fineCaurseState;
            }
            set
            {
                _fineCaurseState = value;
                if (_fineCaurseState == false)
                    mh.FineCaurseSelected = StepSize.Caurse;
                else
                    mh.FineCaurseSelected = StepSize.Fine;

            }
        }

        private bool _relativeAbsolateState;
        public bool RelativeAbsolateToggleButtonState
        {
            get
            {
                return _relativeAbsolateState;
            }
            set
            {
                _relativeAbsolateState = value;
                if (_relativeAbsolateState == false)
                    mh.RelativeAbseluteSelected = MovePreferences.Relative;
                else
                    mh.RelativeAbseluteSelected = MovePreferences.Abselute;

            }
        }

        public string ACCLString { get; set; } = "1000000";
        public string DECLString { get; set; } = "1000000";
        public string VIString { get; set; } = "1000";
        public string VMString { get; set; } = "768000";
        public string SlewString { get; set; } = "1000";
        public string PossitionString { get; set; } = "0";
        public string ErrorString { get; set; } = "No Error";
        #region Ctor
        public ViewModel()
        {
            mh = new MotorHandler();
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            MoveCommand = new RelayCommand(Move);
            ExitCommand = new RelayCommand(Exit);
            RCommand = new RelayCommand(Read);
            WCommand = new RelayCommand(Write);
            getPositionAndError = new Timer(new TimerCallback(ReadPositionAndError), null, 3000, 3000);
        }

        private void ReadPositionAndError(object state)
        {
            if (mh.Connection.Connected)
            {
                PossitionString = mh.Possition.ToString();
                ErrorString = mh.Error;
                OnPropertyChange("PossitionString");
                OnPropertyChange("ErrorString");
            }
        }

        #endregion


        #region Icommand Var

        private ICommand _clickConnect;
        private ICommand _clickDisconnect;
        private ICommand _clickMove;
        private ICommand _clickExit;
        private ICommand _clickR;
        private ICommand _clickW;
        #endregion

        #region Icommand Properties
        public ICommand ConnectCommand
        {
            get { return _clickConnect; }
            set { _clickConnect = value; }
        }
        public ICommand DisconnectCommand
        {
            get { return _clickDisconnect; }
            set { _clickDisconnect = value; }
        }
        public ICommand MoveCommand
        {
            get { return _clickMove; }
            set { _clickMove = value; }
        }
        public ICommand ExitCommand
        {
            get { return _clickExit; }
            set { _clickExit = value; }
        }
        public ICommand RCommand
        {
            get { return _clickR; }
            set { _clickR = value; }
        }
        public ICommand WCommand
        {
            get { return _clickW; }
            set { _clickW = value; }
        }
        #endregion


        private void Write(object obj)
        {
            switch ((string)obj)
            {
                case "WriteACCL":
                    {
                        int value = 0;
                        if (int.TryParse(ACCLString, out value))
                            mh.ACCL = value;
                        else
                            LogText = "bad ACCL value";
                        break;
                    }
                case "WriteDECL":
                    {
                        int value = 0;
                        if (int.TryParse(DECLString, out value))
                            mh.DECL = value;
                        else
                            LogText = "bad DECL value";
                        break;
                    }
                case "WriteVI":
                    {
                        int value = 0;
                        if (int.TryParse(VIString, out value))
                            mh.VI = value;
                        else
                            LogText = "bad VI value";
                        break;
                    }
                case "WriteVM":
                    {
                        int value = 0;
                        if (int.TryParse(VMString, out value))
                            mh.VM = value;
                        else
                            LogText = "bad VM value";
                        break;
                    }
                case "WriteSlew":
                    {
                        break;
                    }
                default:
                    break;
            }
        }
        private void Read(object obj)
        {
            switch ((string)obj)
            {
                case "ReadACCL":
                    {
                        ACCLString = mh.ACCL.ToString();
                        OnPropertyChange("ACCLString");
                        break;
                    }
                case "ReadDECL":
                    {
                        DECLString = mh.DECL.ToString();
                        OnPropertyChange("DECLString");
                        break;
                    }
                case "ReadVI":
                    {
                        VIString = mh.VI.ToString();
                        OnPropertyChange("VIString");
                        break;
                    }
                case "ReadVM":
                    {
                        VMString = mh.VM.ToString();
                        OnPropertyChange("VMString");
                        break;
                    }
                case "ReadSlew":
                    {
                        //value = mh.ReadSlew;
                        break;
                    }
                default:
                    break;
            }

        }
        private void Exit(object obj)
        {
            Disconnect(null);
            Application.Current.MainWindow.Close();
        }

        private void Move(object obj)
        {
            switch ((string)obj)
            {
                case "SeekHome":
                    {
                        mh.SeekHome();
                        break;
                    }
                case "Home":
                    {
                        mh.MoveHome();
                        break;
                    }
                case "Up":
                    {
                        mh.MoveUP();
                        break;
                    }
                case "Down":
                    {
                        mh.MoveDown();
                        break;
                    }
                default:
                    break;
            }
        }

        private void Disconnect(object obj)
        {
            if (mh != null && mh.Connection.Connected)
                mh.Disconnect();
            IsConnectionSucceded = false;
            OnPropertyChange("IsConnectionSucceded");
        }

        private void Connect(object obj)
        {
            IsConnectionSucceded = mh.Connect("192.168.1.253", 502);
            OnPropertyChange("IsConnectionSucceded");
            /*
             * Discrete input
             * 0-Limit+
             * 1-Limit-
             * 2-Home
             * 
             */
            //_client = new ModbusClient();
            //_client.Connect("192.168.1.253", 502);
            //if (_client.Connected)
            //bool[] currentGate = _client.ReadDiscreteInputs(0, 4);
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

            //_client.WriteMultipleRegisters(67, new int[2] { 50000, 0 });//Move Absolute
            //_client.WriteMultipleRegisters(70, new int[2] { 50000, 0 });//Move Relative
            //_client.WriteMultipleRegisters(70, new int[2] { 50000, 0 });//Move Relative
            //_client.WriteMultipleRegisters(70, new int[2] { 50000, 0 });//Move Relative
            //int[] Position1 = _client.ReadHoldingRegisters(87, 2);// Position[1]*16^4+Position[0]
        }
        public string LogText
        {
            get { return _logString; }
            set
            {
                if (value == string.Empty)
                    _logString = string.Empty;
                else if (_logString == string.Empty)
                    _logString += value;
                else
                    _logString += Environment.NewLine + value;
                OnPropertyChange("LogText");
            }
        }


        private void OnPropertyChange(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }
    }
}
