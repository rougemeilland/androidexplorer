using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AndroidExplorer
{
    class DevicePropertiesNode
        : TreeNode
    {
        private static Regex _property_list_pattern;
        private string _device_id;
        private DateTime _expred_date_time;


        static DevicePropertiesNode()
        {
            _property_list_pattern = new Regex(@"^\[(?<name>[^\]]+)\]: \[(?<value>[^\]]*)\]$", RegexOptions.Compiled);
        }

        public DevicePropertiesNode(string device_id)
            : base("プロパティ")
        {
            _device_id = device_id;
            _expred_date_time = DateTime.UtcNow;
            SetPropertiesCommand = new DelegateCommand(
                async () =>
                {
                    await ExternalCommand.Adb.RunShell(this, _device_id, "stop");
                    await AdbSetProperty("log.tag.AppWidgetConfigureActivity", "DEBUG");
                    await AdbSetProperty("log.tag.TimeZoneSelectionItems", "DEBUG");
                    await AdbSetProperty("log.tag.AppWidget", "DEBUG");
                    await ExternalCommand.Adb.RunShell(this, _device_id, "start");
                    await Update(true);
                },
                () => true);
        }

        public override bool IsActiveSetProperties => true;

        async public override Task Update(bool forcely)
        {
            var now = DateTime.UtcNow;
            if (!forcely && now < _expred_date_time)
                return;
            var result_text = await ExternalCommand.Adb.RunShell(this, _device_id, "getprop");
            if (result_text == null)
                return;
            _expred_date_time = now.AddMinutes(1);
            var new_properties = result_text
                .Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    var m = _property_list_pattern.Match(s);
                    if (!m.Success)
                        return null;
                    else
                    {
                        return new
                        {
                            name = m.Groups["name"].Value,
                            value = m.Groups["value"].Value
                        };
                    }
                })
                .Where(item => item != null)
                .ToDictionary(item => item.name, item => item);
            var deleting_properties = new List<DetailItem>();
            foreach (var property in DetailItems)
            {
                if (new_properties.ContainsKey(property.Name))
                {
                    var new_property = new_properties[property.Name];
                    property.Value = new_property.value;
                }
                else
                    deleting_properties.Add(property);
            }
            foreach (var deleting_property in deleting_properties)
                DetailItems.Remove(deleting_property);
            var current_properties = DetailItems
                .Select(item => item as DetailItem)
                .Where(item => item != null)
                .ToDictionary(item => item.Name, item => item);
            foreach (var new_property in new_properties.Values)
            {
                if (!current_properties.ContainsKey(new_property.name))
                    DetailItems.Add(new DetailItem(new_property.name, new_property.value));
            }
        }
        async private Task AdbSetProperty(string name, string value)
        {
            await ExternalCommand.Adb.RunShell(this, _device_id, string.Format("setprop {0} {1}", name, value));
        }

    }
}
