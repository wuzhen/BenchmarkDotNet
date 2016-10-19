﻿using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Portability;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Tests.Loggers;
using BenchmarkDotNet.Toolchains;
using Xunit;
using Xunit.Abstractions;

namespace BenchmarkDotNet.IntegrationTests
{
    public class MultipleRuntimesTest
    {
        private readonly ITestOutputHelper output;

        public MultipleRuntimesTest(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        [Fact]
        public void SingleBenchmarkCanBeExecutedForMultpleRuntimes()
        {
            var summary = BenchmarkRunner
                .Run<C>(
                    ManualConfig.CreateEmpty()
                                .With(new Job(Job.Dry, EnvMode.Core))
                                .With(new Job(Job.Dry, EnvMode.Clr))
                                .With(DefaultColumnProviders.Instance)
                                .With(new OutputLogger(output)));

            Assert.True(summary.Reports
                .All(report => report.ExecuteResults
                .All(executeResult => executeResult.FoundExecutable)));

            Assert.True(summary.Reports.All(report => report.AllMeasurements.Any()));

            Assert.True(summary.Reports
                .Single(report => report.Benchmark.Job.Env.Runtime == Runtime.Clr)
                .ExecuteResults
                .All(executeResult => executeResult.Data.Contains("Classic")));

            Assert.True(summary.Reports
                .Single(report => report.Benchmark.Job.Env.Runtime == Runtime.Core)
                .ExecuteResults
                .All(executeResult => executeResult.Data.Contains("Core")));

            Assert.Contains("Clr 4", summary.JobRuntimes);
            Assert.Contains("Core", summary.JobRuntimes);
        }
    }

    // this test was suffering from too long path ex so I had to rename the class and benchmark method to fit within the limit
    public class C
    {
        [Benchmark]
        public void B()
        {
            Console.WriteLine($"{RuntimeInformation.GetCurrentRuntime().GetToolchain()}");
        }
    }
}