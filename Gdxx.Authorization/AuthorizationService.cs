using System;
using System.Threading.Tasks;
using GDCourse.LicenseService.Client;
using GDCourse.LicenseService.Core;

namespace Gdxx.Authorization
{
    /// <summary>
    /// 授权码服务
    /// </summary>
    public sealed class AuthorizationService
    {
        private readonly string id;
        private readonly string version;
        private readonly SoftType softType = SoftType.Normal;

        /// <summary>
        /// 实例化授权码服务
        /// </summary>
        /// <param name="address"></param>
        /// <param name="id"></param>
        /// <param name="version"></param>
        public AuthorizationService(string address, string id, string version)
        {
            LicenseHelper.InitLicenseServer(address);
            this.id = id;
            this.version = version;
        }

        public AuthorizationInfo GetInfo()
        {
            LicenseHelper.GetRegisteredInformation(out var licenseCode, out var userCode, out var machineCode, out var expiredDate, out var registerTime);
            var span = (expiredDate - registerTime);
            var info = new AuthorizationInfo()
            {
                StartDate = registerTime,
                EndDate = expiredDate,
                AvailableDays = (int)Math.Ceiling(span.TotalDays)
            };
            return info;
        }

        /// <summary>
        /// 检查用户是否已激活
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public async Task<CheckLicenseResult> Check(string userCode)
        {
            if (LicenseHelper.CheckRegistered())
            {
                var licenseDescription = await LicenseHelper.ReadLicenseDescriptionAsync();
                var result = await LicenseHelper.Check(licenseDescription, id, version, softType, userCode);
                return result;
            }
            return default;
        }

        /// <summary>
        /// 激活授权码
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<LicenseRegisterResult> Activation(string userCode, string code)
        {
            var result = await LicenseHelper.Register(code, id, version, softType, userCode);
            return result;
        }
    }
}