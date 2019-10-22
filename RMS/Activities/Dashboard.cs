using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using RMS.Adapters;
using RMS.Models;

namespace RMS.Activities
{
    [Activity(Label = "Dashboard")]
    public class Dashboard : MainActivity
    {
        public string Token;
        public int Node;
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

            View dashLayout = inflater.Inflate(Resource.Layout.dashboard, null, true);
            contentMain.AddView(dashLayout);

            Token = Intent.Extras.GetString("Token");
            Node = Intent.Extras.GetInt("Node");
            var dashboard = GetDashboard();

            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.dashLayout);
            foreach (Header h in dashboard)
            {
                TableLayout.LayoutParams layoutparams = new TableLayout.LayoutParams(TableLayout.LayoutParams.MatchParent, TableLayout.LayoutParams.WrapContent);

                var table = new TableLayout(this);
                table.LayoutParameters = layoutparams;

                var heading = new TableRow(this);
                var title = new TextView(this)
                {
                    Text = h.header,
                    TextSize = 20,
                    Gravity = GravityFlags.CenterHorizontal
                };

                heading.AddView(title);
                table.AddView(heading);

                foreach (DashItem item in h.items)
                {
                    TableRow.LayoutParams rowParams = new TableRow.LayoutParams(TableRow.LayoutParams.MatchParent, TableRow.LayoutParams.WrapContent, 10);
                    var row = new TableRow(this);
                    row.LayoutParameters = rowParams;

                    TableRow.LayoutParams param = new TableRow.LayoutParams(TableRow.LayoutParams.MatchParent, TableRow.LayoutParams.WrapContent, 6);

                    var label = new TextView(this)
                    {
                        Text = item.display_detail,
                        Gravity = GravityFlags.Left,
                        LayoutParameters = param
                    };


                    var value = new TextView(this)
                    {
                        Text = item.display_value,
                        Gravity = GravityFlags.Right
                    };

                    var unit = new TextView(this)
                    {
                        Text = item.display_unit,
                        Gravity = GravityFlags.Right
                    };

                    row.AddView(label);
                    row.AddView(value);
                    row.AddView(unit);

                    table.AddView(row);
                }
                layout.AddView(table);
            }

            //foreach (Header header in dashboard)
            //{
            //    LinearLayout.LayoutParams linearParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);

            //    LinearLayout listLayout = new LinearLayout(this)
            //    {
            //        LayoutParameters = linearParams,
            //        Orientation = Orientation.Vertical

            //    };
            //    ListView.LayoutParams layoutParams = new ListView.LayoutParams(ListView.LayoutParams.MatchParent, ListView.LayoutParams.WrapContent);
            //    var list = new ListView(this)
            //    {
            //        LayoutParameters = layoutParams,
            //        Adapter = new DashboardAdapter(this, header.items, Token)
            //    };
            //    TextView title = new TextView(this)
            //    {
            //        Text = header.header,
            //        TextSize = 25
            //    };
            //    listLayout.AddView(title);
            //    listLayout.AddView(list);
            //    layout.AddView(listLayout);
            //}

        }

        private List<Header> GetDashboard()
        {
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Set("Authorization", "Bearer " + Token);
                wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + Token;
                Console.WriteLine(wc.Headers[HttpRequestHeader.Authorization]);
                string json = wc.DownloadString("http://13.210.251.7/api/nodes/" + 56 + "/dashboard");
                JObject data = JObject.Parse(json);

                string dashJSON = data["data"].ToString();
                List<Header> dashboard = JsonConvert.DeserializeObject<List<Header>>(dashJSON);

                //if (data["combos"] != null)
                //{
                //    string comboJSON = data["combos"].ToString();
                //    List<Combo> combos = JsonConvert.DeserializeObject<List<Combo>>(comboJSON);
                //    foreach (Combo combo in combos)
                //    {
                //        var item = items.Find(x => x.address_id == combo.address_id);
                //        item.combos.Add(combo);
                //    }
                //}

                //string headerJSON = data["headers"].ToString();
                //List<Header> headers = JsonConvert.DeserializeObject<List<Header>>(headerJSON);
                //foreach (DashItem item in items)
                //{
                //    Header header = headers.Find(x => x.header == item.header && x.sub_header == item.sub_header);
                //    header.items.Add(item);
                //}

                return dashboard;
            }
        }
    }
}