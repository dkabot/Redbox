using Redbox.Core;

namespace Redbox.Shell
{
    public class Options
    {
        private Options()
        {
        }

        public static Options Instance => Singleton<Options>.Instance;
    }
}