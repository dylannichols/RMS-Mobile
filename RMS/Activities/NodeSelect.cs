using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

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
using RMS.Models;

namespace RMS.Activities
{
    [Activity(Label = "NodeSelect")]
    public class NodeSelect : MainActivity
    {

        public string Token;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            LinearLayout contentMain = FindViewById<LinearLayout>(Resource.Id.contentMain);

            LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);

            View nodeSelectView = inflater.Inflate(Resource.Layout.node_select, null, true);
            contentMain.AddView(nodeSelectView);

            Token = Intent.Extras.GetString("Token");

            List<Node> nodes = GetNodes();
            var layout = FindViewById<LinearLayout>(Resource.Id.nodeLayout);
            foreach (Node n in nodes)
            {
                Console.WriteLine(n.node_name);
                var button = new Button(this)
                {
                    Text = n.node_name,
                    Id = n.idx
                };

                button.Click += (s, arg) =>
                {
                    Intent activity = new Intent(this, typeof(Dashboard));
                    activity.PutExtra("Node", button.Id);
                    activity.PutExtra("Token", Token);
                    StartActivity(activity);
                };
                layout.AddView(button);
            }
        }

        private List<Node> GetNodes()
        {
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Set("Authorization", "Bearer " + Token);
                wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + Token;
                Console.WriteLine(wc.Headers[HttpRequestHeader.Authorization]);
                string json = wc.DownloadString("http://13.210.251.7/api/nodes");
                JObject data = JObject.Parse(json);
                string nodeJson = data["data"].ToString();
                Console.WriteLine(nodeJson);

                List<Node> nodes = JsonConvert.DeserializeObject<List<Node>>(nodeJson);
                Console.WriteLine("Nodes: " + nodes.Count);
                return nodes;
            }
        }
    }
}