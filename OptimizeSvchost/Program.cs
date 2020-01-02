using System;
using System.Management;
using System.Security;
using Microsoft.Win32;
using System.Security.Principal;

/// <summary>
/// Created by. neonkid 11/25/17
/// </summary>
namespace OptimizeSvchost
{
    class Program
    {
        static String currentVersion;

        static void UpperWindows1709()
        {
            Console.WriteLine(@"==========================ERROR==============================");
            Console.WriteLine(@"=                                                           =");
            Console.WriteLine(@"= 죄송합니다.....                                           =");
            Console.WriteLine(@"=                                                           =");
            Console.WriteLine(@"= (Version: " + currentVersion + ")            =");
            Console.WriteLine(@"= 현재 Windows 버전에서는 이 프로그램이 지원되지 않습니다.  =");
            Console.WriteLine(@"=============================================================");
            Console.ReadKey();
        }

        static void PleaseAdministrator()
        {
            Console.WriteLine(@"=========================NOTICE==============================");
            Console.WriteLine(@"=                                                           =");
            Console.WriteLine(@"= 이 프로그램은 svchost.exe 프로세스의 갯수를 최적화 하고자 =");
            Console.WriteLine(@"= 개발되었으며, 관리자 모드에서만 이용하실 수 있습니다.     =");
            Console.WriteLine(@"=                                                           =");
            Console.WriteLine(@"= 관리자 모드에서 다시 실행해주시기 바랍니다.               =");
            Console.WriteLine(@"=============================================================");
            Console.ReadKey();
        }

        static void ShowMenu()
        {
            Console.WriteLine(@"===================MENU==================");
            Console.WriteLine(@"1. Svchost 그룹화");
            Console.WriteLine(@"2. Svchost 세분화");
            Console.WriteLine(@"0. 프로그램 종료");
            Console.WriteLine(@"=========================================");
        }

        static void setRegisterValue(Int32 value)
        {
            const string registryPath = @"SYSTEM\CurrentControlSet\Control";
            const string errorMessage = @"최적화 설정 중 오류가 발생했습니다: ";

            Console.WriteLine();
            try
            {
                RegistryKey regInfo = Registry.LocalMachine.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                regInfo.SetValue(@"SvcHostSplitThresholdInKB", value, RegistryValueKind.DWord);

                Console.WriteLine("설정이 완료되었습니다: " + toHex(value));
                Console.WriteLine("윈도우를 재시작하십시오.");
                Console.ReadKey();

                regInfo.Close();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(errorMessage + ex.Message);
                Console.ReadKey();
            }
            catch (SecurityException ex)
            {
                Console.WriteLine(errorMessage + ex.Message);
                Console.ReadKey();
            }
        }

        static void Main(string[] args)
        {
            Boolean flag = true;
            ManagementClass cls = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection instances = cls.GetInstances();

            Console.ForegroundColor = ConsoleColor.Green;

            if (!IsWindows10Creators())
            {
                UpperWindows1709();
                return;
            }

            if (!IsAdministrator())
            {
                PleaseAdministrator();
                return;
            }

            do
            {
                ShowMenu();
                Console.Write("원하시는 번호를 입력하세요: ");
                switch (Console.Read())
                {
                    case 48:
                        flag = !flag;
                        break;

                    case 49:
                        Int32 total_memory = 0x00;

                        // 시스템의 총 메모리 용량을 16진수 값으로 가져옵니다..
                        foreach (ManagementObject info in instances)
                        {
                            String total_hex = info["TotalVisibleMemorySize"].ToString();
                            total_memory = toDec(total_hex);
                        }
                        setRegisterValue(total_memory);
                        break;

                    case 50:
                        setRegisterValue(0x380000);
                        break;
                }
                Console.Clear();
            } while (flag);
        }

        // 관리자 권한으로 실행한 것인지, 아닌지를 체크합니다...
        static bool IsAdministrator()
        {
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            if (windowsIdentity != null)
            {
                WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);
                return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }

        // Windows 10 Fall Creators Update 버전인지를 확인합니다....
        static bool IsWindows10Creators()
        {
            OperatingSystem os = Environment.OSVersion;
            string path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            string releaseId = Registry.LocalMachine.OpenSubKey(path).GetValue("ReleaseId").ToString();
            currentVersion = os.VersionString + releaseId;

            if (Int32.Parse(releaseId) >= 1709)
                return true;
            return false;
        }

        // 10진수 --> 16진수 변환
        static String toHex(Int32 num)
        {
            string hex = num.ToString("x");
            if (hex.Length % 2 != 0)
                hex = "0" + hex;
            return hex;
        }

        // 16진수 --> 10진수 변환
        static Int32 toDec(String hex)
        {
            return Convert.ToInt32(hex, 16);
        }
    }
}
