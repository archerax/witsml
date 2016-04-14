﻿//----------------------------------------------------------------------- 
// PDS.Witsml.Server, 2016.1
//
// Copyright 2016 Petrotechnical Data Systems
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Linq;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PDS.Witsml.Server.Data.Logs
{
    [TestClass]
    public class Log141AddTests
    {
        private DevKit141Aspect DevKit;
        private Well Well;
        private Wellbore Wellbore;

        [TestInitialize]
        public void TestSetUp()
        {
            DevKit = new DevKit141Aspect();

            DevKit.Store.CapServerProviders = DevKit.Store.CapServerProviders
                .Where(x => x.DataSchemaVersion == OptionsIn.DataVersion.Version141.Value)
                .ToArray();

            Well = new Well { Name = DevKit.Name("Well 01"), TimeZone = DevKit.TimeZone };

            Wellbore = new Wellbore()
            {
                NameWell = Well.Name,
                Name = DevKit.Name("Wellbore 01")
            };
        }

        [TestMethod]
        public void Log_can_be_added_without_depth_data()
        {
            Well.Uid = "804415d0-b5e7-4389-a3c6-cdb790f5485f";
            Well.Name = "Test Well 1.4.1.1";

            // check if well already exists
            var wlResults = DevKit.Query<WellList, Well>(Well);
            if (!wlResults.Any())
            {
                DevKit.Add<WellList, Well>(Well);
            }

            Wellbore.Uid = "d3e7d4bf-0f29-4c2b-974d-4871cf8001fd";
            Wellbore.Name = "Test Wellbore 1.4.1.1";
            Wellbore.UidWell = Well.Uid;
            Wellbore.NameWell = Well.Name;

            // check if wellbore already exists
            var wbResults = DevKit.Query<WellboreList, Wellbore>(Wellbore);
            if (!wbResults.Any())
            {
                DevKit.Add<WellboreList, Wellbore>(Wellbore);
            }

            var log = CreateLog("e2401b72-550f-4695-ab27-d5b0589bde17", "Test Depth Log 1.4.1.1", Well, Wellbore);

            // check if log already exists
            var logResults = DevKit.Query<LogList, Log>(log);
            if (!logResults.Any())
            {
                DevKit.InitHeader(log, LogIndexType.measureddepth);
                var response = DevKit.Add<LogList, Log>(log);
                Assert.AreEqual((short)ErrorCodes.Success, response.Result);
            }
        }

        [TestMethod]
        public void Log_can_be_added_without_time_data()
        {
            Well.Uid = "804415d0-b5e7-4389-a3c6-cdb790f5485f";
            Well.Name = "Test Well 1.4.1.1";

            // check if well already exists
            var wlResults = DevKit.Query<WellList, Well>(Well);
            if (!wlResults.Any())
            {
                DevKit.Add<WellList, Well>(Well);
            }

            Wellbore.Uid = "d3e7d4bf-0f29-4c2b-974d-4871cf8001fd";
            Wellbore.Name = "Test Wellbore 1.4.1.1";
            Wellbore.UidWell = Well.Uid;
            Wellbore.NameWell = Well.Name;

            // check if wellbore already exists
            var wbResults = DevKit.Query<WellboreList, Wellbore>(Wellbore);
            if (!wbResults.Any())
            {
                DevKit.Add<WellboreList, Wellbore>(Wellbore);
            }

            var log = CreateLog(
                "e2401b72-550f-4695-ab27-d5b0589bde18", 
                "Test Time Log 1.4.1.1", 
                Well, 
                Wellbore);

            // check if log already exists
            var logResults = DevKit.Query<LogList, Log>(log);
            if (!logResults.Any())
            {
                DevKit.InitHeader(log, LogIndexType.datetime);
                var response = DevKit.Add<LogList, Log>(log);
                Assert.AreEqual((short)ErrorCodes.Success, response.Result);
            }
        }

        [TestMethod]
        public void Log_can_be_added_with_depth_data()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog(
                null, 
                DevKit.Name("Log can be added with depth data"), 
                Wellbore.UidWell, 
                Well.Name, 
                response.SuppMsgOut, 
                Wellbore.Name);

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 10);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);
        }

        [TestMethod]
        public void Log_can_be_added_with_time_data()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;

            var log = CreateLog(
                null, 
                DevKit.Name("Log can be added with time data"), 
                Wellbore.UidWell, 
                Well.Name, 
                uidWellbore, 
                Wellbore.Name);

            DevKit.InitHeader(log, LogIndexType.datetime);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 10, 1, false);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void Test_add_unsequenced_increasing_depth_log()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog("", DevKit.Name("Log 01"), Wellbore.UidWell, Well.Name, response.SuppMsgOut, Wellbore.Name);
            log.StartIndex = new GenericMeasure(13, "ft");
            log.EndIndex = new GenericMeasure(17, "ft");
            log.LogData = DevKit.List(new LogData() { Data = DevKit.List<string>() });

            var logData = log.LogData.First();
            logData.Data.Add("13,13.1,13.2");
            logData.Data.Add("16,16.1,16.2");
            logData.Data.Add("15,15.1,15.2");
            logData.Data.Add("14,14.1,14.2");
            logData.Data.Add("17,17.1,17.2");
            
            DevKit.InitHeader(log, LogIndexType.measureddepth);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, log.UidWell, null, log.UidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].LogData.Count);
            Assert.AreEqual(5, results[0].LogData[0].Data.Count);

            var resultLogData = results[0].LogData[0].Data;           
            int index = 13;
            foreach (string row in resultLogData)
            {
                string[] columns = row.Split(',');
                int outIndex = int.Parse(columns[0]);
                Assert.AreEqual(index, outIndex);

                double outColumn1 = double.Parse(columns[1]);
                Assert.AreEqual(index + 0.1, outColumn1);

                double outColumn2 = double.Parse(columns[2]);
                Assert.AreEqual(index + 0.2, outColumn2);
                index++;
            }
        }

        [TestMethod]
        public void Test_add_unsequenced_decreasing_depth_log()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog("", DevKit.Name("Log 01"), Wellbore.UidWell, Well.Name, response.SuppMsgOut, Wellbore.Name);
            log.StartIndex = new GenericMeasure(13, "ft");
            log.EndIndex = new GenericMeasure(17, "ft");
            log.LogData = DevKit.List(new LogData() { Data = DevKit.List<string>() });

            var logData = log.LogData.First();
            logData.Data.Add("13,13.1,13.2");
            logData.Data.Add("16,16.1,16.2");
            logData.Data.Add("15,15.1,15.2");
            logData.Data.Add("14,14.1,14.2");
            logData.Data.Add("17,17.1,17.2");

            DevKit.InitHeader(log, LogIndexType.measureddepth, false);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, log.UidWell, null, log.UidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].LogData.Count);
            Assert.AreEqual(5, results[0].LogData[0].Data.Count);

            var resultLogData = results[0].LogData[0].Data;
            int index = 17;
            foreach (string row in resultLogData)
            {
                string[] columns = row.Split(',');
                int outIndex = int.Parse(columns[0]);
                Assert.AreEqual(index, outIndex);

                double outColumn1 = double.Parse(columns[1]);
                Assert.AreEqual(index + 0.1, outColumn1);

                double outColumn2 = double.Parse(columns[2]);
                Assert.AreEqual(index + 0.2, outColumn2);
                index--;
            }
        }

        [TestMethod]
        public void Test_add_unsequenced_increasing_time_log()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog("", DevKit.Name("Log 01"), Wellbore.UidWell, Well.Name, response.SuppMsgOut, Wellbore.Name);
            log.StartDateTimeIndex = new Energistics.DataAccess.Timestamp();
            log.EndDateTimeIndex = new Energistics.DataAccess.Timestamp();
            log.LogData = DevKit.List(new LogData() { Data = DevKit.List<string>() });

            var logData = log.LogData.First();
            logData.Data.Add("2016-04-13T15:30:42.0000000-05:00,30.1,30.2");
            logData.Data.Add("2016-04-13T15:35:42.0000000-05:00,35.1,35.2");
            logData.Data.Add("2016-04-13T15:31:42.0000000-05:00,31.1,31.2");
            logData.Data.Add("2016-04-13T15:32:42.0000000-05:00,32.1,32.2");
            logData.Data.Add("2016-04-13T15:33:42.0000000-05:00,33.1,33.2");
            logData.Data.Add("2016-04-13T15:34:42.0000000-05:00,34.1,34.2");

            DevKit.InitHeader(log, LogIndexType.datetime);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, log.UidWell, null, log.UidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].LogData.Count);
            Assert.AreEqual(6, results[0].LogData[0].Data.Count);

            var resultLogData = results[0].LogData[0].Data;
            int index = 30;
            DateTimeOffset? previousDateTime = null;
            foreach (string row in resultLogData)
            {
                string[] columns = row.Split(',');
                DateTimeOffset outIndex = DateTimeOffset.Parse(columns[0]);
                Assert.AreEqual(index, outIndex.Minute);
                if (previousDateTime.HasValue)
                {
                    Assert.IsTrue((outIndex.ToUnixTimeSeconds() - previousDateTime.Value.ToUnixTimeSeconds()) == 60);
                }
                previousDateTime = outIndex;

                double outColumn1 = double.Parse(columns[1]);
                Assert.AreEqual(index + 0.1, outColumn1);

                double outColumn2 = double.Parse(columns[2]);
                Assert.AreEqual(index + 0.2, outColumn2);
                index++;
            }
        }

        [TestMethod]
        public void Test_add_unsequenced_decreasing_time_log()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog("", DevKit.Name("Log 01"), Wellbore.UidWell, Well.Name, response.SuppMsgOut, Wellbore.Name);
            log.StartDateTimeIndex = new Energistics.DataAccess.Timestamp();
            log.EndDateTimeIndex = new Energistics.DataAccess.Timestamp();
            log.LogData = DevKit.List(new LogData() { Data = DevKit.List<string>() });

            var logData = log.LogData.First();
            logData.Data.Add("2016-04-13T15:30:42.0000000-05:00,30.1,30.2");
            logData.Data.Add("2016-04-13T15:35:42.0000000-05:00,35.1,35.2");
            logData.Data.Add("2016-04-13T15:31:42.0000000-05:00,31.1,31.2");
            logData.Data.Add("2016-04-13T15:32:42.0000000-05:00,32.1,32.2");
            logData.Data.Add("2016-04-13T15:33:42.0000000-05:00,33.1,33.2");
            logData.Data.Add("2016-04-13T15:34:42.0000000-05:00,34.1,34.2");

            DevKit.InitHeader(log, LogIndexType.datetime, false);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, log.UidWell, null, log.UidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].LogData.Count);
            Assert.AreEqual(6, results[0].LogData[0].Data.Count);

            var resultLogData = results[0].LogData[0].Data;
            int index = 35;
            DateTimeOffset? previousDateTime = null;
            foreach (string row in resultLogData)
            {
                string[] columns = row.Split(',');
                DateTimeOffset outIndex = DateTimeOffset.Parse(columns[0]);
                Assert.AreEqual(index, outIndex.Minute);
                if (previousDateTime.HasValue)
                {
                    Assert.IsTrue((outIndex.ToUnixTimeSeconds() - previousDateTime.Value.ToUnixTimeSeconds()) == -60);
                }
                previousDateTime = outIndex;

                double outColumn1 = double.Parse(columns[1]);
                Assert.AreEqual(index + 0.1, outColumn1);

                double outColumn2 = double.Parse(columns[2]);
                Assert.AreEqual(index + 0.2, outColumn2);
                index--;
            }
        }

        [TestMethod]
        public void Test_append_log_data()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test append log data"), 
                Wellbore.UidWell, 
                Well.Name, 
                response.SuppMsgOut, 
                Wellbore.Name);
            log.StartIndex = new GenericMeasure(5, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 10);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;

            log = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            log.StartIndex = new GenericMeasure(17, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6);

            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);

            var result = results.First();
            var logData = result.LogData.FirstOrDefault();

            Assert.IsNotNull(logData);
            Assert.AreEqual(16, logData.Data.Count);
        }

        [TestMethod]
        public void Test_prepend_log_data()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test prepend log data"), 
                Wellbore.UidWell, 
                Well.Name, 
                response.SuppMsgOut, 
                Wellbore.Name);
            log.StartIndex = new GenericMeasure(17, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 10);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;

            log = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            log.StartIndex = new GenericMeasure(5, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6);

            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);

            var result = results.First();
            var logData = result.LogData.FirstOrDefault();

            Assert.IsNotNull(logData);
            Assert.AreEqual(16, logData.Data.Count);
        }

        [TestMethod]
        public void Test_update_overlapping_log_data()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test update overlapping log data"), 
                Wellbore.UidWell, 
                Well.Name, 
                response.SuppMsgOut, 
                Wellbore.Name);
            log.StartIndex = new GenericMeasure(1, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 8);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;

            log = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            log.StartIndex = new GenericMeasure(4.1, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 3, 0.9);

            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);

            var result = results.First();
            var logData = result.LogData.FirstOrDefault();

            Assert.IsNotNull(logData);
            Assert.AreEqual(9, logData.Data.Count);
        }

        [TestMethod]
        public void Test_overwrite_log_data_chunk()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test overwrite log data chunk"), 
                Wellbore.UidWell, 
                Well.Name, 
                response.SuppMsgOut, 
                Wellbore.Name);
            log.StartIndex = new GenericMeasure(17, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;

            log = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            log.StartIndex = new GenericMeasure(4.1, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 3, 0.9);

            var logData = log.LogData.First();
            logData.Data.Add("21.5, 1, 21.7");

            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);

            var result = results.First();
            logData = result.LogData.FirstOrDefault();

            Assert.IsNotNull(logData);
            Assert.AreEqual(5, logData.Data.Count);
        }

        [TestMethod]
        public void Test_update_log_data_with_different_range_for_each_channel()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test update log data diff range"), 
                Wellbore.UidWell, 
                Well.Name, 
                response.SuppMsgOut, 
                Wellbore.Name);
            log.StartIndex = new GenericMeasure(15, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 8);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;

            log = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            log.StartIndex = new GenericMeasure(13, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6, 0.9);

            var logData = log.LogData.First();
            logData.Data.Clear();

            logData.Data.Add("13,13.1,");
            logData.Data.Add("14,14.1,");
            logData.Data.Add("15,15.1,");
            logData.Data.Add("16,16.1,");
            logData.Data.Add("17,17.1,");
            logData.Data.Add("20,20.1,20.2");
            logData.Data.Add("21,,21.2");
            logData.Data.Add("22,,22.2");
            logData.Data.Add("23,,23.2");

            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);

            var result = results.First();
            logData = result.LogData.FirstOrDefault();

            Assert.IsNotNull(logData);
            Assert.AreEqual(11, logData.Data.Count);

            var data = logData.Data;
            Assert.AreEqual("15,15.1,15", data[2]);
        }

        [TestMethod]
        public void Test_update_log_header()
        {
            var response = DevKit.Add<WellList, Well>(Well);
            Wellbore.UidWell = response.SuppMsgOut;

            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test update log header"), 
                Wellbore.UidWell, 
                Well.Name, 
                uidWellbore, 
                Wellbore.Name);
            log.Description = "Not updated field";
            log.RunNumber = "101";
            log.BhaRunNumber = 1;

            DevKit.InitHeader(log, LogIndexType.measureddepth);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logAdded = results.FirstOrDefault();

            Assert.IsNotNull(logAdded);
            Assert.AreEqual(log.Description, logAdded.Description);
            Assert.AreEqual(log.RunNumber, logAdded.RunNumber);
            Assert.AreEqual(log.BhaRunNumber, logAdded.BhaRunNumber);
            Assert.IsNull(logAdded.CommonData.ItemState);

            var update = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            update.CommonData = new CommonData { ItemState = ItemState.actual };
            update.RunNumber = "102";
            update.BhaRunNumber = 2;

            var updateResponse = DevKit.Update<LogList, Log>(update);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logUpdated = results.FirstOrDefault();

            Assert.IsNotNull(logUpdated);
            Assert.AreEqual(logAdded.Description, logUpdated.Description);
            Assert.AreEqual(update.RunNumber, logUpdated.RunNumber);
            Assert.AreEqual(update.BhaRunNumber, logUpdated.BhaRunNumber);
            Assert.AreEqual(update.CommonData.ItemState, logUpdated.CommonData.ItemState);
        }

        [TestMethod]
        public void Test_update_log_header_update_curve()
        {
            var response = DevKit.Add<WellList, Well>(Well);
            Wellbore.UidWell = response.SuppMsgOut;

            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test update log header update curve"), 
                Wellbore.UidWell, 
                Well.Name, 
                uidWellbore, 
                Wellbore.Name);
            log.Description = "Not updated field";
            log.RunNumber = "101";
            log.BhaRunNumber = 1;

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            log.LogCurveInfo.RemoveAt(2);
            log.LogData.Clear();
            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logAdded = results.FirstOrDefault();

            Assert.IsNotNull(logAdded);
            Assert.AreEqual(log.Description, logAdded.Description);
            Assert.AreEqual(log.RunNumber, logAdded.RunNumber);
            Assert.AreEqual(log.BhaRunNumber, logAdded.BhaRunNumber);
            Assert.IsNull(logAdded.CommonData.ItemState);
            var logCurve = logAdded.LogCurveInfo.FirstOrDefault(c => c.Uid == "ROP");
            Assert.IsNull(logCurve.CurveDescription);

            var update = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            update.CommonData = new CommonData { ItemState = ItemState.actual };
            update.RunNumber = "102";
            update.BhaRunNumber = 2;

            DevKit.InitHeader(update, LogIndexType.measureddepth);
            update.LogCurveInfo.RemoveAt(2);
            update.LogCurveInfo.RemoveAt(0);
            update.LogData.Clear();
            var updateCurve = update.LogCurveInfo.First();
            updateCurve.CurveDescription = "Updated description";

            var updateResponse = DevKit.Update<LogList, Log>(update);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logUpdated = results.FirstOrDefault();

            Assert.IsNotNull(logUpdated);
            Assert.AreEqual(logAdded.Description, logUpdated.Description);
            Assert.AreEqual(update.RunNumber, logUpdated.RunNumber);
            Assert.AreEqual(update.BhaRunNumber, logUpdated.BhaRunNumber);
            Assert.AreEqual(update.CommonData.ItemState, logUpdated.CommonData.ItemState);
            logCurve = logUpdated.LogCurveInfo.FirstOrDefault(c => c.Uid == "ROP");
            Assert.AreEqual(updateCurve.CurveDescription, logCurve.CurveDescription);
        }

        [TestMethod]
        public void Test_update_log_header_add_curve()
        {
            var response = DevKit.Add<WellList, Well>(Well);
            Wellbore.UidWell = response.SuppMsgOut;

            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test update log header add curve"), 
                Wellbore.UidWell, 
                Well.Name, 
                uidWellbore, 
                Wellbore.Name);

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            log.LogCurveInfo.RemoveRange(1, 2);
            log.LogData.Clear();

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logAdded = results.FirstOrDefault();

            Assert.IsNotNull(logAdded);
            Assert.AreEqual(1, logAdded.LogCurveInfo.Count);
            Assert.AreEqual(log.LogCurveInfo.Count, logAdded.LogCurveInfo.Count);

            var update = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            DevKit.InitHeader(update, LogIndexType.measureddepth);
            update.LogCurveInfo.RemoveAt(2);
            update.LogCurveInfo.RemoveAt(0);
            update.LogData.Clear();

            var updateResponse = DevKit.Update<LogList, Log>(update);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logUpdated = results.FirstOrDefault();

            Assert.IsNotNull(logUpdated);
            Assert.AreEqual(2, logUpdated.LogCurveInfo.Count);
        }

        [TestMethod]
        public void Test_log_index_direction_default_and_update()
        {
            var response = DevKit.Add<WellList, Well>(Well);
            Wellbore.UidWell = response.SuppMsgOut;

            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test log index direction default"), 
                Wellbore.UidWell, 
                Well.Name, 
                uidWellbore, 
                Wellbore.Name);
            log.RunNumber = "101";
            log.IndexCurve = "MD";
            log.IndexType = LogIndexType.measureddepth;

            DevKit.InitHeader(log, log.IndexType.Value);
            log.Direction = null;

            Assert.IsFalse(log.Direction.HasValue);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logAdded = results.FirstOrDefault();

            Assert.IsNotNull(logAdded);
            Assert.AreEqual(LogIndexDirection.increasing, logAdded.Direction);
            Assert.AreEqual(log.RunNumber, logAdded.RunNumber);

            var update = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            update.Direction = LogIndexDirection.decreasing;
            update.RunNumber = "102";

            var updateResponse = DevKit.Update<LogList, Log>(update);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logUpdated = results.FirstOrDefault();

            Assert.IsNotNull(logUpdated);
            Assert.AreEqual(LogIndexDirection.increasing, logAdded.Direction);
            Assert.AreEqual(update.RunNumber, logUpdated.RunNumber);
        }

        [TestMethod]
        public void Test_update_log_data_and_index_range()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test update log data and index range"), 
                Wellbore.UidWell, 
                Well.Name, 
                response.SuppMsgOut, 
                Wellbore.Name);
            log.StartIndex = new GenericMeasure(15, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 8);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidWellbore = log.UidWellbore;
            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logAdded = results.FirstOrDefault();

            Assert.IsNotNull(logAdded);
            Assert.AreEqual(15, logAdded.StartIndex.Value);
            Assert.AreEqual(22, logAdded.EndIndex.Value);
            var mdCurve = logAdded.LogCurveInfo.FirstOrDefault(c => c.Mnemonic.Value == logAdded.IndexCurve);
            Assert.AreEqual(logAdded.StartIndex.Value, mdCurve.MinIndex.Value);
            Assert.AreEqual(logAdded.EndIndex.Value, mdCurve.MaxIndex.Value);
            var curve2 = logAdded.LogCurveInfo[1];
            Assert.IsNull(curve2.MinIndex);
            Assert.IsNull(curve2.MaxIndex);
            var curve3 = logAdded.LogCurveInfo[2];
            Assert.AreEqual(logAdded.StartIndex.Value, curve3.MinIndex.Value);
            Assert.AreEqual(logAdded.EndIndex.Value, curve3.MaxIndex.Value);

            log = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);
            log.StartIndex = new GenericMeasure(13, "m");

            DevKit.InitHeader(log, LogIndexType.measureddepth);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 6, 0.9);

            var logData = log.LogData.First();
            logData.Data.Clear();

            logData.Data.Add("13,13.1,");
            logData.Data.Add("14,14.1,");
            logData.Data.Add("15,15.1,");
            logData.Data.Add("16,16.1,");
            logData.Data.Add("17,17.1,");
            logData.Data.Add("20,20.1,20.2");
            logData.Data.Add("21,,21.2");
            logData.Data.Add("22,,22.2");
            logData.Data.Add("23,,23.2");

            var updateResponse = DevKit.Update<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);

            var logUpdated = results.First();
            logData = logUpdated.LogData.FirstOrDefault();

            Assert.IsNotNull(logData);
            Assert.AreEqual(11, logData.Data.Count);
            Assert.AreEqual(13, logUpdated.StartIndex.Value);
            Assert.AreEqual(23, logUpdated.EndIndex.Value);
            mdCurve = logUpdated.LogCurveInfo.FirstOrDefault(c => c.Mnemonic.Value == logUpdated.IndexCurve);
            Assert.AreEqual(logUpdated.StartIndex.Value, mdCurve.MinIndex.Value);
            Assert.AreEqual(logUpdated.EndIndex.Value, mdCurve.MaxIndex.Value);
            curve2 = logUpdated.LogCurveInfo[1];
            Assert.AreEqual(13, curve2.MinIndex.Value);
            Assert.AreEqual(20, curve2.MaxIndex.Value);
            curve3 = logUpdated.LogCurveInfo[2];
            Assert.AreEqual(15, curve3.MinIndex.Value);
            Assert.AreEqual(23, curve3.MaxIndex.Value);
        }

        [TestMethod]
        public void Test_log_index_direction_decreasing()
        {
            var response = DevKit.Add<WellList, Well>(Well);
            Wellbore.UidWell = response.SuppMsgOut;

            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;

            var log = CreateLog(
                null, 
                DevKit.Name("Log log index direction decreasing"), 
                Wellbore.UidWell, 
                Well.Name, 
                uidWellbore, 
                Wellbore.Name);
            log.RunNumber = "101";
            log.IndexCurve = "MD";
            log.IndexType = LogIndexType.measureddepth;
            log.Direction = LogIndexDirection.decreasing;

            DevKit.InitHeader(log, log.IndexType.Value, increasing: false);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 100, 0.9, increasing: false);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = CreateLog(uidLog, null, Wellbore.UidWell, null, uidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var logAdded = results.FirstOrDefault();

            Assert.IsNotNull(logAdded);
            Assert.AreEqual(LogIndexDirection.decreasing, logAdded.Direction);
            Assert.AreEqual(log.RunNumber, logAdded.RunNumber);

            var logData = log.LogData.FirstOrDefault();
            var firstIndex = int.Parse(logData.Data[0].Split(',')[0]);
            var secondIndex = int.Parse(logData.Data[1].Split(',')[0]);
            Assert.IsTrue(firstIndex > secondIndex);
        }

        [TestMethod]
        public void Test_update_with_unsequenced_increasing_depth_log_data()
        {
            var response = DevKit.Add<WellList, Well>(Well);

            Wellbore.UidWell = response.SuppMsgOut;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            var log = CreateLog(null, DevKit.Name("Log 01"), Wellbore.UidWell, Well.Name, response.SuppMsgOut, Wellbore.Name);
            log.StartIndex = new GenericMeasure(13, "ft");
            log.EndIndex = new GenericMeasure(17, "ft");
            log.LogData = DevKit.List(new LogData() { Data = DevKit.List<string>() });

            var logData = log.LogData.First();
            logData.Data.Add("10,10.1,10.2");           
            logData.Data.Add("15,15.1,15.2");         
            logData.Data.Add("16,16.1,16.2");
            logData.Data.Add("17,17.1,17.2");
            logData.Data.Add("18,18.1,18.2");

            DevKit.InitHeader(log, LogIndexType.measureddepth);

            response = DevKit.Add<LogList, Log>(log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            // Update
            var updateLog = CreateLog(uidLog, log.Name, log.UidWell, log.NameWell, log.UidWellbore, log.NameWellbore);
            updateLog.LogData = DevKit.List(new LogData() { Data = DevKit.List<string>() });
            updateLog.LogData[0].MnemonicList = log.LogData.First().MnemonicList;
            updateLog.LogData[0].UnitList = log.LogData.First().UnitList;
            logData = updateLog.LogData.First();
            logData.Data.Add("13,13.1,13.2");
            logData.Data.Add("12,12.1,12.2");
            logData.Data.Add("11,11.1,11.2");
            logData.Data.Add("14,14.1,14.2");

            var updateResponse = DevKit.Update<LogList, Log>(updateLog);
            Assert.AreEqual((short)ErrorCodes.Success, updateResponse.Result);

            // Query
            var query = CreateLog(uidLog, null, log.UidWell, null, log.UidWellbore, null);

            var results = DevKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].LogData.Count);
            Assert.AreEqual(9, results[0].LogData[0].Data.Count);

            var resultLogData = results[0].LogData[0].Data;
            double index = 10;
            foreach (string row in resultLogData)
            {
                string[] columns = row.Split(',');
                double outIndex = double.Parse(columns[0]);
                Assert.AreEqual(index, outIndex);

                double outColumn1 = double.Parse(columns[1]);
                Assert.AreEqual(index + 0.1, outColumn1);

                double outColumn2 = double.Parse(columns[2]);
                Assert.AreEqual(index + 0.2, outColumn2);
                index++;
            }
        }

        [TestMethod]
        public void Test_error_code_443_invalid_unit_of_measure_value()
        {

            var response = DevKit.Add<WellList, Well>(Well);
            var uidWell = response.SuppMsgOut;
            Wellbore.UidWell = response.SuppMsgOut;

            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;
            var logName = "Log Test -443 - Invalid Uom";
            var startIndexUom = "abc";
            var endIndexUom = startIndexUom;
            
            string xmlIn = CreateXmlLog(
                logName, 
                uidWell, 
                Well.Name, 
                uidWellbore, 
                Wellbore.Name, 
                startIndexUom, 
                endIndexUom);
            response = DevKit.AddToStore(ObjectTypes.Log, xmlIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.InvalidUnitOfMeasure, response.Result);
        }

        [TestMethod]
        public void Test_error_code_453_missing_unit_for_measure_data()
        {
            var response = DevKit.Add<WellList, Well>(Well);
            var uidWell = response.SuppMsgOut;
            Wellbore.UidWell = response.SuppMsgOut;

            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;
            var logName = "Log Test -453 - Missing Uom";

            string xmlIn = CreateXmlLog(
                logName, 
                uidWell, 
                Well.Name, 
                uidWellbore, 
                Wellbore.Name, 
                startIndexUom: null, 
                endIndexUom: null);
            response = DevKit.AddToStore(ObjectTypes.Log, xmlIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingUnitForMeasureData, response.Result);
        }

        [TestMethod]
        public void Test_error_code_406_missing_parent_uid()
        {
            var response = DevKit.Add<WellList, Well>(Well);
            Wellbore.UidWell = response.SuppMsgOut;

            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);
            var uidWellbore = response.SuppMsgOut;

            var log = CreateLog(
                null, 
                DevKit.Name("Log Test error code -406 missing parent"), 
                null, 
                Well.Name, 
                null, 
                Wellbore.Name);
            log.RunNumber = "101";
            log.IndexCurve = "MD";
            log.IndexType = LogIndexType.measureddepth;
            log.Direction = LogIndexDirection.decreasing;

            DevKit.InitHeader(log, log.IndexType.Value, increasing: false);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 100, 0.9, increasing: false);

            response = DevKit.Add<LogList, Log>(log);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingParentUid, response.Result);
        }

        [TestMethod]
        public void Test_error_code_478_parent_uid_case_not_matching()
        {
            // Base uid
            var uid = "arent-well-01-for-error-code-478" + DevKit.Uid();

            // Well Uid with uppercase "P"
            Well.Uid = "P" + uid;
            Well.Name = DevKit.Name("Well-to-add-01");
            var response = DevKit.Add<WellList, Well>(Well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            // Well Uid with uppercase "P"
            Wellbore.Name = DevKit.Name("Wellbore-to-add-02");
            Wellbore.NameWell = Well.Name;
            Wellbore.UidWell = "P" + uid;
            response = DevKit.Add<WellboreList, Wellbore>(Wellbore);

            // Well Uid with lowercase "p"
            var log = CreateLog(
                DevKit.Uid(), 
                name: DevKit.Name("Log Test error code -478 parent uid case"), 
                uidWell: "p" + uid, 
                nameWell: Well.Name, 
                uidWellbore: response.SuppMsgOut, 
                nameWellbore: Wellbore.Name);
            log.RunNumber = "101";
            log.IndexCurve = "MD";
            log.IndexType = LogIndexType.measureddepth;
            log.Direction = LogIndexDirection.decreasing;
            DevKit.InitHeader(log, log.IndexType.Value, increasing: false);
            DevKit.InitDataMany(log, DevKit.Mnemonics(log), DevKit.Units(log), 100, 0.9, increasing: false);

            response = DevKit.Add<LogList, Log>(log);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.IncorrectCaseParentUid, response.Result);
        }

        #region Helper Methods

        private Log CreateLog(string uid, string name, Well well, Wellbore wellbore)
        {
            return CreateLog(uid, name, well.Uid, well.Name, wellbore.Uid, wellbore.Name);
        }

        private Log CreateLog(string uid, string name, string uidWell, string nameWell, string uidWellbore, string nameWellbore)
        {
            return new Log()
            {
                Uid = uid,
                Name = name,
                UidWell = uidWell,
                NameWell = nameWell,
                UidWellbore = uidWellbore,
                NameWellbore = nameWellbore,
            };
        }

        private string CreateXmlLog(string logName, string uidWell, string nameWell, string uidWellbore, string nameWellbore, string startIndexUom, string endIndexUom)
        {
            string xmlIn =
                "<logs xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:dc=\"http://purl.org/dc/terms/\" xmlns:gml=\"http://www.opengis.net/gml/3.2\" version=\"1.4.1.1\" xmlns=\"http://www.witsml.org/schemas/1series\">" +
                "    <log uidWell=\"" + uidWell + "\" uidWellbore=\"" + uidWellbore + "\">" +
                "        <nameWell>" + Well.Name + "</nameWell>" +
                "        <nameWellbore>" + Wellbore.Name + "</nameWellbore>" +
                "        <name>" + logName + "</name>" +
                "        <serviceCompany>Service Company Name</serviceCompany>" +
                "        <indexType>measured depth</indexType>" +
                (string.IsNullOrEmpty(startIndexUom) ? "<startIndex>499</startIndex>" : "<startIndex uom =\"" + startIndexUom + "\">499</startIndex>") +
                (string.IsNullOrEmpty(endIndexUom) ? "<endIndex>509.01</endIndex>" : "<endIndex uom =\"" + endIndexUom + "\">509.01</endIndex>") +
                "        <stepIncrement uom =\"m\">0</stepIncrement>" +
                "        <indexCurve>Mdepth</indexCurve>" +
                "        <logCurveInfo uid=\"lci-1\">" +
                "            <mnemonic>Mdepth</mnemonic>" +
                "            <unit>m</unit>" +
                "            <mnemAlias>md</mnemAlias>" +
                "            <nullValue>-999.25</nullValue>" +
                "            <minIndex uom=\"m\">499</minIndex>" +
                "            <maxIndex uom=\"m\">509.01</maxIndex>" +
                "            <typeLogData>double</typeLogData>" +
                "        </logCurveInfo>" +
                "        <logCurveInfo uid=\"lci-2\">" +
                "            <mnemonic>Vdepth</mnemonic>" +
                "            <unit>m</unit>" +
                "            <mnemAlias>tvd</mnemAlias>" +
                "            <nullValue>-999.25</nullValue>" +
                "            <minIndex uom=\"m\">499</minIndex>" +
                "            <maxIndex uom=\"m\">509.01</maxIndex>" +
                "            <typeLogData>double</typeLogData >" +
                "        </logCurveInfo >" +
                "    </log>" +
                "</logs>";

            return xmlIn;
        }
        #endregion
    }
}
