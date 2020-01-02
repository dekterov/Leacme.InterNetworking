// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Humanizer.Bytes;

namespace Leacme.Lib.InterNetworking {

	public class Library {

		public Library() {

		}

		/// <summary>
		/// Get the current network interfaces for the machine, can be used with the PrunedNetInterface class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<NetworkInterface> GetNetworkInterfaces() {
			return NetworkInterface.GetAllNetworkInterfaces();
		}
	}

	/// <summary>
	/// A simplified model of the NetworkInterface class.
	/// /// </summary>
	public class PrunedNetInterface {
		public double ReceivedMB { get; } = 0;
		public double SentMB { get; } = 0;
		public string Name { get; } = "N/A";
		public string Id { get; } = "N/A";
		public string Description { get; } = "N/A";
		public NetworkInterfaceType Type { get; } = NetworkInterfaceType.Unknown;
		public OperationalStatus Status { get; } = OperationalStatus.Unknown;
		public double SpeedMbps { get; } = 0;
		public string MacAddress { get; } = "N/A";
		public IEnumerable<IPAddress> Address { get; } = new List<IPAddress>();
		public IEnumerable<IPAddress> Gateway { get; } = new List<IPAddress>();
		public IEnumerable<IPAddress> Dns { get; } = new List<IPAddress>();

		public PrunedNetInterface(NetworkInterface i) {
			var stats = i.GetIPStatistics();
			ReceivedMB = Math.Round(ByteSize.FromBytes(stats.BytesReceived).Megabytes, 2);
			SentMB = Math.Round(ByteSize.FromBytes(stats.BytesSent).Megabytes, 2);

			Name = i.Name;
			Id = i.Id;
			Description = i.Description;
			Type = i.NetworkInterfaceType;
			Status = i.OperationalStatus;

			try {
				SpeedMbps = Math.Round(i.Speed * 0.000001, 1);
			} catch {
				//
			}
			if (i.GetPhysicalAddress().ToString()?.Length.Equals(12) == true) {
				MacAddress = string.Join(":", Enumerable.Range(0, 6).Select(z => i.GetPhysicalAddress()?.ToString().Replace("-", "").Substring(z * 2, 2)));
			}

			var props = i.GetIPProperties();
			Address = props.UnicastAddresses.Select(z => z.Address);
			Gateway = props.GatewayAddresses.Select(z => z.Address);
			Dns = props.DnsAddresses;
		}
	}

}