#if FALSE
namespace Amicitia.ResourceWrappers
{
    using System.IO;
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;

    internal class ResourceClassNameWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.ResourceClassName
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.ResourceClassName, (res, path) =>
                res.WrappedObject = new ResourceClassName(path)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.ResourceClassName, (res, path) =>
                (res as ResourceClassNameWrapper).WrappedObject.Save(path)
            },
        };

        /************************************/
        /* Import / export method overrides */
        /************************************/
        protected override Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetImportDelegates()
        {
            return ImportDelegates;
        }

        protected override Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetExportDelegates()
        {
            return ExportDelegates;
        }

        protected override SupportedFileType[] GetSupportedFileTypes()
        {
            return FileFilterTypes;
        }

        /***************/
        /* Constructor */
        /***************/
        public ResourceClassName(string text, ResourceClassName res) 
            : base(text, res, SupportedFileType.ResourceClassName, false /* set to true if context menu states were modified */ )
        {
			// set additional states here
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new ResourceClassName WrappedObject
        {
            get { return (ResourceClassName)m_wrappedObject; }
            set { SetProperty(ref m_wrappedObject, value); }
        }

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        internal override void RebuildWrappedObject()
        {
			/* your code here */
        }

        internal override void InitializeWrapper()
        {
			/* your code here */

            base.InitializeWrapper();
        }
    }
}
#endif