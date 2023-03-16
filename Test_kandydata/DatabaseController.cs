using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mplerpsw.DAL;
using Test_kandydata;

namespace mplerpsw.DAL.Controllers
{
    public static class DatabaseController
    {
        public static HmfModelDataContext GetDatabaseContext()
        {
            string connStr = mplerpsw.DAL.Controllers.ConfigurationController.GetConnectionString();

            string sqlAccount = mplerpsw.DAL.Controllers.ConfigurationController.GetSetting("SqlAccount");

            string sqlPassDec = mplerpsw.DAL.Controllers.ConfigurationController.GetSetting("SqlPass");

            string sqlPassword = mplerpsw.DAL.Controllers.ConfigurationController.Decrypt(sqlPassDec);

            connStr = connStr.Replace("@userName", sqlAccount);
            connStr = connStr.Replace("@password", sqlPassword);

            return new HmfModelDataContext(connStr);
        }
    }
}
