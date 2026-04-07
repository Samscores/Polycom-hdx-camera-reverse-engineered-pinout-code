# Polycom-hdx-camera-reverse-engineered-pinout-code
This project, completed by Sam and Connor on 04/07/26, focused on reverse engineering and interfacing with a Polycom camera system to extract video output and establish control using a gamepad and a keyboard. Developed on a desktop computer using Linux Mint, the setup utilized a DB9 serial port for communication and required a very thorough and detailed analysis of the camera’s HDCI connector. During testing, it was discovered that the top row of pins on the female HDCI port was arranged in backwards order (15 to 1 from right to left), which led to extensive multimeter probing to identify the correct pinout, while the remaining rows followed a more standard numbering system . For video extraction, YPbPr component signals were mapped from specific pins (47 for Y, 15 for Pr, and 13 for Pb) and connected via Dupont wires to a Kramer presentation switch, which converted the signal to HDMI and sent the output to a monitor. After fine tuning we produced a clear and detailed image akin to modern cameras. To enable camera control, a legacy PC with a DB9 port was connected using a passthrough adapter to establish RS-232 communication; however, progress was initially blocked due to Polycom (now HP) having discontinued the proprietary protocol that was used for this camera. This problem was overcome however by locating a reverse-engineered, community-driven implementation of the protocol, which once found provided us the necessary command structure we needed. Using this information, control functionality was implemented within PolycomPTZGUI by adding custom code to the mainwindow.axaml.cs file, ultimately enabling successful communication and control of the camera system.
// We used ls/dev/tty to tell what com port the camera was on. Make sure you adjust this to match your com port to your own setup in the code.

Steps that we took to get communcication and video feed working.

Identified that the female HDCI port pin numbering runs right-to-left (15–1), not left-to-right

Used a multimeter to verify and map the correct pins

Confirmed other pins were standard; video pins located on row 3 (31–45)

Wired video output using:
Pin 47 → Y
Pin 15 → Pr
Pin 13 → Pb

Connected pins via Dupont wires to a Kramer presentation switch

Converted YPbPr signal to HDMI and connected to a monitor

Fine-tuned setup to achieve a clear video feed

Used an old PC with a DB9 port and passthrough connector for serial communication

Attempted communication with Polycom camera via RS232

Encountered issue: proprietary (now discontinued) Polycom/HP protocol

Found reverse-engineered protocol in an online repository

Used the protocol commands to control the camera

Implemented control code in PolycomPTZGUI (mainwindow.axaml.cs)

