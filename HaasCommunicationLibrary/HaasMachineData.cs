namespace HaasCommunicationLibrary
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="HaasMachineData" />
    /// </summary>
    public class HaasMachineData
    {
        /// <summary>
        /// Gets or sets the MachineStatus
        /// </summary>
        public string MachineStatus { get; set; }

        /// <summary>
        /// Gets or sets the PowerOnTime
        /// </summary>
        public string PowerOnTime { get; set; }

        /// <summary>
        /// Gets or sets the MachineMode
        /// </summary>
        public string MachineMode { get; set; }

        /// <summary>
        /// Gets or sets the MachineProgramStatus
        /// </summary>
        public string MachineProgramStatus { get; set; }

        /// <summary>
        /// Gets or sets the TotalPartCount
        /// </summary>
        public int TotalPartCount { get; set; }

        /// <summary>
        /// Gets or sets the PreviousCycleTime
        /// </summary>
        public string PreviousCycleTime { get; set; }

        /// <summary>
        /// Gets or sets the LastCycleTime
        /// </summary>
        public string LastCycleTime { get; set; }

        /// <summary>
        /// Gets or sets the MotionTime
        /// </summary>
        public string MotionTime { get; set; }

        /// <summary>
        /// Gets or sets the CurrentToolNumberInUse
        /// </summary>
        public int CurrentToolNumberInUse { get; set; }

        /// <summary>
        /// Gets or sets the TotalNumberOfToolChanges
        /// </summary>
        public int TotalNumberOfToolChanges { get; set; }

        /// <summary>
        /// Gets or sets the SpindleSpeed
        /// </summary>
        public double SpindleSpeed { get; set; }

        /// <summary>
        /// Gets or sets the AxisActualPositions
        /// </summary>
        public Dictionary<string, double> AxisActualPositions { get; set; }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return $"Machine Status: {MachineStatus}, Power On: {PowerOnTime}, Mode: {MachineMode}, Program Status: {MachineProgramStatus}, Parts: {TotalPartCount}, Spindle: {SpindleSpeed} RPM";
        }
    }
}
