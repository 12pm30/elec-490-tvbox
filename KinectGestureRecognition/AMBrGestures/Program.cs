using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;

namespace AMBrGestures
{
    class Program
    {

        private static void KinectActionEventHandler(object sender, KinectRecognizedActionEventArgs e)
        {
            Console.WriteLine("Kinect Action Recognized");
            Console.WriteLine("Action Source: {0}", e.ActionSource.ToString());
            Console.WriteLine("Action Type: {0}", e.ActionType.ToString());
            Console.WriteLine("");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting");

            GestureRecognition _gestureRecog;
            AmbrSpeechRecognition _speechRecog;

            //The gesture and speech services don't actually care if there's a sensor attached. it will just return nothing
            _gestureRecog = new GestureRecognition();
            _speechRecog = new AmbrSpeechRecognition();

            //Subscribe to the events
            _gestureRecog.KinectActionRecognized += KinectActionEventHandler;
            _speechRecog.KinectActionRecognized += KinectActionEventHandler;

            //Register the gestures
            _gestureRecog.Init().Wait();

            Console.WriteLine("Ready");

            //Wait until the user hits "escape"
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                // do something
            }

        }
    }
}


