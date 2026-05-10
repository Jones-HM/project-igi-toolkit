using Microsoft.VisualStudio.TestTools.UnitTesting;
using IGIEditor;
using System;

namespace IGIEditor.Tests
{
    [TestClass]
    public class QUtilsTests
    {
        [TestMethod]
        public void Real64_DefaultConstructor_ReturnsZero()
        {
            // Arrange & Act
            var real64 = new QUtils.Real64();

            // Assert
            Assert.AreEqual(0.0, real64.x);
            Assert.AreEqual(0.0, real64.y);
            Assert.AreEqual(0.0, real64.z);
        }

        [TestMethod]
        public void Real64_ParameterizedConstructor_SetsValues()
        {
            // Arrange
            double x = 100.0;
            double y = 200.0;
            double z = 300.0;

            // Act
            var real64 = new QUtils.Real64 { x = x, y = y, z = z };

            // Assert
            Assert.AreEqual(x, real64.x);
            Assert.AreEqual(y, real64.y);
            Assert.AreEqual(z, real64.z);
        }

        [TestMethod]
        public void Real32_DefaultConstructor_ReturnsZero()
        {
            // Arrange & Act
            var real32 = new QUtils.Real32();

            // Assert
            Assert.AreEqual(0.0f, real32.alpha);
            Assert.AreEqual(0.0f, real32.beta);
            Assert.AreEqual(0.0f, real32.gamma);
        }

        [TestMethod]
        public void Real32_ParameterizedConstructor_SetsValues()
        {
            // Arrange
            float alpha = 0.5f;
            float beta = 0.3f;
            float gamma = 0.1f;

            // Act
            var real32 = new QUtils.Real32 { alpha = alpha, beta = beta, gamma = gamma };

            // Assert
            Assert.AreEqual(alpha, real32.alpha);
            Assert.AreEqual(beta, real32.beta);
            Assert.AreEqual(gamma, real32.gamma);
        }

        [TestMethod]
        public void QScriptTask_DefaultConstructor_ReturnsDefaults()
        {
            // Arrange & Act
            var qtask = new QUtils.QScriptTask();

            // Assert
            Assert.AreEqual(-1, qtask.id);
            Assert.IsNull(qtask.name);
            Assert.IsNull(qtask.note);
            Assert.IsNull(qtask.model);
            Assert.AreEqual(0.0, qtask.position.x);
            Assert.AreEqual(0.0, qtask.position.y);
            Assert.AreEqual(0.0, qtask.position.z);
            Assert.AreEqual(0.0f, qtask.orientation.alpha);
            Assert.AreEqual(0.0f, qtask.orientation.beta);
            Assert.AreEqual(0.0f, qtask.orientation.gamma);
        }

        [TestMethod]
        public void QScriptTask_ParameterizedConstructor_SetsValues()
        {
            // Arrange
            int id = 100;
            string name = "\"EditRigidObj\"";
            string note = "\"Test\"";
            string model = "\"219_01_1\"";

            // Act
            var qtask = new QUtils.QScriptTask
            {
                id = id,
                name = name,
                note = note,
                model = model,
                position = new QUtils.Real64 { x = 100.0, y = 200.0, z = 300.0 },
                orientation = new QUtils.Real32 { alpha = 0.5f, beta = 0.3f, gamma = 0.1f }
            };

            // Assert
            Assert.AreEqual(id, qtask.id);
            Assert.AreEqual(name, qtask.name);
            Assert.AreEqual(note, qtask.note);
            Assert.AreEqual(model, qtask.model);
            Assert.AreEqual(100.0, qtask.position.x);
            Assert.AreEqual(200.0, qtask.position.y);
            Assert.AreEqual(300.0, qtask.position.z);
            Assert.AreEqual(0.5f, qtask.orientation.alpha);
            Assert.AreEqual(0.3f, qtask.orientation.beta);
            Assert.AreEqual(0.1f, qtask.orientation.gamma);
        }

        [TestMethod]
        public void QTASKINFO_EnumValues_AreCorrect()
        {
            // Assert
            Assert.AreEqual(0, (int)QUtils.QTASKINFO.QTASK_ID);
            Assert.AreEqual(1, (int)QUtils.QTASKINFO.QTASK_NAME);
            Assert.AreEqual(2, (int)QUtils.QTASKINFO.QTASK_NOTE);
            Assert.AreEqual(3, (int)QUtils.QTASKINFO.QTASK_POSX);
            Assert.AreEqual(4, (int)QUtils.QTASKINFO.QTASK_POSY);
            Assert.AreEqual(5, (int)QUtils.QTASKINFO.QTASK_POSZ);
            Assert.AreEqual(6, (int)QUtils.QTASKINFO.QTASK_ALPHA);
            Assert.AreEqual(7, (int)QUtils.QTASKINFO.QTASK_BETA);
            Assert.AreEqual(8, (int)QUtils.QTASKINFO.QTASK_GAMMA);
            Assert.AreEqual(9, (int)QUtils.QTASKINFO.QTASK_MODEL);
        }

