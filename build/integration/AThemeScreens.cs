using System; using System.Drawing; using System.IO; using System.Windows.Forms; using System.Runtime.InteropServices; using KeePass.Plugins; using KeePass.Forms; using KeePassLib; using KeePassLib.Security; using KeePassLib.Serialization;
[assembly:System.Reflection.AssemblyProduct("KeePass Plugin")]
namespace AThemeScreens {
 public sealed class AThemeScreensExt:Plugin {
 IPluginHost host; string output;
 [DllImport("user32.dll")] static extern bool PrintWindow(IntPtr handle,IntPtr dc,uint flags);
 public override bool Initialize(IPluginHost value){host=value;output=Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(AThemeScreensExt).Assembly.Location),"../screens"));Directory.CreateDirectory(output);host.MainWindow.Shown+=delegate{host.MainWindow.BeginInvoke(new MethodInvoker(Run));};return true;}
 void Capture(Form form,string name){using(Bitmap bitmap=new Bitmap(form.Width,form.Height)){using(Graphics graphics=Graphics.FromImage(bitmap)){IntPtr dc=graphics.GetHdc();try{if(!PrintWindow(form.Handle,dc,0))throw new Exception("PrintWindow failed");}finally{graphics.ReleaseHdc(dc);}}bitmap.Save(Path.Combine(output,name+".png"),System.Drawing.Imaging.ImageFormat.Png);}}
 void Dialog(Form form,string name){form.StartPosition=FormStartPosition.Manual;form.Location=new Point(80,80);Timer timer=new Timer();timer.Interval=450;timer.Tick+=delegate{timer.Stop();Capture(form,name);form.DialogResult=DialogResult.Cancel;form.Close();};form.Shown+=delegate{timer.Start();};form.ShowDialog(host.MainWindow);timer.Dispose();form.Dispose();}
 void Run(){try{
 var main=host.MainWindow;main.WindowState=FormWindowState.Normal;main.Size=new Size(1120,660);main.Location=new Point(40,40);
 var doc=main.DocumentManager.CreateNewDocument(true);var db=doc.Database;db.New(new IOConnectionInfo{Path="Demo.kdbx"},new KeePassLib.Keys.CompositeKey());db.RootGroup.Name="Demo";
 var work=new PwGroup(true,true,"Arbeit",PwIcon.Folder);var home=new PwGroup(true,true,"Privat",PwIcon.Home);var online=new PwGroup(true,true,"Internet",PwIcon.World);db.RootGroup.AddGroup(work,true);db.RootGroup.AddGroup(home,true);db.RootGroup.AddGroup(online,true);
 string[] names={"Mail","Projektportal","Cloudspeicher","Team-Wiki","Kalender","Testserver"};
 for(int i=0;i<names.Length;i++){var entry=new PwEntry(true,true);entry.IconId=PwIcon.World;entry.Strings.Set("Title",new ProtectedString(false,names[i]));entry.Strings.Set("UserName",new ProtectedString(false,"demo@example.com"));entry.Strings.Set("Password",new ProtectedString(true,"Fiktives-Demo-Passwort-2026!"));entry.Strings.Set("URL",new ProtectedString(false,"https://example.com/"));entry.Strings.Set("Notes",new ProtectedString(false,"Fiktiver Eintrag für die Theme-Vorschau."));work.AddEntry(entry,true);}
 main.UpdateUI(true,doc,true,work,true,work,false);
 var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 var list=(ListView)main.GetType().GetField("m_lvEntries",flags).GetValue(main);
 int[] widths={140,165,85,170,240}; for(int i=0;i<list.Columns.Count && i<widths.Length;i++)list.Columns[i].Width=widths[i];
 var tree=(TreeView)main.GetType().GetField("m_tvGroups",flags).GetValue(main);
 tree.BackColor=host.CustomConfig.GetString("KeeTheme.Template","").Contains("ModernGreen") ? Color.FromArgb(53,99,84):Color.FromArgb(98,98,98);
 Application.DoEvents();Timer pause=new Timer();pause.Interval=600;pause.Tick+=delegate{pause.Stop();try{Capture(main,"main");var edit=new PwEntryForm();edit.InitEx(work.Entries.GetAt(0),PwEditMode.EditExistingEntry,db,main.ClientIcons,false,false);Dialog(edit,"entry");Dialog(new OptionsForm(),"options");File.WriteAllText(Path.Combine(output,"done.txt"),"PASS fictional screenshots");}catch(Exception ex){File.WriteAllText(Path.Combine(output,"error.txt"),ex.ToString());}db.Modified=false;main.Close();};pause.Start();
 }catch(Exception ex){File.WriteAllText(Path.Combine(output,"error.txt"),ex.ToString());host.MainWindow.Close();}}
 }
}