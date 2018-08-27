using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KISS.DBProvider;
using System.IO;

namespace KISS
{
    public class UpdateDB
    {
        public static bool Process(Properties props)
        {
            bool success = true;
            IDBProvider provider;

            switch (props.Provider.ToUpper())
            {
                case "SQLSERVER":
                    provider = new SQLServer(props);
                    break;
                default:
                    throw new Exception("Invalid DB Provider Provided, Excepted Providers are MSQL");
            }

            int versionNo = provider.VersionNo();
            if (props.Verbose)
                Console.WriteLine("DataBase is Currently at Version: {0}", versionNo);
            DirectoryInfo directory =  new DirectoryInfo(props.VersionScriptsFolder);
            FileInfo[] files = directory.GetFiles("*.sql");
            List<FileInfo> goodFiles = new List<FileInfo>();
            int scriptNo;
            //ignore any file that does not have a number
            foreach (FileInfo file in files)
            {
                if (file.Name.Length > 1 && file.Name.Contains("-") && Int32.TryParse(file.Name.Substring(0, file.Name.IndexOf("-")), out scriptNo))
                    goodFiles.Add(file);
            }

            // Lets now just grab the files that still need to be processed i.e. version number greater than that of the DB
            goodFiles = goodFiles.Where(x => Convert.ToInt32(x.Name.Substring(0, x.Name.IndexOf("-"))) > versionNo).OrderBy(y => y.Name).ToList();
            foreach (FileInfo file in goodFiles)
            {
                if (props.Verbose)
                    Console.WriteLine("Executing the Script File: {0}", file.FullName);
                try
                {
                    versionNo++;
                    provider.ExecuteScriptAndUpdateVersionTable(versionNo, file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error running script {0}", file.FullName);
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException.Message);
                    success = false;
                    versionNo--;
                    break;
                }
            }
            if (goodFiles.Count == 0 && props.Verbose)
                Console.WriteLine("There are no new DataBase scripts to execute");
            if (props.Verbose)
                Console.WriteLine("Done executing scripts.  The Database is now at Version: {0}", versionNo);
            return success;
        }
        
    }
}
