using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace KISS
{
    public class ArgumentHelper
    {
        public static void processProps(List<string> largs)
        {
            string props = largs.FirstOrDefault(x => x.Contains("/props:"));
            if (!String.IsNullOrEmpty(props))
            {
                string filename = props.Replace(@"/props:", "");
                if (string.IsNullOrEmpty(filename))
                    filename = "KISSprops.xml";

                Properties properties = new Properties();
                properties.ConnectionString = "Put Conn String here";
                properties.Provider = "SQLServer";
                properties.Verbose = true;
                properties.VersionTable = "VersionTableNameHere";
                properties.VersionScriptsFolder = @"C:\dev\KISS";


                StringWriter Output = new StringWriter(new StringBuilder());
                string Ret = "";

                XmlSerializer s = new XmlSerializer(properties.GetType());
                s.Serialize(Output, properties);
                Ret = Output.ToString();
                File.WriteAllText(filename, Ret);
            }

            
        }

        public static bool processScripts(List<string> largs)
        {
            bool success = true;
            string file = largs.FirstOrDefault(x => x.Contains("/f:"));
            if (!String.IsNullOrEmpty(file))
            {
                string filename = file.Replace(@"/f:", "");
                Properties properties = new Properties();
                XmlSerializer s = new XmlSerializer(properties.GetType());
                TextReader r = new StreamReader(filename);
                properties = (Properties)s.Deserialize(r);
                if (!properties.VersionScriptsFolder.Contains(":") || !properties.VersionScriptsFolder.Contains(@"\\") )
                {
                   var settingPath = Path.GetDirectoryName(filename);
                   properties.VersionScriptsFolder = Path.Combine(settingPath, properties.VersionScriptsFolder);
                }
                r.Dispose();
                success =  UpdateDB.Process(properties);
                if(properties.Wait)
                    Console.Read();
            }

            return success;
        }

        public static Encoding GetEncoding(string encodingName)
        {
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(encodingName);
            }
            catch (System.ArgumentException)
            {
                encoding = Encoding.Default;
            }
            return encoding;
        }
    }
}
