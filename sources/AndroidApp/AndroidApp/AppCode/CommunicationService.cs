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
using Java.Util;
using Android.Bluetooth;
using System.Runtime.CompilerServices;

namespace AndroidApp.AppCode
{
    public class CommunicationService
    {
        public UUID MY_UUID = UUID.FromString("94f39d29-7d6d-437d-973b-fba39e49d4ee");
        public BluetoothAdapter _adapter;
        public Handler _handler;
        public AcceptThread acceptThread;
        public ConnectThread connectThread;
        public ConnectedThread connectedThread;
        public ConnectionState _state;
        public const string DEVICE_NAME = "raspberrypi";
        public const string NAME = "olhogrego_mobile";
        public const string TOAST = "toast";
        /// <summary>
        /// Constructor. Prepares a new BluetoothChat session.
        /// </summary>
        /// <param name='context'>
        /// The UI Activity Context.
        /// </param>
        /// <param name='handler'>
        /// A Handler to send messages back to the UI Activity.
        /// </param>
        public CommunicationService(Context context, Handler handler)
        {
            _adapter = BluetoothAdapter.DefaultAdapter;
            _state = ConnectionState.STATE_NONE;
            _handler = handler;
        }

        /// <summary>
        /// Set the current state of the chat connection.
        /// </summary>
        /// <param name='state'>
        /// An integer defining the current connection state.
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SetState(ConnectionState state)
        {
            _state = state;

            // Give the new state to the Handler so the UI Activity can update
            _handler.ObtainMessage((int)MessageState.MESSAGE_STATE_CHANGE, (int)state, -1).SendToTarget();
        }

        /// <summary>
        /// Return the current connection state.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public ConnectionState GetState()
        {
            return _state;
        }

        // Start the chat service. Specifically start AcceptThread to begin a
        // session in listening (server) mode. Called by the Activity onResume()
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            // Cancel any thread attempting to make a connection
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            // Cancel any thread currently running a connection
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // Start the thread to listen on a BluetoothServerSocket
            if (acceptThread == null)
            {
                acceptThread = new AcceptThread(this);
                acceptThread.Start();
            }

            SetState(ConnectionState.STATE_LISTEN);
        }

        /// <summary>
        /// Start the ConnectThread to initiate a connection to a remote device.
        /// </summary>
        /// <param name='device'>
        /// The BluetoothDevice to connect.
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect(BluetoothDevice device)
        {
            // Cancel any thread attempting to make a connection
            if (_state == ConnectionState.STATE_CONNECTING)
            {
                if (connectThread != null)
                {
                    connectThread.Cancel();
                    connectThread = null;
                }
            }

            // Cancel any thread currently running a connection
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // Start the thread to connect with the given device
            connectThread = new ConnectThread(device, this);
            connectThread.Start();

            SetState(ConnectionState.STATE_CONNECTING);
        }

        /// <summary>
        /// Start the ConnectedThread to begin managing a Bluetooth connection
        /// </summary>
        /// <param name='socket'>
        /// The BluetoothSocket on which the connection was made.
        /// </param>
        /// <param name='device'>
        /// The BluetoothDevice that has been connected.
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connected(BluetoothSocket socket, BluetoothDevice device)
        {
            // Cancel the thread that completed the connection
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            // Cancel any thread currently running a connection
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // Cancel the accept thread because we only want to connect to one device
            if (acceptThread != null)
            {
                acceptThread.Cancel();
                acceptThread = null;
            }


            // Start the thread to manage the connection and perform transmissions
            connectedThread = new ConnectedThread(socket, this);
            connectedThread.Start();


            //TODO: adaptar codigo abaixo
            // Send the name of the connected device back to the UI Activity
            var msg = _handler.ObtainMessage((int)MessageState.MESSAGE_DEVICE_NAME);
            Bundle bundle = new Bundle();
            bundle.PutString(DEVICE_NAME, device.Name);
            msg.Data = bundle;
            _handler.SendMessage(msg);
           
            SetState(ConnectionState.STATE_CONNECTED);
        }

        /// <summary>
        /// Stop all threads.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if (acceptThread != null)
            {
                acceptThread.Cancel();
                acceptThread = null;
            }

            SetState(ConnectionState.STATE_NONE);
        }

        /// <summary>
        /// Write to the ConnectedThread in an unsynchronized manner
        /// </summary>
        /// <param name='out'>
        /// The bytes to write.
        /// </param>
        public void Write(byte[] @out)
        {
            // Create temporary object
            ConnectedThread r;
            // Synchronize a copy of the ConnectedThread
            lock (this)
            {
                if (_state != ConnectionState.STATE_CONNECTED)
                    return;
                r = connectedThread;
            }
            // Perform the write unsynchronized
            r.Write(@out);
        }

        /// <summary>
        /// Indicate that the connection attempt failed and notify the UI Activity.
        /// </summary>
        public void ConnectionFailed()
        {
            SetState(ConnectionState.STATE_LISTEN);

            
            // Send a failure message back to the Activity
            var msg = _handler.ObtainMessage((int)MessageState.MESSAGE_TOAST);
            Bundle bundle = new Bundle();
            bundle.PutString(TOAST, "Unable to connect device");
            msg.Data = bundle;
            _handler.SendMessage(msg);
        }

        /// <summary>
        /// Indicate that the connection was lost and notify the UI Activity.
        /// </summary>
        public void ConnectionLost()
        {
            SetState(ConnectionState.STATE_LISTEN);
            
            // Send a failure message back to the Activity
            var msg = _handler.ObtainMessage((int)MessageState.MESSAGE_TOAST);
            Bundle bundle = new Bundle();
            bundle.PutString(TOAST, "Device connection was lost");
            msg.Data = bundle;
            _handler.SendMessage(msg);
        }
    }
}
 