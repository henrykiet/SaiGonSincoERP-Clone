using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

public static class MachineIdHelper
{
    public static string GetMachineId()
    {
        var cpuId = GetCpuId();
        var mac = GetMacAddress();
        var disk = GetDiskSerialNumber();

        string raw = $"{cpuId}-{mac}-{disk}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return BitConverter.ToString(hash).Replace("-", "");
    }

    private static string GetCpuId()
    {
        try
        {
            using (var mc = new ManagementClass("Win32_Processor"))
            {
                foreach (var mo in mc.GetInstances())
                    return mo["ProcessorId"]?.ToString() ?? "CPU_NULL";
            }
        }
        catch { return "CPU_ERR"; }

        return "CPU_UNKNOWN"; // 👈 Thêm dòng này để đảm bảo return ở mọi path
    }


    private static string GetMacAddress()
    {
        try
        {
            using var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            foreach (var mo in mc.GetInstances())
            {
                if ((bool)(mo["IPEnabled"] ?? false))
                    return mo["MACAddress"]?.ToString() ?? "";
            }
        }
        catch { }
        return "MAC_UNKNOWN";
    }

    private static string GetDiskSerialNumber()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
            foreach (ManagementObject mo in searcher.Get())
            {
                var serial = mo["SerialNumber"]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(serial))
                    return serial;
            }
        }
        catch { }
        return "DISK_UNKNOWN";
    }
}
