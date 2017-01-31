using Motor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Motor
{

    class FOA : FOABase
    {
        public string SerialNumber { get; set; }
        public string PartNumber { get; set; }
        public string FirmWareVersion { get; set; }
        public Socket Connection { get; set; }
        public double DataRecive { get; set; }
        public string SpecData { get; set; }
        //public double MV { get; set; }
        private double _mv=1;

        public double MV
        {
            get { return _mv; }
            set { _mv = value; }
        }

        private static volatile int dldnow;
        private static Timer _MVTimer;


        #region Constructor
        public FOA(string address) : base(address)
        {
            //IpAddress = address;
        }
        #endregion


        public override void Discconect()
        {
            if (Connection != null && Connection.Connected)
            {
                Connection.Disconnect(true);
            }
        }
        public override bool InitFOAConnection(string address)
        {
            try
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(address), 503);
                Connection = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Connection.Connect(endpoint);
                InitialMotorParams();//if success write init successful
                SetErrorMessageToZero();
                WriteMotorSpecToLog();
                //_MVTimer = new Timer(new TimerCallback(GetVMByTime), dldnow,100,100);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SetErrorMessageToZero()
        {
            Connection.Send(Encoding.ASCII.GetBytes(SET_ERROR_TO_ZERO));
        }

        private void GetVMByTime(object state)
        {
            _MVTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            GetMV();
            _MVTimer.Change(100, 100);
        }

        private void WriteMotorSpecToLog()
        {
            Connection.Send(Encoding.ASCII.GetBytes(READ_SERIAL_NUMBER));
            ReceiveData();
            SerialNumber = SpecData;
            Connection.Send(Encoding.ASCII.GetBytes(READ_PART_NUMBER));
            ReceiveData();
            PartNumber = SpecData;
            Connection.Send(Encoding.ASCII.GetBytes(READ_FIRMWARE_VERSION));
            ReceiveData();
            FirmWareVersion = SpecData;
            //write to log
        }
        private bool InitialMotorParams()
        {
            if (Connection != null && Connection.Connected)
            {
                Connection.Send(Encoding.ASCII.GetBytes(INIT_LIMIT_HOME_PARAM));
                return true;
            }
            return false;
        }
        private void ReceiveData()
        {
            byte[] buffer = new byte[256];
            Connection.Receive(buffer);
            double temp;
            if (double.TryParse(Encoding.ASCII.GetString(buffer), out temp))
                DataRecive = temp;
            else
                SpecData = Encoding.ASCII.GetString(buffer);
        }


        public override void MoveRelative(Direction direction, StepSize stepsize)
        {
            if (Connection != null && Connection.Connected)
            {
                string _moveRelative = MOVE_RELATIVE.Replace("?", direction == Direction.Positive ? ((int)stepsize).ToString() : (-1 * (int)stepsize).ToString());
                Connection.Send(Encoding.ASCII.GetBytes(_moveRelative));
            }
        }
        public override void MoveAbsolute(Direction direction, StepSize stepsize)
        {
            string _moveAbsolute = MOVE_ABSOLUTE.Replace("?", direction == Direction.Positive ? ((int)stepsize).ToString() : (-1 * (int)stepsize).ToString());
            Connection.Send(Encoding.ASCII.GetBytes(_moveAbsolute));
        }
        public override void MoveHome()
        {
            string _moveHome = MOVE_ABSOLUTE.Replace("?", "0");
            Connection.Send(Encoding.ASCII.GetBytes(_moveHome));
        }
        public override void SeekHome()
        {
            Connection.Send(Encoding.ASCII.GetBytes(SEEK_HOME));
            SetPositionToZero();
        }
        public override void SetPositionToZero()
        {
            string _setPositionToZero = SET_POSITION.Replace("?", "0");
            Connection.Send(Encoding.ASCII.GetBytes(_setPositionToZero));
        }
        public override double GetPosition()
        {
            while (GetMV() == 1) ;
            Connection.Send(Encoding.ASCII.GetBytes(READ_POSITION));
            ReceiveData();
            return DataRecive;
        }
        public override void SetAcceleration(int acceleration)
        {
            string _accl = SET_ACCL.Replace("?", acceleration.ToString());
            Connection.Send(Encoding.ASCII.GetBytes(_accl));
        }
        public override double GetAcceleration()
        {
            Connection.Send(Encoding.ASCII.GetBytes(READ_ACCL));
            ReceiveData();
            return DataRecive;
        }
        public override void SetDeceleration(int deceleration)
        {
            string _decl = SET_DECL.Replace("?", deceleration.ToString());
            Connection.Send(Encoding.ASCII.GetBytes(_decl));
        }
        public override double GetDecelerationn()
        {
            Connection.Send(Encoding.ASCII.GetBytes(READ_DECL));
            ReceiveData();
            return DataRecive;
        }
        public override void SetVM(int vm)
        {
            string _vm = SET_VM.Replace("?", vm.ToString());
            Connection.Send(Encoding.ASCII.GetBytes(_vm));
        }
        public override double GetVM()
        {
            Connection.Send(Encoding.ASCII.GetBytes(READ_VM));
            ReceiveData();
            return DataRecive;
        }
        public override void SetVI(int vi)
        {
            string _vi = SET_VI.Replace("?", vi.ToString());
            Connection.Send(Encoding.ASCII.GetBytes(_vi));
        }
        public override double GetVI()
        {
            Connection.Send(Encoding.ASCII.GetBytes(READ_VI));
            ReceiveData();
            return DataRecive;
        }

        public override double GetError()
        {
            Connection.Send(Encoding.ASCII.GetBytes(READ_ERROR));
            ReceiveData();
            return DataRecive;
        }
        public override void SetEncoderEnable(bool isenable)
        {
            string _ee = SET_ENCODER.Replace("?", isenable==true?"1":"0");
            Connection.Send(Encoding.ASCII.GetBytes(_ee));
        }
        public override double GetMV()
        {
            byte[] buffer = new byte[256];
            Connection.Send(Encoding.ASCII.GetBytes(READ_MV));
            Connection.Receive(buffer);
            MV= double.Parse(Encoding.ASCII.GetString(buffer));
            return MV;
        }
    }
}
