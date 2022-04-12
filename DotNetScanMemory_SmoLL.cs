using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

// Token: 0x02000004 RID: 4
public class DotNetScanMemory_SmoLL
{
	// Token: 0x0600000C RID: 12
	[DllImport("kernel32.dll")]
	public static extern uint GetLastError();

	// Token: 0x0600000D RID: 13
	[DllImport("kernel32.dll")]
	public static extern int OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	// Token: 0x0600000E RID: 14
	[DllImport("kernel32.dll")]
	protected static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesRead);

	// Token: 0x0600000F RID: 15
	[DllImport("kernel32.dll")]
	public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);

	// Token: 0x06000010 RID: 16
	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out DotNetScanMemory_SmoLL.MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000011 RID: 17 RVA: 0x00002295 File Offset: 0x00000495
	// (set) Token: 0x06000012 RID: 18 RVA: 0x0000229D File Offset: 0x0000049D
	private List<DotNetScanMemory_SmoLL.MEMORY_BASIC_INFORMATION> MappedMemory { get; set; }

	// Token: 0x06000013 RID: 19 RVA: 0x000022A6 File Offset: 0x000004A6
	public static string GetSystemMessage(uint errorCode)
	{
		return new Win32Exception((int)errorCode).Message;
	}

	// Token: 0x06000014 RID: 20 RVA: 0x000022B4 File Offset: 0x000004B4
	protected void MemInfo(IntPtr pHandle)
	{
		IntPtr intPtr = 0;
		intPtr = (IntPtr)((long)this.InicioScan);
		while ((long)intPtr <= (long)this.FimScan)
		{
			DotNetScanMemory_SmoLL.MEMORY_BASIC_INFORMATION memory_BASIC_INFORMATION = default(DotNetScanMemory_SmoLL.MEMORY_BASIC_INFORMATION);
			if (DotNetScanMemory_SmoLL.VirtualQueryEx(pHandle, intPtr, out memory_BASIC_INFORMATION, Marshal.SizeOf(memory_BASIC_INFORMATION)) == 0)
			{
				break;
			}
			if ((memory_BASIC_INFORMATION.State & 4096U) != 0U && (memory_BASIC_INFORMATION.Protect & 256U) != 256U)
			{
				this.MappedMemory.Add(memory_BASIC_INFORMATION);
			}
			intPtr = new IntPtr(memory_BASIC_INFORMATION.BaseAddress.ToInt32() + (int)memory_BASIC_INFORMATION.RegionSize);
		}
	}

	// Token: 0x06000015 RID: 21 RVA: 0x0000234C File Offset: 0x0000054C
	protected IntPtr ScanInBuff(IntPtr Address, byte[] Buff, string[] StrMask)
	{
		int num = Buff.Length;
		int num2 = StrMask.Length;
		int num3 = num2 - 1;
		byte[] array = new byte[num2];
		for (int i = 0; i < num2; i++)
		{
			if (StrMask[i] == "??")
			{
				array[i] = 0;
			}
			else
			{
				array[i] = Convert.ToByte(StrMask[i], 16);
			}
		}
		for (int j = 0; j <= num - num2 - 1; j++)
		{
			if (Buff[j] == array[0])
			{
				int num4 = num3;
				while (StrMask[num4] == "??" || Buff[j + num4] == array[num4])
				{
					if (num4 == 0)
					{
						if (this.StopTheFirst)
						{
							return new IntPtr(j);
						}
						if ((long)(Address.ToInt32() + j) >= (long)this.InicioScan && (long)(Address.ToInt32() + j) <= (long)this.FimScan)
						{
							this.AddressList.Add((IntPtr)(Address.ToInt32() + j));
							break;
						}
						break;
					}
					else
					{
						num4--;
					}
				}
			}
		}
		return IntPtr.Zero;
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00002448 File Offset: 0x00000648
	public Process GetPID(string ProcessName)
	{
		try
		{
			return Process.GetProcessesByName(ProcessName)[0];
		}
		catch
		{
		}
		return null;
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00002478 File Offset: 0x00000678
	public IntPtr[] ScanArray(Process P, string ArrayString)
	{
		EnablePrivileges.GoDebugPriv();
		IntPtr[] array = new IntPtr[1];
		Logs.DeleteLog();
		if (P == null)
		{
			return new IntPtr[1];
		}
		this.Attacked = Process.GetProcessById(P.Id);
		string[] array2 = ArrayString.Split(new char[]
		{
			" "[0]
		});
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] == "?")
			{
				array2[i] = "??";
			}
		}
		this.MappedMemory = new List<DotNetScanMemory_SmoLL.MEMORY_BASIC_INFORMATION>();
		this.MemInfo(this.Attacked.Handle);
		for (int j = 0; j < this.MappedMemory.Count; j++)
		{
			byte[] array3 = new byte[this.MappedMemory[j].RegionSize];
			DotNetScanMemory_SmoLL.ReadProcessMemory(this.Attacked.Handle, this.MappedMemory[j].BaseAddress, array3, this.MappedMemory[j].RegionSize, 0);
			IntPtr value = IntPtr.Zero;
			if (array3.Length != 0)
			{
				value = this.ScanInBuff(this.MappedMemory[j].BaseAddress, array3, array2);
			}
			if (this.StopTheFirst && value != IntPtr.Zero)
			{
				array = new IntPtr[0];
				array[0] = (IntPtr)(this.MappedMemory[j].BaseAddress.ToInt32() + value.ToInt32());
				return array;
			}
		}
		if (!this.StopTheFirst && this.AddressList.Count > 0)
		{
			array = new IntPtr[this.AddressList.Count];
			for (int k = 0; k < this.AddressList.Count; k++)
			{
				array[k] = this.AddressList[k];
			}
			this.AddressList.Clear();
			return array;
		}
		return array;
	}

	// Token: 0x06000018 RID: 24 RVA: 0x0000264C File Offset: 0x0000084C
	public bool WriteArray(IntPtr address, string ArrayString)
	{
		if (this.Attacked == null)
		{
			return false;
		}
		string[] array = ArrayString.Split(new char[]
		{
			" "[0]
		});
		byte[] array2 = new byte[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == "?" || array[i] == "??")
			{
				array2[i] = 0;
			}
			else
			{
				array2[i] = Convert.ToByte(array[i], 16);
			}
		}
		return DotNetScanMemory_SmoLL.WriteProcessMemory((int)this.Attacked.Handle, address.ToInt32(), array2, array2.Length, 0);
	}

	// Token: 0x06000019 RID: 25 RVA: 0x000026E8 File Offset: 0x000008E8
	public Process GetChrome()
	{
		foreach (Process process in Process.GetProcessesByName("chrome"))
		{
			try
			{
				using (IEnumerator enumerator = process.Modules.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (((ProcessModule)enumerator.Current).FileName.Contains("pepflashplayer.dll"))
						{
							return process;
						}
					}
				}
			}
			catch
			{
			}
		}
		return null;
	}

	// Token: 0x04000036 RID: 54
	private uint PROCESS_ALL_ACCESS = 127231U;

	// Token: 0x04000037 RID: 55
	public ulong InicioScan;

	// Token: 0x04000038 RID: 56
	public ulong FimScan = (ulong)-1;

	// Token: 0x04000039 RID: 57
	private bool StopTheFirst;

	// Token: 0x0400003A RID: 58
	private Process Attacked;

	// Token: 0x0400003B RID: 59
	private List<IntPtr> AddressList = new List<IntPtr>();

	// Token: 0x02000008 RID: 8
	protected struct MEMORY_BASIC_INFORMATION
	{
		// Token: 0x04000043 RID: 67
		public IntPtr BaseAddress;

		// Token: 0x04000044 RID: 68
		public IntPtr AllocationBase;

		// Token: 0x04000045 RID: 69
		public uint AllocationProtect;

		// Token: 0x04000046 RID: 70
		public uint RegionSize;

		// Token: 0x04000047 RID: 71
		public uint State;

		// Token: 0x04000048 RID: 72
		public uint Protect;

		// Token: 0x04000049 RID: 73
		public uint Type;
	}

	// Token: 0x02000009 RID: 9
	private enum AllocationProtectEnum : uint
	{
		// Token: 0x0400004B RID: 75
		PAGE_EXECUTE = 16U,
		// Token: 0x0400004C RID: 76
		PAGE_EXECUTE_READ = 32U,
		// Token: 0x0400004D RID: 77
		PAGE_EXECUTE_READWRITE = 64U,
		// Token: 0x0400004E RID: 78
		PAGE_EXECUTE_WRITECOPY = 128U,
		// Token: 0x0400004F RID: 79
		PAGE_NOACCESS = 1U,
		// Token: 0x04000050 RID: 80
		PAGE_READONLY,
		// Token: 0x04000051 RID: 81
		PAGE_READWRITE = 4U,
		// Token: 0x04000052 RID: 82
		PAGE_WRITECOPY = 8U,
		// Token: 0x04000053 RID: 83
		PAGE_GUARD = 256U,
		// Token: 0x04000054 RID: 84
		PAGE_NOCACHE = 512U,
		// Token: 0x04000055 RID: 85
		PAGE_WRITECOMBINE = 1024U
	}

	// Token: 0x0200000A RID: 10
	private enum StateEnum : uint
	{
		// Token: 0x04000057 RID: 87
		MEM_COMMIT = 4096U,
		// Token: 0x04000058 RID: 88
		MEM_FREE = 65536U,
		// Token: 0x04000059 RID: 89
		MEM_RESERVE = 8192U
	}

	// Token: 0x0200000B RID: 11
	private enum TypeEnum : uint
	{
		// Token: 0x0400005B RID: 91
		MEM_IMAGE = 16777216U,
		// Token: 0x0400005C RID: 92
		MEM_MAPPED = 262144U,
		// Token: 0x0400005D RID: 93
		MEM_PRIVATE = 131072U
	}
}
