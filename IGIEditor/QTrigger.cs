using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IGIEditor
{
    public class QTrigger
    {
        public class TriggerTask
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public string Note { get; set; }
            public string FullLine { get; set; }
            public List<TriggerCondition> Conditions { get; set; } = new List<TriggerCondition>();

            public override string ToString()
            {
                return string.Format("[{0}] {1} ({2})", Id, Type, Note);
            }
        }

        public class TriggerCondition
        {
            public string Name { get; set; }
            public int ParamIndex { get; set; }
            public string Value { get; set; }
        }

        private class TriggerParamInfo
        {
            public string Name { get; }
            public int Index { get; }
            public TriggerParamInfo(string name, int index) { Name = name; Index = index; }
        }

        private static Dictionary<string, List<TriggerParamInfo>> triggerParamMap = new Dictionary<string, List<TriggerParamInfo>>()
        {
            { "LevelFlow", new List<TriggerParamInfo> { new TriggerParamInfo("Complete", 10), new TriggerParamInfo("Failed", 11) } },
            { "StatusMessage", new List<TriggerParamInfo> { new TriggerParamInfo("Send", 9) } },
            { "ConditionalSound", new List<TriggerParamInfo> { new TriggerParamInfo("Condition", 3) } },
            { "ConditionalContainer", new List<TriggerParamInfo> { new TriggerParamInfo("Condition", 3) } },
            { "AlarmControl", new List<TriggerParamInfo> { new TriggerParamInfo("Trigger", 22), new TriggerParamInfo("Alarm", 23) } },
            { "SCameraControl", new List<TriggerParamInfo> { new TriggerParamInfo("Detection", 22), new TriggerParamInfo("On", 20) } },
            { "Terminal", new List<TriggerParamInfo> { new TriggerParamInfo("On", 22) } },
            { "LevelTimer", new List<TriggerParamInfo> { new TriggerParamInfo("On", 9), new TriggerParamInfo("Reset", 10) } },
            { "Switch", new List<TriggerParamInfo> { new TriggerParamInfo("On", 9) } },
            { "Heli", new List<TriggerParamInfo> { new TriggerParamInfo("OpenDoor", 16), new TriggerParamInfo("CanFire", 17), new TriggerParamInfo("PlaySound", 18) } },
            { "Car", new List<TriggerParamInfo> { new TriggerParamInfo("OpenDoor", 16), new TriggerParamInfo("CanFire", 17), new TriggerParamInfo("PlaySound", 18) } }
        };

        public static readonly string[] AvailableTriggerProperties = new string[] {
            "isDead", "isExploded", "isDestroyed", "isAlarm", "isTrigger", "isDetection", "isHacked", "isPressed", "isLastPressed", "isClosed", "isOpen", "isLocked", "isFinished", "isSendt", "isSpawned", "isFailed", "isComplete",
            "isOn", "isReset", "isPlaying", "isRun", "isStop", "isStart", "isMoving", "isInUse", "isSearched", "isPicked", "isPickedUp",
            "nValue", "vValue", "nUserData", "vFloor"
        };

        public static List<TriggerTask> ParseTriggerTasks(string qscData)
        {
            var triggerTasks = new List<TriggerTask>();
            if (string.IsNullOrEmpty(qscData)) return triggerTasks;

            int index = 0;
            while ((index = qscData.IndexOf("Task_New", index)) != -1)
            {
                int start = index;
                int balance = 0;
                bool inQuotes = false;
                int end = -1;
                for (int i = index; i < qscData.Length; i++)
                {
                    char c = qscData[i];
                    if (c == '"' && (i == 0 || qscData[i - 1] != '\\')) inQuotes = !inQuotes;
                    if (!inQuotes)
                    {
                        if (c == '(') balance++;
                        else if (c == ')')
                        {
                            balance--;
                            if (balance == 0)
                            {
                                end = i;
                                break;
                            }
                        }
                    }
                }

                if (end != -1)
                {
                    string fullLine = qscData.Substring(start, end - start + 1);
                    var triggerTask = ParseSingleTask(fullLine);
                    if (triggerTask != null)
                    {
                        triggerTasks.Add(triggerTask);
                    }
                    index = end + 1;
                }
                else
                {
                    index += 8;
                }
            }
            return triggerTasks;
        }

        private static TriggerTask ParseSingleTask(string fullLine)
        {
            List<string> args = SplitArgs(fullLine);
            if (args.Count < 3) return null;

            string type = args[1].Trim().Replace("\"", "");
            if (triggerParamMap.ContainsKey(type))
            {
                var task = new TriggerTask();
                string idPart = args[0].Trim();
                int parenIdx = idPart.IndexOf('(');
                string idStr = (parenIdx != -1) ? idPart.Substring(parenIdx + 1).Trim() : idPart.Trim();

                int id;
                if (!int.TryParse(idStr, out id)) id = -1;
                
                // Skip triggers with -1 ID (invalid)
                if (id == -1) return null;
                
                task.Id = id;
                task.Type = type;
                task.Note = args[2].Trim().Replace("\"", "");

                if (type == "StatusMessage" && args.Count > 10)
                {
                    string statusText = args[10].Trim().Replace("\"", "");
                    if (!string.IsNullOrEmpty(statusText))
                        task.Note += " [" + statusText + "]";
                }

                task.FullLine = fullLine;

                foreach (var info in triggerParamMap[type])
                {
                    if (args.Count > info.Index)
                    {
                        task.Conditions.Add(new TriggerCondition
                        {
                            Name = info.Name,
                            ParamIndex = info.Index,
                            Value = args[info.Index].Trim().Trim('"').Replace("\n", " ").Replace("\r", "")
                        });
                    }
                }
                return task;
            }
            return null;
        }

        private static List<string> SplitArgs(string taskNewCall)
        {
            List<string> args = new List<string>();
            int start = taskNewCall.IndexOf('(');
            if (start == -1) return args;
            start++;

            int balance = 0;
            bool inQuotes = false;
            int argStart = start;

            for (int i = start; i < taskNewCall.Length; i++)
            {
                char c = taskNewCall[i];
                if (c == '"' && (i == 0 || taskNewCall[i - 1] != '\\')) inQuotes = !inQuotes;
                if (!inQuotes)
                {
                    if (c == '(') balance++;
                    else if (c == ')')
                    {
                        balance--;
                        if (balance < 0) // End of main Task_New
                        {
                            args.Add(taskNewCall.Substring(argStart, i - argStart));
                            break;
                        }
                    }
                    else if (c == ',' && balance == 0)
                    {
                        args.Add(taskNewCall.Substring(argStart, i - argStart));
                        argStart = i + 1;
                    }
                }
            }
            return args;
        }

        public static string UpdateTaskCondition(string qscData, TriggerTask task, string conditionName, string newValue)
        {
            var cond = task.Conditions.FirstOrDefault(c => c.Name == conditionName);
            if (cond == null) return qscData;

            List<string> args = SplitArgs(task.FullLine);
            if (args.Count <= cond.ParamIndex) return qscData;

            args[cond.ParamIndex] = " \"" + newValue + "\"";

            string newCall = "Task_New(" + string.Join(",", args) + ")";

            int lineIdx = qscData.IndexOf(task.FullLine);
            if (lineIdx != -1)
            {
                return qscData.Remove(lineIdx, task.FullLine.Length).Insert(lineIdx, newCall);
            }
            return qscData;
        }

        public static List<string> GetIndividualTriggers(string fullCondition)
        {
            if (string.IsNullOrWhiteSpace(fullCondition) || fullCondition == "1" || fullCondition == "0")
                return new List<string>();

            string[] splitters = new string[] { "||", "&&" };
            return fullCondition.Split(splitters, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrEmpty(s))
                                .Distinct()
                                .ToList();
        }
    }
}
