using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AmicitiaLibrary.FileSystems.BVP;
using Ookii.Dialogs.WinForms;

namespace Amicitia.ResourceWrappers
{
    public class BvpFileWrapper : ResourceWrapper<BvpFile>
    {
        public int EntryCount => Nodes.Count;

        public BvpFileWrapper(string text, BvpFile resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.BvpArchiveFile, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.BvpArchiveFile, (res, path) => new BvpFile(path));
            RegisterFileAddAction(SupportedFileType.Resource, DefaultFileAddAction);
            RegisterRebuildAction((wrap) =>
            {
                BvpFile file = new BvpFile();

                foreach (BvpEntryWrapper node in Nodes)
                {
                    file.Entries.Add(new BvpEntry(node.Resource.Data, node.Resource.Flag));
                }

                return file;
            });
            RegisterCustomAction("Export all", 0, ExportAllAction);
            RegisterCustomAction("Replace all", 0, ReplaceAllAction);
        }

        protected override void PopulateView()
        {
            for (var i = 0; i < Resource.Entries.Count; i++)
            {
                Nodes.Add(new BvpEntryWrapper($"entry_{i:0000}.bmd", Resource.Entries[i]));
            }
        }

        private void ExportAllAction(object sender, EventArgs e)
        {
            using (var dialog = new VistaFolderBrowserDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                foreach (BvpEntryWrapper entry in Nodes)
                {
                    File.WriteAllBytes(
                        Path.Combine(dialog.SelectedPath, entry.Text),
                        entry.Resource.Data);
                }
            }
        }

        private void ReplaceAllAction(object sender, EventArgs e)
        {
            using (var dialog = new VistaFolderBrowserDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                var notReplaced = new List<string>();
                foreach (BvpEntryWrapper node in Nodes)
                {
                    var file = Path.Combine(dialog.SelectedPath, node.Text);
                    if (File.Exists(file))
                        node.Replace(file);
                }

                if (notReplaced.Count == 0)
                {
                    MessageBox.Show("All entries were replaced successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Some entries were not replaced:\n{string.Join("\n", notReplaced)}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
