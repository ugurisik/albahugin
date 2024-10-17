using Hugin.ExDevice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace albahugin.Hugin.ExDevice {
    public class AsyncClient
    {
        private const int port = 4444;

        private ManualResetEvent connectDone = new ManualResetEvent(initialState: false);

        private ManualResetEvent sendDone = new ManualResetEvent(initialState: false);

        private ManualResetEvent receiveDone = new ManualResetEvent(initialState: false);

        private List<byte> response = new List<byte>();

        private Socket client = null;

        private int bytesSent = 0;

        internal int Available => client.Available;

        public void StartClient(Socket socket)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("192.168.0.176"), 4444);
            client = socket;
            client.BeginConnect(remoteEP, ConnectCallback, client);
            connectDone.WaitOne();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                Console.WriteLine("Socket connected to {0}", socket.RemoteEndPoint.ToString());
                connectDone.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            bytesSent = 0;
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                bytesSent = socket.EndSend(ar);
                sendDone.Set();
            }
            catch
            {
            }
        }

        internal int Send(byte[] packet, int offset, int length)
        {
            return Send(packet, offset, length, SocketFlags.None);
        }

        internal int Send(byte[] packet, int offset, int length, SocketFlags sf)
        {
            client.BeginSend(packet, offset, length, SocketFlags.None, SendCallback, client);
            sendDone.WaitOne();
            return bytesSent;
        }

        internal byte[] Receive(int offset, int length)
        {
            return Receive(offset, length, SocketFlags.None);
        }

        internal byte[] Receive(int offset, int length, SocketFlags sf)
        {
            response = new List<byte>();
            receiveDone.Reset();
            Receive(client, offset, length);
            receiveDone.WaitOne(4000);
            int num = 0;
            while (response.Count < length)
            {
                Thread.Sleep(20);
                num += 20;
                if (num >= client.ReceiveTimeout)
                {
                    throw new SocketException();
                }
            }
            return response.ToArray();
        }

        private void Receive(Socket socket, int offset, int length)
        {
            StateObject stateObject = new StateObject();
            stateObject.workSocket = socket;
            socket.BeginReceive(stateObject.buffer, offset, length, SocketFlags.None, ReceiveCallback, stateObject);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            StateObject stateObject = (StateObject)ar.AsyncState;
            Socket workSocket = stateObject.workSocket;
            try
            {
                int num = workSocket.EndReceive(ar);
                if (num > 0)
                {
                    byte[] array = new byte[num];
                    Buffer.BlockCopy(stateObject.buffer, 0, array, 0, num);
                    stateObject.sb.AddRange(array);
                }
                if (num == 4096)
                {
                    workSocket.BeginReceive(stateObject.buffer, 0, 4096, SocketFlags.None, ReceiveCallback, stateObject);
                    return;
                }
                response.AddRange(stateObject.sb.ToArray());
                Thread.Sleep(20);
                stateObject.sb = new List<byte>();
                receiveDone.Set();
            }
            catch
            {
            }
        }

        private void LogData(string data)
        {
            data += Environment.NewLine;
            StreamWriter streamWriter = new StreamWriter("logAss.txt", append: true);
            streamWriter.Write(data);
            streamWriter.Close();
        }
    }
}


