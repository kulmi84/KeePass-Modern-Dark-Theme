using System.Drawing;
using KeePassLib;

namespace KeeTheme.Theme
{
    internal static class ModernStandardIcons
    {
        internal static bool Draw(Graphics g, Rectangle bounds, int index, Color color)
        {
            string name;
            switch ((PwIcon)index)
            {
                case PwIcon.Key: case PwIcon.MultiKeys: case PwIcon.UserKey: name = "m_tbCopyPassword"; break;
                case PwIcon.Folder: case PwIcon.FolderOpen: case PwIcon.FolderPackage: case PwIcon.MarkedDirectory: name = "m_tbOpenDatabase"; break;
                case PwIcon.UserCommunication: case PwIcon.Identity: name = "m_tbCopyUserName"; break;
                case PwIcon.World: case PwIcon.WorldSocket: case PwIcon.WorldStar: case PwIcon.WorldComputer: name = "m_tbOpenUrl"; break;
                case PwIcon.TerminalEncrypted: case PwIcon.PaperLocked: name = "m_tbLockWorkspace"; break;
                case PwIcon.Clock: case PwIcon.Expired: name = "m_tbViewsShowExpired"; break;
                case PwIcon.List: case PwIcon.ProgramIcons: name = "m_tbEntryViewsDropDown"; break;
                case PwIcon.PaperNew: case PwIcon.Notepad: case PwIcon.Note: name = "m_tbNewDatabase"; break;
                case PwIcon.NetworkServer: name = "standardServer"; break;
                case PwIcon.Home: name = "standardHome"; break;
                case PwIcon.EMail: case PwIcon.EMailBox: name = "standardMail"; break;
                case PwIcon.Tool: case PwIcon.Settings: case PwIcon.Configuration: name = "standardTool"; break;
                case PwIcon.Monitor: case PwIcon.Screen: name = "standardMonitor"; break;
                case PwIcon.TrashBin: name = "standardTrash"; break;
                default: return false;
            }
            return ModernToolbarIcons.Draw(g, bounds, name, color);
        }
    }
}
