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
using System.IO;

namespace AndroidApp.AppCode
{
    public class ConnectedThread : Thread
    {
        private BluetoothSocket mmSocket;
        private Stream mmInStream;
        private Stream mmOutStream;
        private CommunicationService _service;

        public ConnectedThread(BluetoothSocket socket, CommunicationService service)
        {
            mmSocket = socket;
            _service = service;
            Stream tmpIn = null;
            Stream tmpOut = null;

            // Get the BluetoothSocket input and output streams
            try
            {
                tmpIn = socket.InputStream;
                tmpOut = socket.OutputStream;
            }
            catch (Java.IO.IOException e)
            {
            }

            mmInStream = tmpIn;
            mmOutStream = tmpOut;
        }

        public override void Run()
        {
            byte[] buffer = new byte[1024];
            int bytes;

            // Keep listening to the InputStream while connected
            while (true)
            {
                try
                {
                    // Read from the InputStream
                    do
                    {
                        bytes = mmInStream.Read(buffer, 0, buffer.Length);
                    }
                    while (bytes != '\n');
                    var file_name = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(
                                                    Android.OS.Environment.DirectoryDocuments), 
                                                    string.Format("{0}.zip", Guid.NewGuid())).AbsolutePath;
                    using (var fs = new FileStream(file_name, FileMode.Create))
                    {
                        mmInStream.CopyTo(fs);
                    }
                    // Send the obtained bytes to the UI Activity
                    //_service._handler.ObtainMessage((int)MessageState.MESSAGE_READ, bytes, -1, buffer).SendToTarget();
                }
                catch (Java.IO.IOException e)
                {
                    _service.ConnectionLost();
                    break;
                }
            }
        }

        private void WriteFile(byte[] bytes)
        {
        }

        /// <summary>
        /// Write to the connected OutStream.
        /// </summary>
        /// <param name='buffer'>
        /// The bytes to write
        /// </param>
        public void Write(byte[] buffer)
        {
            try
            {
                mmOutStream.Write(buffer, 0, buffer.Length);

                // Share the sent message back to the UI Activity
                _service._handler.ObtainMessage((int)MessageState.MESSAGE_WRITE, -1, -1, buffer)
                    .SendToTarget();
            }
            catch (Java.IO.IOException e)
            {
            }
        }

        public void Cancel()
        {
            try
            {
                mmSocket.Close();
            }
            catch (Java.IO.IOException e)
            {
            }
        }
    }
}