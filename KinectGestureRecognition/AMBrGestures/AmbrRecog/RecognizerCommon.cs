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

    //This is a mirror of the speech grammar XML file
    public enum GestureAction : byte
    {
        ACTIVATION_PHRASE,
        DEACTIVATION_PHRASE,
        SCREEN_OFF,
        SCREEN_PHOTOS,
        SCREEN_VIDEOS,
        SCREEN_MUSIC,
        INPUT_BACK,
        INPUT_UP,
        INPUT_DOWN,
        INPUT_PREVIOUS,
        INPUT_NEXT,
        INPUT_SELECT,
        INPUT_HOME,
        INPUT_CONTEXTMENU,
        INPUT_SCROLLDONE,
        PLAYER_PLAY,
        PLAYER_STOP,
        PLAYER_PAUSE,
        PLAYER_FORWARD,
        PLAYER_REWIND,
        PLAYER_SEEKDONE,
        PLAYER_INFO,
        VOLUME_UP,
        VOLUME_DOWN,
        VOLUME_DONE
    }

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
        private GestureAction _actionType;

        public KinectRecognizedActionEventArgs(KinectActionRecognizedSource source, GestureAction type)
        {
            _actionSource = source;
            _actionType = type;
        }

        public KinectActionRecognizedSource ActionSource { get { return _actionSource; } }
        public GestureAction ActionType { get { return _actionType; } }
    }

    class RecognizerCommon
    {
    }
}
