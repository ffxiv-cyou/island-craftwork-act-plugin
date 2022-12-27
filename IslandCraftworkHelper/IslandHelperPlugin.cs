using Advanced_Combat_Tracker;
using Lotlab.PluginCommon.FFXIV;
using Lotlab.PluginCommon.FFXIV.Parser;
using Lotlab.PluginCommon.Overlay;
using Newtonsoft.Json.Linq;
using RainbowMage.OverlayPlugin;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IslandCraftworkHelper
{
    public class IslandHelperPlugin : IActPluginV1, IOverlayAddonV2
    {
        /// <summary>
        /// FFXIV 解析插件的引用
        /// </summary>
        ACTPluginProxy FFXIV { get; set; } = null;

        /// <summary>
        /// 网络包解析器
        /// </summary>
        NetworkParser parser { get; } = new NetworkParser(false);

        /// <summary>
        /// 状态标签的引用
        /// </summary>
        Label statusLabel = null;

        /// <summary>
        /// 事件源
        /// </summary>
        IslandHelperEventSource eventSource = null;

        void IActPluginV1.InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            // 设置状态标签引用方便后续使用
            statusLabel = pluginStatusText;

            // 遍历所有插件
            var plugins = ActGlobals.oFormActMain.ActPlugins;
            foreach (var item in plugins)
            {
                if (ACTPluginProxy.IsFFXIVPlugin(item.pluginObj))
                {
                    FFXIV = new ACTPluginProxy(item.pluginObj);
                    break;
                }
            }

            // 若解析插件不存在或不正常工作，则提醒用户，并结束初始化
            if (FFXIV == null || !FFXIV.PluginStarted)
            {
                statusLabel.Text = "FFXIV ACT Plugin 工作不正常，无法初始化。";
                return;
            }

            // 注册网络事件
            FFXIV.DataSubscription.NetworkReceived += OnNetworkReceived;

            // 为了简便起见，我们不涉及任何UI相关的内容。
            // 如果你需要添加自己的页面，请自行编写UserControl。
            // pluginScreenSpace.Controls.Add(new YourUserControl());

            // 直接隐藏掉不需要显示的插件页面
            (pluginScreenSpace.Parent as TabControl).TabPages.Remove(pluginScreenSpace);

            // 更新状态标签的内容
            statusLabel.Text = "插件初始化成功，等待悬浮窗初始化";
        }

        void IActPluginV1.DeInitPlugin()
        {
            // 取消注册网络事件
            if (FFXIV != null)
            {
                FFXIV.DataSubscription.NetworkReceived -= OnNetworkReceived;
            }

            FFXIV = null;
            statusLabel.Text = "插件已退出!";
        }

        void IOverlayAddonV2.Init()
        {
            var container = Registry.GetContainer();
            var registry = container.Resolve<Registry>();

            // 注册事件源
            eventSource = new IslandHelperEventSource(container);
            registry.StartEventSource(eventSource);

            // 或者注册悬浮窗预设
            registry.RegisterOverlayPreset2(new IslandHelperOverlayPresent());

            statusLabel.Text = "悬浮窗初始化成功";
        }

        void OnNetworkReceived(string connection, long epoch, byte[] message)
        {
            // 判断是否为目标数据包。
            if (message.Length != Marshal.SizeOf<MJICraftworksInfo>()) return;

            // 解析数据包
            var packet = parser.ParseAsPacket<MJICraftworksInfo>(message);
            if (!packet.IsValid()) return;

            if (eventSource != null)
            {
                eventSource.FireEvent(packet.ToArray());
            }
        }
    }

    public class IslandHelperEventSource : EventSourceBase
    {
        public IslandHelperEventSource(TinyIoCContainer c) : base(c)
        {
            // 设置事件源名称，必须是唯一的
            Name = "IslandCraftworkHelperES";

            // 注册数据源名称。此数据源提供给悬浮窗监听
            RegisterEventTypes(new List<string>()
            {
                "onMJICraftworkData",
            });
        }
        public override Control CreateConfigControl()
        {
            return null;
        }

        public override void LoadConfig(IPluginConfig config)
        {
        }

        public override void SaveConfig(IPluginConfig config)
        {
        }

        public void FireEvent(string data)
        {
            // 将数据发送给悬浮窗
            DispatchEvent(JObject.FromObject(new
            {
                type = "onMJICraftworkData",
                data = data
            }));
        }
    }

    public class IslandHelperOverlayPresent : IOverlayPreset
    {
        string IOverlayPreset.Name => "无人岛工坊助手";

        string IOverlayPreset.Type => "MiniParse";

        string IOverlayPreset.Url => "https://island.ffxiv.cyou/ngld.html";

        int[] IOverlayPreset.Size => new int[2] { 200, 200 };

        bool IOverlayPreset.Locked => false;

        List<string> IOverlayPreset.Supports => new List<string> { "modern" };

        public override string ToString()
        {
            return ((IOverlayPreset)this).Name;
        }
    }
}
