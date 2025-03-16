using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class BaseTreeView : TreeView
    {
        internal const int TV_FIRST = 4352;
        internal const int TVIF_CHILDREN = 64;
        internal const int TVM_GETITEM = 4364;
        internal const int TVM_SETITEM = 4365;

        public void SetNodeButton(TreeNode treeNode, bool flag)
        {
            var lParam = new TVITEM()
            {
                mask = 64,
                hItem = treeNode.Handle,
                cChildren = flag ? 1 : 0
            };
            SendMessage(Handle, 4365U, 0U, ref lParam);
        }

        public bool GetNodeButton(TreeNode treeNode)
        {
            var lParam = new TVITEM()
            {
                mask = 64,
                hItem = treeNode.Handle
            };
            SendMessage(Handle, 4364U, 0U, ref lParam);
            return lParam.cChildren > 0;
        }

        public TreeNode FindTreeNode(TreeNodeCollection nodes, string label)
        {
            var treeNode = (TreeNode)null;
            foreach (TreeNode node in nodes)
                if (!(node.Text != label))
                {
                    treeNode = node;
                    break;
                }

            return treeNode;
        }

        public void SelectTreePath(string path)
        {
            if (path == null)
                return;
            var treeNode1 = (TreeNode)null;
            var treeNode2 = (TreeNode)null;
            foreach (var label in path.Split(PathSeparator.ToCharArray()))
            {
                treeNode1 = FindTreeNode(treeNode1 == null ? Nodes : treeNode1.Nodes, label);
                if (treeNode1 != null)
                {
                    treeNode2 = treeNode1;
                    treeNode1.Expand();
                }
                else
                {
                    break;
                }
            }

            SelectedNode = treeNode2;
        }

        [DllImport("user32.dll")]
        internal static extern uint SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(
            IntPtr hWnd,
            uint msg,
            uint wParam,
            ref TVITEM lParam);

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        internal struct TVITEM
        {
            public uint mask;
            public IntPtr hItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }
    }
}