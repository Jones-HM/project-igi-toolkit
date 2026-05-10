using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IGIEditor
{
    class QTrigger
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
            "nActiveID", "zData", "nValue", "vValue", "nUserData", "vFloor", "nWantedFloor", "vLastFloor", "nDetectionTime", "isHackedThisTick", "isFinishedThisTick", "nTickSendt", "nTick",
            "isLastRun", "vGenerateFactor", "nBackupTimer", "nInactive", "isLastInUse", "nTriggerLastTick", "isLastDetection", "isLastDestroyed", "isLastOn", "nDoorOpenTicks", "isLastOpen", "isLastClosed", "nTicksSinceFinishedDisplay", "nFinishedDisplay", "isFinishedDisplay", "nSpawns", "eDifficulty", "eTeam"
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

                    // Bug Fix: To find nested tasks, we only advance by "Task_New".Length
                    // and continue searching within the current task's content.
                    index += 8;
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

        public enum TriggerOperator { OR, AND, MIXED, NONE }

        public static TriggerOperator InferOperator(string fullCondition)
        {
            if (string.IsNullOrEmpty(fullCondition)) return TriggerOperator.NONE;
            bool hasOr = fullCondition.Contains("||");
            bool hasAnd = fullCondition.Contains("&&");

            if (hasOr && hasAnd) return TriggerOperator.MIXED;
            if (hasOr) return TriggerOperator.OR;
            if (hasAnd) return TriggerOperator.AND;
            return TriggerOperator.NONE;
        }

        public static List<string> GetIndividualTriggers(string fullCondition)
        {
            if (string.IsNullOrWhiteSpace(fullCondition) || fullCondition == "1" || fullCondition == "0")
                return new List<string>();

            var triggers = new List<string>();
            var tokens = Tokenize(fullCondition);

            foreach (var token in tokens)
            {
                if (token.Type == TriggerTokenType.IDENTIFIER)
                {
                    // Filter out Task_New leaf nodes as requested
                    if (!token.Value.Contains("Task_New"))
                        triggers.Add(token.Value);
                }
            }
            return triggers.Distinct().ToList();
        }

        public enum TriggerTokenType { IDENTIFIER, OPERATOR_OR, OPERATOR_AND, LPAREN, RPAREN, NOT }
        public class TriggerToken
        {
            public TriggerTokenType Type { get; set; }
            public string Value { get; set; }
        }

        public static List<TriggerToken> Tokenize(string expression)
        {
            var tokens = new List<TriggerToken>();
            if (string.IsNullOrEmpty(expression)) return tokens;

            var pattern = @"(\|\||&&|\(|\)|!|[a-zA-Z_][a-zA-Z0-9_]*(\.\w+)?)";
            var matches = Regex.Matches(expression, pattern);

            foreach (Match match in matches)
            {
                string val = match.Value;
                var token = new TriggerToken { Value = val };
                if (val == "||") token.Type = TriggerTokenType.OPERATOR_OR;
                else if (val == "&&") token.Type = TriggerTokenType.OPERATOR_AND;
                else if (val == "(") token.Type = TriggerTokenType.LPAREN;
                else if (val == ")") token.Type = TriggerTokenType.RPAREN;
                else if (val == "!") token.Type = TriggerTokenType.NOT;
                else token.Type = TriggerTokenType.IDENTIFIER;
                tokens.Add(token);
            }
            return tokens;
        }

        public static string RebuildExpression(List<string> activeTriggers, TriggerOperator op)
        {
            if (activeTriggers == null || activeTriggers.Count == 0) return "0";
            if (activeTriggers.Count == 1) return activeTriggers[0];

            string operatorStr = (op == TriggerOperator.AND) ? " && " : " || ";
            return string.Join(operatorStr, activeTriggers);
        }

        // Future-proofing: return a list of tokens but filter out Task_New for the simple list-based UI
        public static List<string> GetCleanIdentifiers(string fullCondition)
        {
            return Tokenize(fullCondition)
                .Where(t => t.Type == TriggerTokenType.IDENTIFIER && !t.Value.Contains("Task_New"))
                .Select(t => t.Value)
                .Distinct()
                .ToList();
        }
    }
}
