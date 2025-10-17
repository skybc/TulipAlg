using System;
using System.Collections.ObjectModel;

namespace TulipAlg.Models
{
    /// <summary>
    /// 菜单项模型
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// 菜单显示名称
        /// </summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// 对应的视图类型
        /// </summary>
        public Type? ViewType { get; set; }

        /// <summary>
        /// 子菜单项
        /// </summary>
        public ObservableCollection<MenuItem> Children { get; set; } = new ObservableCollection<MenuItem>();

        /// <summary>
        /// 是否为叶子节点（有对应视图）
        /// </summary>
        public bool IsLeaf => ViewType != null;
    }
}
