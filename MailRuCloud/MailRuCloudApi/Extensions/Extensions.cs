using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using YaR.Clouds.Base;

namespace YaR.Clouds.Extensions
{
    public static class Extensions
    {
        internal static IEnumerable<PublicLinkInfo> ToPublicLinkInfos(this string uristring, string publicBaseUrl)
        {
            if (!string.IsNullOrEmpty(uristring))
                yield return new PublicLinkInfo("", publicBaseUrl, uristring);
        }


        internal static DateTime ToDateTime(this ulong unixTimeStamp)
        {
            var dtDateTime = Epoch.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        internal static long ToUnix(this DateTime date)
        {
            TimeSpan diff = date.ToUniversalTime() - Epoch;

            long seconds = diff.Ticks / TimeSpan.TicksPerSecond;
            return seconds;
        }
        private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        internal static byte[] HexStringToByteArray(this string hex)
        {
            int len = hex.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ToHexString(this byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            //return hex;
            return hex.Replace("-", "");
        }

        //public static string ReadAsText(this WebResponse resp, CancellationTokenSource cancelToken)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        try
        //        {
        //            resp.ReadAsByte(cancelToken.Token, stream);
        //            return Encoding.UTF8.GetString(stream.ToArray());
        //        }
        //        catch
        //        {
        //            //// Cancellation token.
        //            return "7035ba55-7d63-4349-9f73-c454529d4b2e";
        //        }
        //    }
        //}

        public static void ReadAsByte(this WebResponse resp, CancellationToken token, Stream outputStream = null)
        {
            using (Stream responseStream = resp.GetResponseStream())
            {
                var buffer = new byte[65536];
                int bytesRead;

                while (responseStream != null && (bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    token.ThrowIfCancellationRequested();
                    outputStream?.Write(buffer, 0, bytesRead);
                }
            }
        }

        public static T ThrowIf<T>(this T data, Func<T, bool> func, Func<T, Exception> ex)
        {
            if (func(data)) throw ex(data);
            return data;
        }

        //public static T ThrowIf<T>(this Task<T> data, Func<T, bool> func, Exception ex)
        //{
        //    var res = data.Result;
        //    if (func(res)) throw ex;
        //    return res;
        //}


        /// <summary>
        /// Finds the first exception of the requested type.
        /// </summary>
        /// <typeparam name="T">
        /// The type of exception to return
        /// </typeparam>
        /// <param name="ex">
        /// The exception to look in.
        /// </param>
        /// <returns>
        /// The exception or the first inner exception that matches the
        /// given type; null if not found.
        /// </returns>
        public static T InnerOf<T>(this Exception ex)
            where T : Exception
        {
            return (T)InnerOf(ex, typeof(T));
        }

        /// <summary>
        /// Finds the first exception of the requested type.
        /// </summary>
        /// <param name="ex">
        /// The exception to look in.
        /// </param>
        /// <param name="t">
        /// The type of exception to return
        /// </param>
        /// <returns>
        /// The exception or the first inner exception that matches the
        /// given type; null if not found.
        /// </returns>
        public static Exception InnerOf(this Exception ex, Type t)
        {
            while (true)
            {
                if (ex == null || t.IsInstanceOfType(ex)) return ex;

                if (ex is AggregateException ae)
                {
                    foreach (var e in ae.InnerExceptions)
                    {
                        var ret = InnerOf(e, t);
                        if (ret != null) return ret;
                    }
                }

                ex = ex.InnerException;
            }
        }
    }
}