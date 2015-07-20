using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using QuickGraph;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using MsVsShell = Microsoft.VisualStudio.Shell;


#if DONT_COMPILE

Wave 1 - Wireup infra
- Click
- Workspace 
  - Extract
    - [a] build order graph w/ GUID
    - [b] list of projects, dependencies, files, refs, projfile
  - Fire Workspace Created

- SolutionSnapshot 
  - handles Workspace Created
    - Fire Creating snapshot
    - Graph looper
      - Fire snapshoting proj
      - Fire shapshoted proj
    - Fire Created snapshot

- UX
  - Click - changes to Loading
  - Handles Creating snapshot
    - Shows the list of projects, in tree view
  - Handles snapshoting proj - starts marquee
  - Handles snapshoted proj - stops marquee
  - Handles Created snapshot - change to loaded

Wave 2 - make buildable
- Trigger MSBuild
  - Show failure in UX  
- Edit proj file to get build outputs
- Edit proj file to replace proj ref with proj output


Wave 3 - keep buildable [p0 cases]


Wave 4 - keep buildable [p1 cases]














- Initialize
  v Fix OM - With project build order in mind
  - Solution Event Listener
    - Solution Load
    - Disable/Enable Button
  - Pull Workspace
  - Projec Build Order

- TreeView Style

- Incremental Changes
  - Solution Event Listener
  - Project Event Listener
  - File Event Listener
  - TextView Event Listener

- Remaining Pipeline
  - TB Update should come from pipeline
  - ?Merge Events
  - Copy workspace
  - Reference Files
  - Additional Files

#endif

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    [MsVsShell.ProvideToolWindow(typeof(WorkspacePane), Style = MsVsShell.VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    [MsVsShell.ProvideMenuResource(1000, 1)]
    [MsVsShell.PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid("01069CDD-95CE-4620-AC21-DDFF6C57F012")]
    public class PackageToolWindow : MsVsShell.Package
    {
        private MsVsShell.OleMenuCommandService menuService;

        protected override void Initialize()
        {
            base.Initialize();

            var id = new CommandID(GuidsList.guidClientCmdSet, PkgCmdId.cmdidUiEventsWindow);
            DefineCommandHandler(new EventHandler(this.ShowDynamicWindow), id);


            var g = new AdjacencyGraph<string, Edge<string>>();
            var v = g.OutDegree("");
            //v.
        }

        internal MsVsShell.OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
        {
            if (this.Zombied)
                return null;

            if (menuService == null)
            {
                menuService = GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
            }
            MsVsShell.OleMenuCommand command = null;
            if (null != menuService)
            {
                command = new MsVsShell.OleMenuCommand(handler, id);
                menuService.AddCommand(command);
            }
            return command;
        }

        internal string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = this.GetType().GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        private void ShowDynamicWindow(object sender, EventArgs arguments)
        {
            MsVsShell.ToolWindowPane pane = this.FindToolWindow(typeof(WorkspacePane), 0, true);
            if (pane == null)
            {
                throw new COMException(this.GetResourceString("@101"));
            }
            IVsWindowFrame frame = pane.Frame as IVsWindowFrame;
            if (frame == null)
            {
                throw new COMException(this.GetResourceString("@102"));
            }

            ErrorHandler.ThrowOnFailure(frame.Show());
        }
    }
}