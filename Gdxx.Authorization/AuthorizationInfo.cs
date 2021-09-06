using System;

namespace Gdxx.Authorization
{
    /// <summary>
    /// 授权信息
    /// </summary>
    public class AuthorizationInfo
    {
        /// <summary>
        /// 激活日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 剩余时间（单位：天）
        /// </summary>
        public int AvailableDays { get; set; }

        /// <summary>
        /// 到期日期
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}