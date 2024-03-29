﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameHelper.Interfaces.LowLevel;

namespace GameHelper.Utils
{
    public class PortDetector
    {
        public IReadOnlyCollection<ushort> GetUdpPorts(string processName)
        {
            if (string.IsNullOrEmpty(processName)) throw new ArgumentNullException(nameof(processName));

            return new ushort[] { 27000 };

            var processes = Process.GetProcessesByName(processName);
            return GetPorts(processes.Single().Id, "UDP");
        }

        public IReadOnlyCollection<ushort> GetTcpPorts(string processName)
        {
            if (string.IsNullOrEmpty(processName)) throw new ArgumentNullException(nameof(processName));

            var processes = Process.GetProcessesByName(processName);
            return GetPorts(processes.Single().Id, "TCP");
        }

        public IReadOnlyCollection<Channel> GetChannels(string processName)
        {
            if (processName.Equals("NewWorld"))
            {
                return GetTcpPorts(processName)
                    .Select(p => new Channel(p, ChannelProtocol.TCP))
                    //.Union(new[] { new Channel(27000, ChannelProtocol.UDP) })
                    .ToArray();
            }

            throw new NotImplementedException();
        }

        private protected IReadOnlyCollection<ushort> GetPorts(int processId, string protocol)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netstat.exe",
                    Arguments = "-a -o -p " + protocol,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var content = process.StandardOutput.ReadToEnd();

            var result = new List<ushort>();

            var rows = content.Split(Environment.NewLine);
            foreach (var row in rows)
                if (row.Contains(protocol))
                {
                    var parts = row.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    var pId = int.Parse(parts[^1]);
                    if (pId == processId)
                    {
                        var port = ushort.Parse(parts[1].Split(":")[1]);
                        result.Add(port);
                    }
                }

            return result.OrderBy(p => p).ToArray();
        }
    }
}
