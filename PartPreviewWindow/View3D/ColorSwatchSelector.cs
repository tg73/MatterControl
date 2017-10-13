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
using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.DataConverters3D;

namespace MatterHackers.MatterControl.PartPreviewWindow
{
	public class ColorSwatchSelector : FlowLayoutWidget
	{
		private TextImageButtonFactory menuButtonFactory;
		int colorSize = 32;

		public ColorSwatchSelector(IObject3D item, View3DWidget view3DWidget, TextImageButtonFactory menuButtonFactory)
			: base(FlowDirection.TopToBottom)
		{
			this.menuButtonFactory = menuButtonFactory;

			var colorCount = 9;
			double[] lightness = new double[] { .7, .5, .3 };
			RGBA_Bytes[] grayLevel = new RGBA_Bytes[] { RGBA_Bytes.White, new RGBA_Bytes(180, 180, 180), RGBA_Bytes.Gray };
			for (int rowIndex = 0; rowIndex < lightness.Length; rowIndex++)
			{
				var colorRow = new FlowLayoutWidget();
				AddChild(colorRow);

				for (int colorIndex = 0; colorIndex < colorCount; colorIndex++)
				{
					var color = RGBA_Floats.FromHSL(colorIndex / (double)colorCount, 1, lightness[rowIndex]).GetAsRGBA_Bytes();
					colorRow.AddChild(MakeColorButton(item, view3DWidget, color));
				}

				// put in white and black buttons
				colorRow.AddChild(MakeColorButton(item, view3DWidget, grayLevel[rowIndex]));
			}
		}

		private Button MakeColorButton(IObject3D item, View3DWidget view3DWidget, RGBA_Bytes color)
		{
			GuiWidget colorWidget;
			var button = new Button(colorWidget = new GuiWidget()
			{
				BackgroundColor = color,
				Width = colorSize,
				Height = colorSize,
			});

			button.Click += (s, e) =>
			{
				item.Color = colorWidget.BackgroundColor;
				view3DWidget.Invalidate();
			};
			return button;
		}
	}
}