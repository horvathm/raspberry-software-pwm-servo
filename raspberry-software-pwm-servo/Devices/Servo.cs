using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Pwm;

namespace raspberry_software_pwm_servo.Devices
{
    /// <summary>
    /// Laggos, addig nem tokeletes
    /// </summary>
    class Servo : IDisposable, INotifyPropertyChanged
    {
        #region Fields
        public readonly int PIN_NUMBER;
        public readonly int FREQUENCY;
        public readonly int SIGNAL_DURATION;
        public readonly double MIN_PULSE_WIDTH;
        public readonly double MIDDLE_PULSE_WIDTH;
        public readonly double MAX_PULSE_WIDTH;
        public readonly int MAX_ANGLE;

        PwmPin pin;
        PwmController controller;
        Timer t;

        private Object lockObject = new Object();
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AutoFollow {
            get { return autoFollow; }
            set { Set(ref autoFollow, value); }
        }
        private bool autoFollow = true;

        /// <summary>
        /// 
        /// </summary>
        public int DesiredAngle
        {
            get
            {
                return desiredAngle;
            }
            set
            {
                if (value < 0 || value > MAX_ANGLE)
                    throw new ArgumentException("The angle of the servo must be between 0 and MAX_ANGLE");

                if (value == 0)
                    desiredPulseWidth = MIN_PULSE_WIDTH;
                else
                    desiredPulseWidth = MIN_PULSE_WIDTH + (MAX_PULSE_WIDTH - MIN_PULSE_WIDTH) / ((double)MAX_ANGLE / value);

                RaisePropertyChanged(nameof(DesiredPulseWidth));

                Set(ref desiredAngle, value);

                if(AutoFollow)
                {
                    var percentage = desiredPulseWidth / (1000.0 / FREQUENCY);
                    pin.SetActiveDutyCyclePercentage(percentage);
                }
            }
        }
        private int desiredAngle;

        /// <summary>
        /// 
        /// </summary>
        public double DesiredPulseWidth
        {
            get
            {
                return desiredPulseWidth;
            }
            set
            {
                if (value < MIN_PULSE_WIDTH || value > MAX_PULSE_WIDTH)
                    throw new ArgumentException("Pulsewidth is out of range");

                desiredAngle = (int)(((value -MIN_PULSE_WIDTH) / (MAX_PULSE_WIDTH - MIN_PULSE_WIDTH))*MAX_ANGLE);

                RaisePropertyChanged(nameof(DesiredAngle));

                Set(ref desiredPulseWidth, value);

                if (AutoFollow)
                {
                    var percentage = value / (1000.0 / FREQUENCY);
                    pin.SetActiveDutyCyclePercentage(percentage);
                }
            }
        }
        private double desiredPulseWidth;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <param name="frequency"></param>
        /// <param name="minPulseWidth"></param>
        /// <param name="maxPulseWidth"></param>
        /// <param name="maxAngle"></param>
        /// <param name="signalDuration"></param>
        public Servo(int pinNumber, int frequency = 50, double minPulseWidth = 0.7, double maxPulseWidth = 2.6, int maxAngle = 180, int signalDuration = 40)
        {
            this.PIN_NUMBER = pinNumber;
            this.FREQUENCY = frequency;
            this.MIN_PULSE_WIDTH = minPulseWidth;
            this.MAX_PULSE_WIDTH = maxPulseWidth;
            this.MAX_ANGLE = maxAngle;
            this.SIGNAL_DURATION = signalDuration;
            this.MIDDLE_PULSE_WIDTH = ((maxPulseWidth - minPulseWidth) / 2) + minPulseWidth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            if (!LightningProvider.IsLightningEnabled)
            {
                throw new Exception("Servo can only be used with Lihtning provider");
            }


            controller = (await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider()))[1];

            pin = controller.OpenPin(PIN_NUMBER);
            controller.SetDesiredFrequency(FREQUENCY);

            pin.Start();

            DesiredPulseWidth = MIDDLE_PULSE_WIDTH;

            MoveServo();
        }

        /// <summary>
        /// 
        /// </summary>
        public void MoveServo()
        {
            var percentage = desiredPulseWidth / (1000.0 / FREQUENCY);
            pin.SetActiveDutyCyclePercentage(percentage);
        }


        /// <summary>
        /// Free up our resources.
        /// </summary>
        public void Dispose()
        {
            pin.Stop();
            pin.Dispose();
            pin = null;

            t.Dispose();
            t = null;
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            // if unchanged return false
            if (Equals(storage, value))
                return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            // if PropertyChanged not null call the Invoke method
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
