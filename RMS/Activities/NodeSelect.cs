using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
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
using Xamarin.Essentials;

namespace RMS.Activities
{
    [Activity(Label = "Select Your Node")]
    public class NodeSelect : MainActivity
    {

        public string Token;
        private List<Node> Nodes;
        public ScrollView ScrollView;
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

            Density = DeviceDisplay.MainDisplayInfo.Density;

            // Page layout set up
            ContentMain = FindViewById<LinearLayout>(Resource.Id.contentMain);

            LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);

            View nodeSelectView = inflater.Inflate(Resource.Layout.node_select, null, true);
            ContentMain.AddView(nodeSelectView);

            var menu = navigationView.Menu;
            var userMenu = menu.FindItem(Resource.Id.userMenu);
            userMenu.SetVisible(true);

            // Check for dashboard errors
            if (Intent.Extras.GetString("Error") != null)
            {
                var toast = Toast.MakeText(this, Intent.Extras.GetString("Error"), ToastLength.Short);
                toast.Show();
            }
            // Set up buttons for Nodes
            Token = Intent.Extras.GetString("Token");

            GetNodes();

            // If user only has access to one node then send them straight to dashboard
            if (Nodes.Count == 1)
            {
                Intent activity = new Intent(this, typeof(Dashboard));
                activity.PutExtra("Node", Nodes[0].idx);
                activity.PutExtra("Name", Nodes[0].node_name);
                activity.PutExtra("Token", Token);
                StartActivity(activity);
            }
            // otherwise display buttons for user to select their node
            else
            {
                CreateButtons();
            }

        }

        private void CreateButtons()
        {

            ScrollView = FindViewById<ScrollView>(Resource.Id.nodeScrollView);

            LinearLayout.LayoutParams layoutparams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            LinearLayout layout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = layoutparams,
            };
            layout.SetGravity(GravityFlags.Center);

            ScrollView.AddView(layout);

            foreach (Node n in Nodes)
            {
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                lp.SetMargins(DpToPx(10), DpToPx(5), DpToPx(10), DpToPx(5));


                var button = new Button(this)
                {
                    Text = n.node_name,
                    Id = n.idx,
                    LayoutParameters = lp
                };
                button.SetMinimumWidth(DpToPx(300));
                button.SetMaxWidth(DpToPx(500));
                button.SetMaxHeight(DpToPx(5));
                //button.SetPadding(0, -10, 0, -10);

                GradientDrawable border = new GradientDrawable();

                border.SetColor(Android.Graphics.Color.ParseColor("#3f51b5"));
                border.SetCornerRadius(DpToPx(15));

                button.SetTextColor(Android.Graphics.Color.White);
                button.Background = border;

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

        // API call to initialize Nodes
        private void GetNodes()
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

                    Nodes = JsonConvert.DeserializeObject<List<Node>>(nodeJson);
                }
                // if there's an error, display it to the user
                catch (WebException e)
                {
                    var toast = Toast.MakeText(this, e.Message, ToastLength.Short);
                    toast.Show();
                }

            }
        }


        public new bool OnNavigationItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.logoutNav)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTask);
                StartActivity(intent);
            }
            else
            {
                var node = Nodes.Find(x => x.node_name == item.TitleFormatted.ToString());
                Intent activity = new Intent(this, typeof(Dashboard));
                activity.PutExtra("Node", node.idx);
                activity.PutExtra("Name", node.node_name);
                activity.PutExtra("Token", Token);
                StartActivity(activity);
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.topMenu)
            {
                ScrollView.ScrollTo(0, 0);
            }
            else if (id == Resource.Id.bottomMenu)
            {
                ScrollView.FullScroll(FocusSearchDirection.Down);
            }
            else if (id == Resource.Id.logoutMenu)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}