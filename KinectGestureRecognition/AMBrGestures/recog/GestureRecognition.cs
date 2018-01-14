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

        public event StatusChangedHandler GesturesDetectionStatusChanged;
        //public event GestureChangedHandler GestureChanged;
        public event KinectActionEventHandler KinectActionRecognized;

        public async Task Init()
        {
            var pausePose = new HandPose("PausePose", new PalmPose(new AnyHandContext(), PoseDirection.Forward, PoseDirection.Up),
                    new FingerPose(new AllFingersContext(), FingerFlexion.Open, PoseDirection.Up));
            pausePose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureType.Pause.ToString()));

            var selectPose = new HandPose("selectPose", new PalmPose(new AnyHandContext()), 
                new FingerPose(Finger.Index, FingerFlexion.OpenStretched, PoseDirection.Forward),
                new FingerPose(new AllFingersContext(new [] { Finger.Middle, Finger.Ring, Finger.Pinky}), PoseDirection.Backward));
            selectPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureType.PlaySelect.ToString()));

            var menuPose = new HandPose("menuPose", new PalmPose(new AnyHandContext(), PoseDirection.Backward, PoseDirection.Down),
                new FingerPose (new AllFingersContext(new[] { Finger.Index, Finger.Middle, Finger.Ring }), FingerFlexion.OpenStretched, PoseDirection.Down),
                new FingerPose (new AllFingersContext(new [] {Finger.Thumb, Finger.Pinky}), FingerFlexion.Folded)
                );
            //menuPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(GestureType.Menu);
            menuPose.Triggered += (s, arg) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureType.Menu.ToString()));

            var pinchPoseRewind = GeneratePinchPose("PinchPoseRewind");
            var pinchPoseForward = GeneratePinchPose("PinchPoseForward");

            var rewindMotion = new HandMotion("RewindMotion", new PalmMotion(VerticalMotionSegment.Left));
            rewindMotion.Triggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureType.Rewind.ToString()));

            var forwardMotion = new HandMotion("ForwardMotion", new PalmMotion(VerticalMotionSegment.Right));
            forwardMotion.Triggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureType.Forward.ToString()));

            var keepRewindingPose = GeneratePinchPose("KeepRewindPose");
            var keepForwardingPose = GeneratePinchPose("KeepForwardingPose");

            var releasePoseRewind = GeneratePinchPose("ReleasePoseRewind", true);
            var releasePoseForward = GeneratePinchPose("ReleasePoseForward", true);

            _rewindGesture = new Gesture("RewindGesture", pinchPoseRewind, rewindMotion, keepRewindingPose, releasePoseRewind);

            _rewindGesture.IdleTriggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureType.None.ToString()));

            _forwardGesture = new Gesture("ForwardGesture", pinchPoseForward, forwardMotion, keepForwardingPose, releasePoseForward);

            _forwardGesture.IdleTriggered += (s, args) => KinectActionRecognized?.Invoke(this, new KinectRecognizedActionEventArgs(KinectActionRecognizedSource.Gesture, GestureType.None.ToString()));

            var shouldNeverHappen = new HandPose("shouldNotHappen", new PalmPose(new AnyHandContext(), PoseDirection.Forward, PoseDirection.Down),
                                                       new FingerPose(new AllFingersContext(), FingerFlexion.Open));
            var shouldNeverHappen2 = new HandPose("shouldNotHappen2", new PalmPose(new AnyHandContext(), PoseDirection.Forward, PoseDirection.Down),
                                                       new FingerPose(new AllFingersContext(), FingerFlexion.Open));
            var shouldNeverHappen3 = new HandPose("shouldNotHappen3", new PalmPose(new AnyHandContext(), PoseDirection.Forward, PoseDirection.Down),
                                                       new FingerPose(new AllFingersContext(), FingerFlexion.Open));

            _pauseGesture = new Gesture("PauseGesture", pausePose, shouldNeverHappen);
            _selectGesture = new Gesture("SelectGesture", selectPose, shouldNeverHappen2);
            _menuGesture = new Gesture("MenuGesture", menuPose, shouldNeverHappen3);


            // Step2: Connect to Gesture Service, route StatusChanged event to the UI and register the gesture
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (oldStatus, newStatus) => GesturesDetectionStatusChanged?.Invoke(oldStatus, newStatus);
            await _gesturesService.ConnectAsync();
            await _gesturesService.RegisterGesture(_pauseGesture);
            await _gesturesService.RegisterGesture(_selectGesture);
            await _gesturesService.RegisterGesture(_rewindGesture);
            await _gesturesService.RegisterGesture(_forwardGesture);
            await _gesturesService.RegisterGesture(_menuGesture);
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
    }
}
