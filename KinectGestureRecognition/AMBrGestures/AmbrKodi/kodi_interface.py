from __future__ import division, print_function

import kodipydent
import signal
import socket
import sys
import threading
import time

class KodiApiCommandFailureError(Exception):
    pass

class KodiInterface(object):
    def __init__(self, host, username, password, kodi_port):
        while True:
            try:
                self.kodi = kodipydent.Kodi(host, username, password, kodi_port)
                break
            except Exception:
                print('Could not connect to Kodi, will retry in 10 seconds.')
                time.sleep(10)

    def start_socket(self, socket_port, scope = '127.0.0.1'):
        svr_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        svr_socket.bind((scope, socket_port))
        svr_socket.listen(5)
        while True:
            (clt_socket, clt_address) = svr_socket.accept()
            clt_thread = threading.Thread(target=self._client_thread, args=(clt_socket, clt_address))
            clt_thread.run()

    def _client_thread(self, clt_socket, clt_address):
        print("Received connection from " + str(clt_address))
        sock_stream = clt_socket.makefile('r+')
        command_dict = { 'INPUT_UP' : self._input_up,
                         'INPUT_DOWN': self._input_down,
                         'INPUT_NEXT': self._input_right,
                         'INPUT_PREVIOUS': self._input_left,
                         'INPUT_SELECT': self._input_select,
                         'INPUT_BACK': self._input_back,
                         'INPUT_HOME': self._input_home,
                         'ls_movies': self._list_movies,
                         'PLAYER_PLAY': self._player_play,
                         'PLAYER_PAUSE': self._player_pause,
                         'PLAYER_STOP': self._player_stop,
                         'PLAYER_OPEN': self._player_open,
                         'PLAYER_FORWARD' : self._player_forward,
                         'PLAYER_REWIND' : self._player_rewind,
                         'GUI_NOTIFICATION': self._gui_notification
                       }

        try:
            for line in sock_stream:

                    split_line = line.strip().split(' ')
                    command = split_line[0]
                    params = split_line[1:]

                    if command in command_dict:
                        try:
                            cmdEx = command_dict[command](sock_stream, *params)

                            if cmdEx is None:
                                print('Command %s returned no result.' % (command))
                            elif u'error' in cmdEx.keys():
                                print(cmdEx['error']['data']['stack']['message'])
                                raise KodiApiCommandFailureError(cmdEx['error']['data']['stack']['message'])
                            else:
                                print(command)

                        except Exception as e:
                            print ("Command Error: {} - {}".format(type(e).__name__,str(e)))
                    else:
                        sock_stream.write('Unrecognized command\n')
                        print('Command: %s is not recognized\n' % (command))
                    sock_stream.flush()

        except:
            pass

        sock_stream.close()
        clt_socket.close()

        print("Connection from %s terminated" % (str(clt_address)))
        sys.exit()

    def _gui_notification(self, sock_stream, tit, msg, tim):
        return self.kodi.GUI.ShowNotification(title=tit, message=msg, displaytime=tim)

    def _input_up(self, sock_stream):
        return self.kodi.Input.Up()

    def _input_down(self, sock_stream):
        return self.kodi.Input.Down()

    def _input_right(self, sock_stream):
        return self.kodi.Input.Right()

    def _input_left(self, sock_stream):
        return self.kodi.Input.Left()

    def _input_select(self, sock_stream):
        return self.kodi.Input.Select()

    def _input_back(self, sock_stream):
        return self.kodi.Input.Back()

    def _input_home(self,sock_stream):
        return self.kodi.Input.Home()

    def _list_movies(self, sock_stream):
        for movie in self.kodi.VideoLibrary.GetMovies()['result']['movies']:
            sock_stream.write(str(movie['movieid']) + ":" + str(movie['label']) + '\n')
        sock_stream.write('DONE\n')

    def _player_open(self, sock_stream, content_id):
        try:
            content_id = int(content_id)
        except ValueError:
            sock_stream.write('Content ID must be an integer\n')

        return self.kodi.Player.Open(item={'movieid':content_id})

    def _player_play(self, sock_stream):
        player_ids = [rec['playerid'] for rec in self.kodi.Player.GetActivePlayers()['result']]
        for playerid in player_ids:
            self.kodi.Player.PlayPause(playerid=playerid,play=True)

    def _player_pause(self, sock_stream):
        player_ids = [rec['playerid'] for rec in self.kodi.Player.GetActivePlayers()['result']]
        for playerid in player_ids:
            self.kodi.Player.PlayPause(playerid=playerid,play=False)

    def _player_stop(self, sock_stream):
        player_ids = [rec['playerid'] for rec in self.kodi.Player.GetActivePlayers()['result']]
        for playerid in player_ids:
            self.kodi.Player.Stop(playerid=playerid)

    def _player_forward(self, sock_stream):
        player_ids = [rec['playerid'] for rec in self.kodi.Player.GetActivePlayers()['result']]
        for playerid in player_ids:
            self.kodi.Player.SetSpeed(playerid=playerid, speed=16)

    def _player_rewind(self, sock_stream):
        player_ids = [rec['playerid'] for rec in self.kodi.Player.GetActivePlayers()['result']]
        for playerid in player_ids:
            self.kodi.Player.SetSpeed(playerid=playerid, speed=-16)


def exit_script(signal, frame):
    sys.exit(0)

if __name__ == '__main__':
    if len(sys.argv) != 2:
        print('Usage: kodi_interface.py <port#>')
        sys.exit(1)
    try:
        port = int(sys.argv[1])
        if port <= 0 or port > 65535:
            raise ValueError()
    except ValueError:
        print('Invalid port number')
        sys.exit(1)
    signal.signal(signal.SIGINT, exit_script)
    print('AMBr Python Interface')
    print('To exit, press ctrl+break (Fn+Ctrl+B)')
    print('Current Port: ' + str(sys.argv[1]))
    ki = KodiInterface('localhost', 'kodi', 'Password123', 8080)
    print("Connected to Kodi, starting socket")
    svr_thread = threading.Thread(target=ki.start_socket, args=[port])
    svr_thread.run()
    svr_thread.join()
