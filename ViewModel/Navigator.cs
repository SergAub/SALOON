using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SALOON.ViewModel
{
    internal class Navigator<T>
    {
        private int pageSize;
        private int pageIndex;
        private List<List<T>> pages = new List<List<T>>();

        public Navigator(List<T> list, int pageS) { 

            pageSize = pageS;
            var p = new List<T>();
            int j = 0;

            for (int i = 0; i < list.Count; i++)
            {                
                p.Add(list[i]);
                j++;
                if (j == pageSize || i + 1 == list.Count)
                {
                    pages.Add(p);
                    p = new List<T>();
                    j = 0;
                }
            }

            if (pages[pages.Count - 1].Count == 0)
                pages.RemoveAt(pages.Count - 1);
        }

        public List<T> CurrentPage { 
            get {
                return pages[pageIndex];    
            } 
        }

        public int GetPages()
        {
            return pages.Count;
        }

        public int GetDataSize()
        {
            return pages[pageIndex].Count;
        }

        public void MoveToPage(int page)
        {
            if (page == -1)
                page = pages.Count - 1;
            if (page > pages.Count - 1)
                page = 0;
            pageIndex = page;
        }

        public int GetCurrentPage()
        {
            return pageIndex;
        }
    }
}
