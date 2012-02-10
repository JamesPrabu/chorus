﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	public class PullStorageEnvForTest : IDisposable
	{
		public TemporaryFolder BaseFolder;
		public PullStorageEnvForTest()
		{
			BaseFolder = new TemporaryFolder("PullStorageHelperTests");
		}

		public string PullDataFolderPath
		{
			get { return Path.Combine(BaseFolder.Path, "pullData"); }
		}

		public void Dispose()
		{
			BaseFolder.Dispose();
		}
	}

	[TestFixture]
	public class PullStorageManagerTests
	{

		[Test]
		public void WriteChunk_Text_WriteOk()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				string sampleText = "some sample text";
				byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
				bundleHelper.WriteChunk(0, bytes);
				string fromFile = File.ReadAllText(bundleHelper.BundlePath);
				Assert.That(sampleText, Is.EqualTo(fromFile));
			}
		}

		[Test]
		public void StartOfWindow_WriteBytes_LengthOfBytes()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				string sampleText = "some sample text";
				byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
				bundleHelper.WriteChunk(0, bytes);
				Assert.That(bundleHelper.StartOfWindow, Is.EqualTo(bytes.Length));
			}
		}

		[Test]
		public void StartOfWindow_NoWrites_Zero()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				Assert.That(bundleHelper.StartOfWindow, Is.EqualTo(0));
			}
		}

		[Test]
		public void WriteChunk_WriteMultipleChunks_AssembledOk()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				string[] sampleTexts = new[] { "this is a sentence. ", "Another sentence. ", "Yet another word string." };

				string combinedText = "";
				int startOfWindow = 0;
				foreach (var sampleText in sampleTexts)
				{
					combinedText += sampleText;
					byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
					bundleHelper.WriteChunk(startOfWindow, bytes);
					startOfWindow += bytes.Length;
				}
				Assert.That(combinedText, Is.EqualTo(File.ReadAllText(bundleHelper.BundlePath)));
			}
		}

		[Test]
		public void WriteChunk_MultipleWritesOverWriteSameOffset_AssemblesOk()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				string[] sampleTexts = new[] { "this is a sentence. ", "Another sentence. ", "Yet another word string." };

				string combinedText = "";
				int startOfWindow = 0;
				foreach (var sampleText in sampleTexts)
				{
					combinedText += sampleText;
					byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
					bundleHelper.WriteChunk(startOfWindow, bytes);
					startOfWindow += bytes.Length;
				}
				bundleHelper.WriteChunk(0, Encoding.UTF8.GetBytes(sampleTexts[0]));
				Assert.That(combinedText, Is.EqualTo(File.ReadAllText(bundleHelper.BundlePath)));
			}
		}


	}


	public class BundleStorageEnvForTest : IDisposable
	{
		public TemporaryFolder BaseFolder;
		public BundleStorageEnvForTest()
		{
			BaseFolder = new TemporaryFolder("BundleStorageHelperTests");
		}

		public string DataFolderPath
		{
			get { return Path.Combine(BaseFolder.Path, "data"); }
		}

		public void Dispose()
		{
			BaseFolder.Dispose();
		}
	}

	internal class SimpleStorageManager : BundleStorageManager
	{
		public SimpleStorageManager(string storagePath, string baseHash ) : base(storagePath, baseHash)
		{
		}

		public override string StorageFolderName
		{
			get
			{
				return "data";
			}
		}
	}

	[TestFixture]
	public class BundleStorageManagerTests
	{
		[Test]
		public void Constructor_NoPreviousTransactionIdFile_FileIsCreatedAndPopulated()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				string idFilePath = Path.Combine(e.DataFolderPath, "abcde123.transid");
				Assert.That(File.Exists(idFilePath), Is.False);
				new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				Assert.That(File.Exists(idFilePath), Is.True);
				Assert.That(File.ReadAllText(idFilePath).Trim(), Is.Not.Empty);
			}
		}

		[Test]
		public void Constructor_PreviousTransactionIdFileExists_ReturnsFoundTransactionId()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				var bundleHelper = new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				var bundleHelper2 = new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				Assert.That(bundleHelper.TransactionId, Is.EqualTo(bundleHelper2.TransactionId));
			}
		}

		[Test]
		public void Constructor_PreviousTransactionIdFileNotExists_GetNewId()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				string idFilePath = Path.Combine(e.DataFolderPath, "abcde123.transid");
				var bundleHelper = new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				File.Delete(idFilePath);
				var bundleHelper2 = new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				Assert.That(bundleHelper.TransactionId, Is.Not.EqualTo(bundleHelper2.TransactionId));
			}
		}

		[Test]
		public void Constructor_PullDataFolderNotExist_FolderCreated()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				Assert.That(Directory.Exists(e.DataFolderPath), Is.False);
				new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				Assert.That(Directory.Exists(e.DataFolderPath), Is.True);
			}
		}

		[Test]
		public void Constructor_MultipleInstances_ManagesMultipleInstancesOk()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				var pull1a = new SimpleStorageManager(e.BaseFolder.Path, "number1");
				var pull2a = new SimpleStorageManager(e.BaseFolder.Path, "number2");
				var pull3a = new SimpleStorageManager(e.BaseFolder.Path, "number3");
				var pull4a = new SimpleStorageManager(e.BaseFolder.Path, "number4");
				var pull1b = new SimpleStorageManager(e.BaseFolder.Path, "number1");
				var pull2b = new SimpleStorageManager(e.BaseFolder.Path, "number2");
				var pull3b = new SimpleStorageManager(e.BaseFolder.Path, "number3");
				var pull4b = new SimpleStorageManager(e.BaseFolder.Path, "number4");
				Assert.That(pull1a.TransactionId, Is.EqualTo(pull1b.TransactionId));
				Assert.That(pull2a.TransactionId, Is.EqualTo(pull2b.TransactionId));
				Assert.That(pull3a.TransactionId, Is.EqualTo(pull3b.TransactionId));
				Assert.That(pull4a.TransactionId, Is.EqualTo(pull4b.TransactionId));
			}
		}

		[Test]
		public void Constructor_ExpiredData_DataIsRemoved()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				string expiredFile = Path.Combine(e.DataFolderPath, "123.transid");
				var expiredTime = DateTime.Now;
				expiredTime = expiredTime.AddHours(-50);
				Directory.CreateDirectory(e.DataFolderPath);

				using (var fs = File.Create(expiredFile)) {}
				File.SetCreationTime(expiredFile, expiredTime);

				Assert.That(File.Exists(expiredFile), Is.True);
				var sm = new SimpleStorageManager(e.BaseFolder.Path, "abc");
				Assert.That(File.Exists(expiredFile), Is.False);
			}
		}
	}
}