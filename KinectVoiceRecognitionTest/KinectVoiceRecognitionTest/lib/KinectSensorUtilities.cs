using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace KinectVoiceRecognitionTest
{
    public class KinectSensorUtilities
    {
        private KinectSensorChooser chooser;

        private KinectSensor _sensor = null;

        public KinectSensor sensor
        {
            get
            {
                return _sensor;
            }
        }

        private void sensorChangedEvent(Object sender, KinectChangedEventArgs e)
        {

            if (e.NewSensor != null)
            {
                try
                {
                    _sensor = e.NewSensor;
                    _sensor.Start();
                    Console.WriteLine("Kinect Started: " + e.NewSensor.UniqueKinectId);
                }
                catch
                {
                    Console.WriteLine("Couldn't start Kinect- another process has control.");
                    _sensor = null;
                }
            }

            if (e.OldSensor != null)
            {
                if (_sensor != null)
                {
                    _sensor.Stop();
                    _sensor = null;
                }
                Console.WriteLine("Kinect Removed: " + e.OldSensor.DeviceConnectionId);
            }

        }

        public KinectSensorUtilities()
        {
            chooser = new KinectSensorChooser();
            chooser.KinectChanged += new EventHandler<KinectChangedEventArgs>(this.sensorChangedEvent);
        }

        public void startChooser()
        {
            chooser.Start();
        }
    }
}
