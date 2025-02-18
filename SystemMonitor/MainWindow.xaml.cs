using System;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WPFMessageBox = System.Windows.MessageBox;

namespace SystemMonitor
{
    public partial class MainWindow : Window
    {
        private PerformanceCounter? cpuCounter;
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            InitializePerformanceCounters();
            InitializeTimer();
            InitializeHardwareInfo();
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // 第一次读取总是返回0
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"初始化性能计数器失败: {ex.Message}");
            }
        }

        private void InitializeHardwareInfo()
        {
            try
            {
                // 获取CPU信息
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string cpuName = obj["Name"].ToString() ?? "未知";
                        CpuInfoText.Text = $"CPU: {cpuName}";
                        break;
                    }
                }

                // 获取内存信息
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong totalMemoryBytes = Convert.ToUInt64(obj["TotalPhysicalMemory"]);
                        double totalMemoryGB = totalMemoryBytes / (1024.0 * 1024.0 * 1024.0);
                        MemoryInfoText.Text = $"内存: {totalMemoryGB:F1} GB";
                        break;
                    }
                }

                // 获取GPU信息
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string gpuName = obj["Name"].ToString() ?? "未知";
                        GpuInfoText.Text = $"GPU: {gpuName}";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"获取硬件信息失败: {ex.Message}");
            }
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            await UpdatePerformanceData();
        }

        private async Task UpdatePerformanceData()
        {
            await Task.Run(() =>
            {
                try
                {
                    // 更新CPU使用率
                    if (cpuCounter != null)
                    {
                        float cpuUsage = cpuCounter.NextValue();
                        Dispatcher.Invoke(() =>
                        {
                            CpuUsageText.Text = $"CPU使用率: {cpuUsage:F1}%";
                            CpuProgressBar.Value = cpuUsage;
                        });
                    }

                    // 更新内存使用率
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            ulong totalMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024;
                            ulong freeMemory = Convert.ToUInt64(obj["FreePhysicalMemory"]) * 1024;
                            double memoryUsage = ((totalMemory - freeMemory) / (double)totalMemory) * 100;

                            Dispatcher.Invoke(() =>
                            {
                                MemoryUsageText.Text = $"内存使用率: {memoryUsage:F1}%";
                                MemoryProgressBar.Value = memoryUsage;
                            });
                        }
                    }

                    // 更新GPU使用率
                    using (var searcher = new ManagementObjectSearcher(
                               "SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine WHERE Name LIKE '%3D%'"))
                    {
                        float gpuUsage = 0;
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            gpuUsage += Convert.ToSingle(obj["UtilizationPercentage"]);
                        }

                        Dispatcher.Invoke(() =>
                        {
                            GpuUsageText.Text = $"GPU使用率: {gpuUsage:F1}%";
                            GpuProgressBar.Value = gpuUsage;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => { WPFMessageBox.Show($"更新性能数据失败: {ex.Message}"); });
                }
            });
        }

        private void ToggleHardwareInfo_Click(object sender, RoutedEventArgs e)
        {
            HardwareInfoExpander.IsExpanded = !HardwareInfoExpander.IsExpanded;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void TopMostButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            TopMostButton.Opacity = Topmost ? 1.0 : 0.5;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            timer.Stop();
            cpuCounter?.Dispose();
        }
    }
}