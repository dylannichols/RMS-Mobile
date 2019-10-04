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
    class Header
    {
        public string header { get; set; }
        public string sub_header { get; set; }
        public List<DashItem> items { get; set; }
    }
}