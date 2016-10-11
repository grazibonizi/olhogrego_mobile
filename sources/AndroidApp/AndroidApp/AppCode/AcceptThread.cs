using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Bluetooth;

namespace AndroidApp.AppCode
{
    public class AcceptThread : Thread
    {
        // The local server socket
        private BluetoothServerSocket mmServerSocket;
        private CommunicationService _service;

        public AcceptThread(CommunicationService service)
        {
            _service = service;
            BluetoothServerSocket tmp = null;

            // Create a new listening server socket
            try
            {
                tmp = _service._adapter.ListenUsingRfcommWithServiceRecord(CommunicationService.NAME, _service.MY_UUID);

            }
            catch (Java.IO.IOException e)
            {
            }
            mmServerSocket = tmp;
        }

        public override void Run()
        {
            Name = "AcceptThread";
            BluetoothSocket socket = null;

            // Listen to the server socket if we're not connected
            while (_service._state != ConnectionState.STATE_CONNECTED)
            {
                try
                {
                    // This is a blocking call and will only return on a
                    // successful connection or an exception
                    socket = mmServerSocket.Accept();
                }
                catch (Java.IO.IOException e)
                {
                    break;
                }

                // If a connection was accepted
                if (socket != null)
                {
                    lock (this)
                    {
                        switch (_service._state)
                        {
                            case ConnectionState.STATE_LISTEN:
                            case ConnectionState.STATE_CONNECTING:
                                // Situation normal. Start the connected thread.
                                _service.Connected(socket, socket.RemoteDevice);
                                break;
                            case ConnectionState.STATE_NONE:
                            case ConnectionState.STATE_CONNECTED:
                                // Either not ready or already connected. Terminate new socket.
                                try
                                {
                                    socket.Close();
                                }
                                catch (Java.IO.IOException e)
                                {
                                }
                                break;
                        }
                    }
                }
            }
        }

        public void Cancel()
        {
            try
            {
                mmServerSocket.Close();
            }
            catch (Java.IO.IOException e)
            {
            }
        }
    }
}