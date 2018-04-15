using Android.App;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System;
using Android.Content;
using Android.Runtime;
using Java.Util;

namespace FeederApp
{   


    [Activity(Label = "FeederApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        static int REQUEST_ENABLE_BT = 1;

        class MyReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {

                Toast.MakeText(context, "Received bluetooth device!", ToastLength.Short).Show();
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                Toast.MakeText(context, device.Name, ToastLength.Short).Show();
                BluetoothAdapter.DefaultAdapter.CancelDiscovery();
            }    
        }

        BroadcastReceiver BtReceiver = new MyReceiver();

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            TextView txtBtLog = FindViewById<TextView>(Resource.Id.txtBtLog);

            if (requestCode == REQUEST_ENABLE_BT)
            {
                if (resultCode == Result.Ok)
                {
                    txtBtLog.Text = "Bluetooth enabled.";
                }
            }
        }

    

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

            else if (adapter.IsEnabled)
            {
                adapter.StartDiscovery();
            
                // Register for broadcasts when a device is discovered.
                IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
                RegisterReceiver(BtReceiver, filter);

                
            }



            FindViewById<Button>(Resource.Id.btnMain).Click += (o, e) =>
            txtFeedLog.Text = "Last fed: " + DateTime.Now.ToString("dd/MM hh:mm");

            
        }


        
    protected override void OnDestroy()
        {
            base.OnDestroy();

            UnregisterReceiver(BtReceiver);
        }
        
    }

}

