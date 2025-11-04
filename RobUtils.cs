// Written by Robert Hill 
// 10/2025
// This library provides functionality for a tool that automates staging POS terminals

using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace ROB.Utils
{
    public class RobUtils
    {
        const string FILE_PATH = @"<enter root path>";
        const string DEVICE_TYPE_FILE = @"<enter directory\name.dat>";
        const string STORE_SETTINGS_FILE = @"<enter directory\name.dat>";
        const string LOG_FILE = @"<enter directory\name.log>";
        const string CONFIG_FILE = @"<enter directory\name.ini>";


        public static void CreateFileDir()
        {
            //Create the directories if they do not exist
            try
            {
                Directory.CreateDirectory(FILE_PATH + @"\Files");
                Directory.CreateDirectory(FILE_PATH + @"\Data");
                Directory.CreateDirectory(@"C:\Install");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void WriteToLog(string msg)
        {
            string timeStamp = DateTime.Now.ToString();
            try
            {
                using (StreamWriter sw = File.AppendText(FILE_PATH + LOG_FILE))
                {
                    sw.WriteLine(timeStamp + " " + msg);
                    sw.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static string[] ReadDATFile(string fPath)
        {
            try
            {
                string[] datFile = File.ReadAllLines(fPath);
                return datFile;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static string[] ReadDeviceTypeFile()
        {
            try
            {
                string fpath = FILE_PATH + DEVICE_TYPE_FILE;
                string[] dt_File = ReadDATFile(fpath);
                return dt_File;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static string[] ReadStoreSettingsFile()
        {
            try
            {
                string fpath = FILE_PATH + STORE_SETTINGS_FILE;
                string[] ss_File = ReadDATFile(fpath);
                return ss_File;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string GetENVVAR(string EVName)
        {
            try
            {
                return Environment.GetEnvironmentVariable(EVName, EnvironmentVariableTarget.User);

            }
            catch (Exception)
            {

                throw;
            }

        }

        public static void SetENVVAR(string EVNAME, string value)
        {
            try
            {
				//writing directly to the REG is much faster
                string keyPath = @"HKEY_CURRENT_USER\Environment";
                Microsoft.Win32.Registry.SetValue(keyPath, EVNAME, value);
                //Environment.SetEnvironmentVariable(EVNAME, value,EnvironmentVariableTarget.User);
                WriteToLog("ENV VAR set successful");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void DeleteENVVARS(Dictionary<string, string> ENVVARS)
        {
            try
            {
                foreach (var v in ENVVARS)
                {
                    Environment.SetEnvironmentVariable(v.Key, null, EnvironmentVariableTarget.User);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void GetDATFilesFromNetwork()
        {
            try
            {
                // Get UNC path fron config file
                var iniFile = new INIFile(FILE_PATH + CONFIG_FILE);
                string UNCPath = iniFile.IniReadValue("FILESHARE", "UNC") + @"\DATA";
                // Loop through and copy each dat file from network share to local
                string[] datFiles = Directory.GetFiles(UNCPath);
                foreach (string file in datFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string destination = FILE_PATH + @"\Data\" + fileName;
                    File.Copy(file, destination, true);
                }
                WriteToLog("dat files copied from network.");

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void GetInstallFilesFromNetwork(string deviceType = "")
        {
            try
            {
                var iniFile = new INIFile(FILE_PATH + CONFIG_FILE);
                string UNCPath = iniFile.IniReadValue("FILESHARE", "UNC") + @"\INSTALL\" + deviceType;
                WriteToLog("Getting install files from network.");
                using (Process p = new Process())
                {
                    p.StartInfo.Arguments = string.Format("/C ROBOCOPY /E /Z {0} {1}", UNCPath, @"C:\Install");
                    p.StartInfo.FileName = "CMD.EXE";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    p.WaitForExit();
                }
                WriteToLog("File transfer complete");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string GetPOSTypes()
        {
            try
            {
                var iniFile = new INIFile(FILE_PATH + CONFIG_FILE);
                string types = iniFile.IniReadValue("POSTYPES", "POSTYPES");
                return types;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static void WriteStagingStepsData(string key, string data)
        {
            try
            {
                var iniFile = new INIFile(FILE_PATH + CONFIG_FILE);
                iniFile.IniWriteValue("STAGINGSTEPS", key, data);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string ReadStagingStepsData(string key)
        {
            try
            {
                var iniFile = new INIFile(FILE_PATH + CONFIG_FILE);
                string value = iniFile.IniReadValue("STAGINGSTEPS", key);
                return value;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string GetDeviceNums(string POSType)
        {
            try
            {
                var iniFile = new INIFile(FILE_PATH + CONFIG_FILE);
                string DNums = iniFile.IniReadValue("DEVICENUMS", POSType);
                return DNums;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string GetIP(string devType, string devNum)
        {
            try
            {
                var iniFile = new INIFile(FILE_PATH + CONFIG_FILE);
                string IPNum = iniFile.IniReadValue("IPADDRESS", devType + devNum);
                return IPNum;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void RenameComputer(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Computer name cannot be empty.");

            // Ensure valid NetBIOS name (max 15 chars, no spaces)
            if (newName.Length > 15 || newName.Contains(" "))
                throw new ArgumentException("Computer name must be 15 characters or fewer and contain no spaces.");

            try
            {
                using (var managementClass = new ManagementObject($"Win32_ComputerSystem.Name='{Environment.MachineName}'"))
                {
                    ManagementBaseObject inputArgs = managementClass.GetMethodParameters("Rename");
                    inputArgs["Name"] = newName;

                    // Invoke the rename method with the necessary credentials (null = current user)
                    ManagementBaseObject output = managementClass.InvokeMethod("Rename", inputArgs, null);

                    uint returnValue = (uint)(output["ReturnValue"] ?? 1);
                    if (returnValue == 0)
                    {
                        WriteToLog($"Computer name successfully changed to '{newName}'. Reboot required for changes to apply.");
                    }
                    else
                    {
                        throw new Exception($"Rename failed with error code: {returnValue}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog($"Error renaming computer: {ex.Message}");
            }
        }

        public static void SetComputerDescription(string description)
        {
            try
            {
                WriteToLog($"Setting computer description to {description}");
                string keyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters";
                string key = "srvcomment";
                Microsoft.Win32.Registry.SetValue(keyPath, key, description);
                WriteToLog("Description set successfully.");
            }
            catch (Exception)
            {

                throw;
            }
        }


        public static void ConfigureNetwork(string ipAddress, string subnetMask, string gateway, string dns1, string dns2)
        {
           try
           {
               // Get all enabled network adapters that have IP enabled
               var query = new SelectQuery("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE");
               using (var searcher = new ManagementObjectSearcher(query))
               {
                   var adapters = searcher.Get().Cast<ManagementObject>().ToList();

                   if (adapters.Count == 0)
                   {
                       WriteToLog("No active network adapters found.");
                       return;
                   }

                   // Choose the first active adapter
                   var adapter = adapters.First();

                   WriteToLog($"Configuring adapter: {adapter["Description"]} with \nIP address:{ipAddress} \nsubnetmask:{subnetMask} \ngateway:{gateway} \nDNS:{dns1} & {dns2}");

                   // Set IP address and subnet mask
                   using (var setIP = adapter.GetMethodParameters("EnableStatic"))
                   {
                       setIP["IPAddress"] = new string[] { ipAddress };
                       setIP["SubnetMask"] = new string[] { subnetMask };
                       adapter.InvokeMethod("EnableStatic", setIP, null);
                   }

                   // Set default gateway
                   using (var setGateway = adapter.GetMethodParameters("SetGateways"))
                   {
                       setGateway["DefaultIPGateway"] = new string[] { gateway };
                       setGateway["GatewayCostMetric"] = new int[] { 1 };
                       adapter.InvokeMethod("SetGateways", setGateway, null);
                   }

                   // Set DNS servers
                   using (var setDNS = adapter.GetMethodParameters("SetDNSServerSearchOrder"))
                   {
                       setDNS["DNSServerSearchOrder"] = new string[] { dns1, dns2 };
                       adapter.InvokeMethod("SetDNSServerSearchOrder", setDNS, null);
                   }

                   WriteToLog("Network configuration applied successfully.");
               }
           }
           catch (Exception ex)
           {
               WriteToLog($"Error: {ex.Message}");
           }
        }

        public static void CreateStartupTask(string taskName, string exePath)
        {
            try
            {
                // Build the schtasks command
                string arguments = $"/Create /TN \"{taskName}\" " +
                                   $"/TR \"\\\"{exePath}\\\"\" " +
                                   "/SC ONLOGON " +           // Trigger: on any user logon
                                   "/RL HIGHEST " +           // Run with highest privileges
                                   "/F";                      // Force overwrite if exists

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        WriteToLog("✅ Task created successfully!");
                        WriteToLog(output);
                    }
                    else
                    {
                        WriteToLog("❌ Failed to create task:");
                        WriteToLog(error);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void DeleteTask(string taskName)
        {
            try
            {
                string arguments = $"/Delete /TN \"{taskName}\" /F"; // /F = force deletion without prompt

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"✅ Task \"{taskName}\" deleted successfully.");
                        Console.WriteLine(output);
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to delete task \"{taskName}\":");
                        Console.WriteLine(error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex.Message);
            }
        }

        public static void RebootMachine()
        {
            try
            {
                WriteToLog("Rebooting machine");
                System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            }
            catch (Exception)
            {

                throw;
            }
        }

        //set time zone
        public static void SetSystemTimeZone(string timeZoneId)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "tzutil.exe",
                    Arguments = "/s \"" + timeZoneId + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process != null)
                {
                    process.WaitForExit();
                    TimeZoneInfo.ClearCachedData();
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        //convert time zone abbreviation to string
        public static string ReadTimeZone(string timeZoneId, string dst)
        {
            try
            {
                var iniFile = new INIFile(FILE_PATH + CONFIG_FILE);
                string timeZoneString = iniFile.IniReadValue("TIMEZONES", timeZoneId);
                if (dst == "false") { timeZoneString += "_dstoff"; }
                return timeZoneString;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string RunProcess(string filePath, string arguments = "", bool hideWindow = false)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = hideWindow,
                    WindowStyle = hideWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                };

                var process = new Process { StartInfo = processStartInfo };

                StringBuilder outputBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine($"ERROR: {e.Data}");
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return outputBuilder.ToString().Trim();
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        public static string RunBATorCMD(string filePath, string arguments = "")
        {
            try
            {
                string returnVal = "Process completed";
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = arguments,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                };

                var process = new Process { StartInfo = processStartInfo };

                StringBuilder outputBuilder = new StringBuilder();

                process.Start();

                process.WaitForExit();


                return returnVal;
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        public static string RunCommand(string cmd, string arguments = "")
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + cmd + " " + arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                };


                var process = new Process { StartInfo = processStartInfo };

                StringBuilder outputBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine($"ERROR: {e.Data}");
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return outputBuilder.ToString().Trim();
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

    }

}