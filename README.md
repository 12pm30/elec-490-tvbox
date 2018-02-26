# Team 32 - ELEC 490/498 Capstone Project - Advance Media Browser or AMBr
AMBr is a "Voice and Gesture Controlled TV set-top box". In simpler words, it is a system that would help the users control their entertainment system with just voice commands and gestures.

## Motivation / Reason
* The purpose of AMBr is to make the use entertainment system remote-free, simple and a little modern/futuristic.  
* There are many people with disabilities or people with arthritis, hand tremors and other motor control issues.
* At the same time, let’s say you or someone at your house is cooking or eating. Now, they want to switch their music or videos but their hands are dirty. In the current scenario, they won’t be able to control their entertainment. 
* Also, remotes can be hard to deal with sometimes (in case they go missing, or they have a low battery).
* Hence, we made the system Advanced Media Browser or as we call it - AMBr 
* This system eliminates the need of any kind of physical controller and also enables you to use the system when your hands are occupied. 

## Project Description and Implementation
* The software behind AMBr was implemented as a three-part solution. 
* It consists of Windows software to control the Kinect, the Kodi media centre, and a python interface between the two. 
* The Windows software consists of a C# program that interfaces to the Microsoft Gesture and Speech services. This means that the user's voice commands or gestures will be read/interpreted by the Microsoft Gesture and Speech Services. 
* The C# program then sends recognized actions over a socket interface to a Python script, which communicates with Kodi over a JSON RPC interface. 
* The goal of AMBr was to improve the user experience while interacting with a media system. 
* The system recognizes hand gestures and voice commands in less than 0.75 second. 
* Gesture recognition continues to function properly in the presence of other moving objects. 
* AMBr supports a wide range of commands including play/select, pause, stop, home, context menu, volume up/down, fast forward/rewind. All of these commands are usable through voice as well as gesture.
* AMBr also lets you play movies or music without the need of traversing through the list. All you have to do is say the proper command and AMBr will play it for you as long as it exists. For e.g. (Hey AMBr > Play Movie > "Movie Name"
* To avoid the commands being triggered unintentionally, an activation phrase was implemented for voice commands - Hey AMBr

#

## How to Install 
1. Kinect for Windows 2.0
2. Microsoft Speech Platform SDK v11
3. English TTS Voices from here: https://www.microsoft.com/en-us/download/details.aspx?id=27224
   Zira Pro works well. Download this msi from the above page: `MSSpeech_TTS_en-US_ZiraPro.msi`
