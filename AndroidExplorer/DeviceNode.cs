using System;
using System.Threading.Tasks;
using System.Linq;

namespace AndroidExplorer
{
    class DeviceNode
        : TreeNode
    {
        private string _status;
        private string _usb;
        private string _product;
        private string _model;
        private string _device;
        private string _transport_id;

        public DeviceNode(string device_id, string status, string usb, string product, string model, string device, string transport_id)
            : base(GetDisplayName(model, status))
        {
            DeviceId = device_id;
            _status = status;
            _usb = usb;
            _product = product;
            _model = model;
            _device = device;
            _transport_id = transport_id;
            DetailItems.Add(new DetailItem("device_id", DeviceId));
            if (_status != null)
                DetailItems.Add(new DetailItem("status", _status));
            if (_usb != null)
                DetailItems.Add(new DetailItem("usb", _usb));
            if (_product != null)
                DetailItems.Add(new DetailItem("product", _product));
            if (_model != null)
                DetailItems.Add(new DetailItem("model", _model));
            if (_device != null)
                DetailItems.Add(new DetailItem("device", _device));
            if (_transport_id != null)
                DetailItems.Add(new DetailItem("transport_id", _transport_id));
            if (!Children.Where(item => item is InstalledPackagesNode).Any())
                Children.Add(new InstalledPackagesNode(DeviceId));
            if (!Children.Where(item => item is DevicePropertiesNode).Any())
                Children.Add(new DevicePropertiesNode(DeviceId));
        }

        public string DeviceId { get; private set; }

        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (value != _status)
                {
                    _status = value;
                    RaisePropertyChanged("Status");
                    DisplayName = GetDisplayName(_model, _status);
                    UpdateDetail("Status", value);
                }
            }
        }

        public string Usb
        {
            get
            {
                return _usb;
            }

            set
            {
                if (value != _usb)
                {
                    _usb = value;
                    RaisePropertyChanged("Usb");
                    UpdateDetail("Usb", value);
                }
            }
        }

        public string Product
        {
            get
            {
                return _product;
            }

            set
            {
                if (value != _product)
                {
                    _product = value;
                    RaisePropertyChanged("Product");
                    UpdateDetail("Product", value);
                }
            }
        }

        public string Model
        {
            get
            {
                return _model;
            }

            set
            {
                if (value != _model)
                {
                    _model = value;
                    RaisePropertyChanged("Model");
                    DisplayName = GetDisplayName(_model, _status);
                    UpdateDetail("Model", value);
                }
            }
        }

        public string Device
        {
            get
            {
                return _device;
            }

            set
            {
                if (value != _device)
                {
                    _device = value;
                    RaisePropertyChanged("Device");
                    UpdateDetail("Device", value);
                }
            }
        }

        public string TransportId
        {
            get
            {
                return _transport_id;
            }

            set
            {
                if (value != _transport_id)
                {
                    _transport_id = value;
                    RaisePropertyChanged("TransportId");
                    UpdateDetail("TransportId", value);
                }
            }
        }

        async public override Task Update(bool forcely)
        {
            foreach (var child in Children)
                await child.Update(forcely);
        }

        private static string GetDisplayName(string model, string status)
        {
            if (status == "device")
                return model;
            else if (status == "offline")
                return string.Format("{0}[DISCONNECTED]", model);
            else
                throw new Exception(string.Format("Unknown device status: {0}", status));
        }

        private void UpdateDetail(string name, string value)
        {
            var found = DetailItems
                .Select(item => item as DetailItem)
                .Where(item => item != null && item.Name == name)
                .FirstOrDefault();
            if (found != null)
                found.Value = value;
        }

    }
}
