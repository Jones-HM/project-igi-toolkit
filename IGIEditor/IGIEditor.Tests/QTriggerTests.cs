using Microsoft.VisualStudio.TestTools.UnitTesting;
using IGIEditor;
using System.Collections.Generic;
using System.Linq;

namespace IGIEditor.Tests
{
    [TestClass]
    public class QTriggerTests
    {
        [TestMethod]
        public void ParseTriggerTasks_ValidInput_ReturnsCorrectCount()
        {
            // Arrange
            string qscData = @"Task_New(90, ""StatusMessage"", ""Test"", 24893338.0, -56208296.0, 174457920.0, 0, 0, 0, ""HumanPlayer_0.isDead"", ""MISSION_FAILED"", """", ""fail"", TRUE, FALSE, 2.0)
Task_New(91, ""ConditionalSound"", ""Test2"", ""AlarmControl_70.isAlarm"", ""alarmsystem_working"", 23968154.0, -55235764.0, 174448064.0, FALSE, FALSE, FALSE)";

            // Act
            var result = QTrigger.ParseTriggerTasks(qscData);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void ParseTriggerTasks_InvalidId_SkipsInvalid()
        {
            // Arrange
            string qscData = @"Task_New(-1, ""StatusMessage"", ""Test"", 24893338.0, -56208296.0, 174457920.0, 0, 0, 0, ""HumanPlayer_0.isDead"", ""MISSION_FAILED"", """", ""fail"", TRUE, FALSE, 2.0)
Task_New(90, ""StatusMessage"", ""Test2"", 24893338.0, -56208296.0, 174457920.0, 0, 0, 0, ""HumanPlayer_0.isDead"", ""MISSION_FAILED"", """", ""fail"", TRUE, FALSE, 2.0)";

            // Act
            var result = QTrigger.ParseTriggerTasks(qscData);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(90, result[0].Id);
        }

        [TestMethod]
        public void ParseTriggerTasks_EmptyInput_ReturnsEmptyList()
        {
            // Arrange
            string qscData = "";

            // Act
            var result = QTrigger.ParseTriggerTasks(qscData);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParseTriggerTasks_NullInput_ReturnsEmptyList()
        {
            // Arrange
            string qscData = null;

            // Act
            var result = QTrigger.ParseTriggerTasks(qscData);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParseTriggerTasks_ValidStatusMessage_ParsesCorrectly()
        {
            // Arrange
            string qscData = @"Task_New(90, ""StatusMessage"", ""Test"", 24893338.0, -56208296.0, 174457920.0, 0, 0, 0, ""HumanPlayer_0.isDead"", ""MISSION_FAILED"", """", ""fail"", TRUE, FALSE, 2.0)";

            // Act
            var result = QTrigger.ParseTriggerTasks(qscData);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(90, result[0].Id);
            Assert.AreEqual("StatusMessage", result[0].Type);
            Assert.AreEqual("Test", result[0].Note);
            Assert.IsTrue(result[0].FullLine.Contains("StatusMessage"));
        }

        [TestMethod]
        public void ParseTriggerTasks_ValidConditionalSound_ParsesCorrectly()
        {
            // Arrange
            string qscData = @"Task_New(90, ""ConditionalSound"", ""Test"", ""AlarmControl_70.isAlarm"", ""alarmsystem_working"", 23968154.0, -55235764.0, 174448064.0, FALSE, FALSE, FALSE)";

            // Act
            var result = QTrigger.ParseTriggerTasks(qscData);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(90, result[0].Id);
            Assert.AreEqual("ConditionalSound", result[0].Type);
            Assert.AreEqual(1, result[0].Conditions.Count);
            Assert.AreEqual("Condition", result[0].Conditions[0].Name);
            Assert.AreEqual("AlarmControl_70.isAlarm", result[0].Conditions[0].Value);
        }

        [TestMethod]
        public void ParseTriggerTasks_ValidConditionalContainer_ParsesCorrectly()
        {
            // Arrange
            string qscData = @"Task_New(90, ""ConditionalContainer"", ""Test"", ""Door_402.isClosed"", ""container_action"", 23968154.0, -55235764.0, 174448064.0)";

            // Act
            var result = QTrigger.ParseTriggerTasks(qscData);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("ConditionalContainer", result[0].Type);
            Assert.AreEqual(1, result[0].Conditions.Count);
            Assert.AreEqual("Condition", result[0].Conditions[0].Name);
        }

        [TestMethod]
        public void ParseTriggerTasks_ValidAlarmControl_ParsesCorrectly()
        {
            // Arrange
            string qscData = @"Task_New(90, ""AlarmControl"", ""Test"", 23968154.0, -55235764.0, 174448064.0, 0, 0, 0, ""model"", 1, 1, 1, 0, 0, 0, ""trigger_param"", ""alarm_param"")";

            // Act
            var result = QTrigger.ParseTriggerTasks(qscData);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("AlarmControl", result[0].Type);
            Assert.AreEqual(2, result[0].Conditions.Count);
        }

        [TestMethod]
        public void GetIndividualTriggers_SingleCondition_ReturnsSingle()
        {
            // Arrange
            string condition = "HumanPlayer_0.isDead";

            // Act
            var result = QTrigger.GetIndividualTriggers(condition);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("HumanPlayer_0.isDead", result[0]);
        }

        [TestMethod]
        public void GetIndividualTriggers_MultipleConditionsWithOr_ReturnsMultiple()
        {
            // Arrange
            string condition = "HumanPlayer_0.isDead || Car_1099.isExploded";

            // Act
            var result = QTrigger.GetIndividualTriggers(condition);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("HumanPlayer_0.isDead"));
            Assert.IsTrue(result.Contains("Car_1099.isExploded"));
        }

