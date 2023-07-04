using System.IO;

namespace PainelGerencial.Utils
{
    public static class DirectoryHelper
    {
        public static string Create(string dir)
        {
            DirectoryInfo _dir = new DirectoryInfo(dir);

            try
            {
                if (!_dir.Exists)
                    _dir.Create();

                return _dir.ToString();
            }
            catch //(Exception e)
            {
                return null;
            }
        }
    }
}
