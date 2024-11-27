using SALOON.dbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SALOON.ViewModel
{
    internal class ServiceView
    {
        public Service Service { get; set; }

        public string PriceChёrt {
            get
            {
                if (Service.Discount == null || Service.Discount < 0.00001)
                    return "";

                return $"{(int)Service.Cost} ";
            } 
        }

        public string PriceForTime
        {
            get
            {
                string res;
                if (Service.Discount != null)
                {
                    res = $" {(int)((decimal)(1 - Service.Discount) * Service.Cost)}";
                }
                else
                    res = $" {(int)Service.Cost}"; 
                
                return res + $" рублей за {Service.DurationInSeconds / 60} минут";
            }
        }

        public string Discount
        {
            get
            {
                if (Service.Discount == null || Service.Discount < 0.00001)
                    return "";

                return $"* скидка {(int)(Service.Discount * 100)}%";
            }
        }

        public Visibility AdminVisiblity 
        { 
            get
            {
                if (MainWindow.IsAdmin)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            } 
        }
    }
}
