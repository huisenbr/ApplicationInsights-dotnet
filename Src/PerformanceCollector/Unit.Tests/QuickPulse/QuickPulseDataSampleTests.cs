﻿namespace Unit.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.Implementation.QuickPulse;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QuickPulseDataSampleTests
    {
        private ILookup<string, float> dummyLookup;

        [TestInitialize]
        public void TestInitialize()
        {
            this.dummyLookup = new Dictionary<string, float>().ToLookup(pair => pair.Key, pair => pair.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickPulseDataSampleThrowsWhenAccumulatorIsNull()
        {
            new QuickPulseDataSample(null, this.dummyLookup);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickPulseDataSampleThrowsWhenPerfDataIsNull()
        {
            new QuickPulseDataSample(new QuickPulseDataAccumulator(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickPulseDataSampleThrowsWhenAccumulatorStartTimestampIsNull()
        {
            new QuickPulseDataSample(new QuickPulseDataAccumulator() { EndTimestamp = DateTime.UtcNow }, this.dummyLookup);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickPulseDataSampleThrowsWhenAccumulatorEndTimestampIsNull()
        {
            new QuickPulseDataSample(new QuickPulseDataAccumulator() { StartTimestamp = DateTime.UtcNow }, this.dummyLookup);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void QuickPulseDataSampleThrowsWhenTimestampsAreReversedInTime()
        {
            new QuickPulseDataSample(
                new QuickPulseDataAccumulator() { StartTimestamp = DateTime.UtcNow, EndTimestamp = DateTime.UtcNow.AddSeconds(-1) },
                this.dummyLookup);
        }

        [TestMethod]
        public void QuickPulseDataSampleTimestampsItselfCorrectly()
        {
            // ARRANGE
            var timestampStart = DateTime.UtcNow;
            var timestampEnd = DateTime.UtcNow.AddSeconds(3);
            var accumulator = new QuickPulseDataAccumulator { StartTimestamp = timestampStart, EndTimestamp = timestampEnd };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(timestampStart, dataSample.StartTimestamp);
            Assert.AreEqual(timestampEnd, dataSample.EndTimestamp);
        }

        #region AI data calculation checks

        #region Requests

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRpsCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTime.UtcNow,
                                      EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                                      AIRequestCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIRequestsPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRequestDurationAveCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTime.UtcNow,
                                      EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                                      AIRequestCount = 10,
                                      AIRequestDurationInTicks = TimeSpan.FromSeconds(5).Ticks
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(TimeSpan.FromSeconds(5).Ticks / 10.0, dataSample.AIRequestDurationAve);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRequestsFailedPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTime.UtcNow,
                                      EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                                      AIRequestFailureCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIRequestsFailedPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRequestsSucceededPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTime.UtcNow,
                                      EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                                      AIRequestSuccessCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIRequestsSucceededPerSecond);
        }

        #endregion

        #region Dependency calls

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallsPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTime.UtcNow,
                                      EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                                      AIDependencyCallCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIDependencyCallsPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallDurationAveCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTime.UtcNow,
                                      EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                                      AIDependencyCallCount = 10,
                                      AIDependencyCallDurationInTicks = TimeSpan.FromSeconds(5).Ticks
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(TimeSpan.FromSeconds(5).Ticks / 10.0, dataSample.AIDependencyCallDurationAve);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallsFailedPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTime.UtcNow,
                                      EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                                      AIDependencyCallFailureCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIDependencyCallsFailedPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallsSucceededPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTime.UtcNow,
                                      EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                                      AIDependencyCallSuccessCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIDependencyCallsSucceededPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRequestDurationAveWhenRequestCountIsZeroCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
            {
                StartTimestamp = DateTime.UtcNow,
                EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                AIRequestDurationInTicks = TimeSpan.FromSeconds(5).Ticks,
                AIRequestCount = 0
            };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(0.0, dataSample.AIRequestDurationAve);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallDurationAveWhenDependencyCallCountIsZeroCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
            {
                StartTimestamp = DateTime.UtcNow,
                EndTimestamp = DateTime.UtcNow.AddSeconds(2),
                AIDependencyCallDurationInTicks = TimeSpan.FromSeconds(5).Ticks,
                AIDependencyCallCount = 0
            };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(0.0, dataSample.AIDependencyCallDurationAve);
        }

        #endregion

        #endregion

        #region Perf data calculation checks
        [TestMethod]
        public void QuickPulseDataSampleHandlesAbsentCounterInPerfData()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
            {
                StartTimestamp = DateTime.UtcNow,
                EndTimestamp = DateTime.UtcNow.AddSeconds(2)
            };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyLookup);

            // ASSERT
            Assert.AreEqual(0.0, dataSample.PerfIisRequestsPerSecond);
        }
        #endregion
    }
}