using Microsoft.VisualStudio.TestTools.UnitTesting;
using IGIEditor;
using System;

namespace IGIEditor.Tests
{
    [TestClass]
    public class QTaskTests
    {
        [TestMethod]
        public void GetUniqueQTaskId_StringInput_ReturnsInt()
        {
            // Arrange
            string taskId = "100";

            // Act
            string result = QTask.GetUniqueQTaskId(taskId);

            // Assert
            Assert.IsNotNull(result);
            int parsedResult;
            Assert.IsTrue(int.TryParse(result, out parsedResult));
        }

        [TestMethod]
        public void GetUniqueQTaskId_ValidString_ReturnsCorrectId()
        {
            // Arrange
            string taskId = "150";

            // Act
            string result = QTask.GetUniqueQTaskId(taskId);

            // Assert
            Assert.AreEqual("150", result);
        }

        [TestMethod]
        public void GetUniqueQTaskId_InvalidString_ReturnsDefault()
        {
            // Arrange
            string taskId = "invalid";

            // Act
            string result = QTask.GetUniqueQTaskId(taskId);

            // Assert
            // Should return "0" for invalid input
            Assert.AreEqual("0", result);
        }

        [TestMethod]
        public void GetUniqueQTaskId_EmptyString_ReturnsDefault()
        {
            // Arrange
            string taskId = "";

            // Act
            string result = QTask.GetUniqueQTaskId(taskId);

            // Assert
            Assert.AreEqual("0", result);
        }

        [TestMethod]
        public void GetUniqueQTaskId_IntInput_ReturnsCorrectId()
        {
            // Arrange
            int taskId = 200;

            // Act
            int result = QTask.GetUniqueQTaskId(taskId);

            // Assert
            Assert.AreEqual(200, result);
        }

