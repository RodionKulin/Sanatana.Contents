using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class HtmlElement : HtmlNode
    {
        private readonly Dictionary<string, HtmlAttribute> _attributes;

        public string Name { get; private set; }
        public bool IsClosed { get; private set; }

        public ReadOnlyCollection<HtmlAttribute> Attributes
        {
            get
            {
                return new ReadOnlyCollection<HtmlAttribute>(_attributes.Values.ToArray());
            }
        }
        public List<HtmlNode> ChildNodes { get; private set; }

        private HtmlElement()
        {
            _attributes = new Dictionary<string, HtmlAttribute>();
            ChildNodes = new List<HtmlNode>();
        }

        public HtmlElement(string name, bool isClosed)
            : this()
        {
            Name = name.ToLowerInvariant();
            IsClosed = isClosed;
        }

        public void Add(string name, string value)
        {
            var item = new HtmlAttribute { Name = name.ToLower(), Value = value };
            this._attributes[name.ToLower()] = item;
        }

        public void Add(HtmlNode item)
        {
            this.ChildNodes.Add(item);
        }

        public void Insert(int index, HtmlNode item)
        {
            this.ChildNodes.Insert(index, item);
        }

        public void RemoveChildAt(int index)
        {
            ChildNodes.RemoveAt(index);
        }

        internal void RemoveAll(IEnumerable<HtmlAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (this._attributes.ContainsKey(attribute.Name.ToLower()))
                    this._attributes.Remove(attribute.Name.ToLower());
            }
        }

        public override string ToString()
        {
            var buf = new StringBuilder("<" + Name);
            foreach (HtmlAttribute node in Attributes)
            {
                buf.Append(" " + node.ToString());
            }
            if (ChildNodes.Count == 0)
            {
                buf.Append("/>");
                return buf.ToString();
            }
            else
            {
                buf.Append(">");

                foreach (var node in ChildNodes)
                {
                    buf.Append(node.ToString());
                }

                buf.AppendFormat("</{0}>", Name);
                return buf.ToString();
            }
        }
    }
}
