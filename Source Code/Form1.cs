using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Beacon
{
    public partial class Form1 : Form
    {
        public static string version = "1.0.4";
        private HashSet<string> existingIPAddresses = new HashSet<string>();
        private HashSet<string> existingPNames = new HashSet<string>();


        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out IntPtr lpNumberOfBytesRead);

        private const uint PROCESS_VM_READ = 0x0010;
        private System.Windows.Forms.Timer memoryTimer;
        private System.Windows.Forms.Timer chatTimer;
        private string connectionString = "server=eu02-sql.pebblehost.com;uid=customer_614448_main;pwd=Wy9P#W3aO5yAbwYQVw@f;database=customer_614448_main";

        public Form1()
        {
            InitializeComponent();
            memoryTimer = new System.Windows.Forms.Timer { Interval = 10000 };
            memoryTimer.Tick += async (sender, e) => await MemoryTimer_TickAsync(sender, e);
            chatTimer = new System.Windows.Forms.Timer { Interval = 50 };
            chatTimer.Tick += async (sender, e) => await ChatTimer_TickAsync(sender, e);
            Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            ListAllProcesses();
            await StartMemoryMonitoringAsync();
            await LoadDataIntoDataGridViewAsync();
            this.Text = "Beacon v" + version;
        }

        private HashSet<string> existingIPs = new HashSet<string>();

        private async Task LoadDataIntoDataGridViewAsync()
        {
            try
            {
                int firstDisplayedScrollingRowIndex = gameList.FirstDisplayedScrollingRowIndex;
                DataGridViewRow currentRow = gameList.CurrentRow;

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT ipaddr, charname, gamediff, playercount, lobbycount, hardcore, modname FROM tcpgames";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    var dataTable = new System.Data.DataTable();
                    await Task.Run(() => adapter.Fill(dataTable));
                    gameList.DataSource = dataTable;
                    existingIPAddresses.Clear();

                    foreach (DataGridViewRow row in gameList.Rows)
                    {
                        if (row.Cells["ipaddr"].Value != null)
                            existingIPAddresses.Add(row.Cells["ipaddr"].Value.ToString());
                        if (row.Cells["charname"].Value != null)
                            existingPNames.Add(row.Cells["charname"].Value.ToString());
                    }

                    // Change header names
                    gameList.Columns["ipaddr"].HeaderText = "   IP";
                    gameList.Columns["charname"].HeaderText = "   Host Name";
                    gameList.Columns["gamediff"].HeaderText = "   Difficulty";
                    gameList.Columns["playercount"].HeaderText = "Players";
                    gameList.Columns["lobbycount"].HeaderText = "Slots";
                    gameList.Columns["hardcore"].HeaderText = "   Mode";
                    gameList.Columns["modname"].HeaderText = "    Mod Name";

                    // Add " / 8" to the lobbycount column in the DataTable
                    foreach (DataRow row in dataTable.Rows)
                    {
                        row["lobbycount"] = row["lobbycount"].ToString() + " / 8"; // Append " / 8"
                    }

                    // Set individual column widths
                    gameList.Columns["ipaddr"].Width = 120;
                    gameList.Columns["charname"].Width = 130;
                    gameList.Columns["gamediff"].Width = 100;
                    gameList.Columns["playercount"].Width = 80;
                    gameList.Columns["lobbycount"].Width = 60; // This will be adjusted in the next step
                    gameList.Columns["hardcore"].Width = 100;
                    gameList.Columns["modname"].Width = 140;
                    gameList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                    // Set header styles
                    gameList.EnableHeadersVisualStyles = false;
                    gameList.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkOrange;
                    gameList.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                    gameList.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
                    gameList.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    gameList.ColumnHeadersDefaultCellStyle.Padding = new Padding(0);
                    gameList.ColumnHeadersHeight = 30;

                    // Set cell styles
                    gameList.DefaultCellStyle.Font = new Font("Arial", 10);
                    gameList.BackgroundColor = Color.Black;
                    gameList.ForeColor = Color.WhiteSmoke;
                    gameList.RowsDefaultCellStyle.BackColor = Color.Black;
                    gameList.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(23, 23, 23);
                    gameList.CellBorderStyle = DataGridViewCellBorderStyle.None;

                    // Set row header styles to match background
                    gameList.RowHeadersDefaultCellStyle.BackColor = Color.Black;
                    gameList.RowHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke;

                    foreach (DataGridViewColumn column in gameList.Columns)
                    {
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.HeaderCell.Style.Padding = new Padding(0);

                        if (column.Name != "ipaddr")
                        {
                            column.ReadOnly = true;
                            column.DefaultCellStyle.SelectionBackColor = gameList.DefaultCellStyle.BackColor;
                            column.DefaultCellStyle.SelectionForeColor = gameList.DefaultCellStyle.ForeColor;
                        }
                    }

                    // Hide the scrollbars
                    gameList.ScrollBars = ScrollBars.None;
                    gameList.MouseWheel += gameList_MouseWheel;

                    // Additional settings
                    gameList.EditMode = DataGridViewEditMode.EditProgrammatically;

                    if (gameList.CurrentCell != null)
                        gameList.CurrentCell.Selected = false;

                    gameList.AllowUserToAddRows = false;
                    gameList.RowHeadersVisible = false;

                    gameList.CellClick += gameList_CellClick;
                    gameList.Refresh();

                    if (firstDisplayedScrollingRowIndex >= 0 && firstDisplayedScrollingRowIndex < gameList.Rows.Count)
                        gameList.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;

                    if (currentRow != null)
                        gameList.CurrentCell = currentRow.Cells[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading data: " + ex.Message);
            }
        }

        private void gameList_MouseWheel(object sender, MouseEventArgs e)
        {
            int currentIndex = gameList.FirstDisplayedScrollingRowIndex;
            int scrollRows = -(e.Delta / SystemInformation.MouseWheelScrollDelta);
            int newIndex = Math.Max(0, Math.Min(gameList.Rows.Count - 1, currentIndex + scrollRows));

            gameList.FirstDisplayedScrollingRowIndex = newIndex;
        }

        private void gameList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (gameList.Columns[e.ColumnIndex].Name == "ipaddr")
                {
                    string ipAddress = gameList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

                    if (!string.IsNullOrEmpty(ipAddress))
                        Clipboard.SetText(ipAddress);
                }
            }
        }

        private async Task StartMemoryMonitoringAsync()
        {
            Process process = FindProcessByWindowTitle("Diablo II: Resurrected");
            if (process == null)
            {
                ShowError("Process not found. Please start the game first.");
                return;
            }

            IntPtr processHandle = OpenProcess(PROCESS_VM_READ, false, process.Id);
            if (processHandle == IntPtr.Zero)
            {
                ShowError($"Failed to open process. Error code: {Marshal.GetLastWin32Error()}");
                return;
            }

            IntPtr baseAddress = GetBaseAddress(process);
            if (baseAddress == IntPtr.Zero)
            {
                ShowError("Base address not found.");
                CloseHandle(processHandle);
                return;
            }

            chatTimer.Start();
            await ChatTimer_TickAsync(null, EventArgs.Empty);
            memoryTimer.Start();
            await MemoryTimer_TickAsync(null, EventArgs.Empty);
            
            CloseHandle(processHandle);
        }

        private async Task MemoryTimer_TickAsync(object sender, EventArgs e)
        {
            string localIP = GetLocalIPAddress();
            labelTCPAddress.Text = localIP;

            Process process = FindProcessByWindowTitle("Diablo II: Resurrected");
            if (process == null)
            {
                this.Close();
                //StopMemoryTimerWithError("Process not found.");
                return;
            }

            IntPtr processHandle = OpenProcess(PROCESS_VM_READ, false, process.Id);
            if (processHandle == IntPtr.Zero)
            {
                ShowErrorWithHandleClose(processHandle, "Failed to open process.");
                return;
            }

            IntPtr baseAddress = GetBaseAddress(process);
            if (baseAddress == IntPtr.Zero)
            {
                ShowErrorWithHandleClose(processHandle, "Base address not found.");
                return;
            }

            var memoryData = await Task.Run(() => ReadMemoryData(processHandle, baseAddress));
            var chatData = await Task.Run(() => ReadChatData(processHandle, baseAddress));
            UpdateUI(memoryData, chatData);
            await Task.Run(() => ExecuteSQLUpdateAsync(localIP, memoryData));
            await LoadDataIntoDataGridViewAsync();
            CloseHandle(processHandle);
        }

        private async Task ChatTimer_TickAsync(object sender, EventArgs e)
        {
            Process process = FindProcessByWindowTitle("Diablo II: Resurrected");
            if (process == null)
            {
                this.Close();
                return;
            }

            IntPtr processHandle = OpenProcess(PROCESS_VM_READ, false, process.Id);
            if (processHandle == IntPtr.Zero)
            {
                ShowErrorWithHandleClose(processHandle, "Failed to open process.");
                return;
            }

            IntPtr baseAddress = GetBaseAddress(process);
            if (baseAddress == IntPtr.Zero)
            {
                ShowErrorWithHandleClose(processHandle, "Base address not found.");
                return;
            }

            var chatData = await Task.Run(() => ReadChatData(processHandle, baseAddress));
            UpdateChat(chatData);
            CloseHandle(processHandle);
        }

        private dynamic ReadMemoryData(IntPtr processHandle, IntPtr baseAddress)
        {
            return new
            {
                HostStatus = ReadMemoryAsByte(processHandle, baseAddress, 0x18882E8),
                PlayerCount = ReadMemoryAsByte(processHandle, baseAddress, 0x1d637e4),
                PlayerName = ReadPlayerName(processHandle, baseAddress),
                Difficulty = ReadMemoryAsByte(processHandle, baseAddress, 0x1bf086e),
                LobbyCount = ReadMemoryAsByte(processHandle, baseAddress, 0x1c9be38),
                PlayerMode = ReadMemoryAsByte(processHandle, baseAddress, 0x1888cce),
                ModName = ReadModName(processHandle, baseAddress, 0x1d7f3c5),
            };
        }

        private dynamic ReadChatData(IntPtr processHandle, IntPtr baseAddress)
        {
            return new
            {
                PlayerName = ReadPlayerName(processHandle, baseAddress),
                ClientName = ReadClientName(processHandle, baseAddress),
                ChatMsgHost = ReadModName(processHandle, baseAddress, 0x1deeb53),
                ChatMsgClient = ReadModName(processHandle, baseAddress, 0x18682d9),
                ChatMsgFlag = ReadMemoryAsByte(processHandle, baseAddress, 0x18682d0),

                ChatMsgTime = ReadMsgTime(processHandle, baseAddress)
            };
        }

        private void StopMemoryTimerWithError(string message)
        {
            memoryTimer.Stop();
            chatTimer.Stop();
            ShowError(message);
        }

        private byte ReadMemoryAsByte(IntPtr processHandle, IntPtr baseAddress, int offset)
        {
            IntPtr targetAddress = IntPtr.Add(baseAddress, offset);
            return ReadMemory(processHandle, targetAddress, out byte[] buffer) ? buffer[0] : (byte)0;
        }

        private void UpdateUI(dynamic memoryData, dynamic chatData)
        {
            labelHostStatus.Text = (memoryData.HostStatus == 0x02) ? "Yes" : "No";
            labelPCount.Text = memoryData.PlayerCount.ToString();
            labelLCount.Text = memoryData.LobbyCount.ToString() + " / 8";
            labelPName.Text = memoryData.PlayerName;
            labelModName.Text = memoryData.ModName;

            if (chatData.ChatMsgFlag != 0xD2 && chatData.ChatMsgClient != "")
            {
                string newMessage;

                if (chatData.ChatMsgHost != chatData.ChatMsgClient)
                {
                    newMessage = chatData.ClientName + ": " + chatData.ChatMsgClient;
                }
                else
                {
                    newMessage = chatData.PlayerName + ": " + chatData.ChatMsgClient;
                }

                // Check if the listBox is not empty and if the last message is different
                if (listBox1.Items.Count == 0)
                {
                    listBox1.Items.Add("Test");
                }
                else
                {
                    if (listBox1.Items[listBox1.Items.Count - 1].ToString() != newMessage)
                    {
                        listBox1.Items.Add(newMessage);
                    }
                }
            }



            labelDifficulty.Text = ((int)memoryData.Difficulty) switch
            {
                0 => "Normal",
                1 => "Nightmare",
                2 => "Hell",
                _ => "Unknown"
            };

            labelPMode.Text = ((int)memoryData.PlayerMode) switch
            {
                0 => "Softcore",
                1 => "Hardcore",
                _ => "Unknown"
            };


            byte[] originalArray = chatData.ChatMsgTime;
            byte[] reversedArray = (byte[])originalArray.Clone();

            Array.Reverse(reversedArray);

            string originalHexString = BitConverter.ToString(originalArray).Replace("-", " ");
            //string reversedHexString = BitConverter.ToString(reversedArray).Replace("-", " ").Replace(" ","");
            string reversedHexString = "64939FC3D398";
            long decimalValue = Convert.ToInt64(reversedHexString, 16);
            long decimalEpoch = decimalValue / 1000000000;

            Debug.WriteLine(originalHexString);
            Debug.WriteLine(reversedHexString);
            Debug.WriteLine("Decimal value of reversed array: " + decimalValue);
            Debug.WriteLine("Decimal value of reversed epoch array: " + decimalEpoch);
        }

        private void UpdateChat(dynamic chatData)
        {
            if (chatData.ChatMsgFlag != 0xD2 && chatData.ChatMsgClient != "")
            {
                string newMessage;

                if (chatData.ChatMsgHost != chatData.ChatMsgClient)
                {
                    newMessage = chatData.ClientName + ": " + chatData.ChatMsgClient;
                }
                else
                {
                    newMessage = chatData.PlayerName + ": " + chatData.ChatMsgClient;
                }

                // Check if the listBox is not empty and if the last message is different
                if (listBox1.Items.Count == 0)
                {
                    listBox1.Items.Add("Test");
                }
                else
                {
                    if (listBox1.Items[listBox1.Items.Count - 1].ToString() != newMessage)
                    {
                        listBox1.Items.Add(newMessage);
                    }
                }
            }
        }

        private async Task ExecuteSQLUpdateAsync(string localIP, dynamic memoryData)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    if (memoryData.HostStatus != 0x02)
                    {
                        string deleteQuery = "DELETE FROM tcpgames WHERE ipaddr = @IPAddress";

                        using (MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@IPAddress", localIP);
                            int affectedRows = await deleteCommand.ExecuteNonQueryAsync();

                            if (affectedRows > 0)
                            {
                                existingIPs.Remove(localIP);
                                existingPNames.Remove(memoryData.PlayerName);
                                Invoke((MethodInvoker)delegate
                                {
                                    AddMessageToRichTextBox("Game Removed from Server List", Color.Red);
                                });
                            }
                        }
                    }
                    else
                    {
                        if (!existingPNames.Contains(memoryData.PlayerName) ||
                            (existingPNames.Contains(memoryData.PlayerName) && !existingIPAddresses.Contains(localIP)))
                        {
                            string modName = memoryData.ModName == "TCP" ? "Retail" : memoryData.ModName;
                            string query = @"
                        INSERT INTO tcpgames (ipaddr, hostingnow, charname, gamediff, playercount, lobbycount, hardcore, modname) 
                        VALUES (@IPAddress, @Status, @PName, @GameDiff, @PCount, @LCount, @GameMode, @ModName) 
                        ON DUPLICATE KEY UPDATE 
                            hostingnow = @Status, 
                            charname = @PName, 
                            gamediff = @GameDiff, 
                            playercount = @PCount, 
                            lobbycount = @LCount,
                            hardcore = @GameMode,
                            modname = @ModName;";

                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@IPAddress", localIP);
                                command.Parameters.AddWithValue("@Status", labelHostStatus.Text);
                                command.Parameters.AddWithValue("@PName", memoryData.PlayerName);
                                command.Parameters.AddWithValue("@GameDiff", labelDifficulty.Text);
                                command.Parameters.AddWithValue("@PCount", memoryData.PlayerCount.ToString());
                                command.Parameters.AddWithValue("@LCount", memoryData.LobbyCount.ToString());
                                command.Parameters.AddWithValue("@GameMode", labelPMode.Text);
                                command.Parameters.AddWithValue("@ModName", modName); // Use the modified modName here

                                int rowsAffected = await command.ExecuteNonQueryAsync();

                                if (rowsAffected > 0)
                                {
                                    existingIPs.Add(localIP);
                                    existingPNames.Clear();

                                    Invoke((MethodInvoker)delegate
                                    {
                                        AddMessageToRichTextBox("Game Added to Server List", Color.Green); // Change to Green
                                    });
                                }
                            }
                        }
                    }
                }

                Invoke((MethodInvoker)delegate
                {
                    LoadDataIntoDataGridViewAsync().ConfigureAwait(false);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQL Error: {ex.Message}");
            }
        }


        private void AddMessageToRichTextBox(string message, Color color)
        {
            listLog.SelectionStart = listLog.TextLength;
            listLog.SelectionLength = 0;
            listLog.SelectionColor = color;
            listLog.AppendText(message + Environment.NewLine);
            listLog.SelectionColor = listLog.ForeColor;
            listLog.ScrollToCaret();
        }

        private void ShowErrorWithHandleClose(IntPtr processHandle, string message)
        {
            ShowError(message);
            CloseHandle(processHandle);
        }

        private IntPtr GetBaseAddress(Process process)
        {
            try
            {
                return process.MainModule.BaseAddress;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving base address: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        private bool ReadMemory(IntPtr processHandle, IntPtr address, out byte[] buffer)
        {
            buffer = new byte[1];
            return ReadProcessMemory(processHandle, address, buffer, (uint)buffer.Length, out _);
        }

        private static Process FindProcessByWindowTitle(string windowTitle)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.MainWindowTitle.Equals(windowTitle, StringComparison.OrdinalIgnoreCase))
                        return process;
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }

        private void ListAllProcesses()
        {
            Process[] processes = Process.GetProcesses();
            StringBuilder processList = new StringBuilder("Running Processes:\n");
            foreach (Process proc in processes)
            {
                processList.AppendLine(proc.ProcessName);
            }
        }

        static string GetLocalIPAddress(string adapterName = "Radmin VPN")
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.Name == adapterName && ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = ni.GetIPProperties();

                    foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }

            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }

            throw new Exception("No IPv4 address found.");
        }

        private string ReadPlayerName(IntPtr processHandle, IntPtr baseAddress)
        {
            IntPtr targetAddress = IntPtr.Add(baseAddress, 0x24a2750);
            byte[] buffer = new byte[15];

            if (ReadProcessMemory(processHandle, targetAddress, buffer, (uint)buffer.Length, out _))
            {
                int nameLength = Array.IndexOf(buffer, (byte)0);

                if (nameLength == -1)
                    nameLength = buffer.Length;

                return Encoding.ASCII.GetString(buffer, 0, nameLength).Trim();
            }

            return string.Empty;
        }

        private string ReadClientName(IntPtr processHandle, IntPtr baseAddress)
        {
            IntPtr targetAddress = IntPtr.Add(baseAddress, 0x1878473);
            byte[] buffer = new byte[15];

            if (ReadProcessMemory(processHandle, targetAddress, buffer, (uint)buffer.Length, out _))
            {
                int nameLength = Array.IndexOf(buffer, (byte)0);

                if (nameLength == -1)
                    nameLength = buffer.Length;

                return Encoding.ASCII.GetString(buffer, 0, nameLength).Trim();
            }

            return string.Empty;
        }

        private byte[] ReadMsgTime(IntPtr processHandle, IntPtr baseAddress)
        {
            IntPtr targetAddress = IntPtr.Add(baseAddress, 0x1e43a78);
            byte[] buffer = new byte[6];

            if (ReadProcessMemory(processHandle, targetAddress, buffer, (uint)buffer.Length, out _))
                return buffer;

            return new byte[0];
        }


        private string ReadModName(IntPtr processHandle, IntPtr baseAddress, int offset)
        {
            List<byte> bytes = new List<byte>();
            byte currentByte;

            int currentOffset = offset;
            do
            {
                currentByte = ReadMemoryAsByte(processHandle, baseAddress, currentOffset++);
                if (currentByte != 0x2F && currentByte != 0x00)
                {
                    bytes.Add(currentByte);
                }
            } while (currentByte != 0x2F && currentByte != 0x00);

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DeleteSQLEntry(string localIP)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string deleteQuery = "DELETE FROM tcpgames WHERE ipaddr = @IPAddress";
                    using (MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@IPAddress", localIP);
                        deleteCommand.ExecuteNonQuery();
                    }

                    Debug.WriteLine($"Deleted entry for IP: {localIP}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting SQL entry: {ex.Message}");
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.BalloonTipTitle = "Beacon is still open!";
                notifyIcon1.BalloonTipText = "It has been minimized to your system panel";
                notifyIcon1.Icon = Properties.Resources.TheBeacon;
                notifyIcon1.Text = "Beacon";
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(0);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string localIP = GetLocalIPAddress();
            DeleteSQLEntry(localIP);
        }
    }
}

