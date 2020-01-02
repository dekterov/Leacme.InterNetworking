// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Humanizer;
using Leacme.Lib.InterNetworking;

namespace Leacme.App.InterNetworking {

	public class AppUI {

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library lib = new Library();

		private Grid rootGrid = new Grid();
		private ScrollViewer gridScroll = App.ScrollViewer;
		private IEnumerable<DataGrid> niControls = new List<DataGrid>();

		public AppUI() {

			var itfs = lib.GetNetworkInterfaces().Select(z => new PrunedNetInterface(z));
			foreach (var itf in itfs) {

				var dg = App.DataGrid;
				dg.Height = 430;
				dg.Margin = new Thickness(5);
				dg.SetValue(DataGrid.WidthProperty, AvaloniaProperty.UnsetValue);

				dg.Items = BuildNinfo(itf).Select(z => new { z.Variable, z.Value });

				((List<DataGrid>)niControls).Add(dg);
			}

			var blb1 = App.TextBlock;
			blb1.TextAlignment = TextAlignment.Center;
			blb1.Text = "Current Network Interfaces";

			gridScroll.Height = App.Current.MainWindow.Height - 100;
			gridScroll.Background = Brushes.Transparent;

			App.Current.MainWindow.PropertyChanged += (z, zz) => {
				if (zz.Property.Equals(Window.HeightProperty)) {
					gridScroll.Height = App.Current.MainWindow.Height - 100;
				}
			};

			gridScroll.Content = this.rootGrid;
			rootGrid.Margin = new Thickness(10, 0);
			rootGrid.Background = Brushes.Transparent;
			rootGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
			rootGrid.ColumnDefinitions = new ColumnDefinitions("*,*,*");

			int R = (int)Math.Ceiling((decimal)itfs.Count() / 3);
			for (int i = 0; i < R; i++) {
				rootGrid.RowDefinitions.Add(new RowDefinition());
			}
			int emtCountr = 0;
			for (int r = 0; r < R; r++) {
				for (int c = 0; c < 3; c++) {
					if (emtCountr < niControls.Count()) {
						niControls.ElementAt(emtCountr)[Grid.RowProperty] = r;
						niControls.ElementAt(emtCountr++)[Grid.ColumnProperty] = c;
					}
				}
			}
			rootGrid.Children.AddRange(niControls);
			rootPan.Children.AddRange(new List<IControl> { blb1, gridScroll });

			DispatcherTimer.Run(() => {
				foreach (var nis in lib.GetNetworkInterfaces().Select(z => new PrunedNetInterface(z))) {
					if (niControls.Any(z => {
						return z.Items.Cast<dynamic>().Any(zz => { return zz.Variable.Equals("Id") && nis.Id.Equals(zz.Value); });
					})) {

						var tempNic = niControls.First(z => z.Items.Cast<dynamic>().Any(zz => { return zz.Variable.Equals("Id") && nis.Id.Equals(zz.Value); }));
						tempNic.Items = BuildNinfo(nis).Select(z => new { z.Variable, z.Value });

					}
				}
				return true;
			}, TimeSpan.FromSeconds(3));
		}

		private List<(string Variable, string Value)> BuildNinfo(PrunedNetInterface itf) {
			var niInfo = new List<(string Variable, string Value)>();

			foreach (PropertyInfo propInfo in itf.GetType().GetProperties()) {
				var prop = propInfo.GetValue(itf);
				if (prop is IEnumerable<IPAddress>) {
					foreach (var subListProp in (IEnumerable<IPAddress>)prop) {
						niInfo.Add((propInfo.Name.Humanize().ApplyCase(LetterCasing.Title), subListProp.ToString()));
					}
				} else {
					niInfo.Add((propInfo.Name.Humanize().ApplyCase(LetterCasing.Title), prop.ToString()));
				}
			}
			return niInfo;
		}
	}

}