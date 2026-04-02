using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;

namespace PolycomPTZ_GUI
{
    public partial class MainWindow : Window
    {
        SerialPort cam;

        // Current camera values (MSB = coarse, LSB = fine)
        byte pan = 0x06, panStep = 0xC0;
        byte tilt = 0x02, tiltStep = 0x00;
        byte zoom = 0x08, zoomStep = 0x00;

        // Previous values for change detection
        byte lastPan, lastPanStep, lastTilt, lastTiltStep, lastZoom, lastZoomStep;

        // Limits
        const byte panMin = 0x00, panMax = 0x0F;
        const byte panStepMin = 0x00, panStepMax = 0xCF;
        const byte tiltMin = 0x01, tiltMax = 0x02;
        const byte tiltStepMin = 0x00, tiltStepMax = 0xFF;
        const byte zoomMin = 0x01, zoomMax = 0x11;
        const byte zoomStepMin = 0x00, zoomStepMax = 0xFF;

        // Joystick constants
        const int deadzone = 5000;
        const int maxJoystick = 32767;
        const int maxStep = 1; // max step per loop for smooth control

        public MainWindow()
        {
            InitializeComponent();

            cam = new SerialPort("/dev/ttyS0", 9600, Parity.Even, 8, StopBits.One);
            cam.Open();
            cam.Write(new byte[] { 0x81, 0x40 }, 0, 2); // Initialize camera
            Thread.Sleep(200);

            this.KeyDown += MainWindow_KeyDown;

            // Start continuous joystick polling
            Thread joystickThread = new Thread(JoystickLoop) { IsBackground = true };
            joystickThread.Start();
        }

        private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.C) CenterCamera(); // Reset camera
        }

        private void JoystickLoop()
        {
            string device = "/dev/input/js0";
            if (!File.Exists(device)) { Console.WriteLine("Joystick not found"); return; }

            using var fs = new FileStream(device, FileMode.Open);
            byte[] buffer = new byte[8];

            int x = 0, y = 0, lt = 0, rt = 0;

            while (true)
            {
                if (fs.Read(buffer, 0, 8) != 8) continue;

                short value = BitConverter.ToInt16(buffer, 4);
                byte type = buffer[6];
                byte number = buffer[7];

                // Axes
                if ((type & 0x02) != 0)
                {
                    if (number == 0) x = value;
                    if (number == 1) y = value;
                    if (number == 2) lt = value;
                    if (number == 5) rt = value;
                }

                // Buttons
                if ((type & 0x01) != 0 && number == 0 && value == 1) CenterCamera();

                // Calculate deltas scaled by joystick
                int panDelta = 0, tiltDelta = 0, zoomDelta = 0;

                if (x > deadzone) panDelta = (x - deadzone) * maxStep / (maxJoystick - deadzone);
                else if (x < -deadzone) panDelta = (x + deadzone) * maxStep / (maxJoystick - deadzone);

                if (y > deadzone) tiltDelta = -(y - deadzone) * maxStep / (maxJoystick - deadzone);
                else if (y < -deadzone) tiltDelta = -(y + deadzone) * maxStep / (maxJoystick - deadzone);

                if (rt > 20000) zoomDelta = 1;
                else if (lt > 20000) zoomDelta = -1;

                // Apply movement
                MovePan(panDelta, panDelta);
                MoveTilt(tiltDelta, tiltDelta);
                MoveZoom(zoomDelta, zoomDelta);

                Thread.Sleep(10); // ~100Hz update rate
            }
        }

        private void MovePan(int coarseDelta, int fineDelta)
        {
            pan = Clamp(pan, panMin, panMax, coarseDelta);
            panStep = Clamp(panStep, panStepMin, panStepMax, fineDelta);
            SendCommand(0x04, pan, panStep);
        }

        private void MoveTilt(int coarseDelta, int fineDelta)
        {
            tilt = Clamp(tilt, tiltMin, tiltMax, coarseDelta);
            tiltStep = Clamp(tiltStep, tiltStepMin, tiltStepMax, fineDelta);
            SendCommand(0x05, tilt, tiltStep);
        }

        private void MoveZoom(int coarseDelta, int fineDelta)
        {
            zoom = Clamp(zoom, zoomMin, zoomMax, coarseDelta);
            zoomStep = Clamp(zoomStep, zoomStepMin, zoomStepMax, fineDelta);
            SendCommand(0x02, zoom, zoomStep);
        }

        private byte Clamp(byte value, byte min, byte max, int delta)
        {
            int v = value + delta;
            if (v < min) v = min;
            if (v > max) v = max;
            return (byte)v;
        }

        private void SendCommand(byte cmd, byte msb, byte lsb)
        {
            // Only send if values changed
            bool changed = false;
            if (cmd == 0x04 && (msb != lastPan || lsb != lastPanStep)) changed = true;
            else if (cmd == 0x05 && (msb != lastTilt || lsb != lastTiltStep)) changed = true;
            else if (cmd == 0x02 && (msb != lastZoom || lsb != lastZoomStep)) changed = true;

            if (!changed) return;

            cam.Write(new byte[] { 0x84, 0x43, cmd, msb, lsb }, 0, 5);

            if (cmd == 0x04) { lastPan = msb; lastPanStep = lsb; }
            else if (cmd == 0x05) { lastTilt = msb; lastTiltStep = lsb; }
            else if (cmd == 0x02) { lastZoom = msb; lastZoomStep = lsb; }
        }

        private void CenterCamera()
        {
            pan = 0x06; panStep = 0xC0;
            tilt = 0x02; tiltStep = 0x00;
            zoom = 0x08; zoomStep = 0x00;

            MovePan(0, 0);
            MoveTilt(0, 0);
            MoveZoom(0, 0);
        }
    }
}
