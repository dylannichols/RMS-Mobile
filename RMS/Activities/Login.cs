using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RMS.Activities
{
    [Activity(Label = "Login", MainLauncher = true)]
    public class Login : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login);

            Button loginBtn = FindViewById<Button>(Resource.Id.loginBtn);

            var email = FindViewById<EditText>(Resource.Id.username);
            var password = FindViewById<EditText>(Resource.Id.password);

            loginBtn.Click += async (s, arg) =>
            {
                HttpClient client = new HttpClient();

                var uri = new Uri(string.Format("http://13.210.251.7/api/auth/login?email=" + email.Text.ToString() + "&password=" + password.Text.ToString()));
                HttpResponseMessage response; ;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var activity = new Intent(this, typeof(NodeSelect));
                    StartActivity(activity);
                }


            };


        }
    }
}