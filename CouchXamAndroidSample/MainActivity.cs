using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Couchbase.Lite;
using Couchbase.Lite.Util;
using AndroidHUD;
using System.IO;

namespace CouchXamAndroidSample
{
	[Activity (Label = "CouchXamAndroidSample", MainLauncher = true)]
	public class MainActivity : Activity
	{
		//		int count = 1;
		public const string Tag = "LiteTestCase";
		Manager manager = null;
		Database database = null;
		Replication repls = null;
		string RemoteURLDBName;
		string RemoteDBName;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			#warning "Set your URL and DB name"
			RemoteURLDBName = @"URL";
			RemoteDBName = @"DBName";

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);

			button.Click += delegate {
				//				button.Text = string.Format ("{0} clicks!", count++);

				if (repls == null) {
					StartCBLite();
					database = GetDatabase(RemoteDBName);
					repls = database.CreatePullReplication(new Uri(RemoteURLDBName));

					repls.Changed += (object sender, Replication.ReplicationChangeEventArgs e) => {
						Replication replication = e.Source;

						if (replication == null) return;

						//						Console.WriteLine("Replication : " + replication + " changed.");
						if (!replication.IsRunning) {
							//							String msg = String.Format("Replicator {0} not running", replication);
							AndHUD.Shared.Dismiss (this);
							Console.WriteLine ("I'm not Running");
						}
						else {
							float processed = (float)replication.CompletedChangesCount;
							float total = (float)replication.ChangesCount;
							float progress = processed / total * 100;

							String msg = String.Format("Replicator processed {0} / {1}", processed, total);

							if (processed >= total) {
								AndHUD.Shared.Dismiss (this);
							} else {
								AndHUD.Shared.Show (context: this, status: "Downloading " + progress.ToString () + "%", progress: (int)progress);
							}

							Console.WriteLine (msg);
						}
					};

					repls.Start();
				}

			};

		}

		void StartCBLite()
		{
			string serverPath = GetServerPath();
			var path = new DirectoryInfo(serverPath);

			if (path.Exists)
				path.Delete(true);

			path.Create();

			var testPath = path.CreateSubdirectory("tests");
			manager = new Manager(testPath, Manager.DefaultOptions);
		}

		protected internal virtual string GetServerPath()
		{
			var filesDir = GetRootDirectory().FullName;
			return filesDir;
		}

		DirectoryInfo GetRootDirectory()
		{
			var rootDirectoryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
			var rootDirectory = new DirectoryInfo(Path.Combine(rootDirectoryPath, "couchbase/tests/files"));
			return rootDirectory;
		}

		Database GetDatabase(string dbName)
		{
			Database db = manager.GetExistingDatabase(dbName);
			if (db != null)
			{
				//				var status = false;

				try {
					db.Delete ();
					//					status = true;
				} catch (Exception e) { 
					//					Log.E(Tag, "Cannot delete database " + e.Message);
				}
			}
			db = manager.GetDatabase(dbName);
			return db;
		}

	}
}


