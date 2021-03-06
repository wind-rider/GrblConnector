﻿using GrblConnector.Grbl;
using GrblConnector.Grbl.PushMessages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace GrblConnector.Tests.Tests
{
    [TestClass]
    public class StatusMessageTests
    {
        [TestMethod]
        [DataRow("Idle", MachineState.Idle, -1)]
        [DataRow("Run", MachineState.Run, -1)]
        [DataRow("Hold", MachineState.Hold, -1)]
        [DataRow("Hold:0", MachineState.Hold, 0)]
        [DataRow("Hold:1", MachineState.Hold, 1)]
        [DataRow("Jog", MachineState.Jog, -1)]
        [DataRow("Alarm", MachineState.Alarm, -1)]
        [DataRow("Door", MachineState.Door, -1)]
        [DataRow("Door:0", MachineState.Door, 0)]
        [DataRow("Door:1", MachineState.Door, 1)]
        [DataRow("Door:2", MachineState.Door, 2)]
        [DataRow("Door:3", MachineState.Door, 3)]
        [DataRow("Check", MachineState.Check, -1)]
        [DataRow("Home", MachineState.Home, -1)]
        [DataRow("Sleep", MachineState.Sleep, -1)]
        public void MachineStateTest(string machineState, MachineState expectedState, int expectedSubState)
        {
            var line = $"<{machineState}|MPos:0.000,0.000,0.000|FS:0.0,0>";
            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;
            Assert.AreEqual(statusMsg.MachineState, expectedState);
            Assert.AreEqual(statusMsg.MachineSubState, expectedSubState);
        }

        [TestMethod]
        [DataRow("MPos:0.000,-10.000,5.000", PositionType.MachinePosition, new float[] { 0.0f, -10.0f, 5.0f, 0.0f })]
        [DataRow("WPos:-2.500,0.000,11.000", PositionType.WorkPosition, new float[] { -2.5f, 0.0f, 11.0f, 0.0f })]
        public void CurrentPositionTest(string position, PositionType expectedPositionType, float[] expectedCoordinates)
        {
            var line = $"<Idle|{position}|FS:0.0,0>";
            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;
            Assert.AreEqual(statusMsg.PositionType, expectedPositionType);

            var testFloatArray = new float[4];
            statusMsg.CurrentPosition.CopyTo(testFloatArray);

            for (int i = 0; i < testFloatArray.Length; i++)
            {
                Assert.AreEqual(testFloatArray[i], expectedCoordinates[i]);
            }
        }

        [TestMethod]
        [DataRow("WCO:0.000,1.551,5.664", new float[] { 0.0f, 1.551f, 5.664f, 0.0f })]
        public void WorkCoordinateOffsetTest(string workCoordinateOffset, float[] expectedOffset)
        {
            var line = $"<Hold|MPos:0.000,-10.000,5.000|FS:0.0,0|{workCoordinateOffset}>";

            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;

            var testFloatArray = new float[4];
            statusMsg.WorkCoordinateOffset.CopyTo(testFloatArray);

            for (int i = 0; i < testFloatArray.Length; i++)
            {
                Assert.AreEqual(testFloatArray[i], expectedOffset[i]);
            }
        }

        [TestMethod]
        [DataRow("Bf:15,128", 15, 128)]
        public void BufferStateTest(string bufferState, int expectedAvailableBlocksInPlanner, int expectedAvailableBytesInRx)
        {
            var line = $"<Hold|WPos:-2.500,0.000,11.000|FS:0.0,0|{bufferState}|WCO:0.000,1.551,5.664>";

            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;

            Assert.IsNotNull(statusMsg.BufferState);

            Assert.AreEqual(statusMsg.BufferState.AvailableBlocksInPlannerBuffer, expectedAvailableBlocksInPlanner);
            Assert.AreEqual(statusMsg.BufferState.AvailableBytesInRxBuffer, expectedAvailableBytesInRx);
        }

        [TestMethod]
        [DataRow("Ln:99999", 99999)]
        public void LineNumberTest(string lineNumberInput, int expectedLineNumber)
        {
            var line = $"<Hold|WPos:-2.500,0.000,11.000|FS:0.0,0|{lineNumberInput}|WCO:0.000,1.551,5.664>";

            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;

            Assert.AreEqual(statusMsg.LineNumber, expectedLineNumber);
        }

        [TestMethod]
        [DataRow("F:500", 500.0f, -1)]
        [DataRow("F:500.53", 500.53f, -1)]
        [DataRow("FS:500,8000", 500.0f, 8000)]
        public void FeedSpeedTest(string feedSpeedInput, float expectedFeedrate, int expectedSpindleSpeed)
        {
            var line = $"<Hold|WPos:-2.500,0.000,11.000|FS:0.0,0|{feedSpeedInput}|WCO:0.000,1.551,5.664>";

            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;

            Assert.AreEqual(statusMsg.Feed, expectedFeedrate);
            Assert.AreEqual(statusMsg.SpindleSpeed, expectedSpindleSpeed);
        }

        [TestMethod]
        [DataRow("Pn:XDHA", InputPinState.X | InputPinState.Door | InputPinState.Hold | InputPinState.A)]
        [DataRow("Pn:ZPR", InputPinState.Z | InputPinState.Probe | InputPinState.SoftReset)]
        [DataRow("Pn:SY", InputPinState.CycleStart | InputPinState.Y)]
        public void PinStateTest(string pinStateInput, InputPinState expectedPinFlags)
        {
            var line = $"<Hold|WPos:-2.500,0.000,11.000|FS:0.0,0|{pinStateInput}|WCO:0.000,1.551,5.664>";

            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;

            Assert.AreEqual(statusMsg.InputPinState, expectedPinFlags);
        }

        [TestMethod]
        [DataRow("Ov:23,97,143", 23, 97, 143)]
        public void OverrideTest(string overrideInput, int expectedFeedOverride, int expectedRapidOverride, int expectedSpindleSpeedOverride)
        {
            var line = $"<Hold|WPos:-2.500,0.000,11.000|FS:0.0,0|{overrideInput}|WCO:0.000,1.551,5.664>";

            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;

            Assert.AreEqual(statusMsg.OverrideFeedPercent, expectedFeedOverride);
            Assert.AreEqual(statusMsg.OverrideRapidsPercent, expectedRapidOverride);
            Assert.AreEqual(statusMsg.OverrideSpindleSpeedPercent, expectedSpindleSpeedOverride);
        }

        [TestMethod]
        [DataRow("A:SCM", AccessoryState.SpindleCW | AccessoryState.SpindleCCW | AccessoryState.MistCoolant)]
        [DataRow("A:MSF", AccessoryState.MistCoolant | AccessoryState.SpindleCW | AccessoryState.FloodCoolant)]
        public void AccessoryTest(string accessoryInput, AccessoryState expectedAccessoryFlags)
        {
            var line = $"<Hold|WPos:-2.500,0.000,11.000|FS:0.0,0|{accessoryInput}|WCO:0.000,1.551,5.664>";

            var msg = GrblMessage.Parse(line);
            Assert.IsNotNull(msg);
            Assert.IsInstanceOfType(msg, typeof(StatusReportMessage));

            var statusMsg = msg as StatusReportMessage;

            Assert.AreEqual(statusMsg.AccessoryState, expectedAccessoryFlags);
        }
    }
}
