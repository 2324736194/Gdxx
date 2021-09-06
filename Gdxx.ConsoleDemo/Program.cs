using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using GDCourse.LicenseService.Core;
using Gdxx.Authorization;

namespace Gdxx.ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var address = "http://192.168.30.163:2021";
            var id = "C8859A21-85A5-9378-FDD9-B9942942D479";
            var version = "V1.0";
            var service = new AuthorizationService(address, id, version);
            Test1(service).Wait();
            Console.WriteLine("=====流程结束=====");
            Console.ReadLine();
        }

        static async Task Test1(AuthorizationService service)
        {
            var userCode = "42010019990203556X";
            var result = await service.Check(userCode);
            if (null == result)
            {
                await AuthorizationActivation(service, userCode);
            }
            else
            {
                switch (result.Result)
                {
                    case LicenseCheckResult.Validate:
                        Console.WriteLine("已激活。");
                        break;
                    case LicenseCheckResult.ExpiringLicense:
                        Console.WriteLine($"您的授权码还剩 {result.AvailableDays} 天到期，请及时处理！");
                        break;
                    case LicenseCheckResult.InvalidLicense:
                        Console.WriteLine("授权码已过期，请重新激活。");
                        await AuthorizationActivation(service, userCode);
                        break;
                    case LicenseCheckResult.DisabledLicense:
                        Console.WriteLine("该账户绑定的授权码已被禁用，请重新激活。");
                        await AuthorizationActivation(service, userCode);
                        break;
                    case LicenseCheckResult.ExpiredLicense:
                        Console.WriteLine("该账户绑定的授权码已过期，请重新激活。");
                        await AuthorizationActivation(service, userCode);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 激活授权码
        /// </summary>
        /// <param name="service"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        static async Task AuthorizationActivation(AuthorizationService service,string userCode)
        {
            var Message = string.Empty;
            var running = true;
            Console.WriteLine();
            Console.WriteLine("=====");
            while (running)
            {
                Console.WriteLine("请输入授权码：");
                var code = Console.ReadLine();
                if (string.IsNullOrEmpty(code))
                {
                    Console.WriteLine("提示：授权码不能为空，请重新输入。");
                    continue;
                }
                var result = await Task.Run(() => service.Activation(userCode, code).Result);
                switch (result)
                {
                    case LicenseRegisterResult.Validate:
                        Console.WriteLine("激活成功");
                        running = false;
                        continue;
                    case LicenseRegisterResult.StatusError:
                        Message = "激活失败！授权码状态无效。";
                        break;
                    case LicenseRegisterResult.TooManyMachine:
                        Message = "激活失败！该授权码已被使用，请更换授权码！";
                        break;
                    case LicenseRegisterResult.TooManyUser:
                        Message = "激活失败！关联用户数过多。";
                        break;
                    case LicenseRegisterResult.InvalidLicense:
                        Message = "激活失败！该授权码不存在，请更换授权码！";
                        break;
                    case LicenseRegisterResult.UnavailableLicense:
                        Message = "激活失败！该授权码已过期，请更换授权码！";
                        break;
                    case LicenseRegisterResult.AlreadyUsedLicense:
                        Message = "激活失败！该授权码已被使用，请更换授权码！";
                        break;
                    case LicenseRegisterResult.DisabledLicense:
                        Message = "激活失败！该授权码已被禁用，请联系管理员处理！";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Console.WriteLine(Message);
            }
        }
    }
}
