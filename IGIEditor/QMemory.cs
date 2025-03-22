using QLibc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace IGIEditor
{
    class QMemory
    {
        internal static string gameName = "IGI";
        internal static float deltaToGround = 7000.0f;
        internal static IntPtr gtGameBase = (IntPtr)0x00400000; //Game base address.
        internal static IntPtr humanDamageAddr = (IntPtr)0x0057BABC;
        internal static IntPtr gravityAddr = (IntPtr)0x005333F0;


        internal static void StartGame(string args = "window")
        {
            Process.Start(gameName + "_" + args);
            GT.GT_FindGameProcess(gameName);
        }

        internal static bool FindGame(bool enableLogs = false)
        {
            bool gameFound = false;
            try
            {
                if (enableLogs) GT.GT_EnableLogs();

                var pname = Process.GetProcessesByName(gameName);
                if (pname.Length == 0)
                    gameFound = false;
                else
                {
                    if (GT.GT_FindGameProcess(gameName) != IntPtr.Zero)
                        gameFound = true;
                }
            }
            catch (Exception ex)
            {
                QLog.LogException(MethodBase.GetCurrentMethod().Name, ex);
            }
            return gameFound;
        }

        internal static int GetRunningLevel()
        {
            try
            {
                IntPtr levelAddr = (IntPtr)0x00539560;
                long level = GT.GT_ReadInt(levelAddr);
                if (level > QUtils.GAME_MAX_LEVEL) QLog.ShowSystemFatalError("IGI Editor is limited to " + QUtils.GAME_MAX_LEVEL + " levels only.");
                return (int)level;
            }
            catch (Exception ex) { return -1; }
        }

        internal static void DisableGameWarnings()
        {
            unsafe
            {
                IntPtr disableWarnAddr = (IntPtr)0x00936274;
                int disableWarn = 0;
                GT.GT_WriteAddress(disableWarnAddr, &disableWarn);
            }
        }

        static IntPtr GetGameBaseAddr()
        {
            var pid = GT.GT_GetProcessID();
            return GT.GT_GetGameBaseAddress(pid);
        }

        internal static IntPtr GetHumanHealthAddr()
        {
            var humanAddr = GetHumanBaseAddress() + (int)0x254;
            return humanAddr;
        }

        internal static IntPtr GetHumanBaseAddress(bool addLog = true)
        {
            IntPtr humanBasePtr = IntPtr.Zero, humanBaseAddr = IntPtr.Zero;
            try
            {
                uint humanStaticPtr = (uint)0x0016E210;
                uint[] humanAddrOffs = { 0x8, 0x7CC, 0x14 };

                humanBasePtr = GT.GT_ReadPointerOffset(gtGameBase, humanStaticPtr);
                humanBaseAddr = GT.GT_ReadPointerOffsets(humanBasePtr, humanAddrOffs, (uint)humanAddrOffs.Count() * sizeof(int));

                if (addLog)
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, "HumanBase Pointer 0x" + humanBasePtr + " Address  : 0x" + humanBaseAddr);
            }
            catch (Exception ex)
            {
                QLog.LogException(MethodBase.GetCurrentMethod().Name, ex);
            }
            return humanBaseAddr;
        }

        internal static IntPtr GetWeaponAddress()
        {
            var humanAddr = GetHumanBaseAddress() + (int)0xD48;
            return humanAddr;
        }

        internal static IntPtr GetStatusMsgAddr()
        {
            IntPtr statusMsgBasePointer = IntPtr.Zero, statusMsgAddress = IntPtr.Zero;
            try
            {
                uint statusMsgStaticPointer = (uint)0x001C8A20;
                uint[] statusMsgAddressOffsets = { 0x8, 0x8, 0x8, 0x4E4 };

                statusMsgBasePointer = GT.GT_ReadPointerOffset(gtGameBase, statusMsgStaticPointer);
                statusMsgAddress = GT.GT_ReadPointerOffsets(statusMsgBasePointer, statusMsgAddressOffsets, (uint)statusMsgAddressOffsets.Count() * sizeof(int)) + 0x4C;
            }
            catch (Exception ex)
            {
                QLog.LogException(MethodBase.GetCurrentMethod().Name, ex);
            }
            return statusMsgAddress;
        }

        internal static bool SetStatusMsgText(string statusMsgTxt)
        {
            var statusMsgAddr = GetStatusMsgAddr();
            return GT.GT_WriteMemory(statusMsgAddr, "string", statusMsgTxt);
        }

        internal static string GetStatusMsgText()
        {
            var statusMsgAddr = GetStatusMsgAddr();
            string statusMsg = GT.GT_ReadString(statusMsgAddr);
            return statusMsg;
        }


        static internal float GetRealAngle()
        {
            float angle = 0.0f;
            try
            {
                IntPtr humanBaseAddress = GetHumanBaseAddress() + (int)0x348;
                var angleAddrH = humanBaseAddress + 0x1C4;
                var angleAddrV = humanBaseAddress + 0xBF4;

                angle = GT.GT_ReadFloat(angleAddrH);
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Address : 0x" + angleAddrH.ToString() + " Value : " + angle);
            }
            catch (Exception ex)
            {
                QLog.LogException(MethodBase.GetCurrentMethod().Name, ex);
            }
            return angle;
        }

        internal static void UpdateHumanHealth(QUtils.HEALTH_ACTION healthAction)
        {
            IntPtr humanDamageHitAddr;
            unsafe 
            { 
                humanDamageHitAddr = (IntPtr)GT.GT_ReadInt(humanDamageAddr) + 0xE1;
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Human Damage Hit Address : 0x" + humanDamageHitAddr.ToString("X"));
            }

            if (healthAction == QUtils.HEALTH_ACTION.RESTORE || healthAction == QUtils.HEALTH_ACTION.NONE)
            {
                GT.GT_WriteMemory(humanDamageHitAddr, "byte", "0");
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Health Restored");
            }
            else if (healthAction == QUtils.HEALTH_ACTION.PERMANENT)
            {
                //Enable normal and fence damage scale. 
                QHuman.UpdateHumanPlayerHealth(float.MaxValue, 0.0f, -1);
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Health Permanent Enabled");
            }
            else if (healthAction == QUtils.HEALTH_ACTION.TEMPORARY)
            {
                unsafe
                {
                    //Enable Player Hit damage.
                    GT.GT_WriteMemory(humanDamageHitAddr, "byte", "1");
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Health Hit Enabled");
                }
            }
        }

        internal static void StartLevel(int level, bool windowed = false)
        {
            try
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Called with level " + level + " windowed: " + windowed);
                if (level <= 0 || level > QUtils.GAME_MAX_LEVEL) throw new ArgumentNullException("Level must be between 1-" + QUtils.GAME_MAX_LEVEL);

                var igiProc = Process.GetProcessesByName(gameName);
                if (igiProc.Length > 0) igiProc[0].Kill();

                string igiLevelCmd = "start igi_" + (windowed ? "window" : "full") + ".lnk level" + level;
                QUtils.shortcutExist = QUtils.CheckShortcutExist();
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, " Shortcut Exist: " + QUtils.shortcutExist);

                if (QUtils.shortcutExist)
                {
                    FindGame();
                }
                else
                {
                    if (QUtils.ShowGamePathDialog() == System.Windows.Forms.DialogResult.OK)
                    {

                        if (!QUtils.shortcutExist)
                            throw new System.IO.FileNotFoundException("File igi_window.link shortcut not found");
                    }
                }
                QUtils.ResetCurrentLevel();

                if (QUtils.shortcutExist)
                {
                    QUtils.ShellExec(igiLevelCmd);

                }
            }
            catch (Exception ex)
            {
                QLog.LogException(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        internal static void RestartLevel(bool savePosition = true)
        {
            try
            {
                if (savePosition)
                {
                    //Set the human position.
                    var humanPos = QHuman.GetPositionInMeter();
                    var humanAngle = GetRealAngle();
                    if (humanPos.x != 0.0f || humanPos.y != 0.0f)
                    {
                        string qscData = QHuman.UpdatePositionInMeter(humanPos, humanAngle);
                        if (!String.IsNullOrEmpty(qscData)) QCompiler.Compile(qscData, QUtils.gamePath);
                    }
                    QUtils.Sleep(1);
                }
                GT.ShowAppForeground(QUtils.editorAppName);
                QInternals.RestartLevel();
            }
            catch (Exception ex)
            {
                QLog.LogException(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        internal static void GravitySet(float gravity=84.74169159f)
        {
            bool status = GT.GT_WriteMemory(gravityAddr, "float", gravity.ToString(), true);
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Status : " + status);
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Gravity address : 0x" + gravityAddr.ToString("X") + " Value : " + gravity);
        }
    }
}
