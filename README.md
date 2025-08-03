# WindowsFormsApp1

A Windows Forms application for automated UDP-based device testing and configuration on a local network. The app communicates with hardware modules using a series of custom UDP commands, providing a step-by-step workflow for device discovery, LED validation, MAC address retrieval, pin configuration, and error logging.

## Features

- **UDP Device Discovery:** Broadcasts and receives responses from multiple devices on the network.
- **Automated Test Workflow:** Guides the user through LED validation, MAC address retrieval, pin counting, and pin learning.
- **Error Handling & Logging:** Detects connection issues, short circuits, and device errors, logging them to a file.
- **User Interaction:** Provides UI prompts for manual validation steps (e.g., LED check).
- **Extensible Command Set:** Uses a centralized command dictionary for easy protocol updates.

## Technologies

- C# (.NET Framework 4.7.2)
- Windows Forms (WinForms)
- UDP Networking

## Usage

1. Build and run the application in Visual Studio 2022.
2. Click the "Start Testing" button to begin the automated device test sequence.
3. Follow on-screen prompts to validate device responses and handle errors as needed.
4. Review `error_log.txt` for any issues encountered during testing.

## Project Structure

- `Form1.cs` – Main UI and test workflow logic.
- `UdpService.cs` – UDP communication and device protocol handling.
- `DeviceCommand.cs` – Centralized command definitions (not shown here).
- `Properties/AssemblyInfo.cs` – Assembly metadata.

