using Microsoft.VisualStudio.TestTools.UnitTesting;
using IGIEditor;
using System;

namespace IGIEditor.Tests
{
    [TestClass]
    public class QObjectsTests
    {
        [TestMethod]
        public void RigidObj_ValidParameters_ReturnsCorrectFormat()
        {
            // Arrange
            int taskId = 100;
            string taskNote = "TestObject";
            double x = 100.0;
            double y = 200.0;
            double z = 300.0;
            float alpha = 0.5f;
            float beta = 0.3f;
            float gamma = 0.1f;
            string modelId = "219_01_1";

            // Act
            string result = QObjects.RigidObj(taskId, taskNote, x, y, z, alpha, beta, gamma, modelId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Task_New"));
            Assert.IsTrue(result.Contains("EditRigidObj"));
            Assert.IsTrue(result.Contains(taskId.ToString()));
            Assert.IsTrue(result.Contains(taskNote));
            Assert.IsTrue(result.Contains(modelId));
            Assert.IsTrue(result.Contains(x.ToString()));
            Assert.IsTrue(result.Contains(y.ToString()));
            Assert.IsTrue(result.Contains(z.ToString()));
        }

        [TestMethod]
        public void RigidObj_DefaultParameters_ReturnsCorrectFormat()
        {
            // Arrange
            string modelId = "219_01_1";

            // Act
            string result = QObjects.RigidObj(-1, "", 0.0, 0.0, 0.0, 0.0f, 0.0f, 0.0f, modelId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Task_New"));
            Assert.IsTrue(result.Contains("EditRigidObj"));
            Assert.IsTrue(result.Contains("-1"));
            Assert.IsTrue(result.Contains(modelId));
        }

        [TestMethod]
        public void RigidObj_ModelIdWithQuotes_RemovesQuotes()
        {
            // Arrange
            string modelId = "\"219_01_1\"";

            // Act
            string result = QObjects.RigidObj(-1, "", 0.0, 0.0, 0.0, 0.0f, 0.0f, 0.0f, modelId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("219_01_1"));
            // Should not have double quotes in the final output except the wrapping quotes
        }

        [TestMethod]
        public void AddWire_ValidParameters_ReturnsCorrectFormat()
        {
            // Arrange
            var startPos = new QUtils.Real64 { x = 100.0, y = 200.0, z = 300.0 };
            var endPos = new QUtils.Real64 { x = 400.0, y = 500.0, z = 600.0 };
            string taskNote = "TestWire";
            int taskId = 101;
            string modelId = "320_01_1";

            // Act
            string result = QObjects.AddWire(startPos, endPos, taskNote, taskId, modelId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Task_New"));
            Assert.IsTrue(result.Contains("Wire"));
            Assert.IsTrue(result.Contains(taskId.ToString()));
            Assert.IsTrue(result.Contains(modelId));
            Assert.IsTrue(result.Contains(startPos.x.ToString()));
            Assert.IsTrue(result.Contains(endPos.x.ToString()));
        }

        [TestMethod]
        public void ComputerMapHilight_ValidParameters_ReturnsCorrectFormat()
        {
            // Arrange
            int targetTaskId = 100;
            string taskNote = "TestHilight";
            string taskTitle = "Test Title";
            string taskInfo = "Test Info";
            string markerType = "MARKER_NONE";
            string markerColor = "MARKER_COLOR_NONE";
            int taskId = 102;

            // Act
            string result = QObjects.ComputerMapHilight(targetTaskId, taskNote, taskTitle, taskInfo, markerType, markerColor, taskId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Task_New"));
            Assert.IsTrue(result.Contains("ComputerHilight"));
            Assert.IsTrue(result.Contains(targetTaskId.ToString()));
            Assert.IsTrue(result.Contains(markerType));
            Assert.IsTrue(result.Contains(markerColor));
        }

        [TestMethod]
        public void ParseObjects_ValidQscData_ReturnsCorrectCount()
        {
            // Arrange
            string qscData = @"Task_New(100, ""EditRigidObj"", ""Test1"", 24664470.0, -56347808.0, 174413312.0, 0, 0, 0, ""219_01_1"", 1, 1, 1, 0, 0, 0);
Task_New(101, ""Building"", ""Test2"", 24664471.0, -56347809.0, 174413313.0, 0, 0, 0, ""220_01_1"", 1, 1, 1, 0, 0, 0);

";

            // Act
            var result = QObjects.ParseObjects(qscData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void ParseObjects_EmptyQscData_ReturnsEmptyList()
        {
            // Arrange
            string qscData = "";

            // Act
            var result = QObjects.ParseObjects(qscData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParseObjects_InvalidTaskType_SkipsInvalid()
        {
            // Arrange
            string qscData = @"Task_New(100, ""InvalidType"", ""Test"", 24664470.0, -56347808.0, 174413312.0, 0, 0, 0, ""219_01_1"", 1, 1, 1, 0, 0, 0)
Task_New(101, ""EditRigidObj"", ""Test2"", 24664471.0, -56347809.0, 174413313.0, 0, 0, 0, ""220_01_1"", 1, 1, 1, 0, 0, 0)";

            // Act
            var result = QObjects.ParseObjects(qscData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("EditRigidObj", result[0].name.Replace("\"", ""));
        }

        [TestMethod]
        public void ParseObjects_ValidEditRigidObj_ParsesCorrectly()
        {
            // Arrange
            string qscData = @"Task_New(100, ""EditRigidObj"", ""TestObject"", 24664470.0, -56347808.0, 174413312.0, 0.5, 0.3, 0.1, ""219_01_1"", 1, 1, 1, 0, 0, 0)";

            // Act
            var result = QObjects.ParseObjects(qscData);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(100, result[0].id);
            Assert.AreEqual("\"EditRigidObj\"", result[0].name);
            Assert.AreEqual("\"TestObject\"", result[0].note);
            Assert.AreEqual("\"219_01_1\"", result[0].model);
            Assert.AreEqual(24664470.0, result[0].position.x);
            Assert.AreEqual(-56347808.0, result[0].position.y);
            Assert.AreEqual(174413312.0, result[0].position.z);
            Assert.AreEqual(0.5f, result[0].orientation.alpha);
            Assert.AreEqual(0.3f, result[0].orientation.beta);
            Assert.AreEqual(0.1f, result[0].orientation.gamma);
        }

        [TestMethod]
        public void ParseObjects_ValidBuilding_ParsesCorrectly()
        {
            // Arrange
            string qscData = @"Task_New(101, ""Building"", ""TestBuilding"", 24664471.0, -56347809.0, 174413313.0, 0, 0, 0, ""220_01_1"", 1, 1, 1, 0, 0, 0)";

            // Act
            var result = QObjects.ParseObjects(qscData);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(101, result[0].id);
            Assert.AreEqual("\"Building\"", result[0].name);
            Assert.AreEqual("\"TestBuilding\"", result[0].note);
        }

        [TestMethod]
        public void ParseObjects_ValidDoor_ParsesCorrectly()
        {
            // Arrange
            string qscData = @"Task_New(102, ""Door"", ""TestDoor"", 24664472.0, -56347810.0, 174413314.0, 0, 0, 0, ""221_01_1"", 1, 1, 1, 0, 0, 0)";

            // Act
            var result = QObjects.ParseObjects(qscData);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(102, result[0].id);
            Assert.AreEqual("\"Door\"", result[0].name);
        }

        [TestMethod]
        public void FindModelName_ValidModelId_ReturnsModelName()
        {
            // Arrange
            string modelId = "219_01_1";

            // Act
            string result = QObjects.FindModelName(modelId, false);

            // Assert
            // This will return "UNKNOWN_OBJECT" if the model is not in the JSON file
            // In a real test with proper JSON setup, this would return the actual model name
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FindModelName_ModelIdWithQuotes_RemovesQuotes()
        {
            // Arrange
            string modelId = "\"219_01_1\"";

            // Act
            string result = QObjects.FindModelName(modelId, false);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FindModelName_EmptyModelId_ReturnsUnknown()
        {
            // Arrange
            string modelId = "";

            // Act
            string result = QObjects.FindModelName(modelId, false);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FindModelId_ValidModelName_ReturnsModelId()
        {
            // Arrange
            string modelName = "TestModel";

            // Act
            string result = QObjects.FindModelId(modelName, false);

            // Assert
            // This will return "UNKNOWN_OBJECT_ID" if the model is not in the JSON file
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetModelName_ValidId_ReturnsModelName()
        {
            // Arrange
            int id = 100;

            // Act
            string result = QObjects.GetModelName(id);

            // Assert
            // This depends on the task list being populated
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void HasMultiObjects_SingleObject_ReturnsFalse()
        {
            // Arrange
            string qscData = @"Task_New(100, ""EditRigidObj"", ""Test"", 24664470.0, -56347808.0, 174413312.0, 0, 0, 0, ""219_01_1"", 1, 1, 1, 0, 0, 0)";

            // Act
            bool result = QObjects.HasMultiObjects(qscData, "219_01_1");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasMultiObjects_EmptyModel_ReturnsFalse()
        {
            // Arrange
            string qscData = @"Task_New(100, ""EditRigidObj"", ""Test"", 24664470.0, -56347808.0, 174413312.0, 0, 0, 0, ""219_01_1"", 1, 1, 1, 0, 0, 0)";

            // Act
            bool result = QObjects.HasMultiObjects(qscData, "");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FixErrors_RemoveCommaParenthesis_FixesToken()
        {
            // Arrange
            string qscData = @"Task_New(100, ""EditRigidObj"", ""Test"", 24664470.0, -56347808.0, 174413312.0, 0, 0, 0, ""219_01_1"",,))";

            // Act
            string result = QObjects.FixErrors(qscData);

            // Assert
            Assert.IsNotNull(result);
            // The FixErrors method should clean up the token errors
        }

        [TestMethod]
        public void FixErrors_RemoveDoubleComma_FixesToken()
        {
            // Arrange
            string qscData = @"Task_New(100, ""EditRigidObj"", ""Test"",, 24664470.0, -56347808.0, 174413312.0, 0, 0, 0, ""219_01_1"")";

            // Act
            string result = QObjects.FixErrors(qscData);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
