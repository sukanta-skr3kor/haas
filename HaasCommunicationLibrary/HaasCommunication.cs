namespace HaasCommunicationLibrary
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;

    /// <summary>
    /// Handles communication with the Haas CNC machine
    /// </summary>
    public class HaasCommunication
    {
        /// <summary>
        /// Defines the _serialPort
        /// </summary>
        private readonly SerialPort _serialPort;

        /// <summary>
        /// Defines the NO_DATA
        /// </summary>
        private const string NO_DATA = "NO_DATA";

        /// <summary>
        /// Initializes a new instance of the <see cref="HaasCommunication"/> class.
        /// </summary>
        /// <param name="comPort">The comPort<see cref="string"/></param>
        /// <param name="baudRate">The baudRate<see cref="int"/></param>
        /// <param name="readTimeout">The readTimeout<see cref="int"/></param>
        /// <param name="dataBits">The dataBits<see cref="int"/></param>
        public HaasCommunication(string comPort = "COM1", int baudRate = 9600, int readTimeout = 2000, int dataBits = 7)
        {
            _serialPort = new SerialPort(comPort)
            {
                BaudRate = baudRate,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = dataBits,
                ReadTimeout = readTimeout,
                WriteTimeout = 1000
            };
        }

        /// <summary>
        /// The SendCommand
        /// </summary>
        /// <param name="command">The command<see cref="string"/></param>
        /// <returns>The <see cref="string[]"/></returns>
        private string[] SendCommand(string command)
        {
            return SerialCommunication.SendHaasCommand(command, _serialPort) ?? Array.Empty<string>();
        }

        /// <summary>
        /// The MachineStatus
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string MachineStatus()
        {
            return SendCommand("Q100").Length > 1 ? "Online" : "Offline";
        }

        /// <summary>
        /// The MachineVariableData
        /// </summary>
        /// <param name="variable">The variable<see cref="int"/></param>
        /// <returns>The <see cref="string"/></returns>
        public string MachineVariableData(int variable)
        {
            string[] response = SendCommand($"Q600 {variable}");
            return response.Length > 2 && response[1] == variable.ToString() ? response[2] : string.Empty;
        }

        /// <summary>
        /// The MachineMode
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string MachineMode()
        {
            string[] response = SendCommand("Q104");
            if (response.Length > 1)
            {
                return response[1].ToUpperInvariant() switch
                {
                    "(MDI)" => "MANUAL_DATA_INPUT",
                    "(JOG)" => "MANUAL",
                    "(ZERO RET)" => "AUTOMATIC",
                    _ => "AUTOMATIC"
                };
            }
            return NO_DATA;
        }

        /// <summary>
        /// The MachineProgrameStatus
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string MachineProgrameStatus()
        {
            string[] response = SendCommand("Q500");

            if (response.Length > 1 && response[0] == "PROGRAM")
            {
                return response[1] != "MDI"
                    ? response[1]
                    : response[2] switch
                    {
                        "IDLE" => "READY",
                        "FEED HOLD" => "INTERRUPTED",
                        "ALARM ON" => "STOPPED",
                        _ => "ARMED"
                    };
            }
            return NO_DATA;
        }

        /// <summary>
        /// The AxisActualPositions
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, double}"/></returns>
        public Dictionary<string, double> AxisActualPositions()
        {
            return new Dictionary<string, double>
            {
                { "X", double.TryParse(MachineVariableData(5041), out double x) ? x : 0 },
                { "Y", double.TryParse(MachineVariableData(5042), out double y) ? y : 0 },
                { "Z", double.TryParse(MachineVariableData(5043), out double z) ? z : 0 }
            };
        }

        /// <summary>
        /// The SpindleSpeed
        /// </summary>
        /// <returns>The <see cref="double"/></returns>
        public double SpindleSpeed()
        {
            return double.TryParse(MachineVariableData(3027), out double speed) ? speed : -1;
        }

        /// <summary>
        /// The TotalNumberOfToolChanges
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public int TotalNumberOfToolChanges()
        {
            string[] response = SendCommand("Q200");
            return response.Length > 1 && response[0] == "TOOL CHANGES" && int.TryParse(response[1], out int count) ? count : -1;
        }

        /// <summary>
        /// The CurrentToolNumberInUse
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public int CurrentToolNumberInUse()
        {
            string[] response = SendCommand("Q201");
            return response.Length > 1 && response[0] == "USING TOOL" && int.TryParse(response[1], out int toolNumber) ? toolNumber : -1;
        }

        /// <summary>
        /// The PowerOnTime
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string PowerOnTime() => SendCommand("Q300").Length > 1 ? SendCommand("Q300")[1] : NO_DATA;

        /// <summary>
        /// The MotionTime
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string MotionTime() => SendCommand("Q301").Length > 1 ? SendCommand("Q301")[1] : NO_DATA;

        /// <summary>
        /// The LastCycleTime
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string LastCycleTime() => SendCommand("Q303").Length > 1 ? SendCommand("Q303")[1] : NO_DATA;

        /// <summary>
        /// The PreviousCycleTime
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string PreviousCycleTime() => SendCommand("Q304").Length > 1 ? SendCommand("Q304")[1] : NO_DATA;

        /// <summary>
        /// The TotalPartCount
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public int TotalPartCount()
        {
            int totalCount = 0;

            string[] response1 = SendCommand("Q402");
            if (response1.Length > 1 && response1[0] == "M30 #1" && int.TryParse(response1[1], out int count1))
            {
                totalCount = count1;
            }

            string[] response2 = SendCommand("Q403");
            if (response2.Length > 1 && response2[0] == "M30 #2" && int.TryParse(response2[1], out int count2))
            {
                totalCount += count2;
            }

            return totalCount;
        }

        /// <summary>
        /// The GetAllMachineData
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string GetAllMachineData()
        {
            var machineData = new HaasMachineData();
            try
            {
                machineData.MachineStatus = MachineStatus();

                if (!machineData.MachineStatus.Equals("offline", StringComparison.OrdinalIgnoreCase))
                {
                    machineData.PowerOnTime = PowerOnTime();
                    machineData.MachineMode = MachineMode();
                    machineData.MachineProgramStatus = MachineProgrameStatus();
                    machineData.TotalPartCount = TotalPartCount();
                    machineData.PreviousCycleTime = PreviousCycleTime();
                    machineData.LastCycleTime = LastCycleTime();
                    machineData.MotionTime = MotionTime();
                    machineData.CurrentToolNumberInUse = CurrentToolNumberInUse();
                    machineData.TotalNumberOfToolChanges = TotalNumberOfToolChanges();
                    machineData.SpindleSpeed = SpindleSpeed();
                    machineData.AxisActualPositions = AxisActualPositions();
                }

                return JsonConvert.SerializeObject(machineData, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving machine data: {ex.Message}");
                return "{}";
            }
        }
    }
}
