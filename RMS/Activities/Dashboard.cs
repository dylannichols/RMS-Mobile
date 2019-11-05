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

            select.SetGravity(GravityFlags.Center);
            select.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3f51b5"));
            select.Elevation = 50;
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

                sv.ScrollTo(0, (item.Top - 20));
            };


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


            Drawable refreshImage = GetDrawable(Resource.Drawable.ic_refresh_white_24dp);
            refresh.Background = refreshImage;
            refresh.SetForegroundGravity(GravityFlags.Right);

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

            refreshTimer.SetPadding(0, DpToPx(20), 0, 0);
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
            TableLayout.LayoutParams layoutparams = new TableLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            layoutparams.SetMargins(DpToPx(15), DpToPx(10), DpToPx(15), DpToPx(10));

            var table = new TableLayout(this)
            {
                LayoutParameters = layoutparams,
                Id = i
            };
            i++;
            //table.SetMinimumWidth(DpToPx(250));

            table.Background = border;
            table.SetZ(30);

            table.Click += (s, arg) =>
            {
                if (table.GetZ() == 30)
                {
                    table.SetZ(80);
                }
                else
                {
                    table.SetZ(30);
                }
            };

            // Set up title for table
            var heading = new TableRow(this)
            {
                LayoutParameters = new TableRow.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
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

            title.SetPadding(DpToPx(10), DpToPx(10), DpToPx(10), DpToPx(10));
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

            bool dark = false;
            // add items to table
            foreach (DashItem item in h.items)
            {
                var row = CreateRow(item, dark);
                table.AddView(row);
                if (dark)
                {
                    dark = false;
                }
                else
                {
                    dark = true;
                }
            }
            return table;
        }

        // Set up rows
        TableRow CreateRow(DashItem item, bool dark)
        {
            // Row layout
            TableRow.LayoutParams rowParams = new TableRow.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 10);
            var row = new TableRow(this)
            {
                LayoutParameters = rowParams
            };
            if (dark)
            {
                row.SetBackgroundColor(Android.Graphics.Color.ParseColor("#E8E8E8"));
            }

            TableRow.LayoutParams param = new TableRow.LayoutParams(ViewGroup.LayoutParams.WrapContent, DpToPx(28), 5);
            param.LeftMargin = DpToPx(10);

            // Set up attributes for text
            var textColour = Android.Graphics.Color.ParseColor("#212529");
            var textSize = 15;


            // Label for dash item
            var label = new TextView(this)
            {
                Text = item.display_detail,
                Gravity = GravityFlags.Left
            };
            label.SetTextSize(Android.Util.ComplexUnitType.Sp, textSize);
            label.SetTextColor(textColour);


            label.SetPadding(DpToPx(10), 0, 0, 0);
            row.AddView(label);

            // Set up right hand side of dash item for different types of content
            LinearLayout right = new LinearLayout(this);
            right.LayoutParameters = param;
            right.SetGravity(GravityFlags.Right);
            right.SetPadding(0, 0, DpToPx(10), 0);

            if (item.display_unit == "check")
            {
                var unit = new TextView(this)
                {
                    Text = item.display_value,
                    Gravity = GravityFlags.CenterVertical,
                };
                unit.SetTextSize(Android.Util.ComplexUnitType.Sp, textSize);
                unit.SetTextColor(textColour);

                var check = CreateCheck(item, unit);
                right.AddView(check);

                right.AddView(unit);
            }
            else if (item.display_unit == "combo")
            {
                var spinner = CreateCombo(item);
                right.AddView(spinner);
            }
            else
            {
                // Default set up for right hand side

                // Value of item
                var value = new TextView(this)
                {
                    Text = item.display_value,
                    Gravity = GravityFlags.Right,
                };
                value.SetPadding(0, 0, DpToPx(3), 0);
                value.SetTextSize(Android.Util.ComplexUnitType.Sp, textSize);
                value.SetTextColor(textColour);

                right.AddView(value);

                // Unit of item, list is left out
                if (item.display_unit != "list" && item.display_unit != "switch" && item.display_unit != "time")
                {
                    var unit = new TextView(this)
                    {
                        Text = item.display_unit,
                        Gravity = GravityFlags.Right
                    };
                    unit.SetTextSize(Android.Util.ComplexUnitType.Sp, textSize);
                    unit.SetTextColor(textColour);

                    right.AddView(unit);

                }

            }
            row.AddView(right);

            return row;
        }



        // Creates a switch for checkbox items
        Switch CreateCheck(DashItem item, TextView label)
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
                if (label.Text == "On")
                {
                    label.Text = "Off";
                }
                else
                {
                    label.Text = "On";
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