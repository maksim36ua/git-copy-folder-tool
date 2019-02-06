using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GitToolbar
{
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(GuidList.guidComboBoxPkgString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideOptionPage(typeof(OptionPageGrid), "Git Config Options", "Git Config Options", 0, 0, true)]
	public sealed class GitToolbarPackage : Package
	{
		public string PathToDestinationRepository => ((OptionPageGrid)GetDialogPage(typeof(OptionPageGrid))).PathToDestinationRepository;
		public string PathToGit => ((OptionPageGrid)GetDialogPage(typeof(OptionPageGrid))).PathToGit;
		public string GitCommand => ((OptionPageGrid)GetDialogPage(typeof(OptionPageGrid))).GitCommand;
		public string[] Folders => ((OptionPageGrid)GetDialogPage(typeof(OptionPageGrid))).Folders;
		public string[] Brands => ((OptionPageGrid)GetDialogPage(typeof(OptionPageGrid))).Brands;

		private string SelectedFolder { get; set; }
		private string SelectedBrand { get; set; }

		protected override void Initialize()
		{
			base.Initialize();

			OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

			CommandID folderComboCommandId = new CommandID(GuidList.guidComboBoxCmdSet, (int)GitToolbarPackageCmdIdList.FolderCombo);
			OleMenuCommand folderComboCommand = new OleMenuCommand(OnFolderComboChanged, folderComboCommandId);
			mcs.AddCommand(folderComboCommand);

			CommandID brandComboCommandId = new CommandID(GuidList.guidComboBoxCmdSet, (int)GitToolbarPackageCmdIdList.BrandCombo);
			OleMenuCommand brandComboCommand = new OleMenuCommand(OnBrandComboChanged, brandComboCommandId);
			mcs.AddCommand(brandComboCommand);

			CommandID folderComboGetListCommandId = new CommandID(GuidList.guidComboBoxCmdSet, (int)GitToolbarPackageCmdIdList.FolderComboGetList);
			MenuCommand folderComboGetListCommand = new OleMenuCommand(OnFolderComboGetList, folderComboGetListCommandId);
			mcs.AddCommand(folderComboGetListCommand);

			CommandID brandComboGetListCommandId = new CommandID(GuidList.guidComboBoxCmdSet, (int)GitToolbarPackageCmdIdList.BrandComboGetList);
			MenuCommand brandComboGetListCommand = new OleMenuCommand(OnBrandComboGetList, brandComboGetListCommandId);
			mcs.AddCommand(brandComboGetListCommand);


			CommandID getConfigCommandId = new CommandID(GuidList.guidComboBoxCmdSet, (int)GitToolbarPackageCmdIdList.GetConfigCommandId);
			MenuCommand getConfigCommand = new OleMenuCommand(OnGetConfigPushed, getConfigCommandId);
			mcs.AddCommand(getConfigCommand);
		}

		private void OnFolderComboChanged(object sender, EventArgs e)
		{
			if (e is OleMenuCmdEventArgs eventArgs)
			{
				var input = eventArgs.InValue;
				var vOut = eventArgs.OutValue;

				if (vOut != IntPtr.Zero && input != null)
				{
					throw (new ArgumentException("Both params illegal"));
				}

				if (vOut != IntPtr.Zero)
				{
					Marshal.GetNativeVariantForObject(SelectedFolder, vOut);
				}
				else if (input != null)
				{
					SelectedFolder = input.ToString();
				}
				else
				{
					throw new ArgumentException("Input cant be null");
				}
			}
			else
			{
				throw new ArgumentException("Event args required");
			}
		}

		private void OnBrandComboChanged(object sender, EventArgs e)
		{
			if (e is OleMenuCmdEventArgs eventArgs)
			{
				var input = eventArgs.InValue;
				var vOut = eventArgs.OutValue;

				if (vOut != IntPtr.Zero && input != null)
				{
					throw (new ArgumentException("Both params illegal"));
				}

				if (vOut != IntPtr.Zero)
				{
					Marshal.GetNativeVariantForObject(SelectedBrand, vOut);
				}
				else if (input != null)
				{
					SelectedBrand = input.ToString();
				}
				else
				{
					throw new ArgumentException("Input cant be null");
				}
			}
			else
			{
				throw new ArgumentException("Event args required");
			}
		}

		private void OnFolderComboGetList(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(SelectedFolder))
			{
				SelectedFolder = Folders.First();
			}

			if (null == e || (e == EventArgs.Empty))
			{
				throw (new ArgumentNullException("Event args required"));
			}

			if (!(e is OleMenuCmdEventArgs eventArgs))
				return;

			var inParam = eventArgs.InValue;
			var vOut = eventArgs.OutValue;

			if (inParam != null)
			{
				throw (new ArgumentException("In params illegal"));
			}

			if (vOut != IntPtr.Zero)
			{
				Marshal.GetNativeVariantForObject(Folders, vOut);
			}
			else
			{
				throw (new ArgumentException("Out params required"));
			}
		}

		private void OnBrandComboGetList(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(SelectedBrand))
			{
				SelectedBrand = Brands.First();
			}

			if (null == e || (e == EventArgs.Empty))
			{
				throw (new ArgumentNullException("Event args required"));
			}

			if (!(e is OleMenuCmdEventArgs eventArgs))
				return;

			var inParam = eventArgs.InValue;
			var vOut = eventArgs.OutValue;

			if (inParam != null)
			{
				throw (new ArgumentException("In params illegal"));
			}

			if (vOut != IntPtr.Zero)
			{
				Marshal.GetNativeVariantForObject(Brands, vOut);
			}
			else
			{
				throw (new ArgumentException("Out params required"));
			}
		}

		private void OnGetConfigPushed(object sender, EventArgs e)
		{
			var command = string.Format(GitCommand, SelectedFolder, SelectedBrand);

			using (var process = new Process
			{
				StartInfo = new ProcessStartInfo(PathToGit, command)
				{
					WorkingDirectory = PathToDestinationRepository,
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			})
			{
				process.Start();
				var output = process.StandardOutput.ReadToEnd();
				var errors = process.StandardError.ReadToEnd();
				process.WaitForExit();

				var paneGuid = VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
				if (!(GetGlobalService(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow))
					return;

				outputWindow.CreatePane(paneGuid, "General", 1, 0);
				outputWindow.GetPane(paneGuid, out var pane);
				pane.OutputString($"Output: {output}, Errors: {errors}");
			}
		}

	}

	public class OptionPageGrid : DialogPage
	{
		[Category("Git Config Options Category")]
		[DisplayName("Path To Destination Git Repository")]
		[Description("Disc address to Destination repository")]
		public string PathToDestinationRepository { get; set; } = @"D:\";

		[Category("Git Config Options Category")]
		[DisplayName("Path To Git Executable")]
		[Description("Disc address to Git executable")]
		public string PathToGit { get; set; } = @"C:\Program Files\Git\git-cmd.exe";

		[Category("Git Config Options Category")]
		[DisplayName("Git Command")]
		[Description("Git command for fetching configs")]
		public string GitCommand { get; set; } = @"git get-config {0} {1} && exit";

		[Category("Git Config Options Category")]
		[DisplayName("Folders")]
		[Description("List of Folders in Configuretion repository with brand configs")]
		public string[] Folders { get; set; } = { "Destination 1" };

		[Category("Git Config Options Category")]
		[DisplayName("Brands")]
		[Description("List of Brands")]
		public string[] Brands { get; set; } =
		{
			"Folder 1",
			"Folder 2",
			"Folder 3"
		};
	}

}
