using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AmicitiaLibrary.Graphics.TMX;
using AmicitiaLibrary.PS2.Graphics;

namespace Amicitia.ResourceWrappers
{
	public class TmxListWrapper : GenericListWrapper<TmxFile>
	{
		public TmxListWrapper(string text, IList<TmxFile> resource, Func<TmxFile, int, string> elementNameProvider) : base(text, resource, elementNameProvider)
		{
		}

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Add | CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileAddAction(SupportedFileManager.GetSupportedFileType(typeof(TmxFile)), DefaultFileAddAction);
            RegisterFileAddAction(SupportedFileManager.GetSupportedFileType(typeof(TmxFile)), DefaultFileAddAction);
            RegisterFileAddAction(SupportedFileType.Bitmap, (path, res) => {
                var name = Path.GetFileName(path);
                var tex = new TmxFile(new Bitmap(path),0, PS2PixelFormat.PSMT8,name);//, short userId = 0, PS2PixelFormat pixelFormat = PS2PixelFormat.PSMT8, string comment = ""
                SetRebuildFlag(res.Parent);
                res.Nodes.Add(new TmxFileWrapper(name,tex));
                res.NeedsRebuild = true;
            });

            RegisterRebuildAction(wrap =>
            {
                List<TmxFile> list = new List<TmxFile>();

                foreach (IResourceWrapper node in Nodes)
                {
                    list.Add((TmxFile)node.Resource);
                }

                return list;
            });

            PostInitialize();
        }
    }
}
