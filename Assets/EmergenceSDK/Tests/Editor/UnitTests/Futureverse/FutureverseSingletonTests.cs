using System;
using EmergenceSDK.Runtime.Futureverse;
using EmergenceSDK.Runtime.Futureverse.Internal;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Types;
using NUnit.Framework;

namespace EmergenceSDK.Tests.UnitTests.Futureverse
{
    [TestFixture]
    public class FutureverseSingletonTests
    {
        private IDisposable verboseOutput;
        [OneTimeSetUp]
        public void Setup()
        {
            verboseOutput = EmergenceLogger.VerboseOutput(true);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            verboseOutput?.Dispose();
        }

        [Test]
        public void ForcedEnvironment_IsDevelopment()
        {
            using var forcedEnvironment = InternalFutureverseSingleton.ForcedEnvironment(EmergenceEnvironment.Development);
            Assert.AreEqual(EmergenceEnvironment.Development, FutureverseSingleton.Instance.Environment);
        }

        [Test]
        public void ForcedEnvironment_IsStaging()
        {
            using var forcedEnvironment = InternalFutureverseSingleton.ForcedEnvironment(EmergenceEnvironment.Staging);
            Assert.AreEqual(EmergenceEnvironment.Staging, FutureverseSingleton.Instance.Environment);
        }

        [Test]
        public void ForcedEnvironment_IsProduction()
        {
            using var forcedEnvironment = InternalFutureverseSingleton.ForcedEnvironment(EmergenceEnvironment.Production);
            Assert.AreEqual(EmergenceEnvironment.Production, FutureverseSingleton.Instance.Environment);
        }
        
        [Test]
        public void ForcedEnvironment_ResetsSuccessfully()
        {
            var oldEnvironment = FutureverseSingleton.Instance.Environment;
            var newEnvironment = GetFirstNonMatchingEnum(oldEnvironment);

            using (InternalFutureverseSingleton.ForcedEnvironment(newEnvironment))
            {
                Assert.AreEqual(newEnvironment, FutureverseSingleton.Instance.Environment, "Forced environment did not set");
            }
            Assert.AreEqual(oldEnvironment, FutureverseSingleton.Instance.Environment, "Forced environment did not reset");
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