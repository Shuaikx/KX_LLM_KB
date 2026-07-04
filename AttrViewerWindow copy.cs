using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using XLua;
using AOE;
using UnityEngine.Profiling;

namespace Assets.Editor.AttrViewer
{
    public class AttrViewerWindow : EditorWindow
    {
        private const float kToolbarHeight = 21f;
        private const float kSectionHeaderHeight = 22f;
        private const float kWatchRowHeight = 18f;
        private const float kMaxWatchHeight = 220f;
        private const float kIndentWidth = 14f;

        private const float kPathContentHeight = 36f;

        private List<TreeViewItem> m_watchedItems = new List<TreeViewItem>();
        private List<string> m_watchedPaths = new List<string>();
        private Dictionary<string, Dictionary<string, bool>> m_watchExpandedByPath =
            new Dictionary<string, Dictionary<string, bool>>();
        private bool m_watchExpanded = true;
        private bool m_pathExpanded = true;
        private bool m_variablesExpanded = true;
        private Vector2 m_watchScrollPos;

        private static class Styles
        {
            public static readonly Color WindowBg = new Color(0.145f, 0.145f, 0.149f);
            public static readonly Color SeparatorColor = new Color(0.235f, 0.235f, 0.235f);
            public static readonly Color SectionHeaderBg = new Color(0.169f, 0.169f, 0.169f);
            public static readonly Color RowHoverBg = new Color(0.094f, 0.373f, 0.686f, 0.15f);
            public static readonly Color KeyColor = new Color(0.863f, 0.863f, 0.863f);
            public static readonly Color ValueColor = new Color(0.706f, 0.706f, 0.706f);

            private static GUIStyle s_sectionHeader;
            private static GUIStyle s_watchKey;
            private static GUIStyle s_watchValue;
            private static GUIStyle s_removeButton;
            private static GUIStyle s_pathLabel;
            private static bool s_initialized;

            public static GUIStyle SectionHeader
            {
                get
                {
                    Init();
                    return s_sectionHeader;
                }
            }

            public static GUIStyle WatchKey
            {
                get
                {
                    Init();
                    return s_watchKey;
                }
            }

            public static GUIStyle WatchValue
            {
                get
                {
                    Init();
                    return s_watchValue;
                }
            }

            public static GUIStyle PathLabel
            {
                get
                {
                    Init();
                    return s_pathLabel;
                }
            }

            public static GUIStyle RemoveButton
            {
                get
                {
                    Init();
                    return s_removeButton;
                }
            }

            private static void Init()
            {
                if (s_initialized)
                {
                    return;
                }

                s_sectionHeader = new GUIStyle(EditorStyles.foldout)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    clipping = TextClipping.Clip,
                    padding = new RectOffset(14, 4, 3, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                };
                s_sectionHeader.normal.textColor = new Color(0.749f, 0.749f, 0.749f);
                s_sectionHeader.onNormal.textColor = s_sectionHeader.normal.textColor;
                s_sectionHeader.focused.textColor = s_sectionHeader.normal.textColor;
                s_sectionHeader.onFocused.textColor = s_sectionHeader.normal.textColor;
                s_sectionHeader.active.textColor = s_sectionHeader.normal.textColor;
                s_sectionHeader.onActive.textColor = s_sectionHeader.normal.textColor;

                s_watchKey = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(2, 4, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    normal = { textColor = KeyColor },
                };

                s_watchValue = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(0, 4, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    normal = { textColor = ValueColor },
                };

                s_removeButton = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f) },
                    hover = { textColor = Color.white },
                };

