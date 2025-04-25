from pyLSV2 import LSV2
import logging

# logging
logging.basicConfig(level=logging.DEBUG)

def main():
    # CNC IP and port
    controller_ip = '10.5.8.20'
    controller_port = 19000

    # Create the connection
    cnc = LSV2(controller_ip, controller_port)

    try:
        # Connect
        if cnc.connect():
            print("Connected to Heidenhain CNC.")

            # Get controller info
            sys_info = cnc.get_sysinfo()
            print(f"Controller Info:\n{sys_info}")

            axis_positions = cnc.get_position()
            print(f"Axis Positions: {axis_positions}")

        else:
            print("Failed to connect to CNC.")

    except Exception as e:
        print(f"Error: {e}")

    finally:
        cnc.disconnect()
        print("Disconnected.")

if __name__ == '__main__':
    main()
