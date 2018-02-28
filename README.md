# Advanced Media Browser (AMBR) - ELEC 490/498 Capstone Project
AMBr is a Voice and Gesture Controlled TV set-top box, created as part of the ELEC 490 Capstone Design course at Queen's University. AMBr won first place for the Computer Engineering category at the February 2018 open house. 

In simpler words, it is a system that would help the users control their entertainment system with just voice commands and gestures.

### Authors
* Group: 32
   * Ryan Baxter
   * Mitchell Waite
   * Parv Mital 

## Usage

[Demonstration Video](https://www.youtube.com/watch?v=xJPANPWjZ8g)

[Example Gestures & Commands](gestures.png)

A Kinect v2 sensor must be connected to the system. Only the `Windows` version of the sensor has been tested, it is unknown if the `Xbox One` version of the sensor is compatible.

## Setup Instructions

These instructions have been tested on Windows 10 with Visual Studio 2017 and Kodi 17. The Microsoft Gesture Service included with Prague isn't compatible with Windows 8.

Install Visual Studio and Kodi, and then the following software packages:

1. Install the Kinect for Windows 2.0 SDK : https://www.microsoft.com/en-ca/download/details.aspx?id=44561
2. Microsoft Speech Platform SDK v11: https://www.microsoft.com/en-us/download/details.aspx?id=27226
3. Microsoft Speech Runtime v11: https://www.microsoft.com/en-us/download/details.aspx?id=27225
   * Install both x86 and x64 versions
4. Kinect 2.0 Speech Recognition Pack: https://www.microsoft.com/en-us/download/details.aspx?id=43662
   * Right now, the `en_US` language is the only supported. Install `MSKinectLangPack_enUS.msi`
5. English TTS Voice: https://www.microsoft.com/en-us/download/details.aspx?id=27224
   * Download this msi from the above page: `MSSpeech_TTS_en-US_ZiraPro.msi`
6. Python 2.7 for Windows: https://www.python.org/downloads
   * Ensure `python.exe` is in your path by selecting `Add python.exe to Path` in the installer.
7. Kodipydent library
   * Use the following command: `pip install kodipydent`

## Motivation / Reason
* The purpose of AMBr is to make the use entertainment system remote-free, simple and a little modern/futuristic.  
* There are many people with disabilities or people with arthritis, hand tremors and other motor control issues.
* At the same time, let's say you or someone at your house is cooking or eating. Now, they want to switch their music or videos but their hands are dirty. In the current scenario, they won’t be able to control their entertainment. 
* Also, remotes can be hard to deal with sometimes (in case they go missing, or they have a low battery).
* Hence, we made the system Advanced Media Browser or as we call it - AMBr 
* This system eliminates the need of any kind of physical controller and also enables you to use the system when your hands are occupied. 

## Software Implementation
* The software behind AMBr was implemented as a three-part solution. 
* It consists of Windows software to control the Kinect, the Kodi media centre, and a python interface between the two. 
* The Windows software consists of a C# program that interfaces to the Microsoft Gesture and Speech services. This means that the user's voice commands or gestures will be read/interpreted by the Microsoft Gesture and Speech Services. 
* The C# program then sends recognized actions over a socket interface to a Python script, which communicates with Kodi over a JSON RPC interface. 

## Functional Description
* The goal of AMBr was to improve the user experience while interacting with a media system. 
* The system recognizes hand gestures and voice commands in less than 0.75 second. 
* Gesture recognition continues to function properly in the presence of other moving objects. 
* AMBr supports a wide range of commands including play/select, pause, stop, home, context menu, volume up/down, fast forward/rewind. All of these commands are usable through voice as well as gesture.
* AMBr also lets you play movies or music without the need of traversing through the list. All you have to do is say the proper command and AMBr will play it for you as long as it exists. For e.g. (Hey AMBr > Play Movie > "Movie Name")
* To avoid the commands being triggered unintentionally, an activation phrase was implemented for voice commands - Hey AMBr


