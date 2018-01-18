using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AMBrGestures
{
    class KodiClient
    {
        private Process kodiPython = null;
        private TcpClient kodiTcpClient = null;
        private StreamWriter kodiStreamWriter = null;

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
            kodiStreamWriter.WriteLine(e.ActionType.ToString());
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
