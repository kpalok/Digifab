using Android.App;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System;
using Android.Content;


namespace FeederApp
{   

    [Activity(Label = "FeederApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        static int REQUEST_ENABLE_BT = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            TextView txtFeedLog = FindViewById<TextView>(Resource.Id.txtFeedLog);
            TextView txtBtLog = FindViewById<TextView>(Resource.Id.txtBtLog);

            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null)
            {
                txtBtLog.Text = "Device doesn't support Bluetooth";
            }

            else if (!adapter.IsEnabled)
            {
                Intent enableBtIntetent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntetent, REQUEST_ENABLE_BT);
            }

            FindViewById<Button>(Resource.Id.btnMain).Click += (o, e) =>
            txtFeedLog.Text = "Last fed: " + DateTime.Now.ToString("dd/MM hh:mm");

            // Register for broadcasts when a device is discovered.
            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
        }


        

        
    }
}

