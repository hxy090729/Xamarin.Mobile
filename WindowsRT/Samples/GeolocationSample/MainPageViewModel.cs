﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Xamarin.Geolocation;

namespace GeolocationSample
{
	public class MainPageViewModel
		: INotifyPropertyChanged
	{
		public MainPageViewModel (Windows.UI.Core.CoreDispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
			this.geolocator.DesiredAccuracy = 50;
			this.geolocator.PositionError += GeolocatorOnPositionError;
			this.geolocator.PositionChanged += GeolocatorOnPositionChanged;
			this.getPosition = new DelegatedCommand (GetPositionHandler, s => true);
			this.toggleListening = new DelegatedCommand (ToggleListeningHandler, s => true);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private string status;
		public string Status
		{
			get { return this.status; }
			private set
			{
				if (this.status == value)
					return;

				this.status = value;
				OnPropertyChanged ("Status");
			}
		}

		private readonly DelegatedCommand getPosition;
		public ICommand GetPosition
		{
			get { return this.getPosition; }
		}

		private readonly DelegatedCommand toggleListening;
		public ICommand ToggleListening
		{
			get { return this.toggleListening; }
		}

		private Position currentPosition;
		public Position CurrentPosition
		{
			get { return this.currentPosition; }
			private set
			{
				if (this.currentPosition == value)
					return;

				this.currentPosition = value;
				OnPropertyChanged ("CurrentPosition");
			}
		}

		private readonly CoreDispatcher dispatcher;
		private readonly Geolocator geolocator = new Geolocator();

		private async void GetPositionHandler (object state)
		{
			if (!this.geolocator.IsGeolocationEnabled)
			{
				Status = "Location disabled";
				return;
			}

			try
			{
				Position p = await this.geolocator.GetPositionAsync (10000);
				CurrentPosition = p;
			}
			catch (GeolocationException ex)
			{
				Status = "Error: (" + ex.Error + ") " + ex.Message;
			}
			catch (TaskCanceledException)
			{
				Status = "Canceled";
			}
		}

		private void ToggleListeningHandler (object o)
		{
			if (!this.geolocator.IsGeolocationEnabled)
			{
				Status = "Location disabled";
				return;
			}

			if (!this.geolocator.IsListening)
			{
				Status = "Listening";
				this.geolocator.StartListening (0, 0);
			}
			else
			{
				Status = "Stopped listening";
				this.geolocator.StopListening();
			}

			OnPropertyChanged ("Status");
		}

		private void GeolocatorOnPositionError (object sender, PositionErrorEventArgs e)
		{
			Status = e.Error.ToString();
		}

		private void GeolocatorOnPositionChanged (object sender, PositionEventArgs e)
		{
			CurrentPosition = e.Position;
		}

		private void OnPropertyChanged (string name)
		{
			this.dispatcher.RunAsync (CoreDispatcherPriority.Normal, () =>
			{
				var changed = PropertyChanged;
				if (changed != null)
					changed (this, new PropertyChangedEventArgs (name));
			});
		}
	}
}