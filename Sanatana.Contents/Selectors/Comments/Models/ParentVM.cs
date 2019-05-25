using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Selectors.Comments
{
    public class ParentVM<T>
    {
        public T Item { get; set; }
        public bool HasChildren { get; set; }
        public List<ParentVM<T>> Children { get; set; }


        //init
        public ParentVM()
        {
        }
        public ParentVM(T item)
        {
            Item = item;
        }
    }
}
