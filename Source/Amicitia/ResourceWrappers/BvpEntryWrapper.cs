using System.IO;
using AmicitiaLibrary.FileSystems.BVP;

namespace Amicitia.ResourceWrappers
{
    public class BvpEntryWrapper : ResourceWrapper<BvpEntry>
    {
        public int Flag
        {
            get => Resource.Flag;
            set => SetProperty(Resource, value);
        }

        public BvpEntryWrapper(string text, BvpEntry resource) : base(text, resource) { }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.MessageScript, (res, path) => File.WriteAllBytes(path, res.Data));
            RegisterFileReplaceAction(SupportedFileType.MessageScript, (res, path) => new BvpEntry(File.ReadAllBytes(path), res.Flag));
        }

        protected override void PopulateView() { }
    }
}
