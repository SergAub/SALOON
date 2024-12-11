using SALOON.dbModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SALOON.ViewModel
{
    internal class ServicePhotoView
    {
        public ServicePhoto ServicePhoto { get; set; }

        public string PhotoPath { 
            get { 
           
                return Path.GetFullPath(ServicePhoto.PhotoPath);
            } 
        }
    }
}
