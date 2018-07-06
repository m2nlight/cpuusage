```
cpuusage v0.1.0
Usage:
  cpuusage.exe [-f:<list|table|csv>] [-s<milliseconds>] [-t<times>] [-o:<filename>] [-p] <instance_names|*>
  cpuusage.exe -l [-o:<filename>] [-p] [<instance_names|*>]
  -f    output format, default to list.
  -s    sleep millisenconds, default to 1000.
  -t    times, default to forever.
  -p    pause when out of times.
  -o    output to file.
  -l    print name of processes only.
  -h    print help.
  -v    print version.

Example:
  cpuusage.exe _Total Idle System Svchost cpuusage
  cpuusage.exe *
  cpuusage.exe * -f:csv -s200 -t10 > 1.csv
  cpuusage.exe -f:csv -s200 -t10 chrome firefox -o:2.csv
```