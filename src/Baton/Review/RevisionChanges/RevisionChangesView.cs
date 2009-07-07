﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using Baton.Review.ChangePresentation;
using Chorus.merge;

namespace Baton.Review.RevisionChanges
{
	public partial class RevisionChangesView : UserControl
	{
		private readonly RevisionChangesModel _model;

		public RevisionChangesView(RevisionChangesModel model)
		{
			InitializeComponent();
			_model = model;
			_model.UpdateDisplay += OnUpdateDisplay;
		}

		void OnUpdateDisplay(object sender, EventArgs e)
		{
			var items = new List<ListViewItem>();
			listView1.Items.Clear();
			if (_model.ChangeReports != null)
			{
				foreach (var report in _model.ChangeReports)
				{
					IChangePresenter presenter = ChangePresenterFactory.GetChangePresenter(report);
					var row = new ListViewItem(new string[] {presenter.GetDataLabel(), presenter.GetActionLabel()});
					items.Add(row);
				}

				listView1.Items.AddRange(items.ToArray());
			}
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			_model.SelectedChangeChanged(CurrentRecord);
		}

		protected IChangeReport CurrentRecord
		{
			get
			{
				if (listView1.SelectedItems.Count == 1)
				{
					return listView1.SelectedItems[0].Tag as IChangeReport;
				}
				else
				{
					return null;
				}
			}
		}
	}

}