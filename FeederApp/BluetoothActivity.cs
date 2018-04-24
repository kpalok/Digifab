using Android.Bluetooth;
using Android.App;
using Android.Content;

namespace FeederApp
{
    public class BluetoothActivity : Activity
    {
        public class MyReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                if (BluetoothDevice.ActionFound.Equals(action))
                {
                    // Discovery has found a device. Get the BluetoothDevice
                    // object and its info from the Intent.
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    MainActivity activity = new MainActivity();
                    activity.SetBtText("Connected to: " + device.Name);
                    BluetoothAdapter.DefaultAdapter.CancelDiscovery();
                }
            }
        }
    }
}




