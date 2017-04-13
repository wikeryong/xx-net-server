using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xx_tcp
{
    public class PrintUtils
    {
        private static log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void PrintHex(string msg)
        {
            PrintHex(Encoding.GetEncoding("GBK").GetBytes(msg));
        }
        public static void PrintHex(byte[] bytes)
        {
            LOG.Debug("---------------------------------- Raw  data ---------------------------------");
            string tmp = "|OFFSET   | ";
            for (int i = 0; i < 16; i++)
            {
                tmp += i.ToString("X").PadLeft(2, '0') + " ";
            }
            tmp += "|ASCII           |";
            LOG.Debug(tmp);
            tmp = "";
            int lineCnt = 0;
            int dataLen = bytes.Length;
            if (dataLen % 16 != 0)
            {
                lineCnt = dataLen / 16 + 1;
            }
            else
            {
                lineCnt = dataLen / 16;
            }
            for (int i = 0; i < lineCnt; i++)
            {
                tmp += "|" + (i + 1).ToString("X").PadLeft(8, '0') + " | ";
                byte[] lineBytes = new byte[16];
                for (int j = 0; j < 16; j++)
                {
                    if ((i * 16 + j) < dataLen)
                    {
                        lineBytes[j] = bytes[j + i * 16];
                        tmp += bytes[j + i * 16].ToString("X").PadLeft(2, '0') + " ";
                    }
                    else
                    {
                        tmp += "   ";
                    }

                }
                tmp += "|" + Encoding.ASCII.GetString(lineBytes).Replace("?", ".") + "|";
                LOG.Debug(tmp);
                tmp = "";
            }
        }
    }
}
