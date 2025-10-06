using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;

namespace Rundenzeiten
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // EPPlus 8: Lizenz EINMALIG setzen (eine der beiden Varianten wählen)
            ExcelPackage.License.SetNonCommercialOrganization("SV Remse Radsport e.V.");
            // oder:
            // ExcelPackage.License.SetNonCommercialPersonal("Yannick Runst");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
