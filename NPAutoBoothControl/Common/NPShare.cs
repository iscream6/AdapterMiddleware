using System;
using System.Text;
using System.Net;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Drawing;

namespace Common
{
    public class NPShare
    {
        int _Timeout = 120;
        String _SendData = "Test";
        String _currImagePath = null;

        public NPShare() { this.Encoding = Encoding.Default; }

        private int PingTimeout { set { _Timeout = value; } get { return _Timeout; } }
        private String PingSendData { set { _SendData = value; } get { return _SendData; } }

        public String CurrentImagePath
        {
            get { return _currImagePath; }
            set { _currImagePath = value; }
        }

        public Encoding Encoding { get; set; }

        /// <summary>
        /// Ping
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public Boolean NetPing(String IP)
        {
            # region Ping Test

            PingOptions options = new PingOptions();
            byte[] buffer = Encoding.Default.GetBytes(PingSendData);

            options.DontFragment = true;
            try
            {
                PingReply reply = new Ping().Send(IPAddress.Parse(IP), PingTimeout, buffer, options);

                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;

            }
            #endregion
        }

        /// <summary>
        /// 문자열을 반대로 바꿈
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public String StringReverse(String s)
        {
            #region 
            Array a = s.ToCharArray();
            Array.Reverse(a);

            return Encoding.Default.GetString((byte[])Encoding.Default.GetBytes((char[])a));
            #endregion
        }


        public String Right(String s, int nSize)
        {
            #region
            int nLen = s.Length;
            int nGet = nSize;

            if (nLen < 1) return String.Empty;
            if (nLen < nGet) nGet = nLen;
            if (nGet == 0) nGet = 1;

            return s.Substring(nLen - 1, nGet);
            #endregion
        }

        public Boolean IsDigit(String s) { return System.Text.RegularExpressions.Regex.IsMatch(s, "^\\d+$"); }
        public Boolean IsNumeric(String s) { return System.Text.RegularExpressions.Regex.IsMatch(s, @"^[+-]?\d*$"); }


        /// <summary>
        /// 네트웍이 살아 있는지 확인
        /// </summary>
        /// <returns></returns>
        public Boolean IsNetworkConnect()
        {
            #region 
            return NetworkInterface.GetIsNetworkAvailable();
            #endregion
        }

