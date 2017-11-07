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

namespace VRKC
{
    class Program
    {
        private static SpeechRecognitionEngine sre;
        private static KinectSensor sensor;
        private static ProcessStartInfo pyProcessStart;
        private static Process pyProcess;

        static void Main(string[] args)
        {
            //Initializy python starting info
            pyProcessStart = new ProcessStartInfo("C:\\Python27\\python.exe");
            //pyProcessStart.RedirectStandardError = true;
            pyProcessStart.RedirectStandardInput = true;
            pyProcessStart.RedirectStandardOutput = true;
            pyProcessStart.UseShellExecute = false;
            /*
                pyProcess = new Process();
                pyProcess.StartInfo = pyProcessStart;
                pyProcess.Start();

               
                */
           
            pyProcess = Process.Start(pyProcessStart);

            //Start up python
            pyProcess.StandardInput.AutoFlush = true;

            pyProcess.StandardInput.WriteLine("from kodipydent import Kodi");
            pyProcess.StandardInput.WriteLine("mykodi = Kodi('127.0.0.1')");
            //inWriter.Close();

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
                
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "MEDIA_PLAY":
                        Process.Start("python","-c \"from kodipydent import Kodi;mykodi = Kodi('127.0.0.1');mykodi.Player.PlayPause(1,play=True)\"",);
                        break;
                    case "MEDIA_PAUSE":
                        Process.Start("python", "-c \"from kodipydent import Kodi;mykodi = Kodi('127.0.0.1');mykodi.Player.PlayPause(1,play=False)\"");
                        break;
                    default:
                        break;
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
