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
    [Activity(Label = "Node Select")]
    public class NodeSelect : MainActivity
    {

        public string Token;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // General Set up
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

            // Page layout set up
            LinearLayout contentMain = FindViewById<LinearLayout>(Resource.Id.contentMain);

            LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);

            View nodeSelectView = inflater.Inflate(Resource.Layout.node_select, null, true);
            contentMain.AddView(nodeSelectView);

            // Check for dashboard errors
            if (Intent.Extras.GetString("Error") != null)
            {
                var toast = Toast.MakeText(this, Intent.Extras.GetString("Error"), ToastLength.Short);
                toast.Show();
            }
            // Set up buttons for nodes
            Token = Intent.Extras.GetString("Token");

            List<Node> nodes = GetNodes();

            // If user only has access to one node then send them straight to dashboard
            if (nodes.Count == 1)
            {
                Intent activity = new Intent(this, typeof(Dashboard));
                activity.PutExtra("Node", nodes[0].idx);
                activity.PutExtra("Name", nodes[0].node_name);
                activity.PutExtra("Token", Token);
                StartActivity(activity);
            }
            // otherwise display buttons for user to select their node
            else
            {
                var layout = FindViewById<LinearLayout>(Resource.Id.nodeLayout);

                foreach (Node n in nodes)
                {
                    var button = new Button(this)
                    {
                        Text = n.node_name,
                        Id = n.idx
                    };

                    // event listener for when user clicks a node
                    button.Click += (s, arg) =>
                    {
                        Intent activity = new Intent(this, typeof(Dashboard));
                        activity.PutExtra("Node", button.Id);
                        activity.PutExtra("Name", n.node_name);
                        activity.PutExtra("Token", Token);
                        StartActivity(activity);
                    };
                    layout.AddView(button);
                }
            }

        }

        // API call to initialize nodes
        private List<Node> GetNodes()
        {
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Set("Authorization", "Bearer " + Token);
                wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + Token;
                try
                {
                    string json = wc.DownloadString("http://13.210.251.7/api/nodes");
                    JObject data = JObject.Parse(json);
                    string nodeJson = data["data"].ToString();
                    Console.WriteLine(nodeJson);

                    List<Node> nodes = JsonConvert.DeserializeObject<List<Node>>(nodeJson);
                    return nodes;
                }
                // if there's an error, display it to the user
                catch (WebException e)
                {
                    var toast = Toast.MakeText(this, e.Message, ToastLength.Short);
                    toast.Show();
                    return null;
                }
                
            }
        }
    }
}