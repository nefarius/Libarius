using System.IO;

namespace Libarius.Filesystem
{
    /// <summary>
    ///     Utility class to provide some common file system tasks.
    /// </summary>
    public static class FilesystemHelper
    {
        /// <summary>
        ///     Checks if a file is locked e.g. the file can't get exclusive read access.
        /// </summary>
        /// <param name="file">The file info to check.</param>
        /// <returns>True if file is locked, false otherwise.</returns>
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}