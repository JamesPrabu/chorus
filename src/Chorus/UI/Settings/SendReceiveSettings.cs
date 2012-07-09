﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Chorus.UI.Misc;
using Chorus.Utilities.code;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Settings
{
	public partial class SendReceiveSettings : Form
	{
		private SettingsModel _model;

		private ServerSettingsModel _internetModel;

		private NetworkFolderSettingsModel _sharedFolderModel;


		[Obsolete("for designer support only")]
		public SendReceiveSettings()
		{
			InitializeComponent();
		}

		public SendReceiveSettings(string repositoryLocation)
		{
			InitializeComponent();

			RequireThat.Directory(repositoryLocation).Exists();
			var repository = HgRepository.CreateOrLocate(repositoryLocation, new NullProgress());
			_model = new SettingsModel(repository);
			userNameTextBox.Text = _model.GetUserName(new NullProgress());

			_internetModel = new ServerSettingsModel();
			_internetModel.InitFromProjectPath(repositoryLocation);
			_serverSettingsControl.Model = _internetModel;

			_internetButtonEnabledCheckBox.CheckedChanged += internetCheckChanged;
			_internetButtonEnabledCheckBox.Checked = Properties.Settings.Default.InternetEnabled;
			_serverSettingsControl.Enabled = _internetButtonEnabledCheckBox.Checked;

			_sharedFolderModel = new NetworkFolderSettingsModel();
			_sharedFolderModel.InitFromProjectPath(repositoryLocation);
			_sharedFolderSettingsControl.Model = _sharedFolderModel;

			_sharedFolderButtonEnabledCheckBox.CheckedChanged += networkFolderCheckChanged;
			_sharedFolderButtonEnabledCheckBox.Checked = Properties.Settings.Default.SharedFolderEnabled;
			_sharedFolderSettingsControl.Enabled = _sharedFolderButtonEnabledCheckBox.Checked;
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			if(_internetButtonEnabledCheckBox.Checked)
			{
				_internetModel.SaveSettings();
			}
			if (_sharedFolderButtonEnabledCheckBox.Checked)
			{
				_sharedFolderModel.SaveSettings();
			}
			_model.SaveSettings();
			Properties.Settings.Default.InternetEnabled = _internetButtonEnabledCheckBox.Checked;
			Properties.Settings.Default.SharedFolderEnabled = _sharedFolderButtonEnabledCheckBox.Checked;
			Properties.Settings.Default.Save();
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void userNameTextBox_TextChanged(object sender, EventArgs e)
		{
			var _userName = userNameTextBox;
			if (_model.GetUserName(new NullProgress()) != _userName.Text.Trim() && _userName.Text.Trim().Length > 0)
			{
				_model.SetUserName(_userName.Text.Trim(), new NullProgress());
			}
		}

		private void internetCheckChanged(object sender, EventArgs e)
		{
			_serverSettingsControl.Enabled = _internetButtonEnabledCheckBox.Checked;
		}

		private void networkFolderCheckChanged(object sender, EventArgs e)
		{
			_sharedFolderSettingsControl.Enabled = _sharedFolderButtonEnabledCheckBox.Checked;
		}
	}
}