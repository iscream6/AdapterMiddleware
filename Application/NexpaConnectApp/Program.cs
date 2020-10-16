using NexpaAdapterStandardLib;
using NpmAdapter;
using System;
//using static NexpaAdapterStandardLib.AbstractPipe;

/// <summary>
/// .Net Core 3.1 Version
/// </summary>
namespace NexpaConnectApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("help 명령어로 도움말 확인");

            MainService();

            Console.WriteLine("Exit Program...");
            
        }

        public static void MainService()
        {
            //AdapterFactory adapterFactory = new AdapterFactory();
            //var test = adapterFactory.MakeServerAdapter(AdapterFactory.Adapters.HttpServer, "8080");
            //test.Run();

            //TcpHNClient client = new TcpHNClient(;
            //cli를 통해...command 와 인자를 line 으로 받는다....
            //server, client 가동.....
            //exit <==== 닫기
            //run <=== 실행
            //disc <=== 닫기
            //conn <== 열기

            AdapterPipe pipe = new AdapterPipe();


            bool bForever = true;
            while (bForever)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("cmd : ");
                Console.ResetColor();
                string readLine = Console.ReadLine();

                if (string.IsNullOrEmpty(readLine))
                {
                    Console.WriteLine("도움말은 -help 명령어로 확인 가능합니다.");
                }
                else
                {
                    string[] cmd = readLine.Split(" ");


                    switch (cmd[0].Trim().ToLower())
                    {
                        case "init":
                            pipe.GeneratePipe();
                            break;
                        case "start": // nexpa or homenet
                            if (cmd.Length > 1)
                            {
                                var type = (AdapterType)Enum.Parse(typeof(AdapterType), cmd[1]);
                                Console.WriteLine(pipe.StartAdapter(type));
                            }
                            else
                            {
                                Console.WriteLine("Usable command : nexpa or homenet");
                            }
                            break;
                        case "stop":
                            if (cmd.Length > 1)
                            {
                                var type = (AdapterType)Enum.Parse(typeof(AdapterType), cmd[1]);
                                Console.WriteLine(pipe.StopAdapter(type));
                            }
                            else
                            {
                                Console.WriteLine("Usable command : nexpa or homenet");
                            }
                            break;
                        case "help": //도움말
                            DisplayHelpMessage();
                            break;
                        case "exit": //종료
                            bForever = false;
                            break;
                    }
                }
            }
        }


        #region Display Message

        /// <summary>
        /// -help
        /// </summary>
        public static void DisplayHelpMessage()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-help : 도움말");
            Console.WriteLine("-show : 실행중인 모든 Session 확인");
            Console.WriteLine("-show Option");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  tcp-server  : 실행중인 tcp server session 확인");
            Console.WriteLine("  tcp-client  : 실행중인 tcp client session 확인");
            Console.WriteLine("  http-server : 실행중인 http server session 확인");
            Console.WriteLine("  http-client : 실행중인 http client session 확인");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-mk : Session 생성");
            Console.WriteLine("-mk Option");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  tcp-server [port] : tcp server session 생성");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-exit : 프로그램 종료");
            Console.ResetColor();
        }

        private static void DisplayErrorMessage(int errorCode)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (errorCode == -1)
            {
                Console.WriteLine("인자가 부족합니다....");
            }
            else if (errorCode == -2)
            {
                Console.WriteLine("인자값이 잘못되었습니다....");
            }
            Console.ResetColor();
        }

        #endregion
    }
}
