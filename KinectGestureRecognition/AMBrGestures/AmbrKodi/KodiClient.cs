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
        private Boolean seekInProgress = false;
        private Boolean videoPaused = false;

        public KodiClient()
        {
            Console.WriteLine("KodiClient constructed");

            kodiPython = new Process();
            
            kodiPython.EnableRaisingEvents = true;
            kodiPython.Exited += clientKilledEventHandler;

            startKodiClientProcess();
        }

        public void KinectActionEventHandler(object sender, KinectRecognizedActionEventArgs e)
        {
            GestureAction action = e.ActionType;
            KinectActionRecognizedSource source = e.ActionSource;
            if (source == KinectActionRecognizedSource.Gesture){
                if (action == GestureAction.INPUT_UP || action == GestureAction.INPUT_DOWN)
                {
                    // Ignore scroll actions while one is already in progress
                    if (scrollTimer != null)
                    {
                        scrollTimer = new Timer(1000);
                        scrollTimer.Elapsed += (sendr, ev) => kodiStreamWriter.WriteLine(action.ToString());
                        scrollTimer.Start();
                    }
                }
                else if (action == GestureAction.INPUT_PREVIOUS || action == GestureAction.INPUT_NEXT)
                {
                    // Ignore scroll actions while one is already in progress
                    if (scrollTimer != null)
                    {
                        scrollTimer = new Timer(1500);
                        scrollTimer.Elapsed += (sendr, ev) => kodiStreamWriter.WriteLine(action.ToString());
                        scrollTimer.Start();
                    }
                }
                else if (action == GestureAction.INPUT_SCROLLDONE)
                {
                    scrollTimer?.Stop();
                    scrollTimer?.Dispose();
                    scrollTimer = null;
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
                    if (videoPaused)
                    {
                        kodiStreamWriter.WriteLine(GestureAction.PLAYER_PAUSE.ToString());
                    }
                    else
                    {
                        kodiStreamWriter.WriteLine(GestureAction.PLAYER_PLAY.ToString());
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
                else if (action == GestureAction.SCREEN_HOME)
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
                kodiStreamWriter.WriteLine(action.ToString());
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
