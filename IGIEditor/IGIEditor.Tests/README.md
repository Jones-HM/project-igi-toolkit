# IGIEditor Unit Tests

This project contains comprehensive unit tests for the IGIEditor application.

## Test Files

### QTriggerTests.cs
Comprehensive unit tests for the QTrigger class:
- ParseTriggerTasks with various input scenarios (valid, invalid, empty, null)
- Trigger ID validation (skipping -1 IDs)
- ParseSingleTask for different trigger types (StatusMessage, ConditionalSound, ConditionalContainer, AlarmControl)
- GetIndividualTriggers with various conditions (single, multiple with OR/AND, mixed operators)
- UpdateTaskCondition for enabling/disabling triggers
- AvailableTriggerProperties validation

### QObjectsTests.cs
Comprehensive unit tests for the QObjects class:
- RigidObj with various parameters (default, valid, with quotes)
- AddWire with valid parameters
- ComputerMapHilight with valid parameters
- ParseObjects with various QSC data scenarios (valid, empty, invalid types)
- ParseObjects for different object types (EditRigidObj, Building, Door)
- FindModelName and FindModelId methods
- HasMultiObjects detection
- FixErrors for various token issues

### QTaskTests.cs
Comprehensive unit tests for the QTask class:
- GetUniqueQTaskId with string and int inputs
- GetTaskIdFromData with various scenarios
- GetNextTaskId with empty, single, and multiple ID lists
- Task ID generation logic validation

### QUtilsTests.cs
Comprehensive unit tests for the QUtils class:
- Real64 and Real32 struct constructors and properties
- QScriptTask class constructors and properties
- QTASKINFO enum value validation
- FileExtensions and constants validation
- Sleep method with various delays
- Slice string extension method
- HasBinaryContent detection
- GetTaskIdFromLine parsing

### QCompilerTests.cs
Comprehensive unit tests for the QCompiler class:
- CompileEx with various QSC data scenarios (valid, empty, null, invalid)
- Compile with backup options
- CompileEx with multiple tasks, syntax errors, comments, whitespace
- CompileEx with special characters, negative coordinates, floating point orientation

## Running the Tests

### Prerequisites
- .NET Framework 4.8
- .NET SDK 9.0 or later
- MSTest framework

### Build and Run
```bash
cd IGIEditor.Tests
dotnet restore
dotnet build
dotnet test
```

## Notes

Some tests are marked as requiring actual file I/O or game state and are currently commented out. These tests would need:
- Actual QSC file access
- Game state simulation
- Compiler setup
- IGIModels.json file access

The test project includes a `nuget.config` file to handle NuGet package resolution issues.

## Test Coverage

The test suite covers:
- ✅ Trigger parsing and validation
- ✅ Object parsing and manipulation
- ✅ Task ID generation and management
- ✅ Utility functions and helpers
- ✅ Compiler operations (where possible without external dependencies)
- ✅ Edge cases and error handling
- ✅ Input validation and sanitization

## Future Enhancements

To enable the currently commented-out tests:
1. Set up test data files (sample QSC files, IGIModels.json)
2. Create mock objects for game state
3. Set up test compiler environment
4. Add integration tests for end-to-end scenarios
