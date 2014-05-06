using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Couchbase.Lite;
using Couchbase.Lite.Util;
//using AndroidHUD;
using System.IO;

namespace CouchXamAndroidSample
{
	[Activity (Label = "CouchXamAndroidSample", MainLauncher = true)]
	public class MainActivity : Activity, Android.Views.View.IOnClickListener {
		//		int count = 1;
		public const string Tag = "LiteTestCase";
		Manager manager = null;
		Database database = null;
		Replication repls = null;
		string RemoteURLDBName;
		string RemoteDBName;
		Button btnReplicate, btnStop, btnCount;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			RemoteURLDBName = @"http://192.168.15.20:5984/db1";
			RemoteDBName = @"db_test1"; //3500 rows
			
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			btnReplicate = FindViewById<Button> (Resource.Id.btnReplicate);
			btnStop = FindViewById<Button> (Resource.Id.btnStop);
			btnCount = FindViewById<Button> (Resource.Id.btnCount);
			Button btnGC = FindViewById<Button> (Resource.Id.btnGC);

			btnReplicate.SetOnClickListener (this);
			btnStop.SetOnClickListener (this);
			btnCount.SetOnClickListener (this);
			btnGC.SetOnClickListener (this);

			StartCBLite ();
			database = GetDatabase (RemoteDBName);
			repls = CreatePullReplication ();

			repls.Changed += HandleChanged;

		}


		public void OnClick(Android.Views.View v){

			int id = v.Id;

			switch (id) {
			case Resource.Id.btnReplicate:
				StartReplication ();
				break;
			case Resource.Id.btnStop:
				StopReplication ();		
				break;
			case Resource.Id.btnCount:
				reviewDocuments ();
				break;
			case Resource.Id.btnGC:
				callGC ();
				break;
			}

		}

		int count = 0;
		int lastcoutn = 0;
		int callGCCount = 50;

		void HandleChanged (object sender, Replication.ReplicationChangeEventArgs e)
		{
			Replication replication = e.Source;

			if (replication == null)
				return;

			//Console.WriteLine("Replication : " + replication + " changed.");
			if (!replication.IsRunning) {
				//String msg = String.Format("Replicator {0} not running", replication);
				//AndHUD.Shared.Dismiss (this);
				Console.WriteLine ("I'm not Running");
			} else {
				float processed = (float)replication.CompletedChangesCount;
				float total = (float)replication.ChangesCount;
				float progress = processed / total * 100;

				String msg = String.Format ("Replicator processed {0} / {1}", processed, total);
				String msg2 = String.Format (" {0} / {1}", processed, total);
				//button.Text = "Replicando"+msg2;
				count = (int) processed;
				int done = count - lastcoutn;
				if (done > callGCCount) { //hack del inge
					lastcoutn = count;
					Log.D ("REPLICATION:", "reaslease mah memory");
					GC.Collect ();
				}

				if (processed >= total) {
					//AndHUD.Shared.Dismiss (this);
				} else {
					//AndHUD.Shared.Show (context: this, status: "Downloading " + progress.ToString () + "% \n" + msg2, progress: (int)progress);
				}

				Console.WriteLine (msg);
				Log.D ("REPLICATION:", msg); //+ row.getDocumentId());
			}
			
		}

		void StartCBLite ()
		{
			if (manager == null) {
				manager = new Manager ();//(testPath, Manager.DefaultOptions);
			}

		}

		Database GetDatabase (string dbName)
		{
			return manager.GetDatabase (dbName);
		}

		Replication CreatePullReplication ()
		{
			if (database == null) {
				database = GetDatabase (RemoteDBName);
			}

			return database.CreatePullReplication (new Uri (RemoteURLDBName));
		}

		void StartReplication ()
		{
			if (repls == null) {
				repls = CreatePullReplication ();
			}

			try {
				repls.Start ();
			} catch (Exception e) {
				ReleaseObjects ();
				Log.D ("EXCEPTION TRWON:", "RESTARTING");
			}
				
		}

		void StopReplication ()
		{
			try {
				/*foreach(Replication r in database.AllReplications){
						r.Stop(); 
					}*/
				repls.Stop ();
			} catch (Exception e) {
				//ReleaseObjects (); 
				repls = null;
				Log.D ("EXCEPTION TRWON:", "STOPING");
			}

		}

		void reviewDocuments ()
		{
			if (database == null)
				database = GetDatabase (RemoteDBName);

			try {
				//QueryEnumerator rowEnum = query.Run ();
				Log.D ("DOCUMENT COUNT:", " " + database.DocumentCount); //+ row.getDocumentId());
				Toast.MakeText (this, "Docs: " + database.DocumentCount, ToastLength.Short).Show ();
				/*foreach (QueryRow row in rowEnum) {
					Log.D ("Document ID:", " " + row.Document.GetProperty ("nombre")); //+ row.getDocumentId());
				}*/
			} catch (Exception e) {
				Log.D (Tag, e.Message + " ");
			}
		}

		void ReleaseObjects(){
			repls = null;
			database = null;
			manager.Close ();
			GC.Collect();
		}


		void callGC ()
		{
			ReleaseObjects ();
		}
	}
}


