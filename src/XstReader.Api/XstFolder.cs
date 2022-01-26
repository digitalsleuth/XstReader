﻿// Project site: https://github.com/iluvadev/XstReader
//
// Based on the great work of Dijji. 
// Original project: https://github.com/dijji/XstReader
//
// Issues: https://github.com/iluvadev/XstReader/issues
// License (Ms-PL): https://github.com/iluvadev/XstReader/blob/master/license.md
//
// Copyright (c) 2021 iluvadev, and released under Ms-PL License.
// Copyright (c) 2016, Dijji, and released under Ms-PL.  This can be found in the root of this distribution. 

using System.Collections.Generic;
using System.Linq;
using XstReader.Common.BTrees;
using XstReader.ElementProperties;

namespace XstReader
{
    /// <summary>
    /// Class for a Folder stored inside an .ost or .pst file
    /// </summary>
    public class XstFolder : XstElement
    {
        /// <summary>
        /// The Container File
        /// </summary>
        internal protected override XstFile XstFile { get; }

        /// <summary>
        /// Number of messages inside the Folder
        /// </summary>
        public uint ContentCount => Properties[PropertyCanonicalName.PidTagContentCount]?.Value ?? (uint)0;

        /// <summary>
        /// Number of unread messages inside the Folder
        /// </summary>
        public uint ContentUnreadCount => Properties[PropertyCanonicalName.PidTagContentUnreadCount]?.Value ?? (uint)0;

        /// <summary>
        /// The Parent Folder of this Folder
        /// </summary>
        public XstFolder ParentFolder { get; set; }
        private IEnumerable<XstFolder> _Folders = null;

        /// <summary>
        /// The Folders contained inside this Folder
        /// </summary>
        public IEnumerable<XstFolder> Folders => GetFolders();
        public bool HasSubFolders => Folders.Any();

        private string _Path = null;
        /// <summary>
        /// The Path of this Folder
        /// </summary>
        public string Path => _Path ?? (_Path = string.IsNullOrEmpty(ParentFolder?.DisplayName) ? DisplayName : $"{ParentFolder.Path}\\{DisplayName}");

        private IEnumerable<XstMessage> _Messages = null;
        /// <summary>
        /// The Messages contained in the Folder
        /// </summary>
        public IEnumerable<XstMessage> Messages => GetMessages();
        /// <summary>
        /// The unread Messages contained in the Folder
        /// </summary>
        public IEnumerable<XstMessage> UnreadMessages => Messages.Where(m => !m.IsRead);

        private BTree<Node> _SubnodeTreeProperties = null;


        #region Ctor
        internal XstFolder(XstFile xstFile, NID nid, XstFolder parentFolder = null)
        {
            XstFile = xstFile;
            Nid = nid;
            ParentFolder = parentFolder;
            _SubnodeTreeProperties = Ltp.ReadProperties(nid, Properties);
            //_SubnodeTreeProperties = Ltp.ReadProperties<XstFolder>(nid, PropertyGetters.FolderProperties, this);
        }
        #endregion Ctor

        #region Properties
        private protected override IEnumerable<XstProperty> LoadProperties()
            => Ltp.ReadAllProperties(Nid);

        #endregion Properties

        #region Folders
        /// <summary>
        /// Returns all the Folders contained inside this Folder
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XstFolder> GetFolders()
        {
            if (_Folders == null)
                _Folders = Ltp.ReadTableRowIds(NID.TypedNID(EnidType.HIERARCHY_TABLE, Nid))
                              .Where(id => id.nidType == EnidType.NORMAL_FOLDER)
                              .Select(id => new XstFolder(XstFile, id, this))
                              .OrderBy(sf => sf.DisplayName);

            return _Folders;
        }
        #endregion Folders

        #region Messages
        /// <summary>
        /// Returns all the Messages contained inside this Folder
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XstMessage> GetMessages()
        {
            if (_Messages == null)
            {
                if (ContentCount > 0)
                    // Get the Contents table for the folder
                    // For 4K, not all the properties we want are available in the Contents table, so supplement them from the Message itself
                    _Messages = Ltp.ReadTable<XstMessage>(NID.TypedNID(EnidType.CONTENTS_TABLE, Nid),
                                                          (m, id) => m.Initialize(new NID(id), this),
                                                          m => m.ProcessSignedOrEncrypted());
                else
                    _Messages = new XstMessage[0];
            }
            return _Messages;
        }

        #endregion Messages

        private void ClearFolders()
        {
            if (_Folders != null)
                foreach (var folder in _Folders)
                    folder.ClearContentsInternal();
            _Folders = null;
        }
        private void ClearMessages()
        {
            if (_Messages != null)
                foreach (var message in _Messages)
                    message.ClearContentsInternal();
            _Messages = null;
        }
        internal override void ClearContentsInternal()
        {
            base.ClearContentsInternal();
            ClearFolders();
            ClearMessages();
        }
    }
}