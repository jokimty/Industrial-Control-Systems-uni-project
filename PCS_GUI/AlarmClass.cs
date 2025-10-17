
namespace PCS_GUI
{
    public class AlarmClass
    {
        public DateTime timeStamp { get; }
        public string alarmType { get; }
        public string zone { get; }
        public int severity { get; set; }
        public bool newAlarm { get; set; } // Is this alarm a new alarm, or does it already exist in the alarm file?

        public AlarmClass(DateTime timestamp, string alarmtype, string zonE, int severitY, bool newalarm)
        {
            timeStamp = timestamp;
            alarmType = alarmtype;
            zone = zonE;
            severity = severitY;
            newAlarm = newalarm;
        }
        public override string ToString()
        {
            return $"{timeStamp.ToString()},{alarmType},{zone},{severity.ToString()}";
        }
    }
}
