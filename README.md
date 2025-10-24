Mock project for a course in industrial controlsystems.
Uses a raspberry pi connected to 4 buttons to imitate 3 alarms, one of the buttons is used to turn the alarm system on and off.
The GUI utilized multithreading to communicate with raspberry pi, update GUI, and log information in a CSV file. 
The raspberrypi program uses 1 extra thread for the button that turns the alarm system on and off.


Todo:

Make sure the LED actually lights up when you get a working LED.