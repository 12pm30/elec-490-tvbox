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
using System.Xml;

namespace AMBrGestures
{
    class AmbrSpeechRecognition : IKinectActionRecognizer
    {
        private SpeechRecognitionEngine ambrRecognitionEngine = null;
        private util.KinectAudioStream ambrAudioStream = null;
        private KinectSensor ambrSensor = null;
        private Grammar speechGrammar;

        private bool recognizeItemList = false;

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

            //Install the default speech grammar
            InstallDefaultSpeechGrammar();

            ambrRecognitionEngine.SpeechRecognized += SpeechRecognized;
            ambrRecognitionEngine.SpeechRecognitionRejected += SpeechRejected;
            ambrRecognitionEngine.RecognizerUpdateReached += SpeechGrammarChange;

            ambrAudioStream.SpeechActive = true;

            ambrRecognitionEngine.SetInputToAudioStream(ambrAudioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            ambrRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

            //this.ambrSensor.IsAvailableChanged += this.sensorAvailabilityChanged;

        }

        public void RecognizeItemList(XmlDocument doc)
        {
            ambrRecognitionEngine.RequestRecognizerUpdate(doc);
        }

        private void InstallDefaultSpeechGrammar()
        {
            //Install the speech grammar
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(File.ReadAllText("AmbrData\\SpeechGrammar2.xml"))))
            {
                speechGrammar = new Grammar(memoryStream);
                ambrRecognitionEngine.LoadGrammar(speechGrammar);
            }
        }

        private void SpeechGrammarChange(object sender, RecognizerUpdateReachedEventArgs e)
        {
            ambrRecognitionEngine.UnloadAllGrammars();

            if (e.UserToken == null)
            {
                //reset to the old grammar
                recognizeItemList = false;
                InstallDefaultSpeechGrammar();
            }
            else if(e.UserToken.GetType() == typeof(XmlDocument))
            {
                //We theoretically have a grammar we can use...
                recognizeItemList = true;

                var memoryStream = new MemoryStream();
                ((XmlDocument)e.UserToken).Save(memoryStream);

                memoryStream.Position = 0;//reset the stream position to 0
                speechGrammar = new Grammar(memoryStream);
                ambrRecognitionEngine.LoadGrammar(speechGrammar); 
            }
            else
            {
                //something is wrong here...
                throw new ArgumentException("User token was not null or XML");
            }
        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.7;

            if(e.Result.Confidence > ConfidenceThreshold)
            {
                if(recognizeItemList)
                {
                    KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Speech, GestureAction.PLAYER_OPEN, e.Result.Semantics.Value.ToString()));
                }
                else
                {
                    KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Speech, (GestureAction)Enum.Parse(typeof(GestureAction), e.Result.Semantics.Value.ToString())));
                }
            }
        }

        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            if(recognizeItemList)
            {
                KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Speech, GestureAction.PLAYER_OPEN_ERROR));
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