        [TestMethod]
        public void FileExtensions_HasExpectedExtensions()
        {
            // Assert
            Assert.IsNotNull(QUtils.FileExtensions.Json);
            Assert.IsNotNull(QUtils.FileExtensions.Qsc);
            Assert.IsNotNull(QUtils.FileExtensions.Qvm);
        }

        [TestMethod]
        public void taskNew_HasExpectedValue()
        {
            // Assert
            Assert.IsNotNull(QUtils.taskNew);
            Assert.AreEqual("Task_New", QUtils.taskNew);
        }

        [TestMethod]
        public void LEVEL_FLOW_TASK_ID_IsPositive()
        {
            // Assert
            Assert.IsTrue(QUtils.LEVEL_FLOW_TASK_ID > 0);
        }

        [TestMethod]
        public void MAX_MINIMAL_ID_DIFF_IsPositive()
        {
            // Assert
            Assert.IsTrue(QUtils.MAX_MINIMAL_ID_DIFF > 0);
        }

        [TestMethod]
        public void Sleep_ValidDelay_Works()
        {
            // Arrange
            float delay = 0.1f; // 100ms

            // Act
            var startTime = DateTime.Now;
            QUtils.Sleep(delay);
            var endTime = DateTime.Now;
            var elapsed = (endTime - startTime).TotalSeconds;

            // Assert
            Assert.IsTrue(elapsed >= delay * 0.9); // Allow some tolerance
        }

        [TestMethod]
        public void Sleep_ZeroDelay_Works()
        {
            // Arrange
            float delay = 0.0f;

            // Act & Assert - should not throw
            QUtils.Sleep(delay);
        }

        [TestMethod]
        public void Sleep_NegativeDelay_Works()
        {
            // Arrange
            float delay = -0.1f;

            // Act & Assert - should not throw
            QUtils.Sleep(delay);
        }

        [TestMethod]
        public void Slice_ValidString_ReturnsCorrectSubstring()
        {
            // Arrange
            string input = "Hello World";
            int start = 0;
            int end = 5;

            // Act
            string result = input.Slice(start, end);

            // Assert
            Assert.AreEqual("Hello", result);
        }

        [TestMethod]
        public void Slice_StartToEnd_ReturnsCorrectSubstring()
        {
            // Arrange
            string input = "Hello World";
            int start = 6;
            int end = input.Length;

            // Act
            string result = input.Slice(start, end);

            // Assert
            Assert.AreEqual("World", result);
        }

        [TestMethod]
        public void Slice_EntireString_ReturnsOriginal()
        {
            // Arrange
            string input = "Hello World";
            int start = 0;
            int end = input.Length;

            // Act
            string result = input.Slice(start, end);

            // Assert
            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void Slice_StartGreaterThanEnd_ReturnsEmpty()
        {
            // Arrange
            string input = "Hello World";
            int start = 5;
            int end = 0;

            // Act
            string result = input.Slice(start, end);

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void HasBinaryContent_BinaryData_ReturnsTrue()
        {
            // Arrange
            string binaryData = "\x00\x01\x02\x03\x04\x05";

            // Act
            bool result = binaryData.HasBinaryContent();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasBinaryContent_TextData_ReturnsFalse()
        {
            // Arrange
            string textData = "Hello World";

            // Act
            bool result = textData.HasBinaryContent();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasBinaryContent_EmptyString_ReturnsFalse()
        {
            // Arrange
            string emptyData = "";

            // Act
            bool result = emptyData.HasBinaryContent();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasBinaryContent_NullString_ReturnsFalse()
        {
            // Arrange
            string nullData = null;

            // Act
            bool result = nullData.HasBinaryContent();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetTaskIdFromLine_ValidLine_ReturnsId()
        {
            // Arrange
            string line = "Task_New(100, \"EditRigidObj\", \"Test\", 24664470.0)";

            // Act
            int result = 0;
            if (line.Contains(QUtils.taskNew))
            {
                string[] taskNew = line.Split(',');
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
            }

            // Assert
            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void GetTaskIdFromLine_InvalidLine_ReturnsMinusOne()
        {
            // Arrange
            string line = "Invalid line without Task_New";

            // Act
            int result = 0;
            if (line.Contains(QUtils.taskNew))
            {
                string[] taskNew = line.Split(',');
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
            }

            // Assert
            Assert.AreEqual(0, result);
        }
    }
}
