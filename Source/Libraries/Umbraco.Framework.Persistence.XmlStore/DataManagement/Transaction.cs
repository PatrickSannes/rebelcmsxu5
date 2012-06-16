using System;
using System.IO;
using System.Xml.Linq;

using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.DataManagement;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement
{
    public class Transaction : AbstractTransaction
    {
        private string _transactionPath = string.Empty;
        private readonly string _path;
        private XDocument _xDocument;

        public Transaction(XDocument xDocument, string path)
        {
            _xDocument = xDocument;
            _path = path;

            Begin();
        }

        private void Begin()
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, "DemoData...Transaction:Begin");

            GenerateTransactionPath();

#if DEBUG
            Console.WriteLine(string.Format("In DemoData.Begin, about to File.Copy"));
#endif
            File.Copy(_path, _transactionPath);

            // Reload xml from transaction path
            _xDocument = XDocument.Load(_transactionPath);
        }

        private void GenerateTransactionPath()
        {

            if (_transactionPath != string.Empty)
                throw new InvalidOperationException("This transaction is already begun");

            _transactionPath = GenerateBackupPath("open-transaction");

#if DEBUG
            Console.WriteLine(string.Format("In DemoData.GenerateTransactionPath ({0})", _transactionPath));
#endif
        }

        private string GenerateBackupPath(string key)
        {
            return Path.ChangeExtension(_path,
                                 string.Format(".{0}-{1}.xml", key, DateTime.UtcNow.Ticks));
        }

        private void DeleteTransactionFile()
        {
            File.Delete(_transactionPath);
        }

        /// <summary>
        /// Both rollbacks are currently the same, reload the data from the original doc and delete the transaction file
        /// </summary>
        /// <returns></returns>
        protected override bool PerformExplicitRollback()
        {
            if (_transactionPath == string.Empty)
                return false;

            _xDocument = XDocument.Load(_path);
            File.Delete(_transactionPath);
            return true;
        }

        /// <summary>
        /// Both rollbacks are currently the same, reload the data from the original doc and delete the transaction file
        /// </summary>
        /// <returns></returns>
        protected override bool PerformImplicitRollback()
        {
            return PerformExplicitRollback();
        }

        protected override bool PerformCommit()
        {
            // Copy the existing to a backup
            var backupPath = GenerateBackupPath("current-backup");

#if DEBUG
            Console.WriteLine(string.Format("In DemoData.Commit, about to File.Copy (backup)"));
#endif
            File.Copy(_path, backupPath);

#if DEBUG
            Console.WriteLine(string.Format("In DemoData.Commit, about to save over original"));
#endif
            // Write changes to existing path
            _xDocument.Save(_path);

#if DEBUG
            Console.WriteLine(string.Format("In DemoData.Commit, about to File.Delete (backup)"));
#endif
            // Remove the backup
            File.Delete(backupPath);
            DeleteTransactionFile();

            return true;
        }

        protected override void DisposeResources()
        {
            base.DisposeResources();
            try
            {
                DeleteTransactionFile();
            }
            catch
            {
                return;
            }
        }
    }
}