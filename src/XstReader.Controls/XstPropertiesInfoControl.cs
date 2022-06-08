﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XstReader.App.Common;

namespace XstReader.App.Controls
{
    public partial class XstPropertiesInfoControl : UserControl,
                                                    IXstDataSourcedControl<XstElement>
    {
        public XstPropertiesInfoControl()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            if (DesignMode) return;
        }

        private XstElement? _DataSource;
        public XstElement? GetDataSource()
            => _DataSource;

        public void SetDataSource(XstElement? dataSource)
        {
            _DataSource = dataSource;
            LoadProperties();
        }

        private void LoadProperties()
        {
            ElementTypeLabel.Text = _DataSource?.ElementType.ToString();
            ElementNameLabel.Text = _DataSource?.ToString();

            try { PropertyGridInfo.SelectedObject = _DataSource; }
            catch { }
        }

        public void ClearContents()
        {
            GetDataSource()?.ClearContents();
            SetDataSource(null);
        }

    }
}
