Przeliterowywacz (Speller) #PUTTHEVERSIONHERE
Author: PeCeT_full
Website: http://www.komputermania.pl.eu.org
Copyright © by PeCeT_full 2014. Modifying and selling this program is strictly prohibited.
Special thanks to Mark Heath for NAudio library and pfcode for lending the voice for the default speech bank.

If there are any problems or doubts, please contact me.

-------------------
Program description
-------------------

This program, as the name suggests, can spell (i.e. uttering each character separately) by speaking and save prepared speech as a WAVE sound file by using available files reflecting the pronunciation of a specified character. It was written in C# and uses WPF. Runs in 32-bit mode in order to increase the compatibility with the SAPI5 speech synthesizers not intended to work with 64-bit applications.

Hardware and system requirements: 1 GHz or faster processor; 512 MB of available RAM; min. 20 MB of free hard disk space where the program exists; Windows Vista with SP2 or newer operating system with Microsoft .NET Framework 4.5 installed.

--------------------
Handling the program
--------------------

Przeliterowywacz is intuitive to use and has a simple interface. In order to make the program work, some text to read by speaking each character singly is needed to be entered. In this case, you need to click left mouse button on the text area and then, type (or paste from Clipboard) what is intended for you to hear (in case of no space for typing, the area, along with the application window, will be automatically expanded). If your intention is solely to listen to your spelled text then after pressing the "Spell" button, just click on "Play" from the pop-up menu (however, let's keep in mind that program options cannot be changed while working), but when you need the entire thing as a recording, you should select the "Record" command and then, specify the name and location where the sound file ought to be saved – after successfully finished operation, a marquee should appear for a few seconds on the bottom of main window with the message: "Saved."

---------------
Program options
---------------

Przeliterowywacz allows to change the following settings: 

* [Soundbanks only] Enable special and punctuation chars (e.g. comma, exclamation mark, asterisk etc.) – leaving this option marked causes that in any spoken text the following special characters will be taken into account: . , : ; ! ? ' " \ / % * | < > = whereas having it unmarked will make them omitted during reading. Keep in mind, however, that each of these characters corresponds to the file name other than its symbol, thus: 
. – kropka.wav, 
, – przecinek.wav, 
: – dwukropek.wav, 
; – srednik.wav, 
! – wykrzyknik.wav, 
? – pytajnik.wav, 
' – apostrof.wav, 
" – cudzyslow.wav, 
\ – ukosnik_wsteczny.wav, 
/ – ukosnik.wav, 
% – procent.wav, 
* – gwiazdka.wav, 
| – kreska_pionowa.wav, 
< – znak_mniejszosci.wav, 
> – znak_wiekszosci.wav, 
= – znak_rownosci.wav.

* [Soundbanks only] Derive files from the default speech bank if such are missing in the selected one - this options causes uttered characters being played or recorded from the default speech bank once if their equivalents are missing in the currently selected one by the user.

* [SAPI5 only] SAPI5's rate/volume – allows to customize the values of rate and volume of the text spoken by a synthesizer from -10 to 10 for the rate and from 0 to 100 for the volume, respectively.

* Read all zero characters as the letter 'o' – in some places in the world, people tend to pronounce zeros as oh's while spelling and this function meets this prerequisite.

* Use speech banks/SAPI5 synthesis.

* Text input colour scheme – it is used to change the appearance of the area designed for typing text to be read. Three schemes are available to choose from: standard, black & white and console.

* Speech bank – from the list here, you can choose any speech bank, that is a set containing the files with the *.wav extension with distinct pronunciation of individual chars. Such the created list is based on the subfolders with files in the "Banki" folder placed inside the program's home directory found by the application where the subfolder is a bank named like this, yet the default speech bank itself is basically all the sound files that are located in the main directory of "Banki".

* SAPI5 voice – you can select here any SAPI5 synthesizer that is already installed in your system.

* Interface language – the language in which the application shall communicate with the user.

If the changes are to be taken into account after another launch of Przeliterowywacz, you need to press the "Save settings" button yet before turning it off. In case of saving these for the first time, a configuration file called Przeliterowywacz.ini will be created, which in most cases should appear in the individual program's folder located in the special VirtualStore directory of the currently logged user (although when User Account Control is disabled or the program is installed in a completely different location, it will be brought into its own home directory directly). Its manual modification is not recommended, nevertheless it is possible to remove it with no problem since in case of its lack, a new file called the same will be created.
