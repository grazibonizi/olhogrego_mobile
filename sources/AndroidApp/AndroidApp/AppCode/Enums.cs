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

namespace AndroidApp.AppCode
{
    public enum ConnectionState
    {
        STATE_NONE = 0,
        STATE_LISTEN = 1,
        STATE_CONNECTING = 2,
        STATE_CONNECTED = 3
    }

    public enum MessageState
    {
        MESSAGE_STATE_CHANGE = 1,
        MESSAGE_READ = 2,
        MESSAGE_WRITE = 3,
        MESSAGE_DEVICE_NAME = 4,
        MESSAGE_TOAST = 5
    }
}