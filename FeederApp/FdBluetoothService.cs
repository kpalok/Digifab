using System;
using Android.Bluetooth;
using Android.OS;
using Android.Content;
using Android.Widget;
using Java.IO;
using Android.App;

namespace FeederApp
{
    public class FdBluetoothService
    {
        public class ConnectedThread : BluetoothActivity.BaseThread
        {
            private BluetoothSocket mmSocket;
            private System.IO.Stream mmInStream;
            private System.IO.Stream mmOutStream;
            private Context context;

            public ConnectedThread(BluetoothSocket socket)
            {
                mmSocket = socket;
                System.IO.Stream tmpIn = null;
                System.IO.Stream tmpOut = null;

                // Get the input and output streams; using temp objects because
                // member streams are final.
                try
                {
                    tmpIn = socket.InputStream;
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Error occured when creating input stream", ToastLength.Long).Show();
                }
                try
                {
                    tmpOut = socket.OutputStream;
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Error occured when creating output stream", ToastLength.Long).Show();
                }

                mmInStream = tmpIn;
                mmOutStream = tmpOut;
            }

            public override void RunThread()
            {
                //Possible signal listening.
            }

            public void Write(byte[] bytes)
            {              
                try
                {
                    mmOutStream.Write(bytes, 0, 1);
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Error occured when sending data", ToastLength.Long).Show();
                }
            }

            public void Cancel()
            {
                try
                {
                    mmSocket.Close();
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Could not close the connect socket", ToastLength.Long).Show();
                }
            }
        }
    }  
}