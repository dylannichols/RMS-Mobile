using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RMS.Models;

namespace RMS.Adapters
{
    class DashboardAdapter : BaseAdapter
    {

        Context context;
        List<DashItem> items;

        public DashboardAdapter(Context context, List<DashItem> items)
        {
            this.context = context;
            this.items = items;
        }


        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            DashboardAdapterViewHolder holder = null;
            var item = items[position];

            if (view != null)
                holder = view.Tag as DashboardAdapterViewHolder;

            if (holder == null)
            {
                holder = new DashboardAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.dashtable, parent, false);
                holder.Detail = view.FindViewById<TextView>(Resource.Id.detail);
                holder.Value = view.FindViewById<TextView>(Resource.Id.value);
                holder.Unit = view.FindViewById<TextView>(Resource.Id.unit);
                holder.Switch = view.FindViewById<Switch>(Resource.Id.checkbox);
                holder.Spinner = view.FindViewById<Spinner>(Resource.Id.spinner);

                holder.Detail.Text = item.display_detail;

                if (item.display_unit == "check")
                {
                    holder.Value.Visibility = ViewStates.Gone;
                    holder.Unit.Visibility = ViewStates.Gone;
                    holder.Spinner.Visibility = ViewStates.Gone;

                    holder.Switch.Visibility = ViewStates.Visible;
                    if (item.display_value == "On")
                    {
                        holder.Switch.Checked = true;
                    }
                    else
                    {
                        holder.Switch.Checked = false;
                    }
                }
                else if (item.display_unit == "combo")
                {
                    holder.Value.Visibility = ViewStates.Gone;
                    holder.Unit.Visibility = ViewStates.Gone;
                    holder.Switch.Visibility = ViewStates.Gone;
                    holder.Spinner.Visibility = ViewStates.Visible;
                    List<string> labels = new List<string>();
                    foreach (Combo combo in item.combos)
                    {
                        labels.Add(combo.label);
                    }
                    var adapter = new ArrayAdapter<string>(view.Context, Android.Resource.Layout.SimpleSpinnerItem, labels);
                    holder.Spinner.Adapter = adapter;

                }
                else
                {
                    holder.Value.Visibility = ViewStates.Visible;
                    holder.Unit.Visibility = ViewStates.Visible;
                    holder.Switch.Visibility = ViewStates.Gone;
                    holder.Spinner.Visibility = ViewStates.Gone;

                    holder.Value.Text = item.display_value;
                    holder.Unit.Text = item.display_unit;

                }

                view.Tag = holder;
            }


            //fill in your items
            //holder.Title.Text = "new text here";

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return items.Count;
            }
        }

    }

    class DashboardAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Detail { get; set; }
        public TextView Value { get; set; }
        public TextView Unit { get; set; }
        public Switch Switch { get; set; }
        public Spinner Spinner { get; set; }

    }
}