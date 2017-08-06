using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Distributor
{
    public static class ProcessHelpers
    {
        public static string Execute32bitCommand(string command,
                                                 ExitCodeValidator exitCodeValidator,
                                                 string workingDirectory = null)
        {
            var runnerDirectory = Environment.Is64BitOperatingSystem
                                      ? Path.Combine(Environment.SystemDirectory, @"..\SysWoW64")
                                      : Environment.SystemDirectory;
            return ExecuteCommand(Path.Combine(runnerDirectory, "cmd.exe"), command, exitCodeValidator, workingDirectory);
        }

        public static void KillProcess(string imageName,
                                       string machineName = null)
        {
            ExecuteCommand(string.Format("taskkill.exe {0} /im {1} /f", string.IsNullOrEmpty(machineName) ? "" : "/s " + machineName, imageName),
                           IgnoreExitCode);
            WaitProcessTerminates(imageName, machineName);
        }

        public static IEnumerable<Process> GetProcessesByName(string imageName,
                                                              string machineName = null)
        {
            imageName = Path.GetFileNameWithoutExtension(imageName);
            return machineName == null
                       ? Process.GetProcessesByName(imageName)
                       : Process.GetProcessesByName(imageName, machineName);
        }


        public static void KillProcess(int processId,
                                       string machineName = null)
        {
            var command = string.Format("taskkill.exe {0} /pid {1} /f", string.IsNullOrEmpty(machineName) ? "" : "/s " + machineName, processId);
            WaitProcessTerminates(processId, machineName, () => ExecuteCommand(command, IgnoreExitCode));
        }

        public static void RealKill(this Process process)
        {
            if (string.Equals(process.MachineName, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                process.Kill();
            else
                KillProcess(process.Id, process.MachineName);
        }

        public static Process GetProcessById(int processId,
                                             string machineName = null)
        {
            try
            {
                return machineName == null
                           ? Process.GetProcessById(processId)
                           : Process.GetProcessById(processId, machineName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static void WaitProcessTerminates(Func<IEnumerable<Process>> getProcesses,
                                                  Action progress = null,
                                                  TimeSpan? timeSpan = null)
        {
            var actualTimeout = timeSpan ?? TimeSpan.FromMinutes(10);
            var stopWatch = Stopwatch.StartNew();
            var processes = getProcesses().ToArray();
            while (processes.Any())
            {
                Thread.Sleep(100);
                if (stopWatch.Elapsed > actualTimeout)
                    throw new InvalidOperationException(string.Format("Timed out waiting for processes {0}", string.Join(",", processes.Select(x => x.ProcessName))));
                if (progress != null)
                    progress();
                processes = getProcesses().ToArray();
            }
        }

        public static void WaitProcessTerminates(int processId,
                                                 string machineName = null,
                                                 Action progress = null,
                                                 TimeSpan? timeSpan = null)
        {
            WaitProcessTerminates(() => new[] { GetProcessById(processId, machineName) }.Where(x => x != null), progress, timeSpan);
        }

        public static void WaitProcessTerminates(string imageName,
                                                 string machineName = null,
                                                 Action progress = null,
                                                 TimeSpan? timeSpan = null)
        {
            WaitProcessTerminates(() => GetProcessesByName(imageName, machineName).ToArray(), progress, timeSpan);
        }

        public static string ExecuteCommand(string command,
                                            ExitCodeValidator exitCodeValidator,
                                            string workingDirectory = null)
        {
            return ExecuteCommand(Path.Combine(Environment.SystemDirectory, "cmd.exe"), command, exitCodeValidator, workingDirectory);
        }

        public static ProcessResult TryExecuteCommand(string command,
                                                      string workingDirectory = null)
        {
            return TryExecuteWithOutputRedirect(Path.Combine(Environment.SystemDirectory, "cmd.exe"), "/c " + command, workingDirectory);
        }

        public static ProcessResult TryExecute32BitCommand(string command,
                                                           string workingDirectory = null)
        {
            var runnerDirectory = Environment.Is64BitOperatingSystem
                                      ? Path.Combine(Environment.SystemDirectory, @"..\SysWoW64")
                                      : Environment.SystemDirectory;
            return TryExecuteWithOutputRedirect(Path.Combine(runnerDirectory, "cmd.exe"), "/c " + command, workingDirectory);
        }

        public static readonly ExitCodeValidator IgnoreExitCode = _ => false;
        public static readonly ExitCodeValidator FailIfError = result => result.ExitCode != 0;

        public delegate bool ExitCodeValidator(ProcessResult result);

        private static string ExecuteCommand(string shell,
                                             string command,
                                             ExitCodeValidator exitCodeValidator,
                                             string workingDirectory)
        {
            return ExecuteWithOutputRedirect(shell, "/c " + command, exitCodeValidator, workingDirectory);
        }

        public static string ExecuteWithOutputRedirect(string fileName,
                                                       string arguments,
                                                       ExitCodeValidator exitCodeValidator,
                                                       string workingDirectory,
                                                       int? timeout = null)
        {
            var result = TryExecuteWithOutputRedirect(fileName, arguments, workingDirectory, timeout);
            if (exitCodeValidator(result))
                throw new Exception(result.ExitCode+" "+result.Output);
            return result.Output;
        }

        public static ProcessResult TryExecuteWithOutputRedirect(string fileName,
                                                                 string arguments,
                                                                 string workingDirectory,
                                                                 int? timeout = null)
        {
            using (var processOutput = new MemoryStream())
            {
                int exitCode;
                var processWorkingDirectory = (workingDirectory?.Length == 0 ? null : workingDirectory) ?? Path.GetDirectoryName(fileName);
                var synchronizedOutputStream = Stream.Synchronized(processOutput);
                var process1 = Process.Start(new ProcessStartInfo(fileName, arguments)
                                             {
                                                 WorkingDirectory = processWorkingDirectory,
                                                 UseShellExecute = false,
                                                 CreateNoWindow = true,
                                                 RedirectStandardError = true,
                                                 RedirectStandardInput = false,
                                                 RedirectStandardOutput = true
                                             });
                process1.OutputDataReceived += (sender,
                                                args) => synchronizedOutputStream.WriteUTF8Line(args.Data);
                process1.ErrorDataReceived += (sender,
                                               args) => synchronizedOutputStream.WriteUTF8Line(args.Data);
                process1.BeginErrorReadLine();
                process1.BeginOutputReadLine();
                using (var process = process1)
                {
                    if (timeout.HasValue)
                    {
                        process.WaitForExit(timeout.Value);
                        // http://msdn.microsoft.com/ru-ru/library/ty0d8k56(v=vs.110).aspx
                        // without this call output buffer is not flushed
                        process.WaitForExit();
                    }
                    else
                        process.WaitForExit();
                    exitCode = process.ExitCode;
                }

                processOutput.Seek(0, SeekOrigin.Begin);
                var commandResult = processOutput.ReadUTF8String();
                return new ProcessResult
                       {
                           ExitCode = exitCode,
                           Output = commandResult
                       };
            }
        }

        public static int? GetProcessId(string processName,
                                        string machineName)
        {
            //чтобы эта хрень работала, на удаленной машине должен быть запущен сервис Remote Registry (Удаленный Реестр)
            //netsh firewall set service type = remoteadmin mode = mode
            var processes = Process.GetProcessesByName(processName, machineName);
            if (!processes.Any())
                return null;
            if (processes.Length > 1)
                throw new InvalidOperationException(string.Format("there are multiple processes with name {0} on machine {1}",
                                                                  processName, machineName));
            return processes.Single().Id;
        }
    }

    public class ProcessResult
    {
        public bool IsSuccess
        {
            get { return ExitCode == 0; }
        }

        public int ExitCode { get; set; }
        public string Output { get; set; }
    }
}