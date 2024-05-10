using System;
using System.Reflection;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using NUnit.Framework;

namespace EmergenceSDK.Tests.Emergence.Logger
{
    [TestFixture]
    public class EmergenceLoggerTests
    {
        private IDisposable _verboseOutput;
        [OneTimeSetUp]
        public void Setup()
        {
            _verboseOutput = EmergenceLogger.VerboseOutput(true);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _verboseOutput?.Dispose();
        }

        [Test]
        public void VerboseLogging_TogglesSuccessfully()
        {
            var propertyInfo = typeof(EmergenceLogger).GetProperty("VerboseMode", BindingFlags.NonPublic | BindingFlags.Static);
            var getCurrentValue = new Func<bool>(() => (bool)propertyInfo.GetValue(null));
            using var verboseOutput = EmergenceLogger.VerboseOutput(false);
            
            Assert.AreEqual(false, getCurrentValue(), "Verbose logging did not set");
        }

        [Test] public void VerboseMarker_TogglesSuccessfully()
        {
            var propertyInfo = typeof(EmergenceLogger).GetProperty("MarkLogsAsVerbose", BindingFlags.NonPublic | BindingFlags.Static);
            var getCurrentValue = new Func<bool>(() => (bool)propertyInfo.GetValue(null));
            using var verboseMarker = EmergenceLogger.VerboseMarker(true);
            
            Assert.AreEqual(true, getCurrentValue(), "Verbose marker did not set");
        }

        [Test]
        public void VerboseLogging_ResetsSuccessfully()
        {
            var propertyInfo = typeof(EmergenceLogger).GetProperty("VerboseMode", BindingFlags.NonPublic | BindingFlags.Static);
            var getCurrentValue = new Func<bool>(() => (bool)propertyInfo.GetValue(null));
            using (EmergenceLogger.VerboseOutput(false))
            {
                Assert.AreEqual(false, getCurrentValue(), "Verbose logging did not set");
            }
            Assert.AreEqual(true, getCurrentValue(), "Verbose logging did not reset");
        }

        [Test] public void VerboseMarker_ResetsSuccessfully()
        {
            var propertyInfo = typeof(EmergenceLogger).GetProperty("MarkLogsAsVerbose", BindingFlags.NonPublic | BindingFlags.Static);
            var getCurrentValue = new Func<bool>(() => (bool)propertyInfo.GetValue(null));
            using (EmergenceLogger.VerboseMarker(true))
            {
                Assert.AreEqual(true, getCurrentValue(), "Verbose marker did not set");
            }
            Assert.AreEqual(false, getCurrentValue(), "Verbose marker did not reset");
        }
    }
}