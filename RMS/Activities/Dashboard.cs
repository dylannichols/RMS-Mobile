using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            // General set up
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

            // Set up page layout
            LinearLayout contentMain = FindViewById<LinearLayout>(Resource.Id.contentMain);

            LayoutInflater inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);

            View dashLayout = inflater.Inflate(Resource.Layout.dashboard, null, true);
            contentMain.AddView(dashLayout);

            // Set up dashboard
            Token = Intent.Extras.GetString("Token");
            Node = Intent.Extras.GetInt("Node");
            var dashboard = GetDashboard();

            if (dashboard != null)
            {
                LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.dashLayout);
                foreach (Header h in dashboard)
                {
                    var table = CreateTable(h);
                    layout.AddView(table);
                }
            }
        }

        // Creates a table for each category in dashboard
        TableLayout CreateTable(Header h)
        {
            // Create border for each table
            GradientDrawable border = new GradientDrawable();

            border.SetColor(Android.Graphics.Color.White);
            border.SetStroke(1, Android.Graphics.Color.Gray);
            border.SetCornerRadius(8);

            // Set up layout of table
            TableLayout.LayoutParams layoutparams = new TableLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            layoutparams.SetMargins(50, 30, 50, 30);

            var table = new TableLayout(this)
            {
                LayoutParameters = layoutparams,
            };

            table.Background = border;
            table.SetZ(30);

            table.Click += (s, arg) =>
            {
                table.SetZ(80);
            };

            // Set up title for table
            var heading = new TableRow(this)
            {
                LayoutParameters = new TableRow.LayoutParams(ViewGroup.LayoutParams.MatchParent, TableLayout.LayoutParams.WrapContent)
            };
            var title = new TextView(this)
            {
                LayoutParameters = heading.LayoutParameters,
                Text = h.header,
                TextSize = 20,
                Gravity = GravityFlags.CenterHorizontal,
            };
            title.SetPadding(10, 20, 0, 20);
            title.SetTextColor(Android.Graphics.Color.White);
            heading.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3f51b5"));

            heading.AddView(title);
            table.AddView(heading);

            // add items to table
            foreach (DashItem item in h.items)
            {
                var row = CreateRow(item);
                table.AddView(row);
            }
            return table;
        }

        // Set up rows
        TableRow CreateRow(DashItem item)
        {
            // Row layout
            TableRow.LayoutParams rowParams = new TableRow.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 10);
            var row = new TableRow(this)
            {
                LayoutParameters = rowParams
            };

            TableRow.LayoutParams param = new TableRow.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 6)
            {
                LeftMargin = 15,
                TopMargin = 5,
                BottomMargin = 5
            };

            // Label for dash item
            var label = new TextView(this)
            {
                Text = item.display_detail,
                Gravity = GravityFlags.Left,
                LayoutParameters = param
            };

            // Set up right hand side of dash item for different types of content
            if (item.display_unit == "check")
            {
                var check = CreateCheck(item);
                row.AddView(label);
                row.AddView(check);
            }
            else if (item.display_unit == "combo")
            {
                var spinner = CreateCombo(item);
                row.AddView(label);
                row.AddView(spinner);
            }
            else
            {
                // Default set up for right hand side
                TableRow.LayoutParams margin = new TableRow.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    RightMargin = 15
                };

                // Value of item
                var value = new TextView(this)
                {
                    Text = item.display_value,
                    Gravity = GravityFlags.Right
                };
                row.AddView(label);
                row.AddView(value);

                // Unit of item, list is left out
                if (item.display_unit != "list")
                {
                    var unit = new TextView(this)
                    {
                        Text = item.display_unit,
                        Gravity = GravityFlags.Right,
                        LayoutParameters = margin
                    };
                    row.AddView(unit);
                }
            }

            return row;
        }

        // Creates a switch for checkbox items
        Switch CreateCheck(DashItem item)
        {
            // Set up switch
            var check = new Switch(this)
            {
                Gravity = GravityFlags.Right
            };

            if (item.display_value == "On")
            {
                check.Checked = true;
            }
            else
            {
                check.Checked = false;
            }
            int address_id = item.address_id;
            int node_id = item.node_id;

            // Event listener for when user interacts with switch
            check.Click += async (s, arg) =>
            {
                var uri = new Uri(string.Format($"http://13.210.251.7/api/nodes/{node_id}/dashboard/{address_id}"));
                var val = item.display_value;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "PATCH";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + Token);

                using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
                {
                    string json = "{\"data\":\"" + val + "\",\n\"type\": \"check\"}";

                    streamWriter.Write(json);
                }

                var httpResponse = await httpWebRequest.GetResponseAsync();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var toast = Toast.MakeText(this, result.ToString(), ToastLength.Short);
                    toast.Show();
                }
            };

            return check;
        }

        // Creates a spinner for combo dropdowns
        Spinner CreateCombo(DashItem item)
        {
            // Set up spinner
            List<string> labels = new List<string>();
            foreach (Combo combo in item.combos)
            {
                labels.Add(combo.label);
            }
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, labels);

            var spinner = new Spinner(this)
            {
                Adapter = adapter
            };
            int address_id = item.address_id;
            int node_id = item.node_id;

            // Event listener for when user interacts with spinner
            spinner.ItemSelected += async (s, arg) =>
            {
                var uri = new Uri(string.Format($"http://13.210.251.7/api/nodes/{node_id}/dashboard/{address_id}"));
                var val = item.display_value;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "PATCH";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + Token);

                using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
                {
                    string json = "{\"data\":\"" + val + "\",\n\"type\": \"combo\"}";

                    streamWriter.Write(json);
                }

                try
                {
                    var httpResponse = await httpWebRequest.GetResponseAsync();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        var toast = Toast.MakeText(this, result.ToString(), ToastLength.Short);
                        toast.Show();
                    }
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine(ex);
                }

            };
            return spinner;
        }


        // API call to initialize dashboard content
        private List<Header> GetDashboard()
        {
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Set("Authorization", "Bearer " + Token);
                wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + Token;
                Console.WriteLine(wc.Headers[HttpRequestHeader.Authorization]);
                try
                {
                    string json = wc.DownloadString("http://13.210.251.7/api/nodes/" + Node + "/dashboard");
                    JObject data = JObject.Parse(json);

                    string dashJSON = data["data"].ToString();
                    List<Header> dashboard = JsonConvert.DeserializeObject<List<Header>>(dashJSON);

                    return dashboard;
                }
                catch (WebException e)
                {
                    // if dashboard call fails, go back to node select and display an error
                    var intent = new Intent(this, typeof(NodeSelect))
                   .SetFlags(ActivityFlags.ReorderToFront);

                    intent.PutExtra("Error", e.Message);
                    StartActivity(intent);

                    return null;
                }
            }
        }
    }
}