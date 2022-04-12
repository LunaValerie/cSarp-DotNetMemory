using System;
using System.IO;

// Token: 0x02000003 RID: 3
internal class Logs
{
	// Token: 0x06000009 RID: 9 RVA: 0x00002180 File Offset: 0x00000380
	public static void WriteLog(string strLog)
	{
		string text = Directory.GetCurrentDirectory() + "\\";
		text = text + "Log-" + DateTime.Today.ToString("MM-dd-yyyy") + ".txt";
		FileInfo fileInfo = new FileInfo(text);
		DirectoryInfo directoryInfo = new DirectoryInfo(fileInfo.DirectoryName);
		if (!directoryInfo.Exists)
		{
			directoryInfo.Create();
		}
		FileStream stream;
		if (!fileInfo.Exists)
		{
			stream = fileInfo.Create();
		}
		else
		{
			stream = new FileStream(text, FileMode.Append);
		}
		StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.WriteLine("# >>  " + strLog);
		streamWriter.WriteLine("#-------------------------------------------------------------------------");
		streamWriter.Close();
	}

	// Token: 0x0600000A RID: 10 RVA: 0x00002228 File Offset: 0x00000428
	public static void DeleteLog()
	{
		FileInfo fileInfo = new FileInfo(Directory.GetCurrentDirectory() + "\\" + "Log-" + DateTime.Today.ToString("MM-dd-yyyy") + ".txt");
		DirectoryInfo directoryInfo = new DirectoryInfo(fileInfo.DirectoryName);
		if (!directoryInfo.Exists)
		{
			directoryInfo.Delete();
		}
		if (fileInfo.Exists)
		{
			fileInfo.Delete();
		}
	}
}
