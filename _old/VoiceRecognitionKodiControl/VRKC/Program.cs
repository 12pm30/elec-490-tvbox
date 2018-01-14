using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace VRKC
{
    class Program
    {
        private static SpeechRecognitionEngine sre;
        private static KinectSensor sensor;
        private static TcpClient tc;
        private static StreamWriter sjw;

        static void Main(string[] args)
        {
            tc = new TcpClient("127.0.0.1", 14242);
            tc.NoDelay = true;
            sjw = new StreamWriter(tc.GetStream());
            sjw.AutoFlush = true;

            //Initialize Kinect
            Console.WriteLine("Waiting for Kinect...");

            KinectSensorChooser chooser = new KinectSensorChooser();
            chooser.Start();
            

            Console.WriteLine("Waiting for sensor");

            while (chooser.Status != ChooserStatus.SensorStarted)
            {

            }

            Console.WriteLine("Found and Started Kinect: " + chooser.Kinect.UniqueKinectId);

            sensor = chooser.Kinect;

            //Initialize speech recognition engine
            RecognizerInfo ri = GetKinectRecognizer();

            Console.WriteLine(ri.ToString());

            sre = new SpeechRecognitionEngine(ri.Id);

            sre.SpeechRecognized += SpeechRecognized;
            sre.SpeechRecognitionRejected += SpeechRejected;

            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(File.ReadAllText("data\\SpeechGrammar2.xml"))))
            {
                var g = new Grammar(memoryStream);
                sre.LoadGrammar(g);
            }

            sre.SetInputToAudioStream(sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            sre.RecognizeAsync(RecognizeMode.Multiple);

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                // do something
            }

            sre.RecognizeAsyncStop();
            sre.Dispose();
        }

        private static void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.7;

            Console.WriteLine(e.Result.Semantics.Value.ToString() + " (" + e.Result.Confidence.ToString() + ")");

            if (e.Result.Confidence > ConfidenceThreshold)
            {
                try
                {

                    sjw.WriteLine(e.Result.Semantics.Value.ToString());
                    
                }
                catch
                {

                }
            }
            
        }

        private static void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("Rejected Speech");
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }
    }



}
