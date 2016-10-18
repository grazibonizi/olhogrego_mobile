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
    public class Occurence
    {
        public string user_cpf { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
        public string dt_start { get; set; }
        public string dt_end { get; set; }
        public string src { get; set; }
        public int level { get; set; }
    }
}