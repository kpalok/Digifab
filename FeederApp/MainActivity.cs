using Android.App;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Content;
using Android.Views;
using System;

namespace FeederApp
{   
    [Activity(Label = "FeederApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        static int REQUEST_ENABLE_BT = 1;
        byte[] FEED = new byte[] { byte.MaxValue  };
        byte[] CLEAR = new byte[] { 0b0 };
        private BluetoothFeedService service = new BluetoothFeedService();
        private BroadcastReceiver BtReceiver = new BluetoothActivity.MyReceiver();
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);
            Button button = FindViewById<Button>(Resource.Id.btnMain);
            if (adapter == null)
            {

                SetBtText("Device doesn't support bluetooth!");
            }
            else if (!adapter.IsEnabled)
            {
                Intent enableBtIntetent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntetent, REQUEST_ENABLE_BT);
            }
            else
                SetBtText("Bluetooth enabled.");
        }

        [Java.Interop.Export("ButtonClick")]
        public void ButtonClick(View v)
        {
            Button button = (Button)v;
            if (adapter == null)
            {
                SetFeedText("Please connect to your device!");
            }
            else if (adapter.IsEnabled)
            {
                service.Write(FEED);
                SetFeedText("Last fed: " + DateTime.Now.ToString("dd/MM hh:mm"));
                service.Write(CLEAR);
            }                           
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_ENABLE_BT)
            {
                if (resultCode == Result.Ok)
                    SetBtText("Bluetooth enabled.");

                else if (resultCode == Result.Canceled)
                    SetBtText("Bluetooth disabled.");
            }
        }

        public void SetBtText(string txt)
        {
            TextView txtBtLog = FindViewById<TextView>(Resource.Id.txtBtLog);
            txtBtLog.Text = txt;
        }

        public void SetFeedText(string txt)
        {
            TextView txtFeedLog = FindViewById<TextView>(Resource.Id.txtFeedLog);
            txtFeedLog.Text = txt;
        }
    }
}

