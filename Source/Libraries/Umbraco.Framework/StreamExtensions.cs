using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Umbraco.Framework
{
    public static class StreamExtensions
    {

        /// <summary>
        /// Will ensure that all bytes are read from the stream regardless of where it's coming from, this will NOT close the stream, the caller must do that
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <remarks>
        /// See: http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream
        /// </remarks>
        public static byte[] ReadAllBytes(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

    }
}
