﻿/*
Copyright (c) 2017, Lars Brubaker, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System;
using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.PrinterCommunication;
using MatterHackers.SerialPortCommunication.FrostedSerial;

namespace MatterHackers.MatterControl.SlicerConfiguration
{
	public class ComPortField : UIField
	{
		private DropDownList dropdownList;

		public override void Initialize(int tabIndex)
		{
			EventHandler unregisterEvents = null;

			bool canChangeComPort = !PrinterConnection.Instance.PrinterIsConnected && PrinterConnection.Instance.CommunicationState != CommunicationStates.AttemptingToConnect;

			// The COM_PORT control is unique in its approach to the SlicerConfigName. It uses "com_port" settings name to
			// bind to a context that will place it in the SliceSetting view but it binds its values to a machine
			// specific dictionary key that is not exposed in the UI. At runtime we lookup and store to '<machinename>_com_port'
			// ensuring that a single printer can be shared across different devices and we'll select the correct com port in each case
			dropdownList = new DropDownList("None".Localize(), maxHeight: 200)
			{
				ToolTipText = this.HelpText,
				Margin = new BorderDouble(),
				TabIndex = tabIndex,
				Name = "Serial Port Dropdown",
				// Prevent droplist interaction when connected
				Enabled = canChangeComPort,
				TextColor = canChangeComPort ? ActiveTheme.Instance.PrimaryTextColor : new RGBA_Bytes(ActiveTheme.Instance.PrimaryTextColor, 150),
				BorderColor = canChangeComPort ? ActiveTheme.Instance.SecondaryTextColor : new RGBA_Bytes(ActiveTheme.Instance.SecondaryTextColor, 150),
			};

			dropdownList.Click += (s, e) =>
			{
				// TODO: why doesn't this blow up without runonidle?
				RebuildMenuItems();
			};

			RebuildMenuItems();

			// Prevent droplist interaction when connected
			PrinterConnection.Instance.CommunicationStateChanged.RegisterEvent((s, e) =>
			{
				canChangeComPort = !PrinterConnection.Instance.PrinterIsConnected && PrinterConnection.Instance.CommunicationState != CommunicationStates.AttemptingToConnect;
				dropdownList.Enabled = canChangeComPort;
				dropdownList.TextColor = canChangeComPort ? ActiveTheme.Instance.PrimaryTextColor : new RGBA_Bytes(ActiveTheme.Instance.PrimaryTextColor, 150);
				dropdownList.BorderColor = canChangeComPort ? ActiveTheme.Instance.SecondaryTextColor : new RGBA_Bytes(ActiveTheme.Instance.SecondaryTextColor, 150);
			}, ref unregisterEvents);

			// Release event listener on close
			dropdownList.Closed += (s, e) =>
			{
				unregisterEvents?.Invoke(null, null);
			};

			this.Content = dropdownList;
		}

		protected override void OnValueChanged(FieldChangedEventArgs fieldChangedEventArgs)
		{
			dropdownList.SelectedLabel = ActiveSliceSettings.Instance.Helpers.ComPort();
			base.OnValueChanged(fieldChangedEventArgs);
		}

		private void RebuildMenuItems()
		{
			dropdownList.MenuItems.Clear();

			string currentValue = ActiveSliceSettings.Instance.Helpers.ComPort();

			foreach (string listItem in FrostedSerialPort.GetPortNames())
			{
				// Add each serial port to the dropdown list 
				MenuItem newItem = dropdownList.AddItem(listItem);

				// Set control text
				if (newItem.Text == currentValue)
				{
					dropdownList.SelectedLabel = currentValue;
				}

				// When the given menu item is selected, save its value back into settings
				newItem.Selected += (sender, e) =>
				{
					if (sender is MenuItem menuItem)
					{
						this.SetValue(
							menuItem.Text,
							userInitiated: true);
					}
				};
			}
		}
	}
}
