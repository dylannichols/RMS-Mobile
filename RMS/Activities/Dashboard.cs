﻿using System;
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
using RMS.Models;

namespace RMS.Activities
{
    [Activity(Label = "Dashboard")]
    public class Dashboard : MainActivity
    {
        public string Token;
        public string Name;
        public int Node;
        public int i;
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
            navigationView.SetNavigationItemSelectedListener(this);

            // Set up dashboard
            Token = Intent.Extras.GetString("Token");
            Node = Intent.Extras.GetInt("Node");
            Name = Intent.Extras.GetString("Name");

            var menu = navigationView.Menu;

            var userMenu = menu.FindItem(Resource.Id.userMenu);
            userMenu.SetVisible(true);

            IMenuItem dashMenu = menu.FindItem(Resource.Id.dashMenu);
            dashMenu.SetTitle(Name);
            dashMenu.SetTitleCondensed(Name);
            dashMenu.SetVisible(true);

            this.Title = Name;

            InitializePage(true);
        }

        // Begins the process of getting the data and setting up the views that display that data
        void InitializePage(bool firstLoad)
        {
            // call the API
            var dashboard = GetDashboard();

            // If the API call was successful then start the process of setting up
            if (dashboard != null)
            {
                // Some layout scaffolding must be done if the activity is being loaded for the first time
                if (firstLoad)
                {
                    LinearLayout contentMain = FindViewById<LinearLayout>(Resource.Id.contentMain);

                    var select = CreateNavDropdown(dashboard);
                    contentMain.AddView(select);

                    LayoutInflater inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);

                    View dashLayout = inflater.Inflate(Resource.Layout.dashboard, null, true);
                    contentMain.AddView(dashLayout);

                    SwipeRefreshLayout swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeLayout);
                    swipeRefreshLayout.Refresh += (s, arg) =>
                    {
                        swipeRefreshLayout.Refreshing = true;
                        InitializePage(false);
                        swipeRefreshLayout.Refreshing = false;
                    };
                }


                i = 0;
                LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.dashLayout);
                layout.RemoveAllViews();

                var timer = RefreshTimer();
                layout.AddView(timer);

                foreach (Header h in dashboard)
                {
                    var table = CreateTable(h);
                    layout.AddView(table);
                }

                var toast = Toast.MakeText(this, "Data successfully updated", ToastLength.Short);
                toast.Show();
            }
        }

        // Creates the navigation at the top of the page where the user can select a table from the dropdown
        LinearLayout CreateNavDropdown(List<Header> dash)
        {
            // Create a list of headers
            List<string> headers = new List<string>();
            foreach (Header h in dash)
            {
                string label = h.header + " " + h.sub_header;
                headers.Add(label);
            }

            // This has to be created dynamically so it can be above the scrollview in contentmain
            LinearLayout select = new LinearLayout(this)
            { Orientation = Orientation.Horizontal };
            select.SetHorizontalGravity(GravityFlags.Center);
            select.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3f51b5"));
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, headers);

            // Create the dropdown menu that will be used to select a table
            Spinner spinner = new Spinner(this)
            {
                Adapter = adapter
            };
            spinner.SetSelection(0);

            spinner.SetGravity(GravityFlags.CenterHorizontal);
            spinner.BackgroundTintList = GetColorStateList(Resource.Color.primaryTextColor);

            // Event listener so that when a user selects a header they are sent to the relevant table
            spinner.ItemSelected += (s, arg) =>
            {
                var header = spinner.SelectedItemPosition;

                View item = FindViewById(header);

                NestedScrollView sv = FindViewById<NestedScrollView>(Resource.Id.dashScroll);

                sv.ScrollTo(0, item.Top);
            };

            select.SetMinimumHeight(100);
            select.AddView(spinner);

            // Add the refresh button to the bar
            var refresh = CreateRefreshButton();
            select.AddView(refresh);

            return select;
        }

        // Initialize the refresh button and add the event listener
        ImageButton CreateRefreshButton()
        {
            ImageButton refresh = new ImageButton(this);

            Drawable refreshImage = GetDrawable(Resource.Drawable.ic_refresh_white_36dp);
            refresh.Background = refreshImage;
            refresh.SetForegroundGravity(GravityFlags.End);
            refresh.SetScaleType(ImageView.ScaleType.FitXy);

            refresh.Click += (s, arg) =>
            {
                InitializePage(false);
            };

            return refresh;
        }

        // Tells the user how to refresh and when the page was last refreshed
        TextView RefreshTimer()
        {
            var date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            TextView refreshTimer = new TextView(this)
            {
                Text = "Pull down or tap button to refresh data\nLast refreshed: " + date,
                Gravity = GravityFlags.CenterHorizontal
            };

            refreshTimer.SetPadding(0, 20, 0, 0);
            return refreshTimer;
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
            check.Click += (s, arg) =>
            {
                UpdateDashItem("check", item.display_value, node_id, address_id);
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

            int position = labels.IndexOf(item.display_value);

            spinner.SetSelection(position);
            int address_id = item.address_id;
            int node_id = item.node_id;

            // Event listener for when user interacts with spinner
            spinner.ItemSelected += (s, arg) =>
            {
                UpdateDashItem("combo", item.display_value, node_id, address_id);
            };
            return spinner;
        }

        // API call to update dashboard item 
        private async void UpdateDashItem(string type, string value, int node_id, int address_id)
        {
            var uri = new Uri(string.Format($"http://13.210.251.7/api/nodes/{node_id}/dashboard/{address_id}"));

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + Token);

            using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                string json = "{\"data\":\"" + value + "\",\n\"type\": \"" + type + "\"}";

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

                    Intent intent = new Intent(this, typeof(NodeSelect));
                    intent.SetFlags(ActivityFlags.ReorderToFront);

                    intent.PutExtra("Error", e.Message);
                    StartActivity(intent);


                    return null;
                }
            }
        }

        public override bool OnNavigationItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.logoutNav)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }
            else if (item.ItemId == Resource.Id.dashNav)
            {
                InitializePage(false);
            }
            else if (item.ItemId == Resource.Id.selectNav)
            {
                Intent activity = new Intent(this, typeof(NodeSelect));
                activity.PutExtra("Token", Token);
                StartActivity(activity);
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}