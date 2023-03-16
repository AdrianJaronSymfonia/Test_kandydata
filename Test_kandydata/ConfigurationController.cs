using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace mplerpsw.DAL.Controllers
{
    public static class ConfigurationController
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const string _connectionStringName = "erpconnection";

        public static string GetConnectionString()
        {
            string rep = string.Empty;
            ConnectionStringSettings configSect = ConfigurationManager.ConnectionStrings[_connectionStringName];
            rep = configSect.ConnectionString;

            return rep;
        }

        public static bool UpdateSetting(string key, string value)
        {
            bool rep = false;
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string configFile = System.IO.Path.Combine(appPath, "SymfoniaService.exe.config");
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFile;
                System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                config.AppSettings.Settings[key].Value = value;
                config.Save();

                ConfigurationManager.RefreshSection("AppSettings");
                rep = true;
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
                rep = false;
            }

            return rep;
        }

        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "abc123";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "abc123";
            cipherText = cipherText.Replace(" ", "+");
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2146233033)
                    _logger.Error(ex);
            }
            return cipherText;
        }

        public static string GetDocumentType()
        {
            string rep = string.Empty;
            try
            {
                rep = ConfigurationManager.AppSettings.GetValues("DocumentTypeName").FirstOrDefault();
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
            }

            return rep; ;
        }

        public static string GetDocumentSerial()
        {
            string rep = string.Empty;
            try
            {
                rep = ConfigurationManager.AppSettings.GetValues("DocumentSerial").FirstOrDefault();
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
            }

            return rep;
        }

        public static string GetSetting(string key)
        {
            string rep = string.Empty;

            try
            {
                rep = ConfigurationManager.AppSettings.GetValues(key).FirstOrDefault();
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
            }

            return rep;
        }

        private static bool GetIsConnectionStringEncrypted(ConfigurationSection configSect)
        {
            bool rep = false;
            if (configSect.SectionInformation.IsProtected)
            {
                rep = true;
            }

            return rep;
        }

    }
}
