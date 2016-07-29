using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    internal class StreamPostedFile : HttpPostedFileBase
    {
        //поля
        private Stream _inputStream;


        //свойства
        public override Stream InputStream
        {
            get
            {
                return _inputStream;
            }
        }
        public override int ContentLength
        {
            get
            {
                return (int)InputStream.Length;
            }
        }


        //инициализация
        public StreamPostedFile(Stream fileStream)
        {
            _inputStream = fileStream;
        }
    }
}