        [TestMethod]
        public void GetIndividualTriggers_MultipleConditionsWithAnd_ReturnsMultiple()
        {
            // Arrange
            string condition = "Door_402.isClosed && Switch_697.isPressed";

            // Act
            var result = QTrigger.GetIndividualTriggers(condition);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("Door_402.isClosed"));
            Assert.IsTrue(result.Contains("Switch_697.isPressed"));
        }

        [TestMethod]
        public void GetIndividualTriggers_EmptyString_ReturnsEmpty()
        {
            // Arrange
            string condition = "";

            // Act
            var result = QTrigger.GetIndividualTriggers(condition);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetIndividualTriggers_WhitespaceOnly_ReturnsEmpty()
        {
            // Arrange
            string condition = "   ";

            // Act
            var result = QTrigger.GetIndividualTriggers(condition);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetIndividualTriggers_ValueOne_ReturnsEmpty()
        {
            // Arrange
            string condition = "1";

            // Act
            var result = QTrigger.GetIndividualTriggers(condition);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetIndividualTriggers_ValueZero_ReturnsEmpty()
        {
            // Arrange
            string condition = "0";

            // Act
            var result = QTrigger.GetIndividualTriggers(condition);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetIndividualTriggers_MixedOperators_ReturnsCorrectCount()
        {
            // Arrange
            string condition = "HumanPlayer_0.isDead || Car_1099.isExploded && Door_402.isClosed";

            // Act
            var result = QTrigger.GetIndividualTriggers(condition);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void UpdateTaskCondition_ValidCondition_UpdatesCorrectly()
        {
            // Arrange
            string qscData = @"Task_New(90, ""ConditionalSound"", ""Test"", ""AlarmControl_70.isAlarm"", ""alarmsystem_working"", 23968154.0, -55235764.0, 174448064.0, FALSE, FALSE, FALSE)";
            var triggerTasks = QTrigger.ParseTriggerTasks(qscData);
            var task = triggerTasks[0];

            // Act
            string result = QTrigger.UpdateTaskCondition(qscData, task, "Condition", "Door_402.isClosed");

            // Assert
            Assert.IsTrue(result.Contains("Door_402.isClosed"));
            Assert.IsFalse(result.Contains("AlarmControl_70.isAlarm"));
        }

        [TestMethod]
        public void UpdateTaskCondition_EmptyCondition_ClearsCondition()
        {
            // Arrange
            string qscData = @"Task_New(90, ""ConditionalSound"", ""Test"", ""AlarmControl_70.isAlarm"", ""alarmsystem_working"", 23968154.0, -55235764.0, 174448064.0, FALSE, FALSE, FALSE)";
            var triggerTasks = QTrigger.ParseTriggerTasks(qscData);
            var task = triggerTasks[0];

            // Act
            string result = QTrigger.UpdateTaskCondition(qscData, task, "Condition", "");

            // Assert
            Assert.IsTrue(result.Contains("\"\""));
            Assert.IsFalse(result.Contains("AlarmControl_70.isAlarm"));
        }

        [TestMethod]
        public void AvailableTriggerProperties_ContainsExpectedProperties()
        {
            // Act
            var properties = QTrigger.AvailableTriggerProperties;

            // Assert
            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Length > 0);
            CollectionAssert.Contains(properties, "isDead");
            CollectionAssert.Contains(properties, "isExploded");
            CollectionAssert.Contains(properties, "isClosed");
            CollectionAssert.Contains(properties, "isOpen");
        }
    }
}
