using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KZCheats.Core.Services
{
    public static class MemoryHelper
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);


        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        public static bool WriteMemory(Process process, IntPtr address, byte[] value)
        {
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (hProcess == IntPtr.Zero)
                return false;

            bool result = WriteProcessMemory(hProcess, address, value, value.Length, out _);
            CloseHandle(hProcess);
            return result;
        }

        public static IntPtr ParseAddress(string address)
        {
            if (address.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                address = address.Substring(2);
            }

            return (IntPtr)Convert.ToInt64(address, 16);
        }

        public static IntPtr FollowPointerChain(Process process, IntPtr baseAddress, List<int> offsets)
        {
            IntPtr currentAddress = baseAddress;
            byte[] buffer = new byte[8];

            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process.");

            try
            {
                for (int i = 0; i < offsets.Count; i++)
                {
                    int offset = offsets[i];

                    if (i != offsets.Count - 1)
                    {
                        bool read = ReadProcessMemory(hProcess, currentAddress, buffer, buffer.Length, out _);
                        if (!read)
                            Console.WriteLine("Failed to Read Pointer");

                        long nextAddress = BitConverter.ToInt64(buffer, 0);
                        currentAddress = (IntPtr)(nextAddress + offset);
                    }
                    else
                    {
                        currentAddress = IntPtr.Add(currentAddress, offset);
                    }
                }
                return currentAddress;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        public static byte[] ReadMemory(Process process, IntPtr address, int size)
        {
            byte[] buffer = new byte[size];

            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process.");

            try
            {
                bool success = ReadProcessMemory(hProcess, address, buffer, size, out _);
                if (!success)
                    throw new Exception("Failed to read memory.");
                return buffer;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        public static int ParseOffset(string offset)
        {
            if (offset.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToInt32(offset, 16);
            }
            else
            {
                return int.Parse(offset);
            }
        }

    }
}
