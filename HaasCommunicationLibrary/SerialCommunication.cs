namespace HaasCommunicationLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Defines the <see cref="SerialCommunication" />
    /// </summary>
    internal static class SerialCommunication
    {
        /// <summary>
        /// The SendHaasCommand
        /// </summary>
        /// <param name="command">The cmd<see cref="string"/></param>
        /// <param name="port">The port<see cref="string"/></param>
        /// <returns>The <see cref="string[]"/></returns>
        public static string[] SendHaasCommand(string command, SerialPort port)
        {
            if (port == null)
            {
                throw new ArgumentNullException(nameof(port), "Serial port cannot be null.");
            }

            List<string> listOfData = new List<string>();

            try
            {
                // Open serial port connection
                if (!port.IsOpen)
                {
                    port.Open();
                }

                // Write the command and newline to serial
                port.Write(command + "\r\n");

                Thread.Sleep(1000);//delay for 1sec

                // Read response
                string response = port.ReadExisting();

                // Split string and return array of response values
                // Process response
                string[] values = response.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (var index = 1; index <= values.Length - 1; index++)
                {
                    string value = values[index].Trim();

                    if (!string.IsNullOrEmpty(value))
                    {
                        listOfData.Add(value);
                    }
                }
                // Return the trimmed values
                return values.Select(x => x.Trim()).ToArray();
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"Timeout occurred while communicating with Haas machine: {ex.Message}");
                return Array.Empty<string>(); // Return empty array on timeout
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in serial communication: {ex.Message}");
                throw;
            }
            finally
            {
                // Close serial connection
                if (port.IsOpen)
                {
                    port.Close();
                }
            }
        }
    }
}