                s_pathLabel = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(8, 8, 4, 4),
                    margin = new RectOffset(0, 0, 0, 0),
                    normal = { textColor = new Color(0.706f, 0.863f, 0.706f) },
                };

                s_initialized = true;
            }
        }

        private static void DrawSeparatorLine(Rect rect)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), Styles.SeparatorColor);
        }

        private bool DrawSectionHeader(Rect rect, string title, bool expanded)
        {
            DrawSeparatorLine(rect);

            var headerRect = new Rect(rect.x, rect.y + 1f, rect.width, rect.height - 1f);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(headerRect, Styles.SectionHeaderBg);
            }

            var foldoutRect = new Rect(headerRect.x + 4f, headerRect.y, headerRect.width - 8f, headerRect.height);
            return EditorGUI.Foldout(foldoutRect, expanded, title, true, Styles.SectionHeader);
        }

        private void DrawPathContent(Rect area)
        {
            string path = m_treeView?.GetSelectedItemPath() ?? string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                path = "(未选择)";
            }

            GUI.Label(area, path, Styles.PathLabel);
        }

        private float GetWatchContentHeight()
        {
            if (m_watchedItems.Count == 0)
            {
                return 0f;
            }

            return CountVisibleWatchRows(m_watchedItems) * kWatchRowHeight;
        }

        private int CountVisibleWatchRows(List<TreeViewItem> items)
        {
            int count = 0;
            foreach (var item in items)
            {
                count += CountVisibleWatchRows(item);
            }

            return count;
        }

        private int CountVisibleWatchRows(TreeViewItem item)
        {
            int count = 1;
            var luaItem = item as LuaTableTreeViewItem;
            if ((luaItem?.isExpanded ?? false) && luaItem.children != null)
            {
                foreach (var child in luaItem.children)
                {
                    count += CountVisibleWatchRows(child);
                }
            }

            return count;
        }

        private void DrawWatchContent(Rect area)
        {
            if (m_watchedItems.Count == 0)
            {
                return;
            }

            float contentHeight = GetWatchContentHeight();
            float viewHeight = Mathf.Min(contentHeight, kMaxWatchHeight);
            var viewRect = new Rect(area.x, area.y, area.width, viewHeight);
            var contentRect = new Rect(0f, 0f, area.width - (contentHeight > kMaxWatchHeight ? 16f : 0f), contentHeight);

            m_watchScrollPos = GUI.BeginScrollView(viewRect, m_watchScrollPos, contentRect, false, false);
            float y = 0f;
            foreach (var item in m_watchedItems.ToArray())
            {
                y = DrawWatchedItemRect(item, 0, y, contentRect.width);
            }

            GUI.EndScrollView();
        }

        private float DrawWatchedItemRect(TreeViewItem item, int indentLevel, float y, float width)
        {
            var luaItem = item as LuaTableTreeViewItem;
            var rowRect = new Rect(0f, y, width, kWatchRowHeight);
            bool hasChildren = luaItem?.hasChildren ?? false;

            if (Event.current.type == EventType.Repaint && rowRect.Contains(Event.current.mousePosition + m_watchScrollPos))
            {
                EditorGUI.DrawRect(rowRect, Styles.RowHoverBg);
            }

            float x = indentLevel * kIndentWidth + 4f;
            if (hasChildren)
            {
                var arrowRect = new Rect(x, y, 14f, kWatchRowHeight);
                luaItem.isExpanded = EditorGUI.Foldout(arrowRect, luaItem.isExpanded, GUIContent.none, false);
                x += 14f;
            }
            else
            {
                x += 14f;
            }

            ParseDisplayName(item.displayName, out string keyName, out string valueName);
            var keyRect = new Rect(x, y, width * 0.42f, kWatchRowHeight);
            GUI.Label(keyRect, keyName, Styles.WatchKey);

            if (!string.IsNullOrEmpty(valueName))
            {
                var valueRect = new Rect(keyRect.xMax, y, width - keyRect.xMax - 20f, kWatchRowHeight);
                GUI.Label(valueRect, valueName, Styles.WatchValue);
            }

            if (indentLevel == 0)
            {
                var removeRect = new Rect(width - 18f, y + 1f, 16f, kWatchRowHeight - 2f);
                if (GUI.Button(removeRect, "x", Styles.RemoveButton))
                {
                    int index = m_watchedItems.IndexOf(item);
                    m_watchedItems.RemoveAt(index);
                    if (index >= 0 && index < m_watchedPaths.Count)
                    {
                        m_watchExpandedByPath.Remove(m_watchedPaths[index]);
                        m_watchedPaths.RemoveAt(index);
                    }
                }
            }

            y += kWatchRowHeight;
            if ((luaItem?.isExpanded ?? false) && luaItem.children != null)
            {
                foreach (var child in luaItem.children)
                {
                    y = DrawWatchedItemRect(child, indentLevel + 1, y, width);
                }
            }

            return y;
        }

        private static void ParseDisplayName(string displayName, out string keyName, out string valueName)
        {
            int equalsIndex = displayName.IndexOf('=');
            if (equalsIndex > 0)
            {
                keyName = displayName.Substring(0, equalsIndex).Trim();
                valueName = displayName.Substring(equalsIndex + 1).Trim();
                return;
            }

            keyName = displayName;
            valueName = string.Empty;
        }

        public void AddWatchedItem(TreeViewItem item)
        {
            string path = GetItemFullPath(item);
            if (string.IsNullOrEmpty(path) || m_watchedPaths.Contains(path))
            {
                return;
            }

            m_watchedPaths.Add(path);
            m_watchExpandedByPath[path] = CollectExpandedStates(item);
            m_watchedItems.Add(item);
        }

        [MenuItem("AoE/Lua/暂停游戏 &s", false, 20003)]
        public static void PauseGame()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        [MenuItem("AoE/Lua/查看属性系统 &c", false, 20002)]
        public static void OpenAttrViewer()
        {
            var window = EditorWindow.GetWindow<AttrViewerWindow>();
            window.titleContent = new GUIContent("属性系统");
            window.Show();
        }

        [SerializeField]
        private TreeViewState m_treeViewState;
        private LuaTableTreeView m_treeView;
        private SearchField m_SearchField;
        private bool m_initSuc = false;
        private GUIStyle m_errorStyle;
        private LuaTable m_table;
        private void OnEnable()
        {
            m_errorStyle = new GUIStyle();
            m_errorStyle.fontSize = 32;
            m_errorStyle.normal.textColor = Color.red;
            RefreshTable();
        }

        private void RefreshTable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (LuaMgr.instance == null)
            {
                return;
            }

            m_table = LuaMgr.instance.GetByPath<LuaTable>("mgr.userAttr.root");
            if (m_table == null)
            {
                return;
            }

            // 保存当前状态
            var expandedIds = m_treeView?.state.expandedIDs ?? new List<int>();
            var selectedIds = m_treeView?.state.selectedIDs ?? new List<int>();
            var scrollPos = m_treeView?.state.scrollPos ?? Vector2.zero;

            if (m_treeView == null)
            {
                m_treeViewState = new TreeViewState();
                m_treeView = new LuaTableTreeView(m_treeViewState, m_table);
                LuaTableTreeViewItem.Id = 1;
            }
            else
            {
                m_treeView.RefreshData(m_table);
                m_treeView.Reload();

                // 恢复所有状态
                m_treeView.state.expandedIDs = expandedIds;
                m_treeView.state.selectedIDs = selectedIds;
                m_treeView.state.scrollPos = scrollPos;
                
                // 强制刷新视图
                m_treeView.Repaint();
            }

            // 刷新监视列表
            RefreshWatchedItems();

            if (m_SearchField == null)
            {
                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_treeView.SetFocusAndEnsureSelectedItem;
            }

            m_initSuc = true;
        }

        private void RefreshWatchedItems()
        {
            for (int i = 0; i < m_watchedItems.Count && i < m_watchedPaths.Count; i++)
            {
                m_watchExpandedByPath[m_watchedPaths[i]] = CollectExpandedStates(m_watchedItems[i]);
            }

            var newWatchedItems = new List<TreeViewItem>();
            foreach (var path in m_watchedPaths)
            {
                m_watchExpandedByPath.TryGetValue(path, out var expandedStates);
                var item = BuildWatchItemFromPath(path, expandedStates ?? new Dictionary<string, bool>());
                if (item != null)
                {
                    newWatchedItems.Add(item);
                }
            }

            m_watchedItems = newWatchedItems;
        }

        private static string GetItemFullPath(TreeViewItem item)
        {
            if (item is LuaTableTreeViewItem luaItem)
            {
                return luaItem.fullPath;
            }

            ParseDisplayName(item.displayName, out string leafKey, out _);
            var parent = item.parent as LuaTableTreeViewItem;
            if (parent != null)
            {
                return parent.fullPath + "." + leafKey;
            }

            var path = new List<string>();
            var current = item;
            while (current != null)
            {
                if (current.depth >= 0)
                {
                    ParseDisplayName(current.displayName, out string keyName, out _);
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        path.Add(keyName);
                    }
                }

                current = current.parent;
            }

            path.Reverse();
            return string.Join(".", path);
        }

        private static Dictionary<string, bool> CollectExpandedStates(TreeViewItem item)
        {
            var states = new Dictionary<string, bool>();
            CollectExpandedStatesRecursive(item, states);
            return states;
        }

        private static void CollectExpandedStatesRecursive(TreeViewItem item, Dictionary<string, bool> states)
        {
            if (item is LuaTableTreeViewItem luaItem)
            {
                states[luaItem.fullPath] = luaItem.isExpanded;
                if (luaItem.children == null)
                {
                    return;
                }

                foreach (var child in luaItem.children)
                {
                    CollectExpandedStatesRecursive(child, states);
                }
            }
        }

        private static object ParsePathKey(string segment)
        {
            if (segment.StartsWith("[") && segment.EndsWith("]"))
            {
                string inner = segment.Substring(1, segment.Length - 2);
                if (int.TryParse(inner, out int index))
                {
                    return index;
                }

                return inner;
            }

            return segment;
        }

        private static string FormatPathKeyDisplayName(object key)
        {
            if (key is string s)
            {
                return s;
            }

            if (key is int i)
            {
                return $"[{i}]";
            }

            return key?.ToString() ?? string.Empty;
        }

        private bool TryGetValueAtPath(string path, out string lastSegment, out object value)
        {
            lastSegment = null;
            value = null;
            if (m_table == null || string.IsNullOrEmpty(path))
            {
                return false;
            }

            var parts = path.Split('.');
            LuaTable current = m_table;

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "root")
                {
                    continue;
                }

                lastSegment = parts[i];
                object key = ParsePathKey(lastSegment);
                value = current.Get<object>(key);

                if (i < parts.Length - 1)
                {
                    if (!(value is LuaTable nextTable))
                    {
                        return false;
                    }

                    current = nextTable;
                }
            }

            return lastSegment != null;
        }

        private TreeViewItem BuildWatchItemFromPath(string path, Dictionary<string, bool> expandedStates)
        {
            if (!TryGetValueAtPath(path, out string lastSegment, out object value))
            {
                return null;
            }

            string keyName = FormatPathKeyDisplayName(ParsePathKey(lastSegment));
            int id = LuaTableTreeViewItem.Id++;

            if (value is LuaTable luaTable)
            {
                var node = LuaAttrParser.Instance.FindNodeByPath(path);
                string typeName = node?.TypeName ?? string.Empty;
                string itemName = $"{keyName} = table({typeName})";
                var item = new LuaTableTreeViewItem(id, itemName, luaTable);
                ApplyWatchExpandedState(item, expandedStates);
                return item;
            }

            var valueHelper = new LuaTableTreeViewItem(0, "tmp", m_table);
            return new TreeViewItem(id, 0, $"{keyName} = {valueHelper.GetValName(value)}");
        }

        private static void ApplyWatchExpandedState(LuaTableTreeViewItem item, Dictionary<string, bool> expandedStates)
        {
            if (expandedStates.TryGetValue(item.fullPath, out bool expanded))
            {
                item.isExpanded = expanded;
            }

            if (item.children == null)
            {
                return;
            }

            foreach (var child in item.children)
            {
                if (child is LuaTableTreeViewItem luaChild)
                {
                    ApplyWatchExpandedState(luaChild, expandedStates);
                }
            }
        }

        private void DrawToolbar(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, Styles.SectionHeaderBg);
                DrawSeparatorLine(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f));
            }

            var refreshRect = new Rect(rect.x + 4f, rect.y + 1f, 52f, rect.height - 2f);
            if (GUI.Button(refreshRect, "刷新", EditorStyles.toolbarButton))
            {
                RefreshTable();
            }

            if (m_treeView == null)
            {
                m_initSuc = false;
            }

            if (m_initSuc)
            {
                var searchRect = new Rect(rect.xMax - rect.width * 0.45f, rect.y + 1f, rect.width * 0.45f - 4f, rect.height - 2f);
                string str = m_SearchField.OnGUI(searchRect, m_treeView.searchString);
                if (m_treeView.searchString != str)
                {
                    m_treeView.searchString = str;
                    if (string.IsNullOrEmpty(str))
                    {
                        m_treeView.SetFocusAndEnsureSelectedItem();
                    }
                }
            }
        }

        private void OnGUI()
        {
            Profiler.BeginSample("AttrViewerOnGUI");

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, position.height), Styles.WindowBg);
            }

            var toolbarRect = new Rect(0f, 0f, position.width, kToolbarHeight);
            DrawToolbar(toolbarRect);

            float y = kToolbarHeight;
            if (!m_initSuc)
            {
                var errorRect = new Rect(8f, y + 12f, position.width - 16f, 40f);
                GUI.Label(errorRect, "初始化失败，游戏未启动，或者未登录", m_errorStyle);
                Profiler.EndSample();
                return;
            }

            var watchHeaderRect = new Rect(0f, y, position.width, kSectionHeaderHeight);
            m_watchExpanded = DrawSectionHeader(watchHeaderRect, "监视", m_watchExpanded);
            y += kSectionHeaderHeight;

            if (m_watchExpanded)
            {
                float watchHeight = GetWatchContentHeight();
                if (watchHeight > 0f)
                {
                    DrawWatchContent(new Rect(0f, y, position.width, watchHeight));
                    y += watchHeight;
                }
            }

            var pathHeaderRect = new Rect(0f, y, position.width, kSectionHeaderHeight);
            m_pathExpanded = DrawSectionHeader(pathHeaderRect, "路径", m_pathExpanded);
            y += kSectionHeaderHeight;

            if (m_pathExpanded)
            {
                DrawPathContent(new Rect(0f, y, position.width, kPathContentHeight));
                y += kPathContentHeight;
            }

            var variablesHeaderRect = new Rect(0f, y, position.width, kSectionHeaderHeight);
            m_variablesExpanded = DrawSectionHeader(variablesHeaderRect, "变量", m_variablesExpanded);
            y += kSectionHeaderHeight;

            if (m_variablesExpanded)
            {
                var treeRect = new Rect(0f, y, position.width, position.height - y);
                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(treeRect, Styles.WindowBg);
                }

                m_treeView.OnGUI(treeRect);
            }

            Profiler.EndSample();
        }

        private void SaveTable()
        {
            Debug.Log("未实现");

        }
    }

    internal class LuaTableTreeView : TreeView
    {
        private AttrViewerWindow Window => EditorWindow.GetWindow<AttrViewerWindow>();
        private LuaTable m_table;

        public LuaTableTreeView(TreeViewState treeViewState, LuaTable table)
            : base(treeViewState)
        {
            m_table = table;
            showBorder = false;
            showAlternatingRowBackgrounds = true;
            rowHeight = 18;
            Reload();
        }

        public string GetSelectedItemPath()
        {
            if (state.selectedIDs == null || state.selectedIDs.Count == 0)
            {
                return string.Empty;
            }

            var item = FindItem(state.selectedIDs[0], rootItem);
            return GetItemPath(item);
        }

        private static string GetItemPath(TreeViewItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (item is LuaTableTreeViewItem luaItem)
            {
                return luaItem.fullPath;
            }

            var path = new List<string>();
            var current = item;
            while (current != null)
            {
                if (current.depth >= 0)
                {
                    var name = current.displayName;
                    int equalsIndex = name.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        name = name.Substring(0, equalsIndex).Trim();
                    }

                    path.Add(name);
                }

                current = current.parent;
            }

            path.Reverse();
            return string.Join(".", path);
        }

        protected override void DoubleClickedItem(int id)
        {

            var item = FindItem(id, rootItem) as LuaTableTreeViewItem;
            if (item != null)
            {
                string fullPath =  item.fullPath;
                var node  = LuaAttrParser.Instance.FindNodeByPath(fullPath);
                string filePath = LuaAttrParser.Instance.GetFilePathByTypeName(node.TypeName);
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    // 使用VSCode打开文件
                    try
                    {
                        // 跨平台查找VSCode路径
                        string vscodePath = null;
                        var platform = System.Environment.OSVersion.Platform;

                        if (platform == System.PlatformID.Win32NT)
                        {
                            vscodePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Programs\Microsoft VS Code\Code.exe";
                            if (!System.IO.File.Exists(vscodePath))
                            {
                                vscodePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\Microsoft VS Code\Code.exe";
                            }
                        }
                        else if (platform == System.PlatformID.Unix)
                        {
                            // Linux或MacOS
                            vscodePath = "/usr/bin/code";  // Linux默认路径
                            if (!System.IO.File.Exists(vscodePath))
                            {
                                vscodePath = "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code"; // MacOS路径
                            }
                        }

                        // 如果找到有效路径则使用，否则尝试通过命令行调用
                        if (System.IO.File.Exists(vscodePath))
                        {
                            System.Diagnostics.Process.Start(vscodePath, $"-g \"{filePath}\"");
                        }
                        else
                        {
                            // 尝试直接使用code命令（需要VSCode已添加到PATH）
                            var process = new System.Diagnostics.Process();
                            process.StartInfo.FileName = "code";
                            process.StartInfo.Arguments = $"-g \"{filePath}\"";
                            process.StartInfo.UseShellExecute = true;
                            process.Start();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"打开文件失败: {e.Message}");
                    }
                }
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var menu = new GenericMenu();
            var item = FindItem(id, rootItem);

            menu.AddItem(new GUIContent("添加到监视"), false, () =>
            {
                Window.AddWatchedItem(item);
            });

            menu.ShowAsContext();
            base.ContextClickedItem(id);
        }

        public void RefreshData(LuaTable table)
        {
            m_table = table;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new LuaTableTreeViewItem(0, "root", m_table);
            root.depth = -1;
            SetupDepthsFromParentsAndChildren(root);

            // 同步所有节点的展开状态
            root.children?.ForEach(child => 
            {
                if (child is LuaTableTreeViewItem luaChild)
                {
                    luaChild.SyncExpandedState(state);
                    if (luaChild.isExpanded)
                    {
                        luaChild.children?.ForEach(grandChild => 
                            (grandChild as LuaTableTreeViewItem)?.SyncExpandedState(state));
                    }
                }
            });

            // 确保根节点默认展开
            if (!state.expandedIDs.Contains(root.id))
            {
                state.expandedIDs.Add(root.id);
            }

            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            Window.Repaint();
        }

        protected override void SingleClickedItem(int id)
        {
            SetFocusAndEnsureSelectedItem();
        }
    }

    internal class LuaTableTreeViewItem : TreeViewItem
    {
        public bool isExpanded;
        public LuaTable luaTable;
        private bool wasExpanded;
        
        // 新增方法：同步展开状态到TreeView
        public void SyncExpandedState(TreeViewState state)
        {
            isExpanded = state.expandedIDs.Contains(id);
            wasExpanded = isExpanded;
        }

        public string typeName;
        public string filePath;

        public string fullPath => GetPullPath();


        internal static int Id = 1;
        public LuaTableTreeViewItem(int id, string name, LuaTable table) : base(id, 0, name)
        {
            luaTable = table;
            // 继承父节点的展开状态
            if (parent is LuaTableTreeViewItem parentItem)
            {
                isExpanded = parentItem.wasExpanded;
                wasExpanded = parentItem.wasExpanded;
            }
            
            var node = LuaAttrParser.Instance.FindNodeByPath(fullPath);
            if (node != null)   
            {
                typeName = node.TypeName;
                filePath = LuaAttrParser.Instance.GetFilePathByTypeName(typeName);
            }
            var keys = table.GetKeys();

            List<object> keyList = new List<object>();
            foreach (var item in keys)
            {
                keyList.Add(item);
            }

            keyList.Sort((a, b) => GetKeyName(a).CompareTo(GetKeyName(b)));
            foreach (var k in keyList)
            {
                var v = table.Get<object>(k);

                string itemName = $"{GetKeyName(k)} = {GetValName(v)}";

                if (v.GetType() == typeof(LuaTable))
                {
                    Id++;
                    this.AddChild(new LuaTableTreeViewItem(Id, itemName, v as LuaTable));
                }
                else
                {
                    Id++;
                    this.AddChild(new TreeViewItem(Id, 0, itemName));
                }
            }
        }

        public string GetValName(object obj)
        {
            Type t = obj.GetType();

            if (t == typeof(string))
            {
                return obj.ToString();
            }
            else if (t == typeof(bool))
            {
                return "{boolean} " + obj.ToString();
            }
            else if (t == typeof(int) || t == typeof(float) || t == typeof(long))
            {
                return "{number} " + obj.ToString();

            }
            else if (t == typeof(LuaTable))
            {
                return $"table({typeName})";
            }
            else
            {
                return obj.ToString();
            }
        }

        public string GetKeyName(object obj)
        {
            Type t = obj.GetType();

            if (t == typeof(string))
            {
                return obj.ToString();
            }
            else if (t == typeof(int))
            {
                return $"[{obj}]";
            }
            else if (t == typeof(float))
            {
                return t.ToString();
            }
            else if (t == typeof(LuaTable))
            {
                return $"table:{typeName}";
            }
            else
            {
                return obj.ToString();
            }
        }

        private string GetPullPath()
        {
            var path = new List<string>();
            var current = this;

            // 从当前节点向上遍历到根节点
            while (current != null)
            {
                // 提取字段名称部分（去掉类型信息）
                var name = current.displayName;
                var equalsIndex = name.IndexOf('=');
                if (equalsIndex > 0)
                {
                    name = name.Substring(0, equalsIndex).Trim();
                }
                path.Add(name);
                current = current.parent as LuaTableTreeViewItem;
            }

            // 反转路径，从根节点到当前节点
            path.Reverse();

            // 用"."连接路径
            return string.Join(".", path);
        }
    }
}
