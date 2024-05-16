using System;
using EmergenceSDK.Internal;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using NUnit.Framework;

namespace EmergenceSDK.Tests.Emergence
{
    [TestFixture]
    public class EmergenceSingletonTests
    {
        private IDisposable verboseOutput;
        [OneTimeSetUp]
        public void Setup()
        {
            verboseOutput = EmergenceLogger.VerboseOutput(true);
            EmergenceServiceProvider.Load();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            verboseOutput?.Dispose();
            EmergenceServiceProvider.Unload();
        }

        [Test]
        public void ForcedEnvironment_IsDevelopment()
        {
            using var forcedEnvironment = EmergenceSingletonInternal.ForcedEnvironment(EmergenceEnvironment.Development);
            Assert.AreEqual(EmergenceEnvironment.Development, EmergenceSingleton.Instance.Environment, "Forced environment did not set");
        }

        [Test]
        public void ForcedEnvironment_IsStaging()
        {
            using var forcedEnvironment = EmergenceSingletonInternal.ForcedEnvironment(EmergenceEnvironment.Staging);
            Assert.AreEqual(EmergenceEnvironment.Staging, EmergenceSingleton.Instance.Environment, "Forced environment did not set");
        }

        [Test]
        public void ForcedEnvironment_IsProduction()
        {
            using var forcedEnvironment = EmergenceSingletonInternal.ForcedEnvironment(EmergenceEnvironment.Production);
            Assert.AreEqual(EmergenceEnvironment.Production, EmergenceSingleton.Instance.Environment, "Forced environment did not set");
        }

        [Test]
        public void ForcedEnvironment_ResetsSuccessfully()
        {
            var oldEnvironment = EmergenceSingleton.Instance.Environment;
            var newEnvironment = GetFirstNonMatchingEnum(oldEnvironment);

            using (EmergenceSingletonInternal.ForcedEnvironment(newEnvironment))
            {
                Assert.AreEqual(newEnvironment, EmergenceSingleton.Instance.Environment, "Forced environment did not set");
            }
            Assert.AreEqual(oldEnvironment, EmergenceSingleton.Instance.Environment, "Forced environment did not reset");
        }
        
        EmergenceEnvironment GetFirstNonMatchingEnum(EmergenceEnvironment input)
        {
            // Get all values of MyEnum
            var values = Enum.GetValues(typeof(EmergenceEnvironment));
            foreach (EmergenceEnvironment value in values)
            {
                if (value != input)
                    return value;
            }

            throw new ArgumentOutOfRangeException(nameof(input));
        }
    }
}