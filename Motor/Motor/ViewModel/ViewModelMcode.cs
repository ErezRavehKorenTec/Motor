using Motor.RelayCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Motor.ViewModel
{
    public class ViewModelMcode : INotifyPropertyChanged
    {
        private FOA mh;
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
        public ViewModelMcode()
        {
            mh = new FOA("192.168.1.253");
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            MoveCommand = new RelayCommand(Move);
            ExitCommand = new RelayCommand(Exit);
            RCommand = new RelayCommand(Read);
            WCommand = new RelayCommand(Write);
        }

        private void ReadPositionAndError(object state)
        {
            if (mh.Connection.Connected)
            {
                PossitionString = mh.GetPosition().ToString();
                ErrorString = mh.GetError().ToString();
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
                            mh.SetAcceleration(value);
                        else
                            LogText = "bad ACCL value";
                        break;
                    }
                case "WriteDECL":
                    {
                        int value = 0;
                        if (int.TryParse(DECLString, out value))
                            mh.SetDeceleration(value);
                        else
                            LogText = "bad DECL value";
                        break;
                    }
                case "WriteVI":
                    {
                        int value = 0;
                        if (int.TryParse(VIString, out value))
                            mh.SetVI(value);
                        else
                            LogText = "bad VI value";
                        break;
                    }
                case "WriteVM":
                    {
                        int value = 0;
                        if (int.TryParse(VMString, out value))
                            mh.SetVM(value);
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
                        ACCLString = mh.GetAcceleration().ToString();
                        OnPropertyChange("ACCLString");
                        break;
                    }
                case "ReadDECL":
                    {
                        DECLString = mh.GetDecelerationn().ToString();
                        OnPropertyChange("DECLString");
                        break;
                    }
                case "ReadVI":
                    {
                        VIString = mh.GetVI().ToString();
                        OnPropertyChange("VIString");
                        break;
                    }
                case "ReadVM":
                    {
                        VMString = mh.GetVM().ToString();
                        OnPropertyChange("VMString");
                        break;
                    }
                case "ReadSlew":
                    {
                        //value = mh.ReadSlew;
                        break;
                    }
                case "ReadERAndP":
                    {
                        ReadPositionAndError(null);
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
                        ReadPositionAndError(null);
                        break;
                    }
                case "Home":
                    {
                        mh.MoveHome();
                        ReadPositionAndError(null);
                        break;
                    }
                case "Up":
                    {
                        if (!RelativeAbsolateToggleButtonState)
                        {
                            if (FineCaurseToggleButtonState)
                                mh.MoveRelative(Enums.Direction.Positive, Enums.StepSize.Fine);
                            else
                                mh.MoveRelative(Enums.Direction.Positive, Enums.StepSize.Caurse);
                        }
                        else
                        {
                            if (FineCaurseToggleButtonState)
                                mh.MoveAbsolute(Enums.Direction.Positive, Enums.StepSize.Fine);
                            else
                                mh.MoveAbsolute(Enums.Direction.Positive, Enums.StepSize.Caurse);
                        }
                        ReadPositionAndError(null);
                        break;
                    }
                case "Down":
                    {
                        if (!RelativeAbsolateToggleButtonState)
                        {
                            if (FineCaurseToggleButtonState)
                                mh.MoveRelative(Enums.Direction.Negative, Enums.StepSize.Fine);
                            else
                                mh.MoveRelative(Enums.Direction.Negative, Enums.StepSize.Caurse);
                        }
                        else
                        {
                            if (FineCaurseToggleButtonState)
                                mh.MoveAbsolute(Enums.Direction.Negative, Enums.StepSize.Fine);
                            else
                                mh.MoveAbsolute(Enums.Direction.Negative, Enums.StepSize.Caurse);
                        }
                        ReadPositionAndError(null);
                        break;
                    }
                default:
                    break;
            }
        }

        private void Disconnect(object obj)
        {
            if (mh != null && mh.Connection.Connected)
                mh.Discconect();
            IsConnectionSucceded = false;
            OnPropertyChange("IsConnectionSucceded");
        }

        private void Connect(object obj)
        {
            IsConnectionSucceded = mh.InitFOAConnection("192.168.1.253");
            OnPropertyChange("IsConnectionSucceded");
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
