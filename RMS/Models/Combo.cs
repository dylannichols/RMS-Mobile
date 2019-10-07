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
    class Combo
    {
        public int id { get; set; }
        public int node_id { get; set; }
        public int address_id { get; set; }
        public int value_id { get; set; }
        public string label { get; set; }
    }
}