        private const int CONNECT_UPDATE_PROFILE = 0x01;
        private const int NO_ERROR = 0x00;
        private enum ResourceScope { RESOURCE_CONNECTED = 1, RESOURCE_GLOBALNET, RESOURCE_REMEMBERED, RESOURCE_RECENT, RESOURCE_CONTEXT }
        private enum ResourceType { RESOURCETYPE_ANY, RESOURCETYPE_DISK, RESOURCETYPE_PRINT, RESOURCETYPE_RESERVED }
        private enum ResourceUsage
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010
        }
        private enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NETRESOURCE
        {
            public ResourceScope dwScope;
            public ResourceType dwType;
            public ResourceDisplayType dwDisplayType;
            public ResourceUsage dwUsage;
            public String lpLocalName;
            public String lpRemoteName;
            public String lpComments;
            public String lpProvider;
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(ref NETRESOURCE oNetworkResource, string sPassword, string sUserName, int iFlags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string sLocalName, uint iFlags, int iForce);

        public Boolean ConnectShareFolder(String FolderName, String sUser, String sPassword)
        {
            return ConnectShareFolder(FolderName, sUser, sPassword, false);
        }
        public Boolean ConnectShareFolder(String FolderName, String sUser, String sPassword, Boolean bShow)
        {
            try
            {
                WNetCancelConnection2(FolderName, CONNECT_UPDATE_PROFILE, 1);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.Write(e.ToString());
            }

            NETRESOURCE net = new NETRESOURCE();

            net.lpRemoteName = FolderName;
            net.lpLocalName = bShow ? "Z:" : String.Empty;
            net.dwScope = ResourceScope.RESOURCE_CONNECTED;
            net.dwType = bShow ? ResourceType.RESOURCETYPE_DISK : ResourceType.RESOURCETYPE_ANY;
            net.dwDisplayType = ResourceDisplayType.RESOURCEDISPLAYTYPE_SHARE;
            net.dwUsage = ResourceUsage.RESOURCEUSAGE_CONNECTABLE;

            return WNetAddConnection2(ref net, sPassword, sUser, CONNECT_UPDATE_PROFILE) == NO_ERROR;
        }

        /// <summary>
        /// 환경설정파일(.config)에서 값을 읽어옴
        /// 
        /// ExeConfigurationFileMap 을 사용하기 위해서는 참조에 System.Configuration 을 추가해야 한다.
        /// </summary>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public String GetAppValue(String sKey)
        {
            #region
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();

            String strFileName = Path.ChangeExtension(Application.ExecutablePath, ".config").ToUpper();

            configFileMap.ExeConfigFilename = strFileName;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            try
            {
                return config.AppSettings.Settings[sKey].Value;
            }
            catch
            {
                return String.Empty;
            }
            #endregion
        }

        /// <summary>
        /// 환경설정파일(.config)에 값을 저장
        /// </summary>
        /// <param name="sKey"></param>
        /// <param name="sValue"></param>
        public void SetAppValue(String sKey, String sValue)
        {
            #region
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();

            String strFileName = Path.ChangeExtension(Application.ExecutablePath, ".config").ToUpper();

            configFileMap.ExeConfigFilename = strFileName;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            try
            {
                AppSettingsSection cfSection = config.AppSettings;
                cfSection.Settings.Remove(sKey);
                cfSection.Settings.Add(sKey, sValue);
                config.Save(ConfigurationSaveMode.Full);
            }
            catch
            {
            }
            #endregion
        }

        public Boolean IsFileExists(String srcPath)
        {
            bool exists = false;
            Thread t = new Thread(
                                  new ThreadStart(delegate ()
                                  {
                                      exists = System.IO.File.Exists(srcPath);
                                  })
                                 );
            t.Start();
            bool completed = t.Join(500); //half a sec of timeout
            if (!completed) { exists = false; t.Abort(); }
            return exists;
        }


        public void StretchImageFromFile(Label oLabel, String sFileName)
        {
            Image oImage = Image.FromFile(sFileName);

            Bitmap currImage = new Bitmap(oLabel.Width, oLabel.Height);
            Graphics g = Graphics.FromImage(currImage);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(oImage, 0, 0, currImage.Width, currImage.Height);
            oLabel.Image = currImage;

            oImage = null;
            currImage = null;
        }

        public void StretchImageFromFile(Form oForm, String sFileName)
        {
            Image oImage = Image.FromFile(sFileName);

            Bitmap currImage = new Bitmap(oForm.Width, oForm.Height);
            Graphics g = Graphics.FromImage(currImage);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(oImage, 0, 0, currImage.Width, currImage.Height);

            oForm.BackgroundImage = currImage;

            oImage = null;
            currImage = null;
        }

        public void StretchImageFromResource(Label oLabel, Image pImage)
        {
            Image oImage = pImage;

            Bitmap currImage = new Bitmap(oLabel.Width, oLabel.Height);
            Graphics g = Graphics.FromImage(currImage);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(oImage, 0, 0, currImage.Width, currImage.Height);
            oLabel.Image = currImage;

            oImage = null;
            currImage = null;
        }

        public Image GetStretchImageFromFile(String sFileName, Size ImageSize)
        {
            Image oImage = Image.FromFile(sFileName);

            Bitmap currImage = new Bitmap(ImageSize.Width, ImageSize.Height);
            Graphics g = Graphics.FromImage(currImage);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(oImage, 0, 0, currImage.Width, currImage.Height);

            oImage = null;

            return currImage;
        }
    }

    /// <summary>
    /// Event 로그 작성
    /// </summary>
    public class NPEventLog
    {
        const String EVENTLOG_INFORMATION = "INFORMATION";
        const String INIT_SUCCESS = " 성공적으로 초기화 되었습니다.";


        public NPEventLog(string strLogName)
        {
            CreateLog(strLogName);
        }


        public Boolean CreateLog(string strLogName)
        {
            Boolean Reasult = false;

            try
            {
                System.Diagnostics.EventLog.CreateEventSource(strLogName, strLogName);
                System.Diagnostics.EventLog SQLEventLog = new System.Diagnostics.EventLog();

                SQLEventLog.Source = strLogName;
                SQLEventLog.Log = strLogName;

                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry(strLogName + INIT_SUCCESS, EventLogEntryType.Information);


                Reasult = true;

            }
            catch
            {
                Reasult = false;
            }


            return Reasult;
        }


        public void WriteToEventLog(String strLogName, String strSource, String strErrDetail)
        {
            System.Diagnostics.EventLog SQLEventLog = new System.Diagnostics.EventLog();

            try
            {
                if (!System.Diagnostics.EventLog.SourceExists(strLogName)) this.CreateLog(strLogName);

                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry(Convert.ToString(strSource) + Convert.ToString(strErrDetail), EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry(Convert.ToString(EVENTLOG_INFORMATION) + Convert.ToString(ex.Message), EventLogEntryType.Information);
            }
            finally
            {
                SQLEventLog.Dispose();
                SQLEventLog = null;
            }
        }
    }


    /// <summary>
    /// 로그
    /// </summary>
    public class NPLog
    {
        public void DoEventLog(String logmsg)
        {
            EventLog e = new EventLog();
            String eSource = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

            if (!EventLog.SourceExists(eSource))
                EventLog.CreateEventSource(eSource, EventLogEntryType.Error.ToString());

            e.Source = eSource;
            e.WriteEntry(logmsg, EventLogEntryType.Error, 1);
            e.Close();
        }


