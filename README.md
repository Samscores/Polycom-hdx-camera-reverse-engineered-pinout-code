# Polycom-hdx-camera-reverse-engineered-pinout-code
Made with connor
created on linux mint machine with db9 port
Starting process pinout:
We discovered on the female hdci port that communicates with rs232 serial the top row of pins read 15-1 (right to left) and not left to right this cause us a great amount of probing with a multimeter to dicover
the other pins are regular to our knowledge third row being 31-45 for the video feed we used  47 for Y and 15 for PR and 13 for PB. 
we hooked that up to dupont wires towards a kramer presentation switch which esentially took those YPbPr and converted it to hdmi and hooked directly to a monitor for visual feed and we got feed very clear and detailed with fine tuning
after we got visual we took an old pc with a db9 port and used a passthrough connector to turn serial on pc to communicate with polcom camera.
next we hit a bit of a roadblock learning that polycom now hp used a proprietary protocal they had access to accept its been discontinued so we had no way to acess it until we stumbled apon a repo of the protocal we needed from someone reverse enginnering it 
now that we had commands to send to it we used polycomptzgui and in mainwindow.axaml.cs we wrote our code 
