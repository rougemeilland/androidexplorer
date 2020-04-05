using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AndroidExplorer
{
    internal class RootNode
        : TreeNode
    {
        private static Regex _device_list_pattern;
        private DateTime _expred_date_time;

        static RootNode()
        {
            _device_list_pattern = new Regex(@"^(?<id>[^ ]+)\s+(?<status>device|offline)(\s+usb:(?<usb>[^ ]+))?(\s+product:(?<product>[^ ]+))?(\s+model:(?<model>[^ ]+))?(\s+device:(?<device>[^ ]+))?(\s+transport_id:(?<transport_id>[^ ]+))?$", RegexOptions.Compiled);
        }

        public RootNode(string display_name)
            : base(display_name)
        {
            _expred_date_time = DateTime.UtcNow;
        }

        async public override Task Update(bool forcely)
        {
            var now = DateTime.UtcNow;
            if (!forcely && now < _expred_date_time)
                return;
            var result_text = await ExternalCommand.Adb.Run(this, "devices -l");
            if (result_text == null)
                return;
            _expred_date_time = now.AddSeconds(10);
            var new_devices = result_text
                .Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => s != "List of devices attached")
                .Select(s =>
                {
                    var m = _device_list_pattern.Match(s);
                    if (!m.Success)
                        return null;
                    else
                    {
                        return new
                        {
                            id = m.Groups["id"].Value,
                            status = m.Groups["status"].Value,
                            usb = m.Groups["usb"].Value,
                            product = m.Groups["product"].Value,
                            model = m.Groups["model"].Value,
                            device = m.Groups["device"].Value,
                            transport_id = m.Groups["transport_id"].Value,
                        };
                    }
                })
                .Where(item => item != null)
                .ToDictionary(item => item.id, item => item);
            var deleting_items = new List<DeviceNode>();
            foreach (var child in Children)
            {
                var device = child as DeviceNode;
                if (device != null)
                {
                    if (new_devices.ContainsKey(device.DeviceId))
                    {
                        var new_device = new_devices[device.DeviceId];

                        device.Device = new_device.device;
                        device.Model = new_device.model;
                        device.Product = new_device.product;
                        device.Status = new_device.status;
                        device.Usb = new_device.usb;
                        device.TransportId = new_device.transport_id;
                    }
                    else
                        deleting_items.Add(device);
                }
            }
            foreach (var deleting_item in deleting_items)
            {
                deleting_item.Disconnect();
                Children.Remove(deleting_item);
            }
            var current_devices = Children
                .Select(item => item as DeviceNode)
                .Where(item => item != null)
                .ToDictionary(item => item.DeviceId, item => item);
            foreach (var new_device in new_devices.Values)
            {
                if (!current_devices.ContainsKey(new_device.id))
                    Children.Add(new DeviceNode(new_device.id, new_device.status, new_device.usb, new_device.product, new_device.model, new_device.device, new_device.transport_id));
            }
            foreach (var task in Children.Select(child => child.Update(false)).ToArray())
                await task;
        }
    }
}