        public void DoEventLog(EventLogEntryType eType, int eID, String logmsg)
        {
            EventLog e = new EventLog();
            String eSource = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

            if (!EventLog.SourceExists(eSource))
                EventLog.CreateEventSource(eSource, eType.ToString());

            e.Source = eSource;
            e.WriteEntry(logmsg, eType, eID);
            e.Close();
        }


        public void DoLog(String msg)
        {
            /*
            String sFile = Application.ExecutablePath;
            String sFullName = Path.GetFileName(sFile);
            String sFileName = Path.GetFileNameWithoutExtension(sFile);
            String sPath = (sFile.IndexOf(sFullName) != -1 ? sFile.Substring(0, sFile.IndexOf(sFullName)) : String.Empty) + @"log\" + DateTime.Now.ToString("yyyyMMdd") + @"\";

            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);

            String logFile = sPath + sFileName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            if (!File.Exists(logFile))
            {
                FileStream f = File.Create(logFile);
                f.Close();
            }

            try
            {
                StreamWriter sw = File.AppendText(logFile);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "] " + msg);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Trace.Write(e.ToString());
            }
            */
            String sYMD = DateTime.Now.ToString("yyyyMMdd");
            String sYMDPath = String.Format(@"{0}\{1}\{2}\", sYMD.Substring(0, 4), sYMD.Substring(4, 2), sYMD.Substring(6, 2));
            String sFile = Application.ExecutablePath;
            String sFullName = Path.GetFileName(sFile);
            String sFileName = Path.GetFileNameWithoutExtension(sFile);
            String sPath = (sFile.IndexOf(sFullName) != -1 ? sFile.Substring(0, sFile.IndexOf(sFullName)) : String.Empty) + @"log\" + sYMDPath;

            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);

            String logFile = sPath + sFileName + "_" + sYMD + ".log";
            if (!File.Exists(logFile))
            {
                FileStream f = File.Create(logFile);
                f.Close();
            }

            try
            {
                StreamWriter sw = File.AppendText(logFile);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "] " + msg);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Trace.Write(e.ToString());
            }
        }

        public void DoLog(String fileName, String msg)
        {
            /*
            String sFile = Application.ExecutablePath;
            String sFullName = Path.GetFileName(sFile);
            String sFileName = Path.GetFileNameWithoutExtension(sFile);
            String s = Path.GetFullPath(sFileName);
            String sPath = (sFile.IndexOf(sFullName) != -1 ? sFile.Substring(0, sFile.IndexOf(sFullName)) : String.Empty) + @"log\" + DateTime.Now.ToString("yyyyMMdd") + @"\";

            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);

            String logFile = sPath + fileName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            if (!File.Exists(logFile))
            {
                FileStream f = File.Create(logFile);
                f.Close();
            }

            try
            {
                StreamWriter sw = File.AppendText(logFile);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "] " + msg);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Trace.Write(e.ToString());
            }
            */
            String sYMD = DateTime.Now.ToString("yyyyMMdd");
            String sYMDPath = String.Format(@"{0}\{1}\{2}\", sYMD.Substring(0, 4), sYMD.Substring(4, 2), sYMD.Substring(6, 2));
            String sFile = Application.ExecutablePath;
            String sFullName = Path.GetFileName(sFile);
            String sFileName = Path.GetFileNameWithoutExtension(sFile);
            String s = Path.GetFullPath(sFileName);
            String sPath = (sFile.IndexOf(sFullName) != -1 ? sFile.Substring(0, sFile.IndexOf(sFullName)) : String.Empty) + @"log\" + sYMDPath;

            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);

            String logFile = sPath + fileName + "_" + sYMD + ".log";
            if (!File.Exists(logFile))
            {
                FileStream f = File.Create(logFile);
                f.Close();
            }

            try
            {
                StreamWriter sw = File.AppendText(logFile);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "] " + msg);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Trace.Write(e.ToString());
            }
        }
    }


    public class ListViewColumnSorter : System.Collections.IComparer
    {
        private int ColumnToSort;
        private System.Windows.Forms.SortOrder OrderOfSort;
        private CaseInsensitiveComparer ObjectCompare;

        public ListViewColumnSorter()
        {
            ColumnToSort = 0;

            OrderOfSort = System.Windows.Forms.SortOrder.Descending;
            ObjectCompare = new CaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            ListViewItem listviewX, listviewY;

            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            int compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

            if (OrderOfSort == SortOrder.Ascending)
            {
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                return (-compareResult);
            }
            else
            {
                return 0;
            }
        }

        public int SortColumn { get { return ColumnToSort; } set { ColumnToSort = value; } }
        public SortOrder Order { get { return OrderOfSort; } set { OrderOfSort = value; } }
    }
}
