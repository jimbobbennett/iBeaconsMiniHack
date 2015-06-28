using System;

using UIKit;
using Estimote;
using System.Diagnostics;
using Foundation;
using System.Collections.Generic;

namespace ArrghTreasure
{
	/// <summary>
	/// Notes:
	/// sometimes distance is -1 - ignore these rangings.
	/// beacon matches based off uuid and version, not data values.
	/// </summary>
	public partial class ViewController : UITableViewController
	{
		private BeaconManager _beaconManager;
		private List<Beacon> _beacons = new List<Beacon>();

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			TableView.ContentInset = new UIEdgeInsets(20.0f, 0.0f, 0.0f, 0.0f);
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
			TableView.SeparatorInset = UIEdgeInsets.Zero;

			_beaconManager = new BeaconManager();
			_beaconManager.ReturnAllRangedBeaconsAtOnce = true;
			_beaconManager.RangedBeacons += BeaconManagerRangedBeacons;
			_beaconManager.AuthorizationStatusChanged += BeaconManagerAuthorizationStatusChanged;
			_beaconManager.RequestWhenInUseAuthorization();
		}

		private void BeaconManagerAuthorizationStatusChanged (object sender, AuthorizationStatusChangedArgsEventArgs e)
		{
			if (e.Status == CoreLocation.CLAuthorizationStatus.AuthorizedWhenInUse)
			{
				var uuid = new NSUuid ("B9407F30-F5F8-466E-AFF9-25556B57FE6D");
				var region = new BeaconRegion(uuid, "MyRegion");
				_beaconManager.StartRangingBeacons(region);
			}
		}

		private void BeaconManagerRangedBeacons (object sender, RangedBeaconsArgsEventArgs e)
		{
			foreach (var beacon in e.Beacons)
			{
				Debug.WriteLine("Ranged beacon {0}", beacon.Color);

				if (beacon.Distance.Int32Value != -1)
				{
					_beacons.Remove(beacon);
					_beacons.Add(beacon);
				}
				else
				{
					Debug.WriteLine("-1");
				}
			}

			_beacons.Sort(CompareBeaconsByDistance);

			TableView.ReloadData();
		}

		private static int CompareBeaconsByDistance(Beacon x, Beacon y)
		{
			return x.Distance.CompareTo(y.Distance);
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return _beacons.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell("Cell");
			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Subtitle, "Cell");

			var beacon = _beacons[indexPath.Row];

			cell.TextLabel.Text = string.Format("{0} - {1}.{2}", beacon.Color.ToString(), beacon.Major, beacon.Minor);
			cell.ImageView.Image = UIImage.FromBundle("Estimote");
			cell.BackgroundColor = GetColourForBeacon(beacon.Color).ColorWithAlpha(0.5f);
			cell.DetailTextLabel.Text = string.Format("{0:N2}m", beacon.Distance.DoubleValue);

			return cell;
		}

		private UIColor GetColourForBeacon(BeaconColor beaconColor)
		{
			switch (beaconColor)
			{
				case BeaconColor.Blueberry:
					return UIColor.FromRGB(72, 61, 139);
				case BeaconColor.Mint:
					return UIColor.FromRGB(173, 255, 47);
				case BeaconColor.Ice:
					return UIColor.FromRGB(72, 209, 204);
				case BeaconColor.White:
					return UIColor.White;
				default:
					return UIColor.LightGray;
			}
		}
	}
}

