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
using System.IO;

namespace AndroidApp.AppCode
{
    public class CommunicationHandler : Handler
    {
        public CommunicationHandler()
        {
        }

        public override void HandleMessage(Message msg)
        {
            switch ((MessageState)msg.What)
            {
                case MessageState.MESSAGE_STATE_CHANGE:
                    switch ((ConnectionState)msg.Arg1)
                    {
                        case ConnectionState.STATE_CONNECTED:
                        //    bluetoothChat.title.SetText(Resource.String.title_connected_to);
                        //    bluetoothChat.title.Append(bluetoothChat.connectedDeviceName);
                        //    bluetoothChat.conversationArrayAdapter.Clear();
                            break;
                        case ConnectionState.STATE_CONNECTING:
                            //bluetoothChat.title.SetText(Resource.String.title_connecting);
                            break;
                        case ConnectionState.STATE_LISTEN:
                        case ConnectionState.STATE_NONE:
                            //bluetoothChat.title.SetText(Resource.String.title_not_connected);
                            break;
                    }
                    break;
                case MessageState.MESSAGE_WRITE:
                    byte[] writeBuf = (byte[])msg.Obj;
                    // construct a string from the buffer
                    //var writeMessage = new Java.Lang.String(writeBuf);
                    //bluetoothChat.conversationArrayAdapter.Add("Me: " + writeMessage);
                    break;
                case MessageState.MESSAGE_READ:
                    string filename = msg.Obj.ToString();
                    SendFile(filename);
                    //byte[] readBuf = (byte[])msg.Obj;
                    //WriteFile(readBuf);
                    // construct a string from the valid bytes in the buffer
                    //var readMessage = new Java.Lang.String(readBuf, 0, msg.Arg1);
                    //bluetoothChat.conversationArrayAdapter.Add(bluetoothChat.connectedDeviceName + ":  " + readMessage);
                    break;
                case MessageState.MESSAGE_DEVICE_NAME:
                    // save the connected device's name
                    //bluetoothChat.connectedDeviceName = msg.Data.GetString(CommunicationService.DEVICE_NAME);
                    //Toast.MakeText(Application.Context, "Connected to " + bluetoothChat.connectedDeviceName, ToastLength.Short).Show();
                    break;
                case MessageState.MESSAGE_TOAST:
                    //Toast.MakeText(Application.Context, msg.Data.GetString(CommunicationService.TOAST), ToastLength.Short).Show();
                    break;
            }
        }

        private async void SendFile(string filename)
        {
            string base64file = Convert.ToBase64String(File.ReadAllBytes(filename));
            var occurence = new Occurence()
            {
                user_cpf = "",
                lat = 1,
                lng = 2,
                dt_start = DateTime.Now.ToShortDateString(),
                dt_end = DateTime.Now.ToShortDateString(),
                level = 1,
                src = base64file
            };
            var result = await REST_API.RegisterOccurenceAsync(occurence);
        }

        private void WriteFile(byte[] bytes)
        {
        }
    }
}