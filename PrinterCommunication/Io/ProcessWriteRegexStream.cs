﻿/*
Copyright (c) 2017, Lars Brubaker
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MatterHackers.MatterControl.SlicerConfiguration;

namespace MatterHackers.MatterControl.PrinterCommunication.Io
{
	public class ProcessWriteRegexStream : GCodeStreamProxy
	{
		private Regex getQuotedParts = new Regex(@"([""'])(\\?.)*?\1", RegexOptions.Compiled);
		private string write_regex = "";
		private List<(Regex Regex, string Replacement)> WriteLineReplacements = new List<(Regex Regex, string Replacement)>();
		private List<string> multipleCommands = new List<string>();

		public ProcessWriteRegexStream(GCodeStream internalStream)
			: base(internalStream)
		{
		}

		public List<string> ProcessWriteRegEx(string lineToWrite)
		{
			lock (WriteLineReplacements)
			{
				if (write_regex != ActiveSliceSettings.Instance.GetValue(SettingsKey.write_regex))
				{
					WriteLineReplacements.Clear();
					string splitString = "\\n";
					write_regex = ActiveSliceSettings.Instance.GetValue(SettingsKey.write_regex);
					foreach (string regExLine in write_regex.Split(splitString.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
					{
						var matches = getQuotedParts.Matches(regExLine);
						if (matches.Count == 2)
						{
							var search = matches[0].Value.Substring(1, matches[0].Value.Length - 2);
							var replace = matches[1].Value.Substring(1, matches[1].Value.Length - 2);
							WriteLineReplacements.Add((new Regex(search, RegexOptions.Compiled), replace));
						}
					}
				}
			}

			var linesToWrite = new List<string>();
			linesToWrite.Add(lineToWrite);

			var addedLines = new List<string>();
			for (int i = 0; i < linesToWrite.Count; i++)
			{
				foreach (var item in WriteLineReplacements)
				{
					var replaced = item.Regex.Replace(lineToWrite, item.Replacement);
					if (replaced != lineToWrite)
					{
						var replacedLines = replaced.Split(',');
						linesToWrite[i] = replacedLines[0];
						for (int j = 1; j < replacedLines.Length; j++)
						{
							addedLines.Add(replacedLines[j]);
						}
					}
				}
			}

			linesToWrite.AddRange(addedLines);

			return linesToWrite;
		}

		public override string ReadLine()
		{
			if (multipleCommands.Count > 0)
			{
				var nextCommand = multipleCommands[0];
				multipleCommands.RemoveAt(0);
				return nextCommand;
			}

			var baseLine = base.ReadLine();

			if (baseLine == null)
			{
				// No more commands lets exit.
				return null;
			}

			var lines = ProcessWriteRegEx(baseLine);
			for (int i = 1; i < lines.Count; i++)
			{
				multipleCommands.Add(lines[i]);
			}

			return lines[0];
		}
	}
}