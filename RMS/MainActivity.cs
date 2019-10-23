using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RMS.Activities;
using RMS.Models;

namespace RMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // General set up
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            // Page layout set up
            LinearLayout contentMain = FindViewById<LinearLayout>(Resource.Id.contentMain);
            //contentMain.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3f51b5"));

            LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);

            View loginView = inflater.Inflate(Resource.Layout.login, null, true);
            contentMain.AddView(loginView);

            Button loginBtn = FindViewById<Button>(Resource.Id.loginBtn);

            // Event listener for when user presses login button
            loginBtn.Click += async (s, arg) =>
            {
                var email = FindViewById<EditText>(Resource.Id.username).Text.ToString();
                var password = FindViewById<EditText>(Resource.Id.password).Text.ToString();
                User user = new User(email, password);

                string stringData = JsonConvert.SerializeObject(user);
                var contentData = new StringContent(stringData,
    System.Text.Encoding.UTF8, "application/json");
                Console.WriteLine(stringData);
                Console.WriteLine(contentData);

                HttpClient client = new HttpClient();

                var uri = new Uri(string.Format("http://13.210.251.7/api/auth/login"));
                HttpResponseMessage response;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                response = await client.PostAsync(uri, contentData);
                var responseJSON = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(responseJSON);

                // If api call is successful, then send the user to node select
                if (response.IsSuccessStatusCode)
                {
                    string token = data["access_token"].ToString();

                    Intent activity = new Intent(this, typeof(NodeSelect));
                    activity.PutExtra("Token", token);

                    StartActivity(activity);
                } 
                // Otherwise display an error message
                else
                {
                    TextView error = FindViewById<TextView>(Resource.Id.loginError);
                    var message = data["message"].ToString();
                    if (message == "Unauthorized")
                    {
                        message = "Incorrect login information.";
                    }
                    error.Text = message;                    
                 }             
            };        
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_camera)
            {
                // Handle the camera action
            }
            else if (id == Resource.Id.nav_gallery)
            {

            }
            else if (id == Resource.Id.nav_slideshow)
            {

            }
            else if (id == Resource.Id.nav_manage)
            {

            }
            else if (id == Resource.Id.nav_share)
            {

            }
            else if (id == Resource.Id.nav_send)
            {

            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

