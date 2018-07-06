using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace cpuusage
{
    class Program
    {
        const string VERSION = "cpuusage v0.1.0";
        static readonly Dictionary<string, PerformanceCounter> _caches = new Dictionary<string, PerformanceCounter>();
        static string _format = "list";
        static int _sleep = 1000;
        static int _times = -1;
        static bool _pauseWhenFinish = false;
        static StreamWriter  _sw = null;
        static bool _listProcessNames = false;
        static bool _printUsage = false;
        static bool _printVersion = false;

        static void Main(string[] args)
        {
            var processNames = GetProcessNamesAndParseArgs(args);
            if (_printVersion)
            {
                PrintVersion();
            }
            if (_printUsage)
            {
                PrintUsage();
            }
            else if (_listProcessNames)
            {
                PrintProcessNames(processNames);
            }
            else
            {
                switch (_format)
                {
                    case "table":
                        PrintToTable(processNames);
                        break;
                    case "csv":
                        PrintToCsv(processNames);
                        break;
                    case "list":
                        PrintToList(processNames);
                        break;
                    default:
                        Fault(1, "ERROR: -f argument error");
                        break;
                }
            }
            if (_pauseWhenFinish) {
                Console.WriteLine();
                Console.Write("Press any key to EXIT...");
                Console.ReadKey(true);
            }
            if (_sw != null)
            {
                _sw.Close();
            }
        }
        
        static void Fault(int returnCode, string message)
        {
            Console.WriteLine(message);
            if (_sw != null)
            {
                _sw.Close();
            }
            Environment.Exit(returnCode);
        }
        
        static void PrintProcessNames(string[] processNames)
        {
            foreach (var name in processNames)
            {
                Output(string.Format("{0}{1}", name, Environment.NewLine));
            }
        }
        
        static void PrintToList(string[] processNames)
        {
            if (processNames == null || processNames.Length == 0)
            {
                return;
            }
            const string nameTitle = "Name";
            const string cpuUsageTitle = "CPU Usage (%)";
            var nameColumnMaxLength = Math.Max(processNames.Max(n => n.Length), nameTitle.Length);
            var cpuUsageColumnMaxLength = cpuUsageTitle.Length;
            var format = string.Format("{{0,-{0}}}  {{1:0.##}}", nameColumnMaxLength);
            var head = string.Format(format, nameTitle, cpuUsageTitle).ToUpper();
            head += Environment.NewLine + string.Format(format, new string('-', nameColumnMaxLength), new string('-', cpuUsageColumnMaxLength));
            var sb = new StringBuilder();
            while (_times != 0)
            {
                sb.AppendLine(head);
                foreach (var name in processNames)
                {
                    try
                    {
                        sb.AppendFormat(format, name, GetProcessCpuUsage(name));
                        sb.AppendLine();
                    }
                    catch(Exception)
                    {
                    }
                }
                Output(sb.ToString());
                sb.Clear();
                if(_times > 0) {
                    if (--_times == 0) {
                        break;
                    }
                }
                Thread.Sleep(_sleep);
                sb.AppendLine();
            }
        }
        
        static void PrintToTable(string[] processNames)
        {
            if (processNames == null || processNames.Length == 0)
            {
                return;
            }
            var sb = new StringBuilder();
            var sb1 = new StringBuilder();
            foreach (var name in processNames)
            {
                sb.AppendFormat("{0,-6}  ", name);
                sb1.AppendFormat("{0}  ", new string('-', Math.Max(name.Length, 6)));
            }
            sb.Remove(sb.Length - 2, 2);
            sb1.Remove(sb1.Length - 2, 2);
            sb.AppendLine();
            sb.Append(sb1.ToString());
            sb.AppendLine();
            var head = sb.ToString();
            Output(head);
            sb1 = null;
            sb.Clear();
            while (_times != 0)
            {
                for (int i = 0; i < processNames.Length; i++)
                {
                    var name = processNames[i];
                    var value = "";
                    try
                    {
                        value = GetProcessCpuUsage(name).ToString("0.00");
                    }
                    catch(Exception)
                    {
                    }
                    var length = Math.Max(name.Length, 6);
                    value = value.PadLeft(length);
                    if (i + 1 != processNames.Length) {
                        value = string.Format("{0}  ", value);
                    }
                    sb.Append(value);
                }
                Output(sb.ToString());
                sb.Clear();
                if(_times > 0) {
                    if (--_times == 0) {
                        break;
                    }
                }
                Thread.Sleep(_sleep);
                sb.AppendLine();
            }
        }
        
        static void PrintToCsv(string[] processNames)
        {
            if (processNames == null || processNames.Length == 0)
            {
                return;
            }
            var sb = new StringBuilder();
            foreach (var name in processNames)
            {
                var tempName = name.Replace("\"", "\"\"");
                if (name.Contains(",") || name.Contains(" ") || name.Contains("\""))
                {
                    tempName = string.Format("\"{0}\"", tempName);
                }
                sb.AppendFormat("{0},", tempName);
            }
            sb.Remove(sb.Length - 1, 1);
            sb.AppendLine();
            var head = sb.ToString();
            Output(head);
            sb.Clear();
            while (_times != 0)
            {
                for (int i = 0; i < processNames.Length; i++)
                {
                    var name = processNames[i];
                    var value = "";
                    try
                    {
                        value = GetProcessCpuUsage(name).ToString("0.00");
                    }
                    catch(Exception)
                    {
                    }
                    if (i + 1 != processNames.Length)
                    {
                        value = string.Format("{0},", value);
                    }
                    sb.Append(value);
                }
                Output(sb.ToString());
                sb.Clear();
                if(_times > 0) {
                    if (--_times == 0) {
                        break;
                    }
                }
                Thread.Sleep(_sleep);
                sb.AppendLine();
            }
        }

        static string[] GetProcessNamesAndParseArgs(string[] args)
        {
            if (args.Any(n => n.ToLower() == "-v"))
            {
                _printVersion = true;
            }
            if (args.Length == 0 || args.Any(n => n.ToLower() == "-h"))
            {
                _printUsage = true;
                _printVersion = true;
                _pauseWhenFinish = true;
                return null;
            }
            _pauseWhenFinish = args.Any(n => n.ToLower() == "-p");
            if (args.Any(n => n.ToLower() == "-l"))
            {
                _listProcessNames = true;
            }
            var arg = args.FirstOrDefault(n => n.ToLower().StartsWith("-f:"));
            if (arg != null) {
                _format = arg.Substring(3).ToLower();
            }
            arg = args.FirstOrDefault(n => n.ToLower().StartsWith("-s"));
            if (arg != null) {
                int s;
                if (int.TryParse(arg.Substring(2), out s)) {
                    _sleep = s;
                }
            }
            arg = args.FirstOrDefault(n => n.ToLower().StartsWith("-t"));
            if (arg != null) {
                int t;
                if (int.TryParse(arg.Substring(2), out t)) {
                    _times = t;
                }
            }
            arg = args.FirstOrDefault(n => n.ToLower().StartsWith("-o:"));
            if (arg != null) {
                var output = arg.Substring(3).ToLower();
                try
                {
                    _sw = File.CreateText(output);
                }
                catch(Exception ex)
                {
                    if (_sw != null)
                    {
                        _sw.Close();
                    }
                    _sw = null;
                    Fault(2, string.Format("ERROR: {0}", ex.Message));
                }
            }
            
            if (args.Contains("*"))
            {
                return Process.GetProcesses().Select(n => n.ProcessName).OrderBy(n => n).Distinct().ToArray();
            }

            var r = args.Where(n => !n.StartsWith("-")).Select(n => GetFriendlyName(n)).ToArray();
            if (_listProcessNames && r.Length == 0)
            {
                return Process.GetProcesses().Select(n => n.ProcessName).OrderBy(n => n).Distinct().ToArray();
            }
            return r;
        }
        
        static void Output(string message)
        {
            Console.Write(message);
            if (_sw == null)
            {
                return;
            }
            try
            {
                _sw.Write(message);
                _sw.Flush();
            }
            catch (Exception)
            {
                _sw.Close();
                _sw = null;
            }
        }
        
        static void PrintUsage()
        {
            var n = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
            var n1 = Path.GetFileNameWithoutExtension(n);
            Console.Write("Usage:{2}  {0} [-f:<list|table|csv>] [-s<milliseconds>] [-t<times>] [-o:<filename>] [-p] <instance_names|*>{2}  {0} -l [-o:<filename>] [-p] [<instance_names|*>]{2}  -f    output format, default to list.{2}  -s    sleep millisenconds, default to 1000.{2}  -t    times, default to forever.{2}  -p    pause when out of times.{2}  -o    output to file.{2}  -l    print name of processes only.{2}  -h    print help.{2}  -v    print version.{2}{2}Example:{2}  {0} _Total Idle System Svchost {1}{2}  {0} *{2}  {0} * -f:csv -s200 -t10 > 1.csv{2}  {0} -f:csv -s200 -t10 chrome firefox -o:2.csv{2}", n, n1, Environment.NewLine);
        }
        
        static void PrintVersion()
        {
            Console.WriteLine(VERSION);
        }
        
        static string GetFriendlyName(string instanceName)
        {
            var r = new StringBuilder(instanceName);
            for(int i=0; i<r.Length; i++)
            {
                var ch = r[i];
                if (ch=='(')
                {
                    r[i] = '[';
                    continue; 
                }
                if (ch==')')
                {
                    r[i] = ']';
                    continue;
                }
                if (ch=='#' || ch=='\\' || ch== '/')
                {
                    r[i] = '_';
                    continue;
                }
            }
            return r.ToString();
        }

        static float GetProcessCpuUsage(string instanceName)
        {
            // var total = GetPerformanceCounter("_Total").NextValue();
            var value = GetPerformanceCounter(instanceName).NextValue();
            return value / Environment.ProcessorCount;
        }

        static PerformanceCounter GetPerformanceCounter(string instanceName)
        {
            PerformanceCounter r;
            if (_caches.TryGetValue(instanceName, out r))
            {
                return r;
            }
            r = new PerformanceCounter("Process", "% Processor Time", instanceName);
            _caches[instanceName] = r;
            return r;
        }
        
    }
}
