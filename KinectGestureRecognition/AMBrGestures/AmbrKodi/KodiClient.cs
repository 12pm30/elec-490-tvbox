using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace AMBrGestures
{
    class KodiClient
    {
        private Process kodiPython = null;
        private TcpClient kodiTcpClient = null;
        private StreamWriter kodiStreamWriter = null;

        private Timer scrollTimer = null;
        private GestureAction? scrollAction = null;
        private int scrollCount = 0;

        private Timer volumeTimer = null;
        private GestureAction? volumeAction = null;

        private Timer speechTimer = null;

        private Boolean seekInProgress = false;
        private Boolean videoPaused = false;

        private Boolean allowSpeechEvents = false;

        public KodiClient()
        {
            Console.WriteLine("KodiClient constructed");

            kodiPython = new Process();
            
            kodiPython.EnableRaisingEvents = true;
            kodiPython.Exited += clientKilledEventHandler;

            startKodiClientProcess();

            //Set up the timer objects
            scrollTimer = new Timer(1000); //1 sec delay
            scrollTimer.AutoReset = true; //Will keep firing until stopped
            scrollTimer.Enabled = false;
            scrollTimer.Elapsed += scrollTimerEventHandler;

            volumeTimer = new Timer(500);
            volumeTimer.AutoReset = true;
            volumeTimer.Enabled = false;
            volumeTimer.Elapsed += volumeTimerEventHandler;

            speechTimer = new Timer(10000);
            speechTimer.AutoReset = false;
            speechTimer.Enabled = false;
            speechTimer.Elapsed += speechTimerEventHandler;

        }

        public void KinectActionEventHandler(object sender, KinectRecognizedActionEventArgs e)
        {
            GestureAction action = e.ActionType;
            KinectActionRecognizedSource source = e.ActionSource;
             if (source == KinectActionRecognizedSource.Gesture){
                if (action == GestureAction.INPUT_UP || action == GestureAction.INPUT_DOWN)
                {
                    // Ignore scroll actions while one is already in progress
                    if (scrollTimer.Enabled == false)
                    {
                        kodiStreamWriter.WriteLine(action.ToString()); 
                        scrollAction = action; //Set the scroll action we want to do
                        scrollTimer.Interval = 1000; //Set the interval
                        scrollTimer.Start(); //Start the timer
                    }
                }
                else if (action == GestureAction.INPUT_PREVIOUS || action == GestureAction.INPUT_NEXT)
                {
                    // Ignore scroll actions while one is already in progress
                    if (scrollTimer.Enabled == false)
                    {
                        kodiStreamWriter.WriteLine(action.ToString());
                        scrollAction = action; //Set the scroll action we want to do
                        scrollTimer.Interval = 1000; //TODO: Tune the scroll time.
                        scrollTimer.Start(); //Start the timer
                    }
                }
                else if (action == GestureAction.INPUT_SCROLLDONE)
                {
                    scrollTimer.Enabled = false; //Stop the scroll timer, set the action to null
                    scrollAction = null;
                    scrollCount = 0;
                }
                else if (action == GestureAction.VOLUME_DOWN || action == GestureAction.VOLUME_UP)
                {
                    //Set the volume action
                    volumeAction = action;

                    // Ignore volume actions while one is already in progress
                    if (scrollTimer.Enabled == false)
                    {
                        kodiStreamWriter.WriteLine(action.ToString() + " 5");
                        volumeTimer.Start(); //Start the timer
                    }
                }
                else if (action == GestureAction.VOLUME_DONE)
                {
                    volumeAction = null;
                    volumeTimer.Enabled = false;
                }
                else if (action == GestureAction.PLAYER_REWIND || action == GestureAction.PLAYER_FORWARD)
                {
                    // Ignore seek actions while one is already in progress
                    if (seekInProgress == false)
                    {
                        kodiStreamWriter.WriteLine(action.ToString());
                        seekInProgress = true;
                    }
                }
                else if (action == GestureAction.PLAYER_SEEKDONE)
                {
                    if (seekInProgress)
                    {
                        if (videoPaused)
                        {
                            kodiStreamWriter.WriteLine(GestureAction.PLAYER_PAUSE.ToString());
                        }
                        else
                        {
                            kodiStreamWriter.WriteLine(GestureAction.PLAYER_PLAY.ToString());
                        }
                    }
                    seekInProgress = false;
                }
                else if (action == GestureAction.INPUT_SELECT)
                {
                    // Aliasing of select and play, there may be a better way to implement this.
                    kodiStreamWriter.WriteLine(action.ToString());
                    kodiStreamWriter.WriteLine(GestureAction.PLAYER_PLAY.ToString());
                    videoPaused = false;
                }
                else if (action == GestureAction.PLAYER_PAUSE)
                {
                    kodiStreamWriter.WriteLine(action.ToString());
                    videoPaused = true;
                }
                else if (action == GestureAction.INPUT_HOME)
                {
                    // Stop any playing content and return to the home screen.
                    kodiStreamWriter.WriteLine(GestureAction.PLAYER_STOP.ToString());
                    kodiStreamWriter.WriteLine(action.ToString());
                }
                else
                {
                    kodiStreamWriter.WriteLine(action.ToString());
                }
            }
            else
            {
                if (action == GestureAction.ACTIVATION_PHRASE)
                {
                    //Send a message to
                    kodiStreamWriter.WriteLine("GUI_NOTIFICATION 'AMBr' 'Say A Command' 10000");
                    kodiStreamWriter.WriteLine("APPLICATION_MUTE");

                    speechTimer.Stop();
                    speechTimer.Interval = 10000;
                    speechTimer.Start();

                    allowSpeechEvents = true;
                }
                else if (allowSpeechEvents)
                {
                    //Source is a speech event
                   
                    kodiStreamWriter.WriteLine("GUI_NOTIFICATION 'AMBr' 'Say A Command' 2500");

                    speechTimer.Stop();
                    speechTimer.Interval = 2500;
                    speechTimer.Start();

                    if(action == GestureAction.VOLUME_UP)
                    {
                        kodiStreamWriter.WriteLine("VOLUME_UP 10");
                    }
                    else if(action == GestureAction.VOLUME_DOWN)
                    {
                        kodiStreamWriter.WriteLine("VOLUME_DOWN 10");
                    }
                    else
                    {
                        kodiStreamWriter.WriteLine(action.ToString());
                    }

                }
            }
        }

        private void startKodiClientProcess()
        {
            int port = KodiClient.FreeTcpPort();
            ProcessStartInfo ps = new ProcessStartInfo("python", "AmbrKodi\\kodi_interface.py " + port.ToString());
            kodiPython.StartInfo = ps;
            kodiPython.Start();

            while (true)
            {
                try
                {
                    kodiTcpClient = new TcpClient("127.0.0.1", port);
                    break;
                }
                catch { }
            }

            kodiStreamWriter = new StreamWriter(kodiTcpClient.GetStream());
            kodiStreamWriter.AutoFlush = true;
        }

        private void clientKilledEventHandler(object sender, EventArgs e)
        {
            //If for whatever reason the kodi client dies, start it again
            startKodiClientProcess();
        }

        private void scrollTimerEventHandler(object sender, ElapsedEventArgs e)
        {
            if(scrollAction == GestureAction.INPUT_DOWN || scrollAction == GestureAction.INPUT_UP)
            {
                scrollCount += 1;

                if (scrollCount == 5)
                {
                    //at 5 items, increase the scrolling speed a bit
                    scrollTimer.Interval = 500;
                }
                else if(scrollCount == 25)
                {
                    //at 25 items, increase it some more
                    scrollTimer.Interval = 250;
                }

                kodiStreamWriter.WriteLine(scrollAction.ToString());
            }
            else if(scrollAction != null)
            {
                kodiStreamWriter.WriteLine(scrollAction.ToString());
            }
            else
            {
                //If the setup is correct
                Console.WriteLine("Scroll timer is firing, but the scroll action is null.");
            }
        }

        private void volumeTimerEventHandler(object sender, ElapsedEventArgs e)
        {
            kodiStreamWriter.WriteLine(volumeAction.ToString() + " 5");
        }

        private void speechTimerEventHandler(object sender, ElapsedEventArgs e)
        {
            allowSpeechEvents = false;
            //speechTimer.Enabled = false;
            kodiStreamWriter.WriteLine("APPLICATION_UNMUTE");
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }

}
