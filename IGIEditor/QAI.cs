using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace IGIEditor
{
    class QAI
    {
        public class HumanAI
        {
            public int Id { get; }
            public string AIType { get; }
            public int GraphId { get; }
            public HumanAI(int id, string aiType, int graphId) => (Id, AIType, GraphId) = (id, aiType, graphId);
        }

        public class HumanSoldier
        {
            public int SoldierId { get; }
            public Real64 Position { get; }
            public float Angle { get; }
            public string ModelId { get; }
            public int TeamId { get; }
            public int BoneHierarchy { get; }
            public int StandAnimation { get; }
            public HumanAI HumanAIData { get; }
            public HumanSoldier(int soldierId, Real64 position, float angle, string modelId, int teamId, int boneHierarchy, int standAnimation, HumanAI humanAIData)
                => (SoldierId, Position, Angle, ModelId, TeamId, BoneHierarchy, StandAnimation, HumanAIData) = (soldierId, position, angle, modelId, teamId, boneHierarchy, standAnimation, humanAIData);
        }

        public class HumanAIJson
        {
            public int aiCount { get; set; }
            public string aiType { get; set; }
            public int graphId { get; set; }
            public string weapon { get; set; }
            public string model { get; set; }
            public bool guardGenerator { get; set; }
            public int maxSpawns { get; set; }
            public int teamId { get; set; }
            public bool invincible { get; set; }
            public bool advanceView { get; set; }

            public HumanAIJson() { }
            public HumanAIJson(int aiCount, string aiType, int graphId, string weapon, string model, bool guardGenerator, int maxSpawns, int teamId, bool invincible, bool advanceView)
            {
                this.aiCount = aiCount; this.aiType = aiType; this.graphId = graphId; this.weapon = weapon;
                this.model = model; this.guardGenerator = guardGenerator; this.maxSpawns = maxSpawns; this.teamId = teamId;
                this.invincible = invincible; this.advanceView = advanceView;
            }
        }

        class AIModel
        {
            string modelName;
            string modelId;
            char option;
            List<int> levels;

            public string ModelName { get => modelName; set => modelName = value; }
            public string ModelId { get => modelId; set => modelId = value; }
            public char Option { get => option; set => option = value; }
            public List<int> Levels { get => levels; set => levels = value; }

            public AIModel Add(string modelName, string modelId, char option, List<int> levels)
            {
                this.ModelName = modelName;
                this.ModelId = modelId;
                this.Option = option;
                this.Levels = levels;
                return this;
            }
        }

        private static List<AIModel> aiModelList = new List<AIModel>();

        internal static string AddHumanSoldier(string aiType, int aiScriptId, int graphId, Real64 position, float angle, string aiModel, int team, bool addWeapon, string weapon, int ammo, bool guardGenerator)
        {
            if (position == null) position = QHuman.GetHumanTaskList().qtask.position;

            int boneHeirarchy = 1;
            if (aiModel == "015_01_1" || aiModel == "012_01_1")//HumanSoldierFemale.
                boneHeirarchy = GetBoneHeirarchy(aiModel);

            return AddHumanSoldier(aiScriptId, "A.I - " + aiModel, aiType, aiScriptId + 1, graphId, position, angle, aiModel, team, boneHeirarchy, -1, addWeapon, weapon, ammo, guardGenerator);
        }

        internal static string AddHumanSoldier(int taskId, string taskNote, string aiType, int aiScriptId, int graphId, Real64 position, float angle, string model, int team, int boneHeirachy, int standAnimation, bool addWeapon, string weapon, int ammo, bool guardGenerator)
        {
            //Add the A.I (Human soldier)
            string humanSoldierType = (model == "015_01_1" || model == "012_01_1") ? "HumanSoldierFemale" : "HumanSoldier";
            string qtaskSoldier = "\nTask_New(" + taskId + ",\"" + humanSoldierType + "\",\"" + taskNote + "\"," + position.x + "," + position.y + "," + position.z + "," + angle + ",\"" + model + "\"," + team + "," + boneHeirachy + "," + standAnimation + ",\n";
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Ai AIType: '" + aiType + "' ID : " + taskId + ",HumanSoldier : " + QObjects.FindModelName(model) + ", Position: \"," + position.x + "," + position.y + "," + position.z + ", Angle: " + angle + ",\", Model:" + model + "\", TeamId: " + team + ", BoneHeirachy: " + boneHeirachy + ", Stand Animation: " + standAnimation + ",\n");

            //Add A.I type to status message.
            if (team == 0) QUtils.aiFriendTask += humanSoldierType + "_" + taskId + ".isDead && ";

            else QUtils.aiEnenmyTask += humanSoldierType + "_" + taskId + ".isDead && ";

            //Add the weapon.
            if (addWeapon) qtaskSoldier += QHuman.AddWeapon(weapon, ammo);

            //Add AI's script and graph data.
            qtaskSoldier += "Task_New(" + aiScriptId + ",\"HumanAI\",\"" + taskNote + "\",\"" + aiType + "\"," + graphId;
            qtaskSoldier += (!guardGenerator) ? "));" : ")));";
            return qtaskSoldier;
        }

        internal static string AddGuardGenerator(string taskNote = "AI Army", int maxSpawn = 10)
        {
            string qTaskGuardGen = "Task_New(-1, \"GuardGenerator\",\"" + taskNote + "\"," + "\"!HumanPlayer_0.isDead\"," + maxSpawn + ",";
            return qTaskGuardGen;
        }

        internal static string AddHumanSoldier(HumanAIJson humanAiJson)
        {
            bool guardGenerator = humanAiJson.guardGenerator, advanceView = humanAiJson.advanceView, invulnerability = humanAiJson.invincible;
            int maxSpawns = humanAiJson.maxSpawns;
            string aiType = null, aiWeapon = null, modelId = null;
            int aiCount = 1, teamId = 0, aiAmmo = 999;
            int graphId = 0, aiId = 0, patrolId = 0;
            string qscData = null, aiModelName = QObjects.FindModelName(humanAiJson.model);

            //QUtils.aiScriptId = QTask.GenerateUniqueQTaskId(QUtils.aiScriptId);
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "A.I Script Id: " + QUtils.aiScriptId);

            if (humanAiJson != null) aiCount = humanAiJson.aiCount;

            int aiWorkTotal = humanAiJson.aiCount, aiWorkCount = 1, aiWorkPercent = 1;

            for (int index = 1; index <= aiCount; index++)
            {
                if (humanAiJson != null)
                {
                    aiType = humanAiJson.aiType;
                    graphId = humanAiJson.graphId;
                }

                aiId = QUtils.aiScriptId;
                aiId = QTask.GetUniqueQTaskId(aiId); //Get Unique Id for A.I.
                bool aiIdExist = false; //QGraphs.CheckIdExist(aiId, "AI", QUtils.gGameLevel, "AI Id " + aiId + " already exist for current level");

                patrolId = QUtils.aiScriptId + 2;
                patrolId = QTask.GetUniqueQTaskId(patrolId); //Get Unique Id for PatrolId.
                bool patrolIdExist = false; //QGraphs.CheckIdExist(patrolId, "Patrol", QUtils.gGameLevel, "PatrolId " + patrolId + " already exist for current level");
                bool graphIdExist = true; //QGraphs.CheckIdExist(graphId, "Graph", QUtils.gGameLevel, "GraphId " + graphId + " doesn't exist for current level");

                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "A.I Script Id: " + aiId + " A.I Patrol Id: " + patrolId);

                if (!patrolIdExist && !aiIdExist && graphIdExist)
                {
                    int humanId = Convert.ToInt32(aiId);
                    aiId = humanId + 1;

                    //Set A.I Position on Graph.
                    Real64 aiPos = QGraphs.GetGraphPosition(graphId);
                    float aiAngle = QMemory.GetRealAngle();

                    // Setting some randomness to A.I position.
                    aiPos.x += new Random().Next(1000, 100000);
                    aiPos.y += new Random().Next(1000, 100000);

                    if (humanAiJson != null)
                    {
                        modelId = humanAiJson.model;
                        aiWeapon = humanAiJson.weapon;
                        teamId = humanAiJson.teamId;
                        aiAmmo = 999;
                    }

                    //Add GuardGenerator .
                    if (guardGenerator)
                        qscData += AddGuardGenerator("AI Army", maxSpawns);

                    //Add A.I HumanSoldier.
                    qscData += AddHumanSoldier(aiType, humanId, graphId, aiPos, aiAngle, modelId, teamId, true, aiWeapon, aiAmmo, guardGenerator);

                    //Add A.I Script to HumanSoldier.
                    var aiScriptData = AddAIScriptPath(aiType, graphId, aiId, patrolId, QUtils.gGameLevel, invulnerability, advanceView);
                    if (!String.IsNullOrEmpty(aiScriptData))
                    {
                        qscData += aiScriptData;
                        QLog.AddLog(MethodBase.GetCurrentMethod().Name, "AI Added Success Graph Id : " + graphId + " AI Id : " + aiId + " Patrol Id : " + patrolId);
                    }


                }
                QUtils.aiScriptId += 3;

                aiWorkPercent = (int)Math.Round((double)(100 * aiWorkCount) / aiWorkTotal);
                IGIEditorUI.editorRef.SetStatusText("AI " + aiModelName + " #" + index + " added, Completed " + aiWorkPercent + "%");
                aiWorkCount++;
            }
            QUtils.SwitchEditorUI();
            IGIEditorUI.editorRef.SetStatusText("AI Added Success Graph Id : " + graphId + " AI Id : " + aiId + " Patrol Id : " + patrolId);
            return qscData;
        }

        internal static string AddAIScriptPath(string aiType, int graphId, int aiId, int patrolId, int level, bool invulnerability = false, bool advanceView = false)
        {
            string result = null;
            int patrolAlarmId = 0;

            //Append list from Both path - Ai-Patrol/Ai-Script.
            var aiFilesList = System.IO.Directory.GetFiles(QUtils.qedAiPatrolPath).ToList();
            aiFilesList.AddRange(System.IO.Directory.GetFiles(QUtils.qedAiScriptPath).ToList());

            var aiTypeSplit = aiType.Split('_')[1].ToLower();

            foreach (var file in aiFilesList)
            {
                if (file.Contains(aiTypeSplit))
                {
                    //AI Script section.
                    if (file.Contains("script"))
                    {
                        string aiScriptData = QUtils.LoadFile(file);

                        //Add Idle patrol.
                        if (aiScriptData.Contains(QUtils.patroIdleMask))
                        {
                            aiScriptData = aiScriptData.ReplaceFirst(QUtils.patroIdleMask, patrolId.ToString());
                        }

                        //Add Alarm patrol.
                        if (aiScriptData.Contains(QUtils.patroAlarmMask))
                        {
                            aiScriptData = aiScriptData.ReplaceFirst(QUtils.patroAlarmMask, patrolAlarmId.ToString());
                        }

                        //Add Alarm control Id.
                        if (aiScriptData.Contains("(" + QUtils.alarmControlMask + ")"))
                        {
                            var aiPos = QGraphs.GetGraphPosition(graphId);
                            int alarmControlId = 0;
                            alarmControlId = GetNearestDynamicId(aiPos, QUtils.alarmControl);
                            if (alarmControlId == 0)
                                QLog.ShowWarning("Couldn't find nearest alarm Id for AI : " + aiId + " on Graph : " + graphId);
                            aiScriptData = aiScriptData.ReplaceFirst(QUtils.alarmControlMask, alarmControlId.ToString());
                        }


                        //Add Gunner Id.
                        if (aiScriptData.Contains("(" + QUtils.gunnerIdMask + ")"))
                        {
                            var aiPos = QGraphs.GetGraphPosition(graphId);
                            int gunnerId = 0;
                            gunnerId = GetNearestDynamicId(aiPos, QUtils.stationaryGun);
                            if (gunnerId == 0)
                                QLog.ShowWarning("Couldn't find nearest Gunner Id for AI : " + aiId + " on Graph : " + graphId);
                            aiScriptData = aiScriptData.ReplaceFirst(QUtils.gunnerIdMask, gunnerId.ToString()).ReplaceFirst(QUtils.viewGammaMask, "180");//Set View Gamma to 180.
                        }

                        //Add invulnerability if opted for.
                        if (invulnerability)
                        {
                            string invulnerabilityMode = "\nAIFunction_DefaultHandler();\n" +
                                "AIFunction_SetEventPriority(AIEVENT_COMBAT);\n" +
                                "AIFunction_SetInstantDeath(FALSE);\n" +
                                "AIFunction_SetInvulnerability(TRUE);\n";
                            aiScriptData = aiScriptData.ReplaceFirst("AIFunction_DefaultHandler();", invulnerabilityMode.Trim());
                        }

                        //Add advance View if opted for.
                        if (advanceView)
                        {
                            string advanceViewLengthMode = "\nAIFunction_DefaultHandler();\n" +
                                "AIFunction_SetViewGamma(90000);\n" +
                                " AIFunction_SetSecondaryViewGamma(90000);\n" +
                                "AIFunction_SetSecondaryViewAlpha(90000);\n" +
                                "AIFunction_SetViewLength(90000);\n" +
                                "AIFunction_SetSecondaryViewLength(90000);\n";
                            aiScriptData = aiScriptData.ReplaceFirst("AIFunction_DefaultHandler();", advanceViewLengthMode.Trim());
                        }


                        string aiFileName = aiId + ".qsc";
                        QUtils.aiScriptFiles.Add(aiId + ".qvm");
                        var outputAiPath = QUtils.cfgGamePath + level + @"\ai\";

                        QUtils.SaveFile(aiFileName, aiScriptData);
                        QCompiler.Compile(aiFileName, outputAiPath, 0x0);
                        QUtils.FileIODelete(aiFileName);
                    }

                    //PatrolPath section.
                    else if (file.Contains("path"))
                    {
                        string aiPathData = QUtils.LoadFile(file);

                        //var nodesList = QGraphs.GetAllNodes4mGraph(Convert.ToInt32(graphId));//Slow One Old method.
                        var nodesList = QGraphs.GetNodesForGraph(graphId, level);

                        if (aiPathData.Contains("xxxx"))
                        {
                            aiPathData = aiPathData.Replace("xxxx", patrolId.ToString());
                            aiPathData = aiPathData.Replace(")),", "));");
                            result = aiPathData;

                            //Add Alarm path to selected A.I.
                            if (file.Contains("idle"))
                            {
                                QUtils.aiScriptId++;
                                patrolAlarmId = patrolId + 1;
                                string alarmPathFile = file.Replace("idle", "alarm");

                                string aiAlarmPathData = QUtils.LoadFile(alarmPathFile);
                                aiAlarmPathData = aiAlarmPathData.Replace("xxxx", patrolAlarmId.ToString());
                                aiAlarmPathData = aiAlarmPathData.Replace(")),", "));");
                                result = aiPathData + "\n" + aiAlarmPathData;
                            }
                        }

                        int index = 0;
                        if (nodesList.Count <= 2 && !aiType.Contains("AITYPE_SECURITY_PATROL"))
                        {
                            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "AI Patrol Updated to security for aiId : " + aiId + "\tgraphId : " + graphId);
                            result = AddAIScriptPath("AITYPE_SECURITY_PATROL", graphId, aiId, patrolId, level);
                        }
                        else if (aiType.Contains("AITYPE_SECURITY_PATROL"))
                        {
                            for (char c = 'x'; c <= 'y'; c++)
                            {
                                int randIndex = new Random().Next(0, nodesList.Count - 1);
                                string pattern = @"\b" + c + @"\b";
                                string replace = nodesList[index++].ToString();
                                result = Regex.Replace(result, pattern, replace);
                            }
                        }

                        else
                        {
                            var nIdsList = new List<char>() { 'a', 'c', 'b', 'd', 'x', 'y', 'z' };
                            foreach (var nId in nIdsList)
                            {
                                int randIndex = new Random().Next(0, nodesList.Count - 1);
                                if (nodesList.Count >= 20) index = randIndex;
                                if (index >= nodesList.Count) break;
                                string pattern = @"\b" + nId + @"\b";
                                string replace = nodesList[index++].ToString();
                                result = Regex.Replace(result, pattern, replace);
                            }
                        }
                    }
                }
            }

            return "\n" + result + "\n";
        }


        internal static string AddAiTaskDetection(string qscData)
        {
            //Add A.I detection.
            string statusMsg = null;
            statusMsg += "\nTask_New(-1,\"Container\", \"StatusMessages\"," + "\n";

            if (!String.IsNullOrEmpty(QUtils.aiFriendTask))
            {
                var varStringFriendly = QUtils.aiFriendTask.ReplaceLast("&&", string.Empty).Trim();

                var varStringSplit = QUtils.aiFriendTask.Replace("&&", "#").Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var varString in varStringSplit)
                {
                    statusMsg += QUtils.AddStatusMsg(-1, "Friendly man down! Watch out", varString);
                }

                QUtils.anyaTeamTaskId = QTask.GenerateTaskID();
                statusMsg += QUtils.AddStatusMsg(QUtils.anyaTeamTaskId, "Anya team down. Mission Failed", varStringFriendly);
                QUtils.aiFriendTask = null;
            }

            if (!String.IsNullOrEmpty(QUtils.aiEnenmyTask))
            {
                var varStringEnemy = QUtils.aiEnenmyTask.ReplaceLast("&&", string.Empty).Trim();

                var varStringSplit = QUtils.aiEnenmyTask.Replace("&&", "#").Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var varString in varStringSplit)
                {
                    statusMsg += QUtils.AddStatusMsg(-1, "Enemy man down! Great", varString);
                }

                QUtils.ekkTeamTaskId = QTask.GenerateTaskID();
                statusMsg += QUtils.AddStatusMsg(QUtils.ekkTeamTaskId, "Ekk team down. Mission Completed", varStringEnemy);
                QUtils.aiEnenmyTask = null;
            }

            qscData = statusMsg.ReplaceLast("),", "));");
            return qscData;
        }

        internal static string AddPatrolTask(int patrolId, string taskNote = "Patrol Path")
        {
            string qtaskPatrolPath = "Task_New(" + patrolId + ",\"PatrolPath\",\"" + taskNote + "\"," + "\n";
            return qtaskPatrolPath;
        }

        internal static string AddPatrolCommand(string patrolTask, string taskNote, PATROLACTIONS pathCmd, int pathParam, bool lastCmd = false)
        {
            patrolTask += "Task_New(-1,\"PatrolPathCommand\"," + taskNote + pathCmd + "," + pathParam + ")";
            patrolTask += (lastCmd) ? ")," : ",";
            return patrolTask;
        }

        public static string ReadPatrolData(string patrolFile, int patrolId)
        {
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Started ExtractPatrolPathBlock for PatrolId {patrolId}");
            string fileContent = QUtils.LoadFile(patrolFile);
            if (String.IsNullOrEmpty(fileContent))
            {
                QLog.ShowLogError(MethodBase.GetCurrentMethod().Name, "File content is empty.");
                return null;
            }
            string trimmedContent = fileContent.Trim();
            string pattern = $@"Task_New\(\s*{patrolId}\s*,\s*""PatrolPath""\s*,\s*""[^""]*""\s*,\s*(?<block>(?>[^()]+|\((?<DEPTH>)|\)(?<-DEPTH>))*)(?(DEPTH)(?!))\)";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match match = regex.Match(trimmedContent);
            if (!match.Success)
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Data is incorrect format.");
                return null;
            }
            string result = match.Value.Trim();
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Extraction completed successfully with result: " + result);
            return result;
        }


        public static void SavePatrolData(string filename, int patrolId, string dataToUpdate)
        {
            string newData = dataToUpdate.Trim();
            string validatePattern = $@"^Task_New\(\s*{patrolId}\s*,\s*""PatrolPath""";

            if (!Regex.IsMatch(newData, validatePattern))
            {
                QLog.ShowLogError(MethodBase.GetCurrentMethod().Name, "Data is incorrect format.");
                return;
            }

            string fileContent = System.IO.File.ReadAllText(filename);

            if (String.IsNullOrEmpty(fileContent))
            {
                QLog.ShowLogError(MethodBase.GetCurrentMethod().Name, "File content is empty.");
                return;
            }

            string blockPattern = $@"Task_New\(\s*{patrolId}\s*,\s*""PatrolPath""\s*,\s*""""\s*,\s*(?<block>(?>[^()]+|\((?<DEPTH>)|\)(?<-DEPTH>))*)(?(DEPTH)(?!))\)";
            Regex regex = new Regex(blockPattern, RegexOptions.Singleline);
            Match match = regex.Match(fileContent);

            if (!match.Success)
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"No block found for PatrolId {patrolId}.");
                return;
            }

            string updatedContent = regex.Replace(fileContent, newData);
            QUtils.SaveFile(filename, updatedContent);
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Patrol data for id {patrolId} updated successfully.");
        }

        public static List<int> GetPatrolIds(string filename)
        {
            string fileContent = System.IO.File.ReadAllText(filename);

            if (String.IsNullOrEmpty(fileContent))
            {
                QLog.ShowLogError(MethodBase.GetCurrentMethod().Name, "File content is empty.");
                return null;
            }

            var ids = new List<int>();
            string pattern = @"Task_New\(\s*(?<id>-?\d+)\s*,\s*""PatrolPath""";
            foreach (Match m in Regex.Matches(fileContent, pattern, RegexOptions.Singleline))
            {
                if (int.TryParse(m.Groups["id"].Value, out int id))
                    ids.Add(id);
            }
            return ids;
        }


        internal static string RemoveHumanSoldier(string qscData, string aiModel)
        {
            int startIndex = 0, endIndex = 0, lcount = 0, rcount = 0;
            bool startRun = false;
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "called with model : " + aiModel + "\n");

            if (String.IsNullOrEmpty(qscData) || String.IsNullOrEmpty(aiModel))
            {
                QLog.ShowLogError(MethodBase.GetCurrentMethod().Name, "Input data is empty.");
                return null;
            }

            qscData = qscData.Trim();
            var qscDataSplit = qscData.Split('\n');
            string qscTmp = String.Copy(qscData);

            foreach (var data in qscDataSplit)
            {
                if (data.Contains(QUtils.taskNew))
                {
                    if (data.Contains(aiModel) && data.Contains("HumanSoldier"))
                    {
                        if (data.Contains("Task_New(-1,"))
                        {
                            startRun = false;
                        }
                        else
                        {
                            if (data.Contains('('))
                                lcount += data.Count(o => o == '(');

                            startIndex = qscData.IndexOf(data);
                            if (startIndex == -1)
                            {
                                QLog.ShowLogError(MethodBase.GetCurrentMethod().Name, "Data couldn't be found in QData file");
                                return qscTmp;
                            }
                            endIndex += data.Length + 1;
                            startRun = true;
                            continue;
                        }
                    }
                    if (startRun)
                    {
                        if (lcount >= 1)
                            endIndex += data.Length + 1;

                        if (data.Contains('('))
                            lcount += data.Count(o => o == '(');

                        if (data.Contains(')'))
                            rcount += data.Count(o => o == ')');

                        if (lcount == rcount)
                        {
                            startRun = false;
                            var aiSub = qscData.Substring(startIndex, endIndex);
                            qscData = qscData.Replace(aiSub, String.Empty);
                            startIndex = endIndex = lcount = rcount = 0;
                        }
                    }
                }
            }

            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "start index : " + startIndex + "  end index : " + endIndex + "\n");
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Ai Model '" + aiModel + "' removed successfully.");
            return qscData;
        }

        internal static List<string> GetAiModelIds(int level)
        {
            string inputQscPath = QUtils.cfgQscPath + level + "\\" + QUtils.objectsQsc;
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "called with level : " + level);
            string qscData = QUtils.LoadFile(inputQscPath);
            List<string> aiModelIdsList = new List<string>();
            var modelRegex = @"\d{3}_\d{2}_\d{1}";
            var dataLines = qscData.Split('\n');

            foreach (var data in dataLines)
            {
                if (data.Contains("HumanSoldier"))
                {
                    string model = Regex.Match(data, modelRegex).Value;
                    //Friendly A.I exclude.
                    if (model == "015_01_1" || model == "020_01_1" || model == "021_01_1"
                        || model == "022_01_1" || model == "009_01_1" || model == "000_01_1")
                        continue;
                    else
                        aiModelIdsList.Add(model);
                }
            }

            aiModelIdsList = aiModelIdsList.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            return aiModelIdsList;
        }

        internal static List<KeyValuePair<int, Real64>> GetDynamicIds4AI(string dynamicType, bool fromBackup = true)
        {
            string qscData = null;
            if (fromBackup)
            {
                string qscBackupPath = QUtils.cfgQscPath + QUtils.gGameLevel + "\\" + QUtils.objectsQsc;
                qscData = QCryptor.Decrypt(qscBackupPath);
            }

            if (!fromBackup)
                qscData = QUtils.LoadFile();

            var qscDataLines = qscData.Split('\n');
            var dynamiclIdsList = new List<KeyValuePair<int, Real64>>();
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "caled with level : " + QUtils.gGameLevel + " with type : " + dynamicType);

            foreach (var dataLine in qscDataLines)
            {
                if (dataLine.Contains(QUtils.taskNew) && dataLine.Contains(dynamicType))
                {
                    var dynamicTypeData = dataLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, "dynamicType[1] : " + dynamicTypeData[1]);

                    if (dynamicTypeData[1].Trim() == "\"" + dynamicType + "\"")
                    {
                        QLog.AddLog(MethodBase.GetCurrentMethod().Name, "dynamicType Data : " + dynamicTypeData);

                        int dynamicTypeId = Int32.Parse(Regex.Match(dynamicTypeData[0], @"\d+").Value);
                        QLog.AddLog(MethodBase.GetCurrentMethod().Name, "dynamicTypeId : " + dynamicTypeId);

                        Double xPos = Double.Parse(dynamicTypeData[3].Trim());
                        Double yPos = Double.Parse(dynamicTypeData[4].Trim());
                        Double zPos = Double.Parse(dynamicTypeData[5].Trim());

                        Real64 alarmControlPos = new Real64(xPos, yPos, zPos);
                        dynamiclIdsList.Add(new KeyValuePair<int, Real64>(dynamicTypeId, alarmControlPos));
                    }
                }
            }
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "returned list count : " + dynamiclIdsList.Count);
            return dynamiclIdsList;
        }

        internal static int GetNearestDynamicId(Real64 aiPos, string dynamicType)
        {
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "called with pos X : " + aiPos.x + " Y : " + aiPos.y + " Z : " + aiPos.z + " dynamicType : " + dynamicType);

            var dynamicIds = GetDynamicIds4AI(dynamicType, false);
            var diffPosList = new List<KeyValuePair<int, Real64>>();
            int nearestDynamicId = 0;

            if (dynamicIds.Count > 1)
            {
                foreach (var alarmControlId in dynamicIds)
                {
                    var alarmPos = alarmControlId.Value;
                    var diffPos = GetPosDiff(aiPos, alarmPos);
                    diffPosList.Add(new KeyValuePair<int, Real64>(alarmControlId.Key, diffPos));
                }
                diffPosList = diffPosList.OrderBy(o => o.Value.x).ToList();
                nearestDynamicId = diffPosList[0].Key;
            }
            else if (dynamicIds.Count == 1)
            {
                nearestDynamicId = dynamicIds[0].Key;
            }
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "returned : " + nearestDynamicId);
            return nearestDynamicId;
        }

        internal static Real64 GetPosDiff(Real64 pos1, Real64 pos2)
        {
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "called");
            Real64 diffPos = new Real64();

            diffPos.x = Math.Abs(pos1.x - pos2.x);
            diffPos.y = Math.Abs(pos1.y - pos2.y);
            diffPos.z = Math.Abs(pos1.z - pos2.z);
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "returned");
            return diffPos;
        }

        private static int GetBoneHeirarchy(string model)
        {
            int boneHeirarchy = 1;
            int level = QUtils.gGameLevel;
            string inputQscPath = QUtils.cfgQscPath + level + "\\" + QUtils.objectsQsc;
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "level : called with level : " + level + " model : " + model);
            string qscData = QUtils.LoadFile(inputQscPath);
            List<string> aiModelsList = new List<string>();
            var modelRegex = @"\d{3}_\d{2}_\d{1}";
            var dataLines = qscData.Split('\n');

            foreach (var data in dataLines)
            {
                if (data.Contains("HumanSoldier") || data.Contains("HumanSoldierFemale"))
                {
                    string modelData = Regex.Match(data, modelRegex, RegexOptions.RightToLeft).Value;
                    if (model == modelData)
                    {
                        var dataSplit = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string heirarchy = dataSplit[(dataSplit.Length - 1) - 2];
                        boneHeirarchy = Convert.ToInt32(heirarchy);
                        break;
                    }
                }
            }
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, "returned boneHeirarchy : " + boneHeirarchy);
            return boneHeirarchy;
        }

        //A.I Models list for all levels.
        private static void InitAiModelList()
        {
            var allLevelsList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            var frieldyForcesList = new List<int>() { 4, 6, 12, 13, 14 };
            aiModelList.Add(new AIModel().Add("ANYA", "015_01_1", 'e', new List<int>() { 7, 8 }));
            aiModelList.Add(new AIModel().Add("EKK", "012_01_1", 'i', new List<int>() { 12, 14 }));
            aiModelList.Add(new AIModel().Add("JONES", "000_01_1", 'i', allLevelsList));
            aiModelList.Add(new AIModel().Add("SNIPER_01", "001_01_1", 'i', new List<int>() { 1, 3, 9, 13 }));
            aiModelList.Add(new AIModel().Add("SNIPER_02", "001_02_1", 'i', new List<int>() { 7, 8 }));
            aiModelList.Add(new AIModel().Add("GUNNER_406", "011_01_1", 'i', new List<int>() { 6 }));
            aiModelList.Add(new AIModel().Add("GUNNER_407", "011_03_1", 'i', new List<int>() { 6 }));
            aiModelList.Add(new AIModel().Add("SCIENTIST", "011_02_1", 'i', new List<int>() { 6, 9 }));
            aiModelList.Add(new AIModel().Add("PATROL_AK_01", "003_01_1", 'e', new List<int>() { 6, 8, 11, 12, 13, 14 }));
            aiModelList.Add(new AIModel().Add("PATROL_AK_02", "003_02_1", 'i', new List<int>() { 7, 8, 11 }));
            aiModelList.Add(new AIModel().Add("PATROL_AK_03", "006_01_1", 'i', new List<int>() { 7, 11 }));
            aiModelList.Add(new AIModel().Add("SECURITY_PATROL_SPAS", "019_01_1", 'i', new List<int>() { 1, 3, 5, 7, 8, 9, 11, 14 }));
            aiModelList.Add(new AIModel().Add("GUNNER", "013_01_1", 'i', new List<int>() { 2, 4, 7, 8, 9, 10, 12, 13 }));
            aiModelList.Add(new AIModel().Add("SOLDIER", "008_01_1", 'i', new List<int>() { 3, 5, 9, 11, 12 }));
            aiModelList.Add(new AIModel().Add("HARRISON", "022_01_1", 'i', new List<int>() { 4, 6, 13 }));
            aiModelList.Add(new AIModel().Add("FRIENDLY_SOLDIER_1", "021_01_1", 'i', frieldyForcesList));
            aiModelList.Add(new AIModel().Add("FRIENDLY_SOLDIER_2", "020_01_1", 'i', frieldyForcesList));
            aiModelList.Add(new AIModel().Add("JOSEP_PRIBOI", "009_02_1", 'i', new List<int>() { 4 }));
            aiModelList.Add(new AIModel().Add("MAFIA_GUARD", "014_01_1", 'i', new List<int>() { 6 }));
            aiModelList.Add(new AIModel().Add("MAFIA_PATROL", "014_02_1", 'i', new List<int>() { 6 }));
            aiModelList.Add(new AIModel().Add("PRIBOI", "009_01_1", 'i', new List<int>() { 6, 7, 9, 10 }));
            aiModelList.Add(new AIModel().Add("GUARD_AK", "004_02_1", 'i', new List<int>() { 9, 10 }));
            aiModelList.Add(new AIModel().Add("SPETNAZ_GUARD_AK", "018_01_1", 'i', new List<int>() { 9, 10, 12, 13, 14 }));
        }

        internal static List<string> GetAiModelNamesList(int level)
        {
            if (aiModelList.Count == 0)
                InitAiModelList();

            var aiModelNamesList = new List<string>();
            foreach (var aiModel in aiModelList)
            {
                if (aiModel.Option == 'i')
                {
                    if (aiModel.Levels.Contains(level))
                        aiModelNamesList.Add(aiModel.ModelName);
                }
                else if (aiModel.Option == 'e')
                {
                    if (!aiModel.Levels.Contains(level))
                        aiModelNamesList.Add(aiModel.ModelName);
                }
            }
            return aiModelNamesList;
        }


        internal static string GetAiModelId4Name(string aiModelName)
        {
            if (aiModelList.Count == 0)
                InitAiModelList();
            foreach (var aiModel in aiModelList)
            {
                if (aiModelName == aiModel.ModelName)
                {
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, "model name '" + aiModelName + "' Returned model id : " + aiModel.ModelId);
                    return aiModel.ModelId;
                }
            }
            return null;
        }

        internal static List<string> GetAiTypes()
        {
            return QUtils.aiTypes;
        }

        public static HumanSoldier ReadHumanSoldierBySoldierId(string fileName = "objects.qvm", int soldierId = 0)
        {
            try
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Start reading SoldierId: {soldierId} from {fileName}");

                string content = QUtils.LoadFile(fileName);

                if (string.IsNullOrEmpty(content))
                {
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"File {fileName} is empty.");
                    return null;
                }
                content = content.Trim();
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Loaded file length: {content.Length}");
                string pattern = @"Task_New\(\s*(?<SoldierId>\d+)\s*,\s*""HumanSoldier""\s*,\s*""[^""]*""\s*,\s*(?<PosX>-?\d+(?:\.\d+)?)\s*,\s*(?<PosY>-?\d+(?:\.\d+)?)\s*,\s*(?<PosZ>-?\d+(?:\.\d+)?)\s*,\s*(?<Angle>-?\d+)\s*,\s*""(?<AIType>[^""]+)""\s*,\s*(?<TeamId>-?\d+)\s*,\s*(?<BoneHierarchy>-?\d+)\s*,\s*(?<StandAnimation>-?\d+)";
                var soldierMatch = Regex.Matches(content, pattern)
                                         .Cast<Match>()
                                         .FirstOrDefault(m => int.Parse(m.Groups["SoldierId"].Value) == soldierId);

                if (soldierMatch == null)
                {
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"SoldierId {soldierId} not found.");
                    return null;
                }

                double posX = double.Parse(soldierMatch.Groups["PosX"].Value, CultureInfo.InvariantCulture);
                double posY = double.Parse(soldierMatch.Groups["PosY"].Value, CultureInfo.InvariantCulture);
                double posZ = double.Parse(soldierMatch.Groups["PosZ"].Value, CultureInfo.InvariantCulture);
                int angle = int.Parse(soldierMatch.Groups["Angle"].Value);
                string modelId = soldierMatch.Groups["ModelId"].Value;
                int teamId = int.Parse(soldierMatch.Groups["TeamId"].Value);
                int boneHierarchy = int.Parse(soldierMatch.Groups["BoneHierarchy"].Value);
                int standAnimation = int.Parse(soldierMatch.Groups["StandAnimation"].Value);

                var humanAIMatch = Regex.Match(soldierMatch.Value,
                    @"Task_New\(\s*(?<Id>\d+)\s*,\s*""HumanAI""\s*,\s*""[^""]*""\s*,\s*""(?<AIType>[^""]+)""\s*,\s*(?<GraphId>-?\d+)\s*\)");

                HumanAI humanAIData = humanAIMatch.Success
                    ? new HumanAI(
                          int.Parse(humanAIMatch.Groups["Id"].Value),
                          humanAIMatch.Groups["AIType"].Value,
                          int.Parse(humanAIMatch.Groups["GraphId"].Value))
                    : null;

                if (humanAIData != null)
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Found HumanAI Id: {humanAIData.Id}");

                QLog.AddLog(MethodBase.GetCurrentMethod().Name,
                    $"Parsed SoldierId {soldierId}: Pos({posX},{posY},{posZ}), Angle {angle}, ModelId {modelId}, TeamId {teamId}, " +
                    $"BoneHierarchy {boneHierarchy}, StandAnimation {standAnimation}, HumanAI Id {humanAIData.Id}, AIType {humanAIData.AIType}, GraphId {humanAIData.GraphId}");

                return new HumanSoldier(soldierId, new Real64(posX, posY, posZ), angle, modelId, teamId, boneHierarchy, standAnimation, humanAIData);
            }
            catch (Exception ex)
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Exception: {ex.Message}", DEBUG_TYPE.Error);
                return null;
            }
        }

        public static HumanSoldier ReadHumanSoldierByHumanAIId(string fileName = "objects.qvm", int targetHumanAIId = 0)
        {
            try
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Start reading HumanAI Id: {targetHumanAIId} from {fileName}");
                string content = QUtils.LoadFile(fileName);
                if (string.IsNullOrEmpty(content))
                {
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"File {fileName} is empty.");
                    return null;
                }

                // Trim each line and rebuild the content for consistency.
                var trimmedLines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                                          .Select(line => line.Trim());
                content = string.Join(Environment.NewLine, trimmedLines);
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Loaded file length: {content.Length}");

                var lines = trimmedLines;

                // Locate the HumanAI line matching the targetHumanAIId.
                var humanAILine = lines
                    .Select((line, idx) => new { line, idx })
                    .FirstOrDefault(x => x.line.Contains("\"HumanAI\"") &&
                        int.TryParse(Regex.Match(x.line, @"Task_New\(\s*(?<Id>-?\d+)\s*,\s*""HumanAI""")
                        .Groups["Id"].Value, out int id) && id == targetHumanAIId);
                if (humanAILine == null)
                {
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"HumanAI Id {targetHumanAIId} not found.");
                    return null;
                }
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Found HumanAI at line {humanAILine.idx}");

                // Locate the nearest preceding HumanSoldier line.
                var soldierLine = lines
                    .Select((line, idx) => new { line, idx })
                    .Where(x => x.idx < humanAILine.idx && x.line.Contains("\"HumanSoldier\""))
                    .LastOrDefault();
                if (soldierLine == null)
                {
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"No HumanSoldier found for HumanAI Id {targetHumanAIId}");
                    return null;
                }
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Found HumanSoldier at line {soldierLine.idx}");

                // Assemble the complete soldier block (handles nested parentheses).
                int balance = 0;
                var sb = new System.Text.StringBuilder();
                foreach (var line in lines.Skip(soldierLine.idx))
                {
                    foreach (char ch in line)
                        balance += ch == '(' ? 1 : ch == ')' ? -1 : 0;
                    sb.Append(line);
                    if (balance == 0 && sb.Length > 0)
                        break;
                }
                string soldierBlock = sb.ToString();
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Assembled soldier block.");

                // Fix: Updated regex to handle soldier task id as -1 and a decimal (double) angle.
                var soldierMatch = Regex.Match(soldierBlock,
                    @"Task_New\(\s*(?<SoldierId>-?\d+)\s*,\s*""HumanSoldier""\s*,\s*""[^""]*""\s*,\s*(?<PosX>-?\d+(?:\.\d+)?)\s*,\s*(?<PosY>-?\d+(?:\.\d+)?)\s*,\s*(?<PosZ>-?\d+(?:\.\d+)?)\s*,\s*(?<Angle>-?\d+(?:\.\d+)?)\s*,\s*""(?<ModelId>[^""]+)""\s*,\s*(?<TeamId>-?\d+)\s*,\s*(?<BoneHierarchy>-?\d+)\s*,\s*(?<StandAnimation>-?\d+)");
                if (!soldierMatch.Success)
                {
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Failed to parse soldier block.");
                    return null;
                }

                int soldierId = int.Parse(soldierMatch.Groups["SoldierId"].Value);
                var pos = new Real64(
                    double.Parse(soldierMatch.Groups["PosX"].Value, CultureInfo.InvariantCulture),
                    double.Parse(soldierMatch.Groups["PosY"].Value, CultureInfo.InvariantCulture),
                    double.Parse(soldierMatch.Groups["PosZ"].Value, CultureInfo.InvariantCulture));

                float angle = float.Parse(soldierMatch.Groups["Angle"].Value, NumberStyles.Float, CultureInfo.InvariantCulture);
                string modelId = soldierMatch.Groups["ModelId"].Value;
                int teamId = int.Parse(soldierMatch.Groups["TeamId"].Value);
                int boneHierarchy = int.Parse(soldierMatch.Groups["BoneHierarchy"].Value);
                int standAnimation = int.Parse(soldierMatch.Groups["StandAnimation"].Value);

                var humanAIData = Regex.Matches(soldierBlock,
                    @"Task_New\(\s*(?<Id>-?\d+)\s*,\s*""HumanAI""\s*,\s*""[^""]*""\s*,\s*""(?<AIType>[^""]+)""\s*,\s*(?<GraphId>-?\d+)\s*\)")
                    .Cast<Match>()
                    .Select(m => new HumanAI(
                        int.Parse(m.Groups["Id"].Value),
                        m.Groups["AIType"].Value,
                        int.Parse(m.Groups["GraphId"].Value)))
                    .FirstOrDefault(ai => ai.Id == targetHumanAIId);

                if (humanAIData == null)
                {
                    QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"HumanAI data for Id {targetHumanAIId} not found.");
                    return null;
                }

                QLog.AddLog(MethodBase.GetCurrentMethod().Name,
                    $"Parsed SoldierId {soldierId}: Pos({pos.x},{pos.y},{pos.z}), Angle {angle}, ModelId {modelId}, TeamId {teamId}, " +
                    $"BoneHierarchy {boneHierarchy}, StandAnimation {standAnimation}, HumanAI Id {humanAIData.Id}, AIType {humanAIData.AIType}, GraphId {humanAIData.GraphId}");
                return new HumanSoldier(soldierId, pos, angle, modelId, teamId, boneHierarchy, standAnimation, humanAIData);
            }
            catch (Exception ex)
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Exception: {ex.Message}", DEBUG_TYPE.Error);
                return null;
            }
        }

        public static void WriteHumanSoldierBySoldierId(string fileName = "objects.qvm", HumanSoldier soldier = null)
        {
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Starting write by SoldierId: {soldier?.SoldierId}");
            if (soldier == null)
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Provided soldier is null");
                return;
            }
            string soldierRecord = $"Task_New({soldier.SoldierId}, \"HumanSoldier\", \"\", {soldier.Position.x.ToString(CultureInfo.InvariantCulture)}, {soldier.Position.y.ToString(CultureInfo.InvariantCulture)}, {soldier.Position.z.ToString(CultureInfo.InvariantCulture)}, {soldier.Angle}, \"{soldier.ModelId}\", {soldier.TeamId}, {soldier.BoneHierarchy}, {soldier.StandAnimation}";
            if (soldier.HumanAIData != null)
            {
                soldierRecord += $", Task_New({soldier.HumanAIData.Id}, \"HumanAI\", \"\", \"{soldier.HumanAIData.AIType}\", {soldier.HumanAIData.GraphId})";
            }
            soldierRecord += ")";
            QUtils.SaveFile(soldierRecord, true);
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Written HumanSoldier record: {soldierRecord}");
        }

        public static void WriteHumanSoldierByHumanAIId(string fileName = "objects.qvm", HumanSoldier soldier = null)
        {
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Starting write by Id: {soldier?.HumanAIData?.Id}");
            if (soldier == null)
            {
                QLog.AddLog(MethodBase.GetCurrentMethod().Name, "Provided soldier is null");
                return;
            }
            string soldierRecord = $"Task_New({soldier.SoldierId}, \"HumanSoldier\", \"\", {soldier.Position.x.ToString(CultureInfo.InvariantCulture)}, {soldier.Position.y.ToString(CultureInfo.InvariantCulture)}, {soldier.Position.z.ToString(CultureInfo.InvariantCulture)}, {soldier.Angle}, \"{soldier.ModelId}\", {soldier.TeamId}, {soldier.BoneHierarchy}, {soldier.StandAnimation}";
            if (soldier.HumanAIData != null)
            {
                soldierRecord += $", Task_New({soldier.HumanAIData.Id}, \"HumanAI\", \"\", \"{soldier.HumanAIData.AIType}\", {soldier.HumanAIData.GraphId})";
            }
            soldierRecord += ")";
            QUtils.SaveFile(soldierRecord, true);
            QLog.AddLog(MethodBase.GetCurrentMethod().Name, $"Written HumanSoldier record with Id: {soldier.HumanAIData?.Id}");
        }
    }
}
