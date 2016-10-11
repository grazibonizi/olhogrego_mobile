using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Bluetooth;
using System.Linq;
using AndroidApp.AppCode;
using System.Text;
using Java.Lang;

namespace AndroidApp
{
    [Activity(Label = "AndroidApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private const string TAG = "BluetoothChatService";
        private const int REQUEST_CONNECT_DEVICE = 1;
        private const int REQUEST_ENABLE_BT = 2;
        private const bool Debug = true;
        //private ConnectionThread cnn;
        private CommunicationService service;
        private StringBuffer outStringBuffer;

        private BluetoothAdapter bluetoothAdapter = null;

        private TextView txtConnectedDevice;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get local Bluetooth adapter
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // If the adapter is null, then Bluetooth is not supported
            if (bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Bluetooth is not available", ToastLength.Long).Show();
                Finish();
                return;
            }

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            txtConnectedDevice = FindViewById<TextView>(Resource.Id.txtConnectedDevice);
            button.Click += Button_Click;

            Button btnIniciar = FindViewById<Button>(Resource.Id.btnIniciar);
            Button btnFinalizar = FindViewById<Button>(Resource.Id.btnFinalizar);

            btnIniciar.Click += BtnIniciar_Click;
            btnFinalizar.Click += BtnFinalizar_Click;

            StartCommunicationService();
        }

        private void StartCommunicationService()
        {
            service = new CommunicationService(this, new CommunicationHandler());
            outStringBuffer = new StringBuffer("");
        }

        private void SendMessage(Java.Lang.String message)
        {
            // Check that we're actually connected before trying anything
            if (service.GetState() != ConnectionState.STATE_CONNECTED)
            {
                Toast.MakeText(this, Resource.String.not_connected, ToastLength.Short).Show();
                return;
            }

            // Check that there's actually something to send
            if (message.Length() > 0)
            {
                // Get the message bytes and tell the BluetoothChatService to write
                byte[] send = message.GetBytes();
                service.Write(send);

                // Reset out string buffer to zero and clear the edit text field
                outStringBuffer.SetLength(0);
            }
        }

        private void BtnFinalizar_Click(object sender, EventArgs e)
        {
            SendMessage(new Java.Lang.String("Finalizar"));
        }

        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            SendMessage(new Java.Lang.String("Iniciar"));
        }

        protected override void OnStart()
        {
            base.OnStart();
            
            if (!bluetoothAdapter.IsEnabled)
            {
                Intent enableIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableIntent, REQUEST_ENABLE_BT);
                txtConnectedDevice.Text = "Nenhum dispositivo conectado.";
            }
            else
            {
                var pairedDevices = bluetoothAdapter.BondedDevices;                
                if (pairedDevices.Count > 0)
                {
                    foreach (var device in pairedDevices)
                    {
                        if (device.Name.Equals(CommunicationService.DEVICE_NAME))
                        {
                            txtConnectedDevice.Text = string.Format("Conectado ao {0}, id {1}", device.Name, device.Address);
                            service.Connect(device);
                            break;
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            var serverIntent = new Intent(this, typeof(DeviceListActivity));
            StartActivityForResult(serverIntent, REQUEST_CONNECT_DEVICE);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (Debug)
                Log.Debug(TAG, "onActivityResult " + resultCode);

            switch (requestCode)
            {
                case REQUEST_CONNECT_DEVICE:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Result.Ok)
                    {
                        // Get the device MAC address
                        var address = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                        // Get the BLuetoothDevice object
                        BluetoothDevice device = bluetoothAdapter.GetRemoteDevice(address);
                        txtConnectedDevice.Text = string.Format("Conectado ao {0}, id {1}", device.Name, device.Address);
                        service.Connect(device);
                    }
                    break;
                case REQUEST_ENABLE_BT:
                    // When the request to enable Bluetooth returns
                    if (resultCode == Result.Ok)
                    {
                    }
                    else
                    {
                        // User did not enable Bluetooth or an error occured
                        Log.Debug(TAG, "BT not enabled");
                        Toast.MakeText(this, Resource.String.bt_not_enabled_leaving, ToastLength.Short).Show();
                        Finish();
                    }
                    break;
            }
        }
    }
}

