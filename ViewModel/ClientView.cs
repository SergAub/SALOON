using SALOON.dbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SALOON.ViewModel
{
    internal class ClientView
    {
        public Client Client { get; set; }

        public List<TagView> Tags { get; set; } = new List<TagView>();

        private string lvisit = null;
        public string LastVisit {
            get {
                if (lvisit != null) return lvisit;

                using (var db = new Entities())
                {
                    try
                    {
                        var cx = db.ClientService.Where(x => x.ClientID == Client.ID).Max(x => x.StartTime);
                        lvisit = cx.ToShortDateString();
                    }
                    catch {
                        lvisit = "-";
                    }
                }

                return lvisit;
            } 
        }

        private int visits = -1;
        public int Visits { 
            get {
                if (visits != -1) return visits;

                using (var db = new Entities())
                {
                    visits = db.ClientService.Where(x => x.ClientID == Client.ID).Count();
                }

                return visits;
            } 
        }
    }
}
