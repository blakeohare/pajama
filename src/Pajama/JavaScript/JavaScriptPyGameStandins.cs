using System.Collections.Generic;

namespace Pajama.JavaScript
{
	internal class JavaScriptPyGameStandins : PyGameStandins
	{
		public JavaScriptPyGameStandins(List<string> imagesToPreload)
			: base(imagesToPreload)
		{ }

		protected override void Serialize_PjEvent_convertKeyCode(string indent, List<string> buffer)
		{
			List<string> mapping = new List<string>();
			List<string> keys = new List<string>()
			{
				"enter|enter",
				"space|space",
				"ctrl|ctrl",
				"up|up",
				"down|down",
				"left|left",
				"right|right",
			};

			foreach (string key in keys)
			{
				string[] parts = key.Split('|');
				mapping.Add("\"" + parts[0] + "\": \"" + parts[1] + "\"");
			}
			// This might not actually be necessary. We shall see.
			this.TheseLines(indent, buffer,
				"if (J.sh_PJEvent.keyCodeMapping == null) {",
				"	J.sh_PJEvent.keyCodeMapping = { " + string.Join(", ", mapping) + " };",
				"}",
				"var output = J.sh_PJEvent.keyCodeMapping[rawKeyCode];",
				"return output === undefined ? null : output;"
				);
		}

		protected override void Serialize_PjImage_init(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"if (filepath != null) {",
				"	var original = Q.getImage(filepath);",
				"	this.qImage = new Q.Image(original.width, original.height);",
				"	this.qImage.blit(original, 0, 0);",
				"	this.width = this.qImage.width;",
				"	this.height = this.qImage.height;",
				"} else {",
				"	this.qImage = new Q.Image(width, height);",
				"	this.width = width;",
				"	this.height = height;",
				"}"
				);
		}

		protected override void Serialize_PjImage_blit(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"this.qImage.blit(img.qImage, x, y);");
		}

		protected override void Serialize_PjImage_loadFromFile(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"return new J.PJImage(path, 0, 0)");
		}

		protected override void Serialize_PjImage_createBlank(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"return new J.PJImage(null, width, height);");
		}

		protected override void Serialize_PjGame_init(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"this.postInit = postInit;",
				"this.updater = updater;",
				"this.renderer = renderer;",
				"this.width = screenWidth;",
				"this.height = screenHeight;",
				"this.fps = targetFps;",
				"this.quitAttempt = false;");
		}

		protected override void Serialize_PjGame_start(string indent, List<string> buffer, List<string> imagesToPreload)
		{
			foreach (string image in imagesToPreload)
			{
				string imageString = "\"" + image.Replace('\\', '/') + "\"";
				this.TheseLines(indent, buffer, "Q.loadImage(" + imageString + ", " + imageString + ");");
			}

			this.TheseLines(indent, buffer,
				"Q.setHostDiv('pj_host');",
				"var postInit = this.postInit;",
				"Q.setMainMethod(function() { postInit(); });",
				"Q.setScaleFactor(1.0);",
				"Q.setFrameRate(this.fps);",
				"var eventQueue = [];",
				"var pressedKeys = {};", // TODO: set all keys
				"J.sm_PJEvent__convertKeyCode('space');",

				"var game = this;",
				"Q.setKeyDown(function(key) {",
				"	var code = J.sm_PJEvent__convertKeyCode(key);",
				"	eventQueue.push(new J.PJEvent('keydown', code, 0, 0, false));",
				"	pressedKeys[code] = true;",
				"});",
				"Q.setKeyUp(function(key) {",
				"	var code = J.sm_PJEvent__convertKeyCode(key);",
				"	eventQueue.push(new J.PJEvent('keyup', code, 0, 0, false));",
				"	pressedKeys[code] = false;",
				"});",
				"Q.setMouseDown(function(x, y, isPrimary) {",
				"	eventQueue.push(new J.PJEvent('mousedown', '', x, y, isPrimary));",
				"});",
				"Q.setMouseUp(function(x, y, isPrimary) {",
				"	eventQueue.push(new J.PJEvent('mouseup', '', x, y, isPrimary));",
				"});",
				"Q.setMouseMove(function(startX, startY, finalX, finalY) {",
				"	eventQueue.push(new J.PJEvent('mousemove', '', finalX, finalY, false));",
				"});",
				"var updater = this.updater;",
				"Q.setUpdateMethod(function() {",
				"	updater(eventQueue, pressedKeys);",
				"	eventQueue.length = 0;",
				"});",
				"var screenAsPjImg = null;",
				"var renderer = this.renderer;",
				"Q.setRenderMethod(function(screen) {",
				"	if (screenAsPjImg == null) {",
				"		screenAsPjImg = new J.PJImage(null, screen.width, screen.height);",
				"		screenAsPjImg.qImage = screen;",
				"	}",
				"	screenAsPjImg.qImage.fill(0, 0, 0);",
				"	renderer(screenAsPjImg);",
				"});",
				"Q.begin(this.width, this.height, [0, 0, 0]);");
		}

		protected override void Serialize_PjGame_tick(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer, "");
		}


		// TODO: Move these drawing functions into game.js

		protected override void Serialize_PjDraw_rectangle(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"var context = image.qImage.context;",
				"context.fillStyle = Q.toHex(r, g, b);",
				"if (border == 0) {",
				"	context.fillRect(x, y, width, height);",
				"} else {",
				"	context.fillRect(x, y, 1, height);",
				"	context.fillRect(x, y, width, 1);",
				"	context.fillRect(x + width - 1, y, 1, height);",
				"	context.fillRect(x, y + height - 1, width, 1);",
				"}");
		}

		protected override void Serialize_PjDraw_ellipse(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"radiusX = radiusX * 4 / 3;", // no idea...
				"var context = image.qImage.context;",
				"context.beginPath();",
				"context.moveTo(centerX, centerY - radiusY);",
				"context.bezierCurveTo(",
				"	centerX + radiusX, centerY - radiusY,",
				"	centerX + radiusX, centerY + radiusY,",
				"	centerX, centerY + radiusY);",
				"context.bezierCurveTo(",
				"	centerX - radiusX, centerY + radiusY,",
				"	centerX - radiusX, centerY - radiusY,",
				"	centerX, centerY - radiusY);",
				"context.fillStyle = Q.toHex(r, g, b);",
				"context.fill();",
				"context.closePath();");
		}

		protected override void Serialize_PjDraw_line(string indent, List<string> buffer)
		{

		}

		protected override void Serialize_PjDraw_text(string indent, List<string> buffer)
		{

		}

		protected override void Serialize_PjImage_fill(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"this.qImage.fill(red, green, blue);");
		}

		protected override void Serialize_Time_currentFloat(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"return Date.now() / 1000;");
		}

		protected override void Serialize_Time_currentInt(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"return Math.floor(Date.now() / 1000);");
		}

		protected override void Serialize_Pj_print(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"Q.print(value);");
		}
	}
}
