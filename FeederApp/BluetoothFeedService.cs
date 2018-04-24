using System;
using System.IO;
using System.Runtime.CompilerServices;
using Android.Bluetooth;
using Android.OS;
using Android.Util;
using Java.Lang;
using Java.Util;

namespace FeederApp
{
    class BluetoothFeedService
    {
        const string TAG = "BluetoothFeedService";
        ConnectedThread connectedThread;
        int state;

        public const int STATE_NONE = 0;
        public const int STATE_CONNECTED = 1;

        public BluetoothFeedService()
        {
            state = STATE_NONE;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetState()
        {
            return state;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }
            state = STATE_NONE;
        }

        public void Write(byte[] @out)
        {
            // Create temporary object
            ConnectedThread r;
            // Synchronize a copy of the ConnectedThread
            lock (this)
            {
                if (state != STATE_CONNECTED)
                {
                    return;
                }
                r = connectedThread;
            }
            // Perform the write unsynchronized
            r.Write(@out);
        }

        class ConnectedThread : Thread
        {
            BluetoothSocket socket;
            Stream inStream;
            Stream outStream;
            BluetoothFeedService service;

            public ConnectedThread(BluetoothSocket socket, BluetoothFeedService service)
            {
                this.socket = socket;
                this.service = service;
                Stream tmpIn = null;
                Stream tmpOut = null;

                // Get the BluetoothSocket input and output streams
                try
                {
                    tmpIn = socket.InputStream;
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "temp sockets not created", e);
                }
                try
                {
                    tmpOut = socket.OutputStream;
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "temp sockets not created", e);
                }
                inStream = tmpIn;
                outStream = tmpOut;
                service.state = STATE_CONNECTED;
            }

            public override void Run()
            {
                byte[] buffer = new byte[1];
                int bytes;

                // Keep listening to the InputStream while connected
                while (service.GetState() == STATE_CONNECTED)
                {
                    try
                    {
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                    }
                    catch (Java.IO.IOException e)
                    {
                        Log.Error(TAG, "disconnected", e);
                        break;
                    }
                }
            }

            public void Write(byte[] buffer)
            {
                try
                {
                    outStream.Write(buffer, 0, buffer.Length);
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "Exception during write", e);
                }
            }

            public void Cancel()
            {
                try
                {
                    socket.Close();
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "close() of connect socket failed", e);
                }
            }
        }
    }
}