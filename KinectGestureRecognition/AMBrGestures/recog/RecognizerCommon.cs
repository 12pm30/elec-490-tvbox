using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMBrGestures
{ 
    public enum GestureType : byte
    {
        Pause,
        PlaySelect,
        Rewind,
        Forward,
        Menu,
        None,
    };

    public enum KinectActionRecognizedSource
    {
        Speech,
        Gesture,
        Other
    }

    public delegate void KinectActionEventHandler(object sender, KinectRecognizedActionEventArgs e);

    public interface IKinectActionRecognizer
    {
        event KinectActionEventHandler KinectActionRecognized;
    }

    public class KinectRecognizedActionEventArgs : EventArgs
    {
        private KinectActionRecognizedSource _actionSource;
        private string _actionType;

        public KinectRecognizedActionEventArgs(KinectActionRecognizedSource source, string type)
        {
            _actionSource = source;
            _actionType = type;
        }

        public KinectActionRecognizedSource ActionSource { get { return _actionSource; } }
        public string ActionType { get { return _actionType; } }
    }

    class RecognizerCommon
    {
    }
}