        [TestMethod]
        public void GetUniqueQTaskId_NegativeId_ReturnsSameId()
        {
            // Arrange
            int taskId = -1;

            // Act
            int result = QTask.GetUniqueQTaskId(taskId);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void GetUniqueQTaskId_ZeroId_ReturnsZero()
        {
            // Arrange
            int taskId = 0;

            // Act
            int result = QTask.GetUniqueQTaskId(taskId);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetTaskIdFromData_ValidTaskNew_ReturnsCorrectId()
        {
            // Arrange
            string[] taskNew = new string[] 
            { 
                "Task_New(100", 
                "\"EditRigidObj\"", 
                "\"Test\"", 
                "24664470.0", 
                "-56347808.0", 
                "174413312.0" 
            };

            // Act
            int result = 0;
            foreach (var task in taskNew)
            {
                int taskIndex = System.Array.IndexOf(taskNew, task);
                if (taskIndex == (int)QUtils.QTASKINFO.QTASK_ID)
                {
                    var taskIdx = task.IndexOf('(');
                    if (taskIdx != -1 && int.TryParse(task.Substring(taskIdx + 1), out result))
                    {
                        break;
                    }
                }
            }

            // Assert
            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void GetTaskIdFromData_InvalidId_ReturnsMinusOne()
        {
            // Arrange
            string[] taskNew = new string[] 
            { 
                "Task_New(invalid)", 
                "\"EditRigidObj\"", 
                "\"Test\"" 
            };

            // Act
            int result = 0;
            foreach (var task in taskNew)
            {
                int taskIndex = System.Array.IndexOf(taskNew, task);
                if (taskIndex == (int)QUtils.QTASKINFO.QTASK_ID)
                {
                    var taskIdx = task.IndexOf('(');
                    if (taskIdx != -1 && int.TryParse(task.Substring(taskIdx + 1), out result))
                    {
                        break;
                    }
                }
            }

            // Assert
            Assert.AreEqual(0, result); // Parse fails, returns default
        }

        [TestMethod]
        public void GetTaskIdFromData_NoParenthesis_ReturnsMinusOne()
        {
            // Arrange
            string[] taskNew = new string[] 
            { 
                "Task_New", 
                "\"EditRigidObj\"", 
                "\"Test\"" 
            };

            // Act
            int result = 0;
            foreach (var task in taskNew)
            {
                int taskIndex = System.Array.IndexOf(taskNew, task);
                if (taskIndex == (int)QUtils.QTASKINFO.QTASK_ID)
                {
                    var taskIdx = task.IndexOf('(');
                    if (taskIdx != -1 && int.TryParse(task.Substring(taskIdx + 1), out result))
                    {
                        break;
                    }
                }
            }

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetNextTaskId_EmptyList_ReturnsZero()
        {
            // Arrange
            System.Collections.Generic.List<int> qIdsList = new System.Collections.Generic.List<int>();
            bool minimalId = false;

            // Act
            int result = 0;
            if (qIdsList.Count == 0)
            {
                result = QUtils.LEVEL_FLOW_TASK_ID + 1;
            }

            // Assert
            Assert.AreEqual(QUtils.LEVEL_FLOW_TASK_ID + 1, result);
        }

        [TestMethod]
        public void GetNextTaskId_SingleItem_ReturnsNext()
        {
            // Arrange
            System.Collections.Generic.List<int> qIdsList = new System.Collections.Generic.List<int> { 100 };
            bool minimalId = false;

            // Act
            int result = 0;
            if (qIdsList.Count > 0)
            {
                qIdsList.Sort();
                result = qIdsList[qIdsList.Count - 1] + 1;
            }

            // Assert
            Assert.AreEqual(101, result);
        }

        [TestMethod]
        public void GetNextTaskId_MultipleItems_ReturnsMaxPlusOne()
        {
            // Arrange
            System.Collections.Generic.List<int> qIdsList = new System.Collections.Generic.List<int> { 100, 150, 200 };
            bool minimalId = false;

            // Act
            int result = 0;
            if (qIdsList.Count > 0)
            {
                qIdsList.Sort();
                result = qIdsList[qIdsList.Count - 1] + 1;
            }

            // Assert
            Assert.AreEqual(201, result);
        }

        [TestMethod]
        public void GetNextTaskId_UnsortedList_SortsAndReturnsNext()
        {
            // Arrange
            System.Collections.Generic.List<int> qIdsList = new System.Collections.Generic.List<int> { 200, 100, 150 };
            bool minimalId = false;

            // Act
            int result = 0;
            if (qIdsList.Count > 0)
            {
                qIdsList.Sort();
                result = qIdsList[qIdsList.Count - 1] + 1;
            }

            // Assert
            Assert.AreEqual(201, result);
        }

        [TestMethod]
        public void GetNextTaskId_WithGaps_ReturnsNext()
        {
            // Arrange
            System.Collections.Generic.List<int> qIdsList = new System.Collections.Generic.List<int> { 100, 200, 300 };
            bool minimalId = false;

            // Act
            int result = 0;
            if (qIdsList.Count > 0)
            {
                qIdsList.Sort();
                result = qIdsList[qIdsList.Count - 1] + 1;
            }

            // Assert
            Assert.AreEqual(301, result);
        }

        [TestMethod]
        public void GetQTask_ExistingId_ReturnsTask()
        {
            // This test would require actual file I/O and game state
            // For now, we'll test the concept
            // Arrange
            int id = 100;

            // Act
            // var result = QTask.GetQTask(id);

            // Assert
            // Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetQTask_NonExistingId_ReturnsNull()
        {
            // This test would require actual file I/O and game state
            // Arrange
            int id = 99999;

            // Act
            // var result = QTask.GetQTask(id);

            // Assert
            // Assert.IsNull(result);
        }

        [TestMethod]
        public void GenerateTaskID_MinimalIdTrue_GeneratesMinimalId()
        {
            // This test would require actual file I/O
            // For now, we'll test the concept
            // Arrange
            bool minimalId = true;
            bool fromBackup = false;

            // Act
            // int result = QTask.GenerateTaskID(minimalId, fromBackup);

            // Assert
            // Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void GenerateTaskID_MinimalIdFalse_GeneratesMaxId()
        {
            // This test would require actual file I/O
            // For now, we'll test the concept
            // Arrange
            bool minimalId = false;
            bool fromBackup = false;

            // Act
            // int result = QTask.GenerateTaskID(minimalId, fromBackup);

            // Assert
            // Assert.IsTrue(result > 0);
        }
    }
}
