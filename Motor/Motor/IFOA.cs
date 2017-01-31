using Motor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motor
{
    public abstract class IFOA
    {
        private string ipAddress;
        private int port;

        public IFOA(string ipAddress)
        {
            this.ipAddress = ipAddress;
        }

        abstract public bool InitFOAConnection(string address);
        abstract public void Discconect();
        abstract public void MoveHome();
        abstract public void MoveRelative(Direction direction,StepSize stepsize);
        abstract public void MoveAbsolute( Direction direction, StepSize stepsize);
        abstract public void SeekHome();
        abstract public void SetPositionToZero();
        abstract public double GetPosition();
        abstract public void SetAcceleration(int acceleration);
        abstract public double GetAcceleration();
        abstract public void SetDeceleration(int deceleration);
        abstract public double GetDecelerationn();
        abstract public void SetVM(int vm);
        abstract public double GetVM();
        abstract public void SetVI(int vi);
        abstract public double GetVI();
        abstract public void SetEncoderEnable(bool isenable);
        abstract public double GetError();
        abstract public double GetMV();
    }
}
