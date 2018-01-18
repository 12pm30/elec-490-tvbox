using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AMBrGestures
{


    public delegate void GestureChangedHandler(GestureType newGesture);

    public sealed class GestureRecognition : IDisposable, IKinectActionRecognizer
	{
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _pauseGesture;
        private Gesture _selectGesture;
        private Gesture _rewindGesture;
        private Gesture _forwardGesture;
        private Gesture _menuGesture;
        private Gesture _backGesture;
        private Gesture _downGesture;
        private Gesture _upGesture;
        private Gesture _leftGesture;
        private Gesture _rightGesture;

        public event StatusChangedHandler GesturesDetectionStatusChanged;
        //public event GestureChangedHandler GestureChanged;
        public event KinectActionEventHandler KinectActionRecognized;

        public async Task Init()
        {
            var pausePose = new HandPose("PausePose", new PalmPose(new AnyHandContext(), PoseDirection.Forward, PoseDirection.Up),
                    new FingerPose(new AllFingersContext(), FingerFlexion.Open, PoseDirection.Up));
            //pausePose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureType.Pause.ToString()));
            pausePose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.PLAYER_PAUSE));

            var selectPose = new HandPose("selectPose", new PalmPose(new AnyHandContext(), PoseDirection.Left, PoseDirection.Forward), 
                new FingerPose(Finger.Index, FingerFlexion.OpenStretched, PoseDirection.Forward),
                new FingerPose(new AllFingersContext(new [] { Finger.Middle, Finger.Ring, Finger.Pinky}), PoseDirection.Backward));
            selectPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_SELECT));

            var menuPose = new HandPose("menuPose", new PalmPose(new AnyHandContext(), PoseDirection.Backward, PoseDirection.Down),
                new FingerPose (new AllFingersContext(new[] { Finger.Index, Finger.Middle, Finger.Ring }), FingerFlexion.OpenStretched, PoseDirection.Down),
                new FingerPose (new AllFingersContext(new [] {Finger.Thumb, Finger.Pinky}), FingerFlexion.Folded)
                );
            //menuPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(GestureType.Menu);
            menuPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_CONTEXTMENU));

            var backPose = new HandPose("BackPose", new PalmPose(new AnyHandContext(), PoseDirection.Backward, PoseDirection.Left),
                new FingerPose(new AllFingersContext(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }), FingerFlexion.Open, PoseDirection.Left));

            backPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_BACK));

            var leftPose = new HandPose("LeftPose", new PalmPose(new AnyHandContext(), PoseDirection.Backward, PoseDirection.Left),
                new FingerPose(Finger.Index, FingerFlexion.OpenStretched, PoseDirection.Left),
                new FingerPose(new AllFingersContext(new[] { Finger.Middle, Finger.Ring, Finger.Pinky }), PoseDirection.Right));

            leftPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_PREVIOUS));

            var rightPose = new HandPose("RightPose", new PalmPose(new AnyHandContext(), PoseDirection.Forward, PoseDirection.Right),
               new FingerPose(Finger.Index, FingerFlexion.OpenStretched, PoseDirection.Right),
               new FingerPose(new AllFingersContext(new[] { Finger.Middle, Finger.Ring, Finger.Pinky }), PoseDirection.Left));

            rightPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_NEXT));

            var pinchPoseRewind = GeneratePinchPose("PinchPoseRewind");
            var pinchPoseForward = GeneratePinchPose("PinchPoseForward");

            var rewindMotion = new HandMotion("RewindMotion", new PalmMotion(VerticalMotionSegment.Left));
            rewindMotion.Triggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.PLAYER_REWIND));

            var forwardMotion = new HandMotion("ForwardMotion", new PalmMotion(VerticalMotionSegment.Right));
            forwardMotion.Triggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.PLAYER_FORWARD));

            var keepRewindingPose = GeneratePinchPose("KeepRewindPose");
            var keepForwardingPose = GeneratePinchPose("KeepForwardingPose");

            var releasePoseRewind = GeneratePinchPose("ReleasePoseRewind", true);
            var releasePoseForward = GeneratePinchPose("ReleasePoseForward", true);

            _rewindGesture = new Gesture("RewindGesture", pinchPoseRewind, rewindMotion, keepRewindingPose, releasePoseRewind);
            _rewindGesture.IdleTriggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.PLAYER_SEEKDONE));

            _forwardGesture = new Gesture("ForwardGesture", pinchPoseForward, forwardMotion, keepForwardingPose, releasePoseForward);
            _forwardGesture.IdleTriggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.PLAYER_SEEKDONE));

            var downStartPose = new HandPose("DownStartPose", new PalmPose(new AnyHandContext(), PoseDirection.Up, PoseDirection.Forward),
                new FingerPose(new AllFingersContext(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }),FingerFlexion.Open, PoseDirection.Forward),
                new FingerPose(Finger.Thumb, FingerFlexion.Open, PoseDirection.Right));

            var downClamPose = new HandPose("DownClamPose", new PalmPose(new AnyHandContext(), PoseDirection.Up, PoseDirection.Forward),
                new FingerPose(new AllFingersContext(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }), FingerFlexion.Folded, PoseDirection.Backward)
                // new FingerPose(Finger.Thumb, FingerFlexion.Open, PoseDirection.Right));
                );

            var downStopPose = new HandPose("DownStopPose", new PalmPose(new AnyHandContext()),
                new FingerPose(new AnyFingerContext(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }), FingerFlexion.Open));

            downClamPose.Triggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_DOWN));
            _downGesture = new Gesture("DownGesture", downStartPose, downClamPose, downStopPose);
            _downGesture.IdleTriggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_SCROLLDONE));

            var upStartPose = new HandPose("UpStartPose", new PalmPose(new AnyHandContext(), PoseDirection.Down, PoseDirection.Forward),
                new FingerPose(new AllFingersContext(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }), FingerFlexion.Open, PoseDirection.Forward),
                new FingerPose(Finger.Thumb, FingerFlexion.Open, PoseDirection.Left));

            var upClamPose = new HandPose("UpClamPose", new PalmPose(new AnyHandContext(), PoseDirection.Down, PoseDirection.Forward),
                new FingerPose(new AllFingersContext(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }), FingerFlexion.Folded, PoseDirection.Backward)
                // new FingerPose(Finger.Thumb, FingerFlexion.Open, PoseDirection.Left));
                );

            var upStopPose = new HandPose("UpStopPose", new PalmPose(new AnyHandContext()),
                new FingerPose(new AnyFingerContext(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }), FingerFlexion.Open));

            upClamPose.Triggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_UP));
            _upGesture = new Gesture("UpGesture", upStartPose, upClamPose, upStopPose);
            _upGesture.IdleTriggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureAction.INPUT_SCROLLDONE));

            _pauseGesture = new Gesture("PauseGesture", pausePose, GenerateSpacerPose("PauseSpacer"));
            _selectGesture = new Gesture("SelectGesture", selectPose, GenerateSpacerPose("SelectSpacer"));
            _menuGesture = new Gesture("MenuGesture", menuPose, GenerateSpacerPose("MenuSpacer"));
            _backGesture = new Gesture("BackGesture", backPose, GenerateSpacerPose("BackSpacer"));
            _leftGesture = new Gesture("LeftGesture", leftPose, GenerateSpacerPose("LeftSpacer"));
            _rightGesture = new Gesture("RightGesture", rightPose, GenerateSpacerPose("RightSpacer"));

            // Step2: Connect to Gesture Service, route StatusChanged event to the UI and register the gesture
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (oldStatus, newStatus) => GesturesDetectionStatusChanged?.Invoke(oldStatus, newStatus);
            await _gesturesService.ConnectAsync();
            await _gesturesService.RegisterGesture(_pauseGesture, true);
            await _gesturesService.RegisterGesture(_selectGesture, true);
            await _gesturesService.RegisterGesture(_rewindGesture, true);
            await _gesturesService.RegisterGesture(_forwardGesture, true);
            await _gesturesService.RegisterGesture(_menuGesture, true);
            await _gesturesService.RegisterGesture(_backGesture, true);
            await _gesturesService.RegisterGesture(_downGesture, true);
            await _gesturesService.RegisterGesture(_upGesture, true);
            await _gesturesService.RegisterGesture(_leftGesture, true);
            await _gesturesService.RegisterGesture(_rightGesture, true);
        }

        public void Dispose() => _gesturesService?.Dispose();

        private HandPose GeneratePinchPose(string name, bool pinchSpread = false)
        {
            var pinchingFingers = new[] { Finger.Thumb, Finger.Index };
            var openFingersContext = pinchSpread ? new AllFingersContext(pinchingFingers) as FingersContext : new AnyFingerContext(pinchingFingers) as FingersContext;
            return new HandPose(name, new FingerPose(openFingersContext, FingerFlexion.Open),
                                      new FingertipDistanceRelation(pinchingFingers, pinchSpread ? RelativeDistance.NotTouching : RelativeDistance.Touching),
                                      new FingertipDistanceRelation(pinchingFingers, RelativeDistance.NotTouching, Finger.Middle));
        }

        private HandPose GenerateSpacerPose(string name)
        {
           return new HandPose(name, new PalmPose(new AnyHandContext(), PoseDirection.Forward, PoseDirection.Down),
                                                       new FingerPose(new AllFingersContext(), FingerFlexion.Open));
        }
    }
}
