using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using Microsoft.Speech.Synthesis;

namespace AMBrGestures
{
    class Program
    {

        private static void KinectActionEventHandler(object sender, KinectRecognizedActionEventArgs e)
        {
            Console.WriteLine("Source: {0}   Action: {1} ", e.ActionSource.ToString(), e.ActionType.ToString());
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting");

            GestureRecognition _gestureRecog;
            AmbrSpeechRecognition _speechRecog;

            KodiClient k = new KodiClient();

            //The gesture and speech services don't actually care if there's a sensor attached. it will just return nothing
            _gestureRecog = new GestureRecognition();
            _speechRecog = new AmbrSpeechRecognition();

            //Subscribe to the events
            _gestureRecog.KinectActionRecognized += KinectActionEventHandler;
            _speechRecog.KinectActionRecognized += KinectActionEventHandler;

            _gestureRecog.KinectActionRecognized += k.KinectActionEventHandler;
            _speechRecog.KinectActionRecognized += k.KinectActionEventHandler;

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


