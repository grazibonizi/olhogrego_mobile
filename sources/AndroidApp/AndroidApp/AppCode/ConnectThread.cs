

using Android.Bluetooth;
using Java.Lang;

namespace AndroidApp.AppCode
{
    public class ConnectThread : Thread
    {
        private BluetoothSocket mmSocket;
        private BluetoothDevice mmDevice;
        private CommunicationService _service;

        public ConnectThread(BluetoothDevice device, CommunicationService service)
        {
            mmDevice = device;
            _service = service;
            BluetoothSocket tmp = null;

            // Get a BluetoothSocket for a connection with the
            // given BluetoothDevice
            try
            {
                //tmp = device.CreateRfcommSocketToServiceRecord(_service.MY_UUID);
                tmp = device.CreateInsecureRfcommSocketToServiceRecord(_service.MY_UUID);
            }
            catch (Java.IO.IOException e)
            {
            }
            mmSocket = tmp;
        }

        public override void Run()
        {
            Name = "ConnectThread";

            // Always cancel discovery because it will slow down a connection
            _service._adapter.CancelDiscovery();

            // Make a connection to the BluetoothSocket
            try
            {
                // This is a blocking call and will only return on a
                // successful connection or an exception
                mmSocket.Connect();
            }
            catch (Java.IO.IOException e)
            {
                _service.ConnectionFailed();
                // Close the socket
                try
                {
                    mmSocket.Close();
                }
                catch (Java.IO.IOException e2)
                {
                }

                // Start the service over to restart listening mode
                _service.Start();
                return;
            }

            // Reset the ConnectThread because we're done
            lock (this)
            {
                _service.connectThread = null;
            }

            // Start the connected thread
            _service.Connected(mmSocket, mmDevice);
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