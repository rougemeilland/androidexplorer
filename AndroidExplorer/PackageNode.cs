using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidExplorer
{
    class PackageNode
        : TreeNode
    {
        public PackageNode(string package_id)
            : base(package_id)
        {
            PackageId = package_id;
        }

        public string PackageId { get; private set; }
    }
}
