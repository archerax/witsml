﻿using System.Collections.Generic;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PDS.Witsml.Data.Logs
{
    [TestClass]
    public class Log141GeneratorTests
    {
        private Log141Generator LogGenerator;
        private Log DepthLog;
        private Log TimeLog;

        [TestInitialize]
        public void TestSetUp()
        {
            LogGenerator = new Log141Generator();
            DepthLog = Create(LogIndexType.measureddepth, LogIndexDirection.decreasing);
            TimeLog = Create(LogIndexType.datetime, LogIndexDirection.increasing);
        }

        [TestMethod]
        public void Can_generate_depth_log()
        {
            LogGenerator.GenerateLogData(DepthLog);

            Assert.IsNotNull(DepthLog);
            Assert.IsNotNull(DepthLog.LogData);
            Assert.IsNotNull(DepthLog.LogData[0].Data);
            Assert.AreEqual(5, DepthLog.LogData[0].Data.Count);
        }

        [TestMethod]
        public void Can_generate_time_log()
        {
            LogGenerator.GenerateLogData(TimeLog, 10);

            Assert.IsNotNull(TimeLog);
            Assert.IsNotNull(TimeLog.LogData);
            Assert.IsNotNull(TimeLog.LogData[0].Data);
            Assert.AreEqual(10, TimeLog.LogData[0].Data.Count);
        }

        private Log Create(LogIndexType indexType, LogIndexDirection direction)
        {
            var log = new Log();

            log.IndexType = indexType;
            log.Direction = direction;
            log.LogCurveInfo = new List<LogCurveInfo>();

            if (indexType == LogIndexType.datetime)
            {
                log.IndexCurve = "TIME";
                log.LogCurveInfo.Add(LogGenerator.CreateDateTimeLogCurveInfo(log.IndexCurve, "s"));
            }
            else
            {
                log.IndexCurve = "MD";
                log.LogCurveInfo.Add(LogGenerator.CreateDoubleLogCurveInfo(log.IndexCurve, "m"));
            }

            log.LogCurveInfo.Add(LogGenerator.CreateDoubleLogCurveInfo("ROP", "m/h"));
            log.LogCurveInfo.Add(LogGenerator.CreateDoubleLogCurveInfo("GR", "gAPI"));

            return log;
        }
    }
}
