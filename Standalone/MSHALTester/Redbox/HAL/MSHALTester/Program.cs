using System;
using System.Windows.Forms;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.MSHALTester;

public class Program : IDisposable
{
    private static Guid m_applicationGuid = new("{8609B87B-126E-489a-98E9-6818639534DF}");
    private NamedLock m_instanceLock;

    public void Dispose()
    {
        if (m_instanceLock == null)
            return;
        m_instanceLock.Dispose();
    }

    [STAThread]
    public static void Main(string[] args)
    {
        using (var program = new Program())
        {
            program.m_instanceLock = new NamedLock(m_applicationGuid.ToString());
            if (!program.m_instanceLock.IsOwned)
            {
                var num = (int)MessageBox.Show("Only one instance of the MS HAL tester is allowed to run.");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var secure = false;
                var str = "UNKNOWN";
                for (var index = 0; index < args.Length; ++index)
                    if (args[index].Equals("--secure", StringComparison.CurrentCultureIgnoreCase))
                        secure = true;
                    else if (args[index].StartsWith("--username"))
                        str = CommandLineOption.GetOptionVal(args[index], str);
                    else
                        Console.WriteLine("Unrecognized option {0}", args[index]);
                Application.Run(new Form1(secure, str));
            }
        }
    }
}