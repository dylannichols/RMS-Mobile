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
        public int i = 0;
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


            // Set up dashboard
            Token = Intent.Extras.GetString("Token");
            Node = Intent.Extras.GetInt("Node");
            var dashboard = GetDashboard();

            var select = CreateNavDropdown(dashboard);
            contentMain.AddView(select);

            LayoutInflater inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);

            View dashLayout = inflater.Inflate(Resource.Layout.dashboard, null, true);
            contentMain.AddView(dashLayout);

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

        LinearLayout CreateNavDropdown(List<Header> dash)
        {
            List<string> headers = new List<string>();
            foreach (Header h in dash)
            {
                string label = h.header + " " + h.sub_header;
                headers.Add(label);
            }

            LinearLayout select = new LinearLayout(this)
            { Orientation = Orientation.Horizontal };
            select.SetHorizontalGravity(GravityFlags.Center);
            select.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3f51b5"));
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, headers);

            Spinner spinner = new Spinner(this)
            {
                Adapter = adapter
            };
            spinner.SetGravity(GravityFlags.CenterHorizontal);

            spinner.ItemSelected += (s, arg) =>
            {
                var header = spinner.SelectedItemPosition;

                View item = FindViewById(header);

                ScrollView sv = FindViewById<ScrollView>(Resource.Id.dashScroll);

                sv.ScrollTo(0, item.Top);
            };
            select.SetMinimumHeight(100);
            select.AddView(spinner);

            return select;
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
                Id = i
            };
            i++;

            table.Background = border;
            table.SetZ(30);

            table.Click += (s, arg) =>
            {
                table.SetZ(80);
            };

            // Set up title for table
            var heading = new TableRow(this)
            {
                LayoutParameters = new TableRow.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            LinearLayout titleContainer = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            var title = new TextView(this)
            {
                Text = h.header,
                TextSize = 20,
            };

            title.SetPadding(10, 20, 20, 20);
            title.SetTextColor(Android.Graphics.Color.White);
            heading.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3f51b5"));
            titleContainer.AddView(title);

            if (h.sub_header != "")
            {
                var subtitle = new TextView(this)
                {
                    Text = h.sub_header,
                    TextSize = 15,
                };

                subtitle.SetTextColor(Android.Graphics.Color.White);
                titleContainer.AddView(subtitle);
            }
            heading.AddView(titleContainer);
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
                using (TableRow.LayoutParams margin = new TableRow.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    RightMargin = 15
                })
                {

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
                        var json = streamReader.ReadToEnd();
                        JObject data = JObject.Parse(json);

                        var toast = Toast.MakeText(this, data["message"].ToString(), ToastLength.Short);
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
                    using (Intent intent = new Intent(this, typeof(NodeSelect))
                   .SetFlags(ActivityFlags.ReorderToFront))
                    {
                        intent.PutExtra("Error", e.Message);
                        StartActivity(intent);
                    }

                    return null;
                }
            }
        }
    }
}