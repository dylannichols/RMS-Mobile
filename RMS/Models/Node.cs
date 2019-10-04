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

namespace RMS.Models
{
    class Node
    {
        public int idx { get; set; }
        public string node_name { get; set; }
        public string hex { get; set; }
        public int connected { get; set; }
        public int lnk_address { get; set; }
        public string coords { get; set; }
        public int zoom { get; set; }
    }
}