using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EmpathyAPI.Helpers
{
    public class Helper
    {
        public static async Task<FileStreamResult> ByteArrayToImage(byte[] data, int width, int height)
        {
            width = (width == 0) ? 100 : width;
            height = (height == 0) ? 100 : height;
            MemoryStream ms = new MemoryStream();

            if (data == null)
            {
                return await Task.Run(() => new FileStreamResult(ms, "image/jpg"));
            }
            else
            {
                try
                {
                    var img= (Bitmap)((new ImageConverter()).ConvertFrom(data));
                    ms = new MemoryStream(img.Resize(width, height).ToByteArray());

                    return await Task.Run(() => new FileStreamResult(ms, "image/jpg"));
                }
                catch (Exception)
                {
                    return await Task.Run(() => new FileStreamResult(ms, "image/jpg"));
                }
            }
        }
    }
}
