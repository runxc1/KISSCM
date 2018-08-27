using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KISS.DBProvider
{
    public interface IDBProvider
    {
        int VersionNo();
        void ExecuteScriptAndUpdateVersionTable(int versionNo, FileInfo fi);
        string GetVersionTableSchema();
    }
}
