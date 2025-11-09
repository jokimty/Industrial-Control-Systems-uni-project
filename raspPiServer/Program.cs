using System;
using System.Device.Gpio;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    internal class Program
    {
        private static bool globalAlarmStatus;
        private static object globalLock = new object();

        static void Main()
        {
            #region Variables
            // Can be modified to guarantee a maximum sampling time of once per <value>.
            // Real sampling time would be slightly longer due to computation.
            int limitSamplingRate = 3000;
            bool localAlarmStatus = false;
            object lockObject = new object();
            int messageCounter = 0;

            // Button controllers
            GpioController controllerBtn1;
            GpioController controllerBtn2;
            GpioController controllerBtn3;
            GpioController controllerBtn4;

            // Pins, GPIO 25, 8, 7, 1
            int buttonPin1 = 22;
            int buttonPin2 = 24;
            int buttonPin3 = 26;
            int buttonPin4 = 28;

            // Connection
            IPAddress ipAddress;
            IPEndPoint localEndPoint;
            Socket? listener = null;
            Socket? handler = null;
            #endregion

            #region Initialize Pins
            // Commented out for testing
            controllerBtn1 = new GpioController(PinNumberingScheme.Board);
            controllerBtn2 = new GpioController(PinNumberingScheme.Board);
            controllerBtn3 = new GpioController(PinNumberingScheme.Board);
            controllerBtn4 = new GpioController(PinNumberingScheme.Board);

            controllerBtn1.OpenPin(buttonPin1, PinMode.InputPullUp);
            controllerBtn2.OpenPin(buttonPin2, PinMode.InputPullUp);
            controllerBtn3.OpenPin(buttonPin3, PinMode.InputPullUp);
            controllerBtn4.OpenPin(buttonPin4, PinMode.InputPullUp);
            Console.WriteLine("Initalization done");
            #endregion

            #region Start Server

            // Real IP for raspberry pi
            ipAddress = IPAddress.Parse("169.254.88.165");

            // Testing locally
            //ipAddress = IPAddress.Parse("127.0.0.1");

            localEndPoint = new IPEndPoint(ipAddress, 11800);
            try
            {
                // Create a Socket that will use Tcp protocol
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);

                // This server will only need to connect to our individual computer.
                listener.Listen(1);
                Console.WriteLine("Waiting for a connection...");
                handler = listener.Accept();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            #endregion

            #region Turn alarms on/off thread
            Thread alarmStatusThread = new Thread(
                () => AlarmStatusThreadMethod(controllerBtn1, buttonPin1));
            alarmStatusThread.IsBackground = true;
            alarmStatusThread.Start();
            alarmStatusThread.Name = "Alarms Status thread";
            #endregion

            while (true)
            {
                try
                {
                    string sendString;

                    #region Read Measurements
                    // Figure out values to return

                    lock (globalLock)
                    {
                        localAlarmStatus = globalAlarmStatus;
                    }
                    if (localAlarmStatus)
                    {

                        sendString = "1";

                        // Pinvalue being low means the button is pressed down.
                        if (controllerBtn2.Read(buttonPin2) == PinValue.Low) { sendString += "1"; }
                        else { sendString += "0"; }
                        if (controllerBtn3.Read(buttonPin3) == PinValue.Low) { sendString += "1"; }
                        else { sendString += "0"; }
                        if (controllerBtn4.Read(buttonPin4) == PinValue.Low) { sendString += "1"; }
                        else { sendString += "0"; }
                    }
                    else
                    {
                        sendString = "0000";
                    }
                    #endregion

                    #region Send measurements
                    // Get command to read and send values
                    string data = null;
                    byte[] bytes = new byte[1024];

                    //Console.WriteLine("Waiting to receive");
                    int bytesRec = handler.Receive(bytes);

                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("True") == 0)
                    {
                        // Send values
                        byte[] msg = Encoding.ASCII.GetBytes(sendString);
                        Console.WriteLine($"Sending message number {messageCounter}: " + sendString);
                        if (handler != null)
                        {
                            handler.Send(msg);
                            Thread.Sleep(limitSamplingRate);

                            messageCounter++;
                        }
                        else
                        {
                            Thread.Sleep(limitSamplingRate);
                        }
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString() + Environment.NewLine);
                    break;
                }
            }
            // Connection has been broken
            handler.Close();
            listener.Close();
            // Restart program, try to establish a new connection.
            controllerBtn1.ClosePin(buttonPin1);
            controllerBtn2.ClosePin(buttonPin2);
            controllerBtn3.ClosePin(buttonPin3);
            controllerBtn4.ClosePin(buttonPin4);
            Main();
        }

        private static void AlarmStatusThreadMethod(GpioController button, int buttonPin)
        {
            bool alarmStatus = false;
            try
            {
                while (true)
                {
                    // Pinvalue being low means the button is pressed down.
                    if (button.Read(buttonPin) == PinValue.Low)
                    {
                        alarmStatus = !alarmStatus; // Toggle it
                        lock (globalLock)
                        {
                            globalAlarmStatus = alarmStatus; // Pass to global variable for sharing
                        }
                        if (alarmStatus) { Console.WriteLine("Alarms turned ON"); }
                        else { Console.WriteLine("Alarms turned OFF"); }
                    }
                    Thread.Sleep(1000); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}