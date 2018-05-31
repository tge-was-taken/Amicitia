using System.IO;
using System.Windows.Forms;
using AmicitiaLibrary.FileSystems.AMD;

namespace Amicitia.ResourceWrappers
{
    public class AmdFileWrapper : ResourceWrapper<AmdFile>
    {
        public AmdFileWrapper(string text, AmdFile resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.AmdFile, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.AmdFile, (res, path) => new AmdFile(path));
            RegisterFileAddAction(SupportedFileType.Resource, DefaultFileAddAction);
            RegisterRebuildAction((wrap) =>
            {
                AmdFile file = new AmdFile();

                foreach (ResourceWrapper<AmdChunk> node in wrap.Nodes)
                {
                    file.Chunks.Add(node.Resource);
                }

                return file;
            });
        }

        protected override void PopulateView()
        {
            foreach (AmdChunk chunk in Resource.Chunks)
            {
                var wrapper = new AmdChunkWrapper( chunk.Tag, chunk );
                Nodes.Add( wrapper );
            }
        }
    }

    public class AmdChunkWrapper : ResourceWrapper<AmdChunk>
    {
        public new string Tag
        {
            get { return Resource.Tag; }
            set { SetProperty(Resource.Tag, value); }
        }

        public int Flags
        {
            get { return Resource.Flags; }
            set { SetProperty(Resource.Flags, value); }
        }

        public int Size
        {
            get { return Resource.Size; }
        }

        public AmdChunkWrapper(string text, AmdChunk resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.Resource, (res, path) => File.WriteAllBytes(path, res.Data));
            RegisterFileReplaceAction(SupportedFileType.Resource, (res, path) => new AmdChunk(res.Tag, res.Flags, File.ReadAllBytes(path)));
        }

        protected override void PopulateView()
        {
        }
    }
}
