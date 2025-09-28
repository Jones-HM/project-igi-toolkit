# Project I.G.I ToolKit

[Project I.G.I](https://en.wikipedia.org/wiki/Project_I.G.I.) ToolKit is all in one game toolkit which lets you edit all game objects including _Buildings/3D/AI/Weapons_ and lets you to design your own _custom level_ in the game and lets you _upload/download_ your level to server and share with your friends.

## Pre-Requisite
- **General section.**
- [DLL File](https://en.wikipedia.org/wiki/Dynamic-link_library) - This project uses [IGI Natives.dll](https://github.com/IGI-Research-Devs/IGI_Internal/) file and inject that into Game to call game native methods. .</br>
- [DLL Injection](https://en.wikipedia.org/wiki/DLL_injection) - This project needs DLL injection into _IGI_ game.</br>
- **Game specific section.**
- [IGI Graphs Structure](https://github.com/IGI-Research-Devs/IGI-Research-Data/blob/main/Research/GRAPH/Graph-Structure.txt) - Project IGI 1 Graph structure data.
- [IGI 3D Models](https://github.com/IGI-Research-Devs/IGI-Research-Data/blob/main/Research/Natives/IGI-Models.txt) - Project IGI uses 3D models in 
form of _MEF_ (**M**esh **E**xternal **F**ile).
- [IGI Camera View](https://www.researchgate.net/figure/Definition-of-pitch-roll-and-yaw-angle-for-camera-state-estimation_fig15_273225757) - IGI use game Camera called [Viewport](https://en.wikipedia.org/wiki/Viewport) to display the game.

## ToolKit components.
ToolKit has several components which it needs in order to work fully to its functionality.
- Server : Located at [OrgFree](http://igiresearchdevelopers.orgfree.com/) which contains Mission files and Resources like Weapon/A.I images for toolkit.
- ~~Database : Located at Github Gist private repo contains privacy information like _I.P,Mac Address_ and _Key_ for ToolKit.~~
- Internal DLL : Located at [IGI-Internals](https://github.com/IGI-Research-Devs/IGI_Internal) is used to call IGI Game internal native methods.
- QToolKit - Located at path `C:\Users\my_username\AppData\Roaming\QToolKit`  This is appdata file comes pre-installed with any version of toolkit and can be downloaded from here [QToolKit Full Version](https://www.mediafire.com/file/7ycvekb2l9fc7qr/QToolKit.zip/file).</br>

## QToolKit components.
**Q**ToolKit was the initial name of the project because toolkit actually modifies **Q** files of games like _QVM,QSC,QAS_ files but later it was changed.</br>

    .
    ├── AIFiles                 # AI Script and Path files.
    ├── QCompiler               # Compiler/Decompiler for IGI.
    ├── QFiles                  # Source files - QSC, Binary files - QVM.
    ├── QGraphs                 # Graphs files and areas.
    ├── QMissions               # Custom Missions files.
    ├── QWeapons                # Custom Weapons files.
    ├── Void                    # Empty Mission files.
    ├── aiIdle.qvm              # File for setting AI to idle state.
    ├── IGIModels.json           # Contains 3D Models information.
    ├── keywords.txt            # Keywords for QVM ToolKit.
    ├── QChecks.dat             # Contain MD5 Hashes to check file integrity.
    └── weaponconfig.qvm        # Weapon config file.


## ToolKit Working flow.
ToolKit most of the time just compiles the script file _QSC_ called _Q_ script source into _QVM_ called _Q_ virtual machine and Restart the level in order to see the affected changes. 
Here is workflow mentioned.
1. Get game level and select proper source file of level located at `QToolKit\QFiles\IGI_QSC\missions\location0\level`.</br>
2. Updates the script file with new script depends upon logic Add/Update/Delete commands.</br>
**Internal compiler**</br>
3. Copies script file from ToolKit `C:\IGI-ToolKit` to `D:\IGI\` and compiles them to binary _QVM_ file and moves them to proper destination.</br>
**External compiler**</br>
3. Copies script file from ToolKit `C:\IGI-ToolKit` to `QToolKit\QCompiler\Compile\input` and compiles them to binary _QVM_ file and convert compiled QVM v7 (IGI 2) to QVM v5 (IGI 1) using _DConv_ tool and moves them to proper destination.</br>
4. Restart the level and see the changes.</br>

## ToolKit sections.
### Level ToolKit:
**Level ToolKit**: Lets you to `Add` or `Remove` Building/Objects in level at any position you want select list of objects to add in level and add then in `Edit mode`.</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/level_toolkit.png)

### Human ToolKit:
**Human ToolKit**: Lets you to Update human Speed/Jump,Health Scale peek and Team Id/Human Camera (1st Person,3rd person).</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/human_toolkit.png)

### Weapon ToolKit:
**Weapon ToolKit**: Lets you to `Add` or `Remove` new weapons in level `permanently` or `temporary` depends which mode is selected `Live` or `Normal` mode.</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/weapon_toolkit.png)

### Weapon Advance ToolKit:
**Weapon Advance ToolKit**: Lets you to `Update` Weapon's advance data like _Name,UI Type,Ammo Damage, Weapon Power/RPM and more_.</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/weapon_advance_toolkit.png)

### A.I ToolKit:
**A.I ToolKit**: Lets you to `Add` or `Remove` `Friendly/Enemy` A.I into level with various properties like _invincible_,_advance view_,_guard generator_ etc.</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/ai_toolkit.png)

### A.I JSON ToolKit.
**JSON ToolKit**: Lets you to `Save` or `Load` A.I to `JSON` file _permanently_ for later use, and you can edit/share json files into toolkit.</br>
![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/ai_json_toolkit.png)

### A.I Script ToolKit  
**Script ToolKit**: Allows you to `Create`, `Edit`, and `Manage` A.I scripts seamlessly. You can save scripts for later use.  
![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/ai_script_toolkit.png)  

### A.I Patrol ToolKit  
**Patrol ToolKit**: Enables you to `Define`, `Modify`, and `Control` A.I patrol routes. Save patrol patterns for reuse.  
![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/ai_patrol_toolkit.png)


### Mission ToolKit
**Mission ToolKit**: Lets you to `Add` or `Remove` `Missions` into level after you design your own custom level enter _Name_ and _Description_ and **Save** your Mission</br>
You can also **Upload** or **Download** new missions from _Server_ and load into your game and play them directly..</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/mission_toolkit.png)

### Position ToolKit
**Position ToolKit**: Lets you to `Update` or `Reset` `Positions` and `Orientation` of Game objects and Humanplayer you can Rotate them to `180` degree or move them to different location easily.</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/position_toolkit.png)


### Texture ToolKit
**Texture ToolKit**: Lets you to `View` and `Replace` `Textures` of game easily with inbuilt custom tools now you can view all game textures and replace them with your own Textures in `.JPG/.PNG` files .</br>
- This can automatically Import textures from `.RES` Resource packed file annd view them directly into toolkit.</br>
- This can automatically Replace Images from `JPG/PNG` to `TGA` Game supported format and turn that into game texture easily.</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/texture_toolkit.png)

### Graph ToolKit.</br> 
**Graph toolkit**: Lets you see visualization of Graph and nodes information, You can `teleport` or `Auto traverse` to Graph or Nodes in _real time_ and see where graph or nodes are in selected level..</br>

![](https://raw.githubusercontent.com/Jones-HM/project-igi-toolkit/refs/heads/develop/IGIEditor/resources/graph_toolkit.png)

### Graph & Links - Level 5. ( IGI 1 ToolKit).
![](https://i.ibb.co/px7fWfS/Node-Links-L5-Area2.png)

### Graph & Links - Level 1. ( IGI 2 ToolKit).
![](https://i.ibb.co/gR1vZSM/IGI2-Graph-Nodes.png)

### Misc ToolKit:
**Misc toolkit**: Lets you to `Reset/Remove` levels back to normal and `Remove Cutscenes`, Export level data,Set Game Frames `FPS` Update Game Music and more.</br>

### IGI ToolKit License Key:
~~This toolkit requires _license key_ to run contact via _email_ or _discord_ and mention your **Username** to identify yourself.</br>
And now you can generate Key from KeyGen for your toolkit for free.</br>~~

## Future version includes:
* All **new UI/UX Design** with More **advanced toolkit**.
* **Real Time ToolKit Support**.
* **Resource/Profile/GameConfig/QCompiler** toolkit.
* **Compiler** and **Assembler** with _Parser_ output like **.qsc** to **.qas** to **.qvm**.

### Profile ToolKit: **_Available in Future version_**.</br> 
**Profile toolkit**: Lets you manage your `Game Profiles` you can update _Name,Level,Mission_ details or  `Add` or `Remove` new player profile for your player.</br>

### Resource ToolKit: **_Available in Future version_**.</br> 
**Resource toolkit**: Lets you see all details of Game resources information like Resource _Name,Address,Id_ and can `Load` or `Unload` any resource in _Real time_.</br>

### GameConfig ToolKit: **_Available in Future version_**.</br> 
**GameConfig toolkit**: Lets you edit game configuration like _Level,Password,ActiveMission,Graphics/Music/Mouse settings,Game Key controls,Game difficulty_ and more.</br> 

### QCompiler ToolKit: **_Available in Future version_**.</br> 
**QCompiler toolkit**: Lets you `Compile/Assemble/Parse` game files which game uses internaly to save/edit game data.</br>

# ToolKit Tutorial on YouTube :
[![QToolKit](https://img.youtube.com/vi/gwj5HNzF9cQ/0.jpg)](https://www.youtube.com/watch?v=gwj5HNzF9cQ)

## Version Update:
**ToolKit version 0.8.7.0 Latest.**

# Chagelogs:
Check out the detailed version history in [Changelogs.md](CHANGELOGS.md)

## Privacy Policy:
The toolkit onwards version 0.7 doesn't store any type of data from _User,Machine_ the toolkit doesn't maintain any sort of database now.

# **DOWNLOAD LINKS**</br>
- **Project I.G.I 1 ToolKit** Version 0.8.7.0 _RELEASED_</br>
[IGI ToolKit](https://github.com/IGI-Research-Devs/IGI-ToolKit/releases/tag/0.8.7.0)</br>

# 📚 FAQ (Frequently Asked Questions)
Checkout the detailed FAQ in [FAQ.md](FAQ.md)

# 📞 Connect with us:
If you encounter any issues with the ToolKit, don't hesitate to contact me 👇

- 🎮 Discord: Feel free to message me at _Jones_IGI#3954_ and join our [Discord server](https://discord.gg/AyVDW7kE6V) for quick support.
- 📧 Email: You can reach me at igiproz.hm@gmail.com for any questions or feedback.
- 🌟 Follow the Project: Stay updated with the latest developments on [GitHub](https://github.com/Jones-HM/).
- 📺 Subscribe to our Channel: Watch useful guides and walkthroughs on our [YouTube](https://www.youtube.com/@igi-research-devs) channel.

## Credits and People
If you want to use this data, respect fellow researchers and give proper credits to people. (давать людям должные кредиты)
- [Yoejin Light](https://vk.com/id436486682) 🌟 : _MTP, Models structure_ and information.
- [Dimon Krevedko](https://vk.com/dimonkrevedko) 🌟 : **Graphs and Nodes** structure and information.
- [Artiom Rotari](https://github.com/NEWME0) 🌟 : _DConv Tools for Decompiler_ and **Scripts**.
- [ORWA S](https://www.youtube.com/@totalwartimelapses6359) 🌟 - **Graphs Area and Nodes** compilation of information.
- [GM123](https://www.youtube.com/@gm1233) 🌟 - **Detailed Models Information**.
- [Dark](https://www.youtube.com/@CRONOQUILLOFFICIAL)🌟 - **Contrinuted on Various Projects and files (Resources,QVM,QSC etc) and UI/UX Designs**.
- [Ferit Coder](https://www.youtube.com/channel/UCpn_gZMkFVBUAe9SJK9hYQA) 🌟 - **Helped with IGI 2 ToolKit **Maps/Models** conversion to IGI 1.**.
- [Neo](https://next.nexusmods.com/profile/xaeroneo?gameId=5664) 🌟 - **Helped with improvement of ToolKit overall and Texture ToolKit.**.
- [Chat-GPT Pro](https://chat.openai.com/) 🌟 - **Understanding complex game mechanics** with _Reverse Engineering_ and **Advaced Data Analysis** of Game files _QVM,QSC,.DAT and more_.

👤 Original Author: </br>
- LinkedIn: [Haseeb-Mir91](https://in.linkedin.com/in/haseebmir91) </br>
- GitHub: [Haseeb-Heaven](https://github.com/haseeb-heaven)


