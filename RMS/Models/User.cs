﻿using System;
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
    class User
    {
        //public int Id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        //public string Access_Token { get; set; }

        public User() { }
        public User(string e, string p)
        {
            email = e;
            password = p;
        }
    }
}