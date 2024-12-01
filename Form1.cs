using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Beacon
{
    public partial class Form1 : Form
    {
        #region Constants/Imports
        public static string version = "1.0.6";
        private HashSet<string> existingIPAddresses = new HashSet<string>();
        private HashSet<string> existingIPs = new HashSet<string>();
        private HashSet<string> existingPNames = new HashSet<string>();
        private HashSet<string> existingPCount = new HashSet<string>();
        private HashSet<string> existingLCount = new HashSet<string>();
        private const uint PROCESS_VM_READ = 0x0010;
        private System.Windows.Forms.Timer memoryTimer;
        private string connectionString = "server=eu02-sql.pebblehost.com;uid=customer_614448_main;pwd=Wy9P#W3aO5yAbwYQVw@f;database=customer_614448_main";

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out IntPtr lpNumberOfBytesRead);
        #endregion

        #region Form Handlers
        public Form1()
        {
            InitializeComponent();
            memoryTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            memoryTimer.Tick += async (sender, e) => await MemoryTimer_TickAsync(sender, e);
            Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            ListAllProcesses();
            await StartMemoryMonitoringAsync();
            await LoadDataIntoDataGridViewAsync();
            this.Text = "Beacon v" + version;

            await PopulateModNamesAsync();
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
        #endregion

        #region Memory Monitoring
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
            UpdateUI(memoryData);
            await Task.Run(() => ExecuteSQLUpdateAsync(localIP, memoryData));
            await LoadDataIntoDataGridViewAsync();
            CloseHandle(processHandle);
        }
        #endregion

        #region Parsing Functions
        private dynamic ReadMemoryData(IntPtr processHandle, IntPtr baseAddress)
        {
            byte playerMode = ReadMemoryAsByte(processHandle, baseAddress, 0x1BF0883);

            return new
            {
                HostStatus = ReadMemoryAsByte(processHandle, baseAddress, 0x18882E8),
                PlayerCount = ReadMemoryAsByte(processHandle, baseAddress, 0x1d637e4),
                PlayerName = ReadPlayerName(processHandle, baseAddress),
                Difficulty = ReadMemoryAsByte(processHandle, baseAddress, 0x1bf086e),
                LobbyCount = ReadMemoryAsByte(processHandle, baseAddress, 0x1c9be38),
                PlayerMode = playerMode,
                IsBit2Set = CheckIfBit2IsSet(playerMode),
                ModName = ReadModName(processHandle, baseAddress, 0x1d7f3c5),
            };
        }

        private bool ReadMemory(IntPtr processHandle, IntPtr address, out byte[] buffer)
        {
            buffer = new byte[1];
            return ReadProcessMemory(processHandle, address, buffer, (uint)buffer.Length, out _);
        }

        private byte ReadMemoryAsByte(IntPtr processHandle, IntPtr baseAddress, int offset)
        {
            IntPtr targetAddress = IntPtr.Add(baseAddress, offset);
            return ReadMemory(processHandle, targetAddress, out byte[] buffer) ? buffer[0] : (byte)0;
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

        private string ReadModVersion()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string modPath = Path.Combine(baseDirectory, @"..\D2R\Mods", labelModName.Text, $"{labelModName.Text}.mpq", "modinfo.json");
            modPath = Path.GetFullPath(modPath);

            if (!File.Exists(modPath))
                return string.Empty;

            try
            {
                string fileContent = File.ReadAllText(modPath);
                string pattern = @"Mod Version:\s*([^(]*)";
                Match match = Regex.Match(fileContent, pattern);
                return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
                return string.Empty;
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
        #endregion

        #region Helper Functions
        private bool CheckIfBit2IsSet(byte value)
        {
            byte shiftedValue = (byte)(value >> 2);
            return (shiftedValue & 0x1) == 0x1;
        }

        private void ShowErrorWithHandleClose(IntPtr processHandle, string message)
        {
            ShowError(message);
            CloseHandle(processHandle);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region SQL Management
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
                                // Update local collections after deletion
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
                        bool isExistingEntry = existingIPAddresses.Contains(localIP);

                        if (!isExistingEntry)
                        {
                            // If the entry does not exist, it's a new entry
                            string modName = memoryData.ModName == "TCP" ? "Retail" : memoryData.ModName;
                            string query = @"
            INSERT INTO tcpgames (ipaddr, charname, gamediff, playercount, lobbycount, hardcore, modname, modversion) 
            VALUES (@IPAddress, @PName, @GameDiff, @PCount, @LCount, @GameMode, @ModName, @ModVersion) 
            ON DUPLICATE KEY UPDATE  
                charname = @PName, 
                gamediff = @GameDiff, 
                playercount = @PCount, 
                lobbycount = @LCount,
                hardcore = @GameMode,
                modname = @ModName,
                modversion = @ModVersion;";

                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@IPAddress", localIP);
                                command.Parameters.AddWithValue("@Status", labelHostStatus.Text);
                                command.Parameters.AddWithValue("@PName", memoryData.PlayerName);
                                command.Parameters.AddWithValue("@GameDiff", labelDifficulty.Text);
                                command.Parameters.AddWithValue("@PCount", memoryData.PlayerCount.ToString());
                                command.Parameters.AddWithValue("@LCount", memoryData.LobbyCount.ToString() + " / 8");
                                command.Parameters.AddWithValue("@GameMode", labelPMode.Text);
                                command.Parameters.AddWithValue("@ModName", modName);
                                command.Parameters.AddWithValue("@ModVersion", labelModVersion.Text);

                                int rowsAffected = await command.ExecuteNonQueryAsync();

                                if (rowsAffected > 0)
                                {
                                    // The game was newly added
                                    existingIPs.Add(localIP);
                                    existingPNames.Clear();
                                    existingPCount.Clear();
                                    existingLCount.Clear();

                                    Invoke((MethodInvoker)delegate
                                    {
                                        AddMessageToRichTextBox("Game Added to Server List", Color.Green); // Only show this message for new games
                                    });
                                }
                            }
                        }
                        else
                        {
                            // Entry exists, so just update the existing entry
                            string query = @"
            UPDATE tcpgames 
            SET hostingnow = @Status, 
                charname = @PName, 
                gamediff = @GameDiff, 
                playercount = @PCount, 
                lobbycount = @LCount,
                hardcore = @GameMode,
                modname = @ModName, 
                modversion = @ModVersion
            WHERE ipaddr = @IPAddress;";

                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@IPAddress", localIP);
                                command.Parameters.AddWithValue("@Status", labelHostStatus.Text);
                                command.Parameters.AddWithValue("@PName", memoryData.PlayerName);
                                command.Parameters.AddWithValue("@GameDiff", labelDifficulty.Text);
                                command.Parameters.AddWithValue("@PCount", memoryData.PlayerCount.ToString());
                                command.Parameters.AddWithValue("@LCount", memoryData.LobbyCount.ToString() + " / 8");
                                command.Parameters.AddWithValue("@GameMode", labelPMode.Text);
                                command.Parameters.AddWithValue("@ModName", memoryData.ModName);
                                command.Parameters.AddWithValue("@ModVersion", labelModVersion.Text);

                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                // Ensure the DataGridView is refreshed after executing the SQL update
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
        #endregion

        #region Process Handlers
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
        #endregion

        #region UI Management
        private async Task LoadDataIntoDataGridViewAsync()
        {
            try
            {
                await PopulateModNamesAsync();

                int firstDisplayedScrollingRowIndex = gameList.FirstDisplayedScrollingRowIndex;
                DataGridViewRow currentRow = gameList.CurrentRow;

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Build query depending on the modname and the checkAllGames checkbox
                    string query = "";
                    string modnameFilter = "";
                    if (comboBox1.Text != "")
                        modnameFilter = comboBox1.SelectedItem.ToString();

                    if (!checkAllGames.Checked)
                        query = "SELECT ipaddr, charname, gamediff, playercount, lobbycount, hardcore, modname, modversion FROM tcpgames";
                    else
                        query = "SELECT ipaddr, charname, gamediff, playercount, lobbycount, hardcore, modname, modversion FROM tcpgames WHERE TRIM(ipaddr) NOT LIKE '192.168.%'";

                    // Apply modname filtering if not set to "All"
                    if (modnameFilter != "All")
                        query += (!checkAllGames.Checked ? " WHERE " : " AND ") + "modname = @modname";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);

                    if (modnameFilter != "All")
                        adapter.SelectCommand.Parameters.AddWithValue("@modname", modnameFilter);

                    var dataTable = new System.Data.DataTable();
                    await Task.Run(() => adapter.Fill(dataTable));

                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Debug.WriteLine("Column: " + column.ColumnName);
                    }

                    if (dataTable.Rows.Count == 0)
                        labelNoGames.Visible = true;
                    else
                        labelNoGames.Visible = false;

                    gameList.DataSource = dataTable;

                    // Additional processing for data
                    existingIPAddresses.Clear();
                    existingPCount.Clear();
                    existingLCount.Clear();

                    foreach (DataGridViewRow row in gameList.Rows)
                    {
                        if (row.Cells["ipaddr"].Value != null)
                            existingIPAddresses.Add(row.Cells["ipaddr"].Value.ToString());
                        if (row.Cells["charname"].Value != null)
                            existingPNames.Add(row.Cells["charname"].Value.ToString());
                    }

                    // Set column headers and apply formatting
                    gameList.Columns["ipaddr"].HeaderText = "   IP";
                    gameList.Columns["charname"].HeaderText = "   Host Name";
                    gameList.Columns["gamediff"].HeaderText = "   Difficulty";
                    gameList.Columns["playercount"].HeaderText = "Players";
                    gameList.Columns["lobbycount"].HeaderText = "Slots";
                    gameList.Columns["hardcore"].HeaderText = "   Mode";
                    gameList.Columns["modname"].HeaderText = "    Mod Name";
                    gameList.Columns["modversion"].HeaderText = " Mod Version";

                    // Set column widths, styles, and other settings
                    gameList.Columns["ipaddr"].Width = 120;
                    gameList.Columns["charname"].Width = 130;
                    gameList.Columns["gamediff"].Width = 100;
                    gameList.Columns["playercount"].Width = 80;
                    gameList.Columns["lobbycount"].Width = 60;
                    gameList.Columns["hardcore"].Width = 100;
                    gameList.Columns["modname"].Width = 140;
                    gameList.Columns["modversion"].Width = 120;
                    gameList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                    gameList.EnableHeadersVisualStyles = false;
                    gameList.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkOrange;
                    gameList.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                    gameList.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
                    gameList.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    gameList.ColumnHeadersDefaultCellStyle.Padding = new Padding(0);
                    gameList.ColumnHeadersHeight = 30;

                    gameList.DefaultCellStyle.Font = new Font("Arial", 10);
                    gameList.BackgroundColor = Color.Black;
                    gameList.ForeColor = Color.WhiteSmoke;
                    gameList.RowsDefaultCellStyle.BackColor = Color.Black;
                    gameList.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(23, 23, 23);
                    gameList.CellBorderStyle = DataGridViewCellBorderStyle.None;

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

                    gameList.ScrollBars = ScrollBars.None;
                    gameList.MouseWheel += gameList_MouseWheel;
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

        private async Task PopulateModNamesAsync()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT DISTINCT modname FROM tcpgames";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    List<string> newModNames = new List<string> { "All" };
                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            newModNames.Add(reader["modname"].ToString());
                        }
                    }

                    // Get current items in comboBox1
                    List<string> currentItems = comboBox1.Items.Cast<string>().ToList();

                    // Only repopulate if items have changed
                    if (!newModNames.SequenceEqual(currentItems))
                    {
                        comboBox1.Items.Clear();
                        foreach (var modName in newModNames)
                        {
                            comboBox1.Items.Add(modName);
                        }
                        comboBox1.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error populating mod names: " + ex.Message);
            }
        }

        private void UpdateUI(dynamic memoryData)
        {
            labelHostStatus.Text = (memoryData.HostStatus == 0x02) ? "Yes" : "No";
            labelPCount.Text = memoryData.PlayerCount.ToString();
            labelLCount.Text = memoryData.LobbyCount.ToString() + " / 8";
            labelPName.Text = memoryData.PlayerName;
            labelModName.Text = memoryData.ModName;
            labelModVersion.Text = ReadModVersion();

            labelDifficulty.Text = ((int)memoryData.Difficulty) switch
            {
                0 => "Normal",
                1 => "Nightmare",
                2 => "Hell",
                _ => "Unknown"
            };

            labelPMode.Text = memoryData.IsBit2Set switch
            {
                true => "Hardcore",
                false => "Softcore",
                _ => "Old Beacon"
            };
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

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadDataIntoDataGridViewAsync();
        }

        private async void checkAllGames_CheckedChanged(object sender, EventArgs e)
        {
            await LoadDataIntoDataGridViewAsync();
        }
        #endregion
    }
}

