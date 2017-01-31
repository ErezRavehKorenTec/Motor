using Motor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Motor
{
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public String sb = string.Empty;
    }
    public abstract class FOABase : IFOA
    {
        protected static string INIT_LIMIT_HOME_PARAM = "'[PROGRAMS]\rIs = 1,3,0\rIs = 2,2,0\rIs = 3,1,0\rLm = 1\r'[END]";
        
        protected static string READ_MV = "'[PROGRAMS]\rPR MV\r'[END]";
        protected static string READ_POSITION = "'[PROGRAMS]\rPR P\r'[END]";
        protected static string READ_PART_NUMBER = "'[PROGRAMS]\rPR PN\r'[END]";
        protected static string READ_SERIAL_NUMBER = "'[PROGRAMS]\rPR SN\r'[END]";
        protected static string READ_FIRMWARE_VERSION = "'[PROGRAMS]\rPR VR\r'[END]";
        protected static string READ_ERROR = "'[PROGRAMS]\rPR ER\r'[END]";
        protected static string READ_VI = "'[PROGRAMS]\rPR VI\r'[END]";
        protected static string READ_VM = "'[PROGRAMS]\rPR VM\r'[END]";
        protected static string READ_ACCL = "'[PROGRAMS]\rPR A\r'[END]";
        protected static string READ_DECL = "'[PROGRAMS]\rPR D\r'[END]";
        protected static string SET_ENCODER = "'[PROGRAMS]\rEE = ?\r'[END]";
        protected static string SET_POSITION = "'[PROGRAMS]\rP=?\r'[END]";
        protected static string SET_ERROR_TO_ZERO = "'[PROGRAMS]\rER=0\r'[END]";
        protected static string SET_VI = "'[PROGRAMS]\rVI=?\r'[END]";
        protected static string SET_VM = "'[PROGRAMS]\rVM=?\r'[END]";
        protected static string SET_ACCL = "'[PROGRAMS]\rA=?\r'[END]";
        protected static string SET_DECL = "'[PROGRAMS]\rD=?\r'[END]";
        protected static string SEEK_HOME = "'[PROGRAMS]\rHM=1\r'[END]";
        protected static string MOVE_RELATIVE = "'[PROGRAMS]\rMR ?\rH\r'[END]";
        protected static string MOVE_ABSOLUTE = "'[PROGRAMS]\rMA ?\rH\r'[END]";
        
        //protected static byte[] SET_EMERGENCY_STOP_TO_ESC = "'[PROGRAMS]\rES= 1\r'[END]";
        //protected static byte[] EMERGENCY_STOP_ESC_COMMAND = BitConverter.GetBytes(27);
        //protected static ManualResetEvent receiveDone = new ManualResetEvent(false);
        //protected static ManualResetEvent connectDone = new ManualResetEvent(false);
        //protected static ManualResetEvent sendDone = new ManualResetEvent(false);
        public FOABase(string address) : base(address)
        {}
        abstract public override bool InitFOAConnection(string address); //set address in InstrumentationBase
        abstract public override void MoveAbsolute(Direction direction, StepSize stepsize);
        abstract public override void MoveRelative( Direction direction, StepSize stepsize);
        abstract public override void MoveHome();

    }
}
