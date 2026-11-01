using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CtYun.Models
{
    public class AppConfig
    {
        [JsonPropertyName("accounts")]
        public List<AccountConfig> Accounts { get; set; } = [];

        [JsonPropertyName("keepAliveSeconds")]
        public int KeepAliveSeconds { get; set; } = 60;

        /// <summary>
        /// 是否隐藏控制台窗口（仅 Windows 生效）。设为 true 后，启动时隐藏黑色窗口，
        /// 日志改由文件日志承载（数据目录/logs/）。首次需要短信验证码绑定设备时请保持 false。
        /// </summary>
        [JsonPropertyName("hideConsole")]
        public bool HideConsole { get; set; } = false;

    }

    public class AccountConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("deviceCode")]
        public string DeviceCode { get; set; }
    }
}
