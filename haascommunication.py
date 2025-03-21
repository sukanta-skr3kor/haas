import json
import serial
import time


class HaasMachineData:
    def __init__(self):
        self.MachineStatus = "NA"
        self.PowerOnTime = "NA"
        self.MachineMode = "NA"
        self.MachineProgramStatus = "NA"
        self.TotalPartCount = 0
        self.PreviousCycleTime = "NA"
        self.LastCycleTime = "NA"
        self.MotionTime = "NA"
        self.CurrentToolNumberInUse = -1
        self.TotalNumberOfToolChanges = -1
        self.SpindleSpeed = -1.0
        self.AxisActualPositions = {}

    def __str__(self):
        return (f"Machine Status: {self.MachineStatus}, Power On: {self.PowerOnTime}, "
                f"Mode: {self.MachineMode}, Program Status: {self.MachineProgramStatus}, "
                f"Parts: {self.TotalPartCount}, Spindle: {self.SpindleSpeed} RPM")


def send_haas_command(command, port):
    try:
        if not port.is_open:
            port.open()

        port.write((command + "\r\n").encode())
        time.sleep(1)

        response = port.read_all().decode(errors="ignore")
        values = [v.strip() for v in response.replace(',', ' ').split() if v.strip()]
        return values

    except serial.SerialTimeoutException as ex:
        print(f"Timeout: {ex}")
        return []
    except Exception as ex:
        print(f"Error: {ex}")
        raise
    finally:
        if port.is_open:
            port.close()


class HaasDataCollector:
    NO_DATA = "NA"

    def __init__(self, com_port="COM1", baudrate=9600, timeout=2, databits=7):
        self.port = serial.Serial(
            port=com_port,
            baudrate=baudrate,
            bytesize=databits,
            stopbits=serial.STOPBITS_ONE,
            parity=serial.PARITY_NONE,
            timeout=timeout,
            write_timeout=0.5
        )

    def send_command(self, command):
        return send_haas_command(command, self.port) or []

    def machine_status(self):
        return "Online" if len(self.send_command("Q100")) > 1 else "Offline"

    def machine_variable_data(self, variable):
        response = self.send_command(f"Q600 {variable}")
        return response[2] if len(response) > 2 and response[1] == str(variable) else ""

    def machine_mode(self):
        response = self.send_command("Q104")
        if len(response) > 1:
            mode = response[1].upper()
            return {
                "(MDI)": "MANUAL_DATA_INPUT",
                "(JOG)": "MANUAL",
                "(ZERO RET)": "AUTOMATIC"
            }.get(mode, "AUTOMATIC")
        return self.NO_DATA

    def machine_program_status(self):
        response = self.send_command("Q500")
        if len(response) > 1 and response[0] == "PROGRAM":
            if response[1] != "MDI":
                return response[1]
            return {
                "IDLE": "READY",
                "FEED HOLD": "INTERRUPTED",
                "ALARM ON": "STOPPED"
            }.get(response[2], "ARMED")
        return self.NO_DATA

    def axis_actual_positions(self):
        return {
            "X": float(self.machine_variable_data(5041) or 0),
            "Y": float(self.machine_variable_data(5042) or 0),
            "Z": float(self.machine_variable_data(5043) or 0)
        }

    def spindle_speed(self):
        try:
            return float(self.machine_variable_data(3027))
        except ValueError:
            return -1.0

    def total_tool_changes(self):
        response = self.send_command("Q200")
        return int(response[1]) if len(response) > 1 and response[0] == "TOOL CHANGES" and response[1].isdigit() else -1

    def current_tool_number(self):
        response = self.send_command("Q201")
        return int(response[1]) if len(response) > 1 and response[0] == "USING TOOL" and response[1].isdigit() else -1

    def power_on_time(self):
        response = self.send_command("Q300")
        return response[1] if len(response) > 1 else self.NO_DATA

    def motion_time(self):
        response = self.send_command("Q301")
        return response[1] if len(response) > 1 else self.NO_DATA

    def last_cycle_time(self):
        response = self.send_command("Q303")
        return response[1] if len(response) > 1 else self.NO_DATA

    def previous_cycle_time(self):
        response = self.send_command("Q304")
        return response[1] if len(response) > 1 else self.NO_DATA

    def total_part_count(self):
        count = 0
        r1 = self.send_command("Q402")
        if len(r1) > 1 and r1[0] == "M30 #1" and r1[1].isdigit():
            count += int(r1[1])
        r2 = self.send_command("Q403")
        if len(r2) > 1 and r2[0] == "M30 #2" and r2[1].isdigit():
            count += int(r2[1])
        return count

    def get_all_machine_data(self):
        data = HaasMachineData()
        try:
            data.MachineStatus = self.machine_status()

            if data.MachineStatus.lower() != "offline":
                data.PowerOnTime = self.power_on_time()
                data.MachineMode = self.machine_mode()
                data.MachineProgramStatus = self.machine_program_status()
                data.TotalPartCount = self.total_part_count()
                data.PreviousCycleTime = self.previous_cycle_time()
                data.LastCycleTime = self.last_cycle_time()
                data.MotionTime = self.motion_time()
                data.CurrentToolNumberInUse = self.current_tool_number()
                data.TotalNumberOfToolChanges = self.total_tool_changes()
                data.SpindleSpeed = self.spindle_speed()
                data.AxisActualPositions = self.axis_actual_positions()

            return json.dumps(data.__dict__, indent=4)
        except Exception as ex:
            print(f"Error retrieving machine data: {ex}")
            return "{}"


# 🔽 Entry point
if __name__ == "__main__":
    collector = HaasDataCollector(com_port="COM1")  # Change to the correct COM port if needed
    status = collector.machine_status()
    print(f"Machine Status: {status}")
