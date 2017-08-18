using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Amicitia.ResourceWrappers
{
    public class GenericListWrapper<TResource> : ResourceWrapper<IList<TResource>>
    {
        private Func<TResource, int, string> mElementNameProvider;

        public int Count => Resource.Count;

        public GenericListWrapper(string text, IList<TResource> resource, Func<TResource, int, string> elementNameProvider) : base(text, resource)
        {
            mElementNameProvider = elementNameProvider;
            PopulateView();
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Add | CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileAddAction(SupportedFileManager.GetSupportedFileType(typeof(TResource)), DefaultFileAddAction);
            RegisterRebuildAction(wrap =>
            {
                List<TResource> list = new List<TResource>();

                foreach (ResourceWrapper<TResource> node in Nodes)
                {
                    list.Add(node.Resource);
                }

                return list;
            });

            PostInitialize();
        }

        /// <summary>
        /// Executed after list is initialized. Can be used to add extra actions or the like.
        /// </summary>
        protected virtual void PostInitialize()
        {

        }

        protected override void PopulateView()
        {
            if (mElementNameProvider != null)
            {
                for (var i = 0; i < Resource.Count; i++)
                {
                    var res = Resource[i];
                    string name = mElementNameProvider.Invoke(res, i);

                    Nodes.Add((TreeNode) ResourceWrapperFactory.GetResourceWrapper(name, res));
                }
            }
        }
    }
}
