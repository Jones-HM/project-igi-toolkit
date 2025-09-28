### ❓ Where can I find the saved toolkit key?
**A:** Press `Windows` + `R` Key then type `%appdata%\QToolKit\` and look for the file `IGIToolKitKey.txt`. 

### ❓ Where are the ToolKit files saved?
**A:** ToolKit stores all files in the Appdata and Temp folders. 📂

### ❓ How can I access the Appdata and Temp folders?
**A:** To access Appdata, press `Windows` + `R` Key then type `%appdata%\QToolKit\`. For Temp, use `%tmp%\IGIToolKitCache` directory. 

### ❓ I got an error: "Exception: System.FormatException: The input string had an invalid format." What should I do?
**A:** Try to change your system language to English and restart the toolkit. 🔄

### ❓ I got an error: "1000\ammo.qsc doesn't exist in checksum." What should I do?
**A:** Press `Windows` + `R` Key then type `%appdata%\QToolKit\` and look for the file `QChecks.dat`. Delete it and restart the toolkit.

### ❓ Where can I find the missions file installed by the toolkit?
**A:** Check `%appdata%\QToolKit\QMissions` folder for all your missions. 🕹️

### ❓ How can I update the ToolKit?
**A:** Open toolkit and go to Misc section and select 'Check for Updates' and set the time interval to 1 minute. ⏰

### ❓ I got the error "No AI Script For Human#123" while loading a custom level. What should I do?
**A:** Go to Misc section and Enable the 'Disable Warnings' checkbox. 🚫

### ❓ How to send or view Logs?
**A:** Go to Misc section and click 'Show Logs' to view them and 'Share logs' to share. 📬

### ❓ I got a LoadLibrary error or DLL injection failed. What should I do?
**A:** If you encounter LoadLibrary error or DLL injection issues, use the manual injector provided with the ToolKit:
- Navigate to `bin/IGI-Injector.bat` in your ToolKit directory and run it to inject the DLL into the game
- Alternatively, use your favorite DLL Injector to inject the required DLL manually
- After successful injection, make sure to use Menu "Level Menu -> Refresh" to refresh the game
- This will ensure the ToolKit can properly communicate with the game 🔧