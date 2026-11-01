using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtYun
{
    /// <summary>
    /// 日志输出：同时写到控制台 和 数据目录下 logs/ 按天滚动文件。
    /// 需要先调用 <see cref="InitLog"/> 传入数据目录；未初始化时仅输出控制台（不写文件）。
    /// </summary>
    internal static class Utility
    {
        private static readonly object _lock = new();

        /// <summary>日志目录；null 表示未初始化，不落盘。</summary>
        private static string _logDir;

        /// <summary>
        /// 初始化文件日志。dataDir 下创建 logs/ 目录，日志文件名为 ctyun-YYYY-MM-DD.log。
        /// </summary>
        public static void InitLog(string dataDir)
        {
            if (string.IsNullOrWhiteSpace(dataDir))
            {
                return;
            }

            try
            {
                var dir = Path.Combine(dataDir, "logs");
                Directory.CreateDirectory(dir);
                _logDir = dir;
                WriteFile($"[启动] 文件日志已开启，目录：{dir}");
            }
            catch (Exception ex)
            {
                _logDir = null;
                Console.WriteLine($"[Utility] 初始化文件日志失败，仅控制台输出：{ex.Message}");
            }
        }

        /// <summary>日志文件所在目录（无则返回 null）。</summary>
        public static string LogDirectory => _logDir;

        public static void WriteLine(ConsoleColor consolecolor, object value)
        {
            var line = $"[{DateTime.Now:HH:mm:ss.fff}] {value}";

            // 控制台（带颜色），窗口隐藏时此处写入隐藏的控制台，不影响文件日志
            try
            {
                var original = Console.ForegroundColor;
                Console.ForegroundColor = consolecolor;
                Console.WriteLine(line);
                Console.ForegroundColor = original;
            }
            catch
            {
                // 忽略控制台写入异常（如无控制台）
            }

            WriteFile(line);
        }

        private static void WriteFile(string line)
        {
            if (_logDir == null)
            {
                return;
            }

            lock (_lock)
            {
                try
                {
                    var today = DateTime.Now.ToString("yyyy-MM-dd");
                    var path = Path.Combine(_logDir, "ctyun-" + today + ".log");

                    // 首次创建时写入 UTF-8 BOM，避免 Windows 记事本乱码
                    if (!File.Exists(path))
                    {
                        File.WriteAllText(path, "\uFEFF");
                    }

                    File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // 文件写入失败不影响程序运行
                }
            }
        }
    }
}