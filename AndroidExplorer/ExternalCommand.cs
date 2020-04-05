using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AndroidExplorer
{
    internal class ExternalCommand
    {
        private static SemaphoreSlim _lock;
        private const int _concurrency_count = 4;
        private string _command_path;


        static ExternalCommand()
        {
            _lock = new SemaphoreSlim(_concurrency_count);
            Adb = new ExternalCommand(@"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe");
        }

        private ExternalCommand(string command_path)
        {
            _command_path = command_path;
        }

        public static ExternalCommand Adb;


        async public Task<string> RunShell(TreeNode owner, string device_id, string arg)
        {
            return await Run(owner, string.Format("-s {0} shell {1}", device_id, arg));
        }

        async public Task<string> Run(TreeNode ownwer, string arg)
        {
            await _lock.WaitAsync();
            try
            {
                if (ownwer.IsDisconnected)
                    return null;
                var info = new ProcessStartInfo();
                info.FileName = _command_path;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.Arguments = arg;
                info.StandardOutputEncoding = Encoding.UTF8;
                info.WorkingDirectory = Path.GetDirectoryName(_command_path);
                return await Task.Run(() =>
                {
                    var proc = Process.Start(info);
                    var text = "";
                    using (var reader = proc.StandardOutput)
                    {
                        text = reader.ReadToEnd();
                    }
                    proc.WaitForExit();
                    if (ownwer.IsDisconnected)
                        return null;
                    return text;
                });
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}

