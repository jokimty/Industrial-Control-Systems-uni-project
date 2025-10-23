using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCS_GUI
{
    public partial class Form1 : Form
    {
        #region Global Variables

        string alarmPath;
        int rowIndex;
        bool connectionStatus = false;
        Socket socketHandler;

        //Multitasking variables
        Thread sensorThread;
        Thread loggerThread;
        Thread updateDGVThread;
        BlockingCollection<AlarmClass> writeToAlarmFile = new(); // Logger thread takes from this and writes to the file
        BlockingCollection<AlarmClass> writeToGUI = new(); // UI thread takes from this and writes to DGV
        CancellationTokenSource sensorThreadTokenSource;
        // Class to handle alarms.
        AlarmClass? selectedAlarm;

        #endregion

        public Form1()
        {
            /*          TODO
            
            FetchNewMeasurements(); probably has many flaws that need to be ironed out
            Sensor thread must be finished and actually made to take info from raspberry pi.
            Raspberry pi program must be made.

            */

            InitializeComponent();

            InitializeFiles(); // Must be initialized before the DataGridView!
            InitializeDGV();

            // Starting logging thread
            loggerThread = new Thread(() => LoggerThreadMethod());
            loggerThread.IsBackground = true;
            loggerThread.Start();
            loggerThread.Name = "Logger thread";

            // Start DGV assistance thread
            updateDGVThread = new Thread(() => UpdateDGVThreadMethod());
            updateDGVThread.IsBackground = true;
            updateDGVThread.Start();
            updateDGVThread.Name = "DGV thread";
        }

        #region Initalization
        private void InitializeFiles()
        {
            /*
            Alarmlog format:
            Timestamp,Alarmtype,Zone,Severity
            */

            string directory = Directory.GetCurrentDirectory();
            alarmPath = directory + "\\alarmlog.csv";
        }
        private void InitializeDGV()
        {
            dgvAlarms.ColumnCount = 4;
            dgvAlarms.Columns[0].Name = "Timestamp";
            dgvAlarms.Columns[1].Name = "Alarmtype";
            dgvAlarms.Columns[2].Name = "Zone";
            dgvAlarms.Columns[3].Name = "Importance";
            dgvAlarms.Columns[0].Width = 150;
            // Populate with alarms
            StreamReader streamReader = new StreamReader(alarmPath);
            string line = streamReader.ReadLine(); // This will read the first line
            string[] splitLine;
            while (line != null)
            {
                splitLine = line.Split(",");
                dgvAlarms.Rows.Add(splitLine[0], splitLine[1], splitLine[2], splitLine[3]);

                // Determine color
                if (Convert.ToInt32(splitLine[3]) == 2)
                {
                    dgvAlarms.Rows[dgvAlarms.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Red;
                }
                if (Convert.ToInt32(splitLine[3]) == 1)
                {
                    dgvAlarms.Rows[dgvAlarms.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Orange;
                }
                // Read new line
                line = streamReader.ReadLine();

            }
            // Sort by alarm severity
            dgvAlarms.Sort(dgvAlarms.Columns[3], ListSortDirection.Descending);

            streamReader.Close();
        }
        #endregion

        #region Threads
        private async Task SensorThreadMethod(CancellationToken token)
        {
            /*
            1.Fetch new values
            2.Check if an alarm needs to be made
            3.Either return to 1., or make an alarm
            4.Send alarm to logger and UI threads via blockingcollection
            */
            AlarmClass tempAlarm;
            List<bool> newReadings;
            //In a larger system these default values would likely not be satisfactory, and this would be needed to changed alongside the constructors below.
            string defaultAlarmType = "Break-in";
            int defaultSeverity = 2;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(100, token);
                    // FetchNewMeasurements returns a list of four bools. 
                    // The first bool tells whether or not the alarms are turned on.
                    // The other three correspond to an active alarm in zone 1, 2, or 3.
                    newReadings = FetchNewMeasurements();

                    DateTime timeStamp = DateTime.Now;
                    // rename timeStamp above to Now if this needs to be changed back
                    //DateTime timeStamp = new DateTime(now.Year, now.Day, now.Month, now.Hour, now.Minute, now.Second);

                    // Alarms
                    if (newReadings[1])
                    {
                        tempAlarm = new AlarmClass(timeStamp, defaultAlarmType, "Zone 1", defaultSeverity, true);
                        writeToAlarmFile.Add(tempAlarm);
                        writeToGUI.Add(tempAlarm);
                    }
                    if (newReadings[2])
                    {
                        tempAlarm = new AlarmClass(timeStamp, defaultAlarmType, "Zone 2", defaultSeverity, true);
                        writeToAlarmFile.Add(tempAlarm);
                        writeToGUI.Add(tempAlarm);
                    }
                    if (newReadings[3])
                    {
                        tempAlarm = new AlarmClass(timeStamp, defaultAlarmType, "Zone 3", defaultSeverity, true);
                        writeToAlarmFile.Add(tempAlarm);
                        writeToGUI.Add(tempAlarm);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }
        private async Task LoggerThreadMethod()
        {
            try
            {
                // Creates list of all alarms.
                List<string> allLines = File.ReadAllLines(alarmPath).ToList();

                // This is where databases start to shine, but at this scale it still isn't a necessity.
                foreach (AlarmClass alarm in writeToAlarmFile.GetConsumingEnumerable())
                {
                    if (alarm.newAlarm)
                    {
                        // Append new alarm to both memory and file
                        allLines.Add(alarm.ToString());
                        File.AppendAllText(alarmPath, alarm.ToString() + Environment.NewLine);
                    }
                    else
                    {
                        // Modify existing alarm in file
                        // Find line in file and modify it.
                        bool found = false;
                        for (int i = 0; i < allLines.Count; i++)
                        {
                            string[] splitLine = allLines[i].Split(",");
                            // Testing line below.
                            //MessageBox.Show($"{splitLine[0]}   {alarm.timeStamp.ToString()}");
                            if (splitLine[0] == alarm.timeStamp.ToString() && splitLine[2] == alarm.zone)
                            {
                                allLines[i] = alarm.ToString();
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            File.WriteAllLines(alarmPath, allLines);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private async Task UpdateDGVThreadMethod()
        {
            foreach (AlarmClass alarm in writeToGUI.GetConsumingEnumerable())
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => { UpdateDGV(alarm); }));
                }
                else
                {
                    UpdateDGV(alarm);
                }
            }
        }
        #endregion

        #region Man-made events & Miscellaneous functions
        private bool ConnectToPi()
        {
            // Returns true if connection was successful.
            // Socket connection via ethernet
            try
            {
                // Connect to a Remote server
                // Get Host IP Address that is used to establish a connection
                // If a host has multiple addresses, you will get a list of addresses
                // IPHostEntry host = Dns.GetHostEntry("localhost");
                //IPAddress ipAddress = host.AddressList[0];

                IPAddress ipAddress = IPAddress.Parse("169.254.88.165");
                // Local testing IP:
                //IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11800);

                // Create a TCP/IP socket.
                socketHandler = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    // Connect to Remote EndPoint
                    socketHandler.Connect(remoteEP);
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            txtConnection.Text = $"Socket connected to {socketHandler.RemoteEndPoint.ToString()}";
                        }));
                    }
                    else
                    {
                        txtConnection.Text = $"Socket connected to {socketHandler.RemoteEndPoint.ToString()}";
                    }
                    return true; // Connection established
                }
                catch (ArgumentNullException ane)
                {
                    MessageBox.Show("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    return false; // Connection timed-out
                    MessageBox.Show("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    MessageBox.Show("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return false; // Connection not established
        }
        private void DisconnectFromPi()
        {
            if (socketHandler != null && connectionStatus)
            {
                socketHandler.Shutdown(SocketShutdown.Both);
                socketHandler.Close();
            }
        }

        private List<bool> FetchNewMeasurements()
        {
            List<bool> returningList = [false, false, false, false];

            try
            {
                byte[] bytes = new byte[1024];
                // Encode the data string into a byte array.

                // This can be modified to work as a bottleneck for the sampling rate.
                Thread.Sleep(100);

                byte[] msg = Encoding.ASCII.GetBytes("True Send alarm values");
                // Send the data through the socket.
                socketHandler.Send(msg);

                // Receive the response from the remote device.
                int bytesRec = socketHandler.Receive(bytes);
                string receivedText = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                    txtConnection.Text = $"Message from Server = {receivedText}";
                    }));
                }
                else
                {
                txtConnection.Text = $"Message from Server = {receivedText}";
                }

                // Make bools
                for (int i = 0; i < 4; i++)
                {
                    if (receivedText[i].ToString() == "1")
                    {
                        returningList[i] = true;
                    }
                }
            }
            catch (Exception e)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        txtConnection.Text = "Connection disabled."; // Obsolete error message: " + e.ToString();
                    }));
                }
                else
                {
                    txtConnection.Text = "Connection disabled."; // Obsolete error message: " + e.ToString();
                }
            }
            return returningList;
        }
        private void UpdateDGV(AlarmClass alarm)
        {
            dgvAlarms.Rows.Add(alarm.timeStamp.ToString(), alarm.alarmType, alarm.zone, alarm.severity.ToString());
            if (alarm.severity == 2)
            {
                dgvAlarms.Rows[dgvAlarms.Rows.Count-1].DefaultCellStyle.BackColor = Color.Red;
            }
            else if (alarm.severity == 1)
            {
                dgvAlarms.Rows[dgvAlarms.Rows.Count-1].DefaultCellStyle.BackColor = Color.Orange;
            }
            dgvAlarms.ClearSelection();
            selectedAlarm = null;
            dgvAlarms.Sort(dgvAlarms.Columns[3], ListSortDirection.Descending);

        }
        private void AlarmInteraction(AlarmClass? alarm, int rowPosition, int changeSeverityFrom)
        {
            if (alarm == null)
            {
                txtResponse.Text = "No alarm selected!";
            }
            else if (alarm.severity != changeSeverityFrom)
            {
                txtResponse.Text = "Wrong alarm level on selected alarm.";
            }
            else if (dgvAlarms.Rows[rowPosition].Cells[3].Value.ToString() == "0")
                // This could be changed to if (changeSeverityFrom == 0), but this might introduce bugs aswell. 
            {   // Removal condition
                dgvAlarms.Rows.RemoveAt(rowPosition);
            }
            else
            {
                // Change severity, and update DGV, send to logger queue.
                alarm.severity = changeSeverityFrom - 1;
                dgvAlarms.Rows[rowPosition].Cells[3].Value = alarm.severity.ToString();
                if (alarm.severity == 0)
                {
                    dgvAlarms.Rows[rowPosition].DefaultCellStyle.BackColor = Color.White;
                }
                else if (alarm.severity == 1)
                {
                    dgvAlarms.Rows[rowPosition].DefaultCellStyle.BackColor = Color.Orange;
                }
                // Tell logging thread about the change.
                //MessageBox.Show("Alarm to be added: " + alarm.ToString() + alarm.newAlarm.ToString());
                writeToAlarmFile.Add(alarm);
            }
            dgvAlarms.ClearSelection();
            selectedAlarm = null;
            dgvAlarms.Sort(dgvAlarms.Columns[3], ListSortDirection.Descending);
        }
        #endregion

        #region Eventhandlers
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            // Socket connection.
            // Connecting is async because it will freeze the UI if it takes too long.
            connectionStatus = await Task.Run(() => ConnectToPi());
            if (connectionStatus)
            {
                // Start sensor thread
                sensorThreadTokenSource = new(); // Renew token in case it has been cancelled.
                sensorThread = new Thread(() => SensorThreadMethod(sensorThreadTokenSource.Token));
                sensorThread.IsBackground = true;
                sensorThread.Start();
                sensorThread.Name = "Sensor thread";
            }
            else
            {
                txtConnection.Text = "Connection failed, likely a time-out.";
            }
        }
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            // Sensor thread stops
            if (sensorThreadTokenSource != null)
            {
                sensorThreadTokenSource.Cancel();
            }

            // Socket disconnection
            DisconnectFromPi();
            txtConnection.Text = "Connection: Off";
            connectionStatus = false;
        }
        private void btnAcknowledge_Click(object sender, EventArgs e)
        {
            // Acknowledge alarm
            AlarmInteraction(selectedAlarm, rowIndex, 2);
        }
        private void btnDismiss_Click(object sender, EventArgs e)
        {
            // Dismiss alarm
            AlarmInteraction(selectedAlarm, rowIndex, 1);
        }
        private void btnArchive_Click(object sender, EventArgs e)
        {
            // Remove alarm from GUI
            AlarmInteraction(selectedAlarm, rowIndex, 0);
        }
        private void btnArchiveAll_Click(object sender, EventArgs e)
        {
            // Remmove all dismissed alarms from GUI
            for (int i = dgvAlarms.Rows.Count - 1; i >= 0; i--)
            {
                if (dgvAlarms.Rows[i].Cells[3].Value.ToString() == "0")
                {
                    dgvAlarms.Rows.RemoveAt(i);
                }
            }
        }

        private void dgvAlarms_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            txtResponse.Text = "";
            rowIndex = dgvAlarms.CurrentCell.RowIndex;

            // DateTime clean = new DateTime (year, month, day, hours, minutes, seconds)

            DateTime alarmValue1 = DateTime.Parse(dgvAlarms.Rows[rowIndex].Cells[0].Value?.ToString()); // Datetime
            string alarmValue2 = dgvAlarms.Rows[rowIndex].Cells[1].Value?.ToString(); // Alarmtype (string)
            string alarmValue3 = dgvAlarms.Rows[rowIndex].Cells[2].Value?.ToString(); // Zone (string)
            int alarmValue4 = Convert.ToInt32(dgvAlarms.Rows[rowIndex].Cells[3].Value); // Severity (int)

            bool alarmValue5 = false; // If it's on the DGV, then it will already have appeared in the writeToAlarmFile collection as a new alarm.
            selectedAlarm = new AlarmClass(alarmValue1, alarmValue2, alarmValue3, alarmValue4, alarmValue5);
        }
        #endregion
    }
}
