using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidExplorer
{
    class InstalledPackagesNode
        : TreeNode
    {
        private string _device_id;
        private DateTime _expred_date_time;


        public InstalledPackagesNode(string device_id)
            : base("インストールされているパッケージ")
        {
            _device_id = device_id;
            _expred_date_time = DateTime.UtcNow;
        }


        async public override Task Update(bool forcely)
        {
            var now = DateTime.UtcNow;
            if (!forcely && now < _expred_date_time)
                return;
            var result_text = await ExternalCommand.Adb.RunShell(this, _device_id, "pm list package");
            if (result_text == null)
                return;
            _expred_date_time = now.AddHours(1);
            var new_packages = result_text
                .Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(item => !string.IsNullOrEmpty(item))
                .ToDictionary(item => item, item => item);
            var deleting_packages = new List<PackageNode>();
            foreach (var child in Children)
            {
                var package = child as PackageNode;
                if (package != null)
                {
                    if (!new_packages.ContainsKey(package.PackageId))
                        deleting_packages.Add(package);
                }
            }
            foreach (var deleting_package in deleting_packages)
            {
                deleting_package.Disconnect();
                Children.Remove(deleting_package);
            }
            var current_packages = Children
                .Select(item => item as PackageNode)
                .Where(item => item != null)
                .ToDictionary(item => item.PackageId, item => item);
            foreach (var new_package in new_packages.Values)
            {
                if (!current_packages.ContainsKey(new_package))
                    Children.Add(new PackageNode(new_package));
            }
            foreach (var task in Children.Select(child => child.Update(false)).ToArray())
                await task;
        }
    }
}
