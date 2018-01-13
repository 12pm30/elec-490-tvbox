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
        private GestureRecognition _gestureRecog;
        public void mainProgram()
        {
            Console.WriteLine("Starting");

            _gestureRecog = new GestureRecognition();
            _gestureRecog.Init();
            while (!_gestureRecog.ready) { };

            Console.WriteLine("Ready");

            //_gestureRecog.GesturesDetectionStatusChanged += (oldStatus, newStatus) => Dispatcher.InvokeAsync(() => txtDetectionServiceStatus.Text = $"Gestures Detection Service [{newStatus.Status}]");

            _gestureRecog.GestureChanged += (newGesture) => 
            {
                Console.WriteLine(newGesture);
            };

            while (!_gestureRecog.done) { };

            _gestureRecog?.Dispose();
        }

        static void Main(string[] args)
        {
            var program = new Program();
            program.mainProgram();
        }
    }
}


