using System;
using System.Diagnostics;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AMBrGestures
{
    class AmbrSpeechRecognition : IKinectActionRecognizer
    {
        private SpeechRecognitionEngine ambrRecognitionEngine = null;
        private RecognizerInfo ambrRecognizerInfo = null;
        private util.KinectAudioStream ambrAudioStream = null;

        private KinectSensor ambrSensor = null;

        public event KinectActionEventHandler KinectActionRecognized;

        public AmbrSpeechRecognition()
        {
            //Set up the kinect sensor. Recognition engine setup is handled when the sensor is available or unavailable
            this.ambrSensor = KinectSensor.GetDefault();
            
            this.ambrSensor.Open();

            //Get a copy of the audio stream that is compatible with the speech engine
            ambrAudioStream = new AMBrGestures.util.KinectAudioStream(ambrSensor.AudioSource.AudioBeams[0].OpenInputStream());

            //Initialize the recognition engine
            ambrRecognitionEngine = new SpeechRecognitionEngine(GetKinectRecognizer());

            //Install the speech grammar
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(File.ReadAllText("data\\SpeechGrammar2.xml"))))
            {
                var g = new Grammar(memoryStream);
                ambrRecognitionEngine.LoadGrammar(g);
            }

            ambrRecognitionEngine.SpeechRecognized += SpeechRecognized;

            ambrAudioStream.SpeechActive = true;

            ambrRecognitionEngine.SetInputToAudioStream(ambrAudioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            ambrRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

            //this.ambrSensor.IsAvailableChanged += this.sensorAvailabilityChanged;

        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.7;

            if(e.Result.Confidence > ConfidenceThreshold)
            {
                KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Speech, e.Result.Semantics.Value.ToString()));
            }
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
