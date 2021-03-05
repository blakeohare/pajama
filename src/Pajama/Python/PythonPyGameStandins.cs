using System.Collections.Generic;

namespace Pajama.Python
{
	internal class PythonPyGameStandins : PyGameStandins
	{
		public PythonPyGameStandins()
			: base(new List<string>())
		{ }

		protected override void Serialize_PjEvent_convertKeyCode(string indent, List<string> buffer)
		{
			List<string> pygameKeys = new List<string> {
				"K_SPACE|space",
				"K_RETURN|enter",
				"K_TAB|tab",
				"K_UP|up",
				"K_DOWN|down",
				"K_LEFT|left",
				"K_RIGHT|right",
				"K_LALT|alt",
				"K_RALT|alt",
				"K_LCTRL|ctrl",
				"K_RCTRL|ctrl",
				"K_LSHIFT|shift",
				"K_RSHIFT|shift"
			};

			foreach (char c in "abcdefghijklmnopqrstuvwxyz")
			{
				pygameKeys.Add("K_" + c + "|" + c);
			}

			for (int i = 0; i < 10; ++i)
			{
				pygameKeys.Add("K_" + i + "|" + i);
			}

			for (int i = 1; i <= 12; ++i)
			{
				pygameKeys.Add("K_F" + i + "|F" + i);
			}

			List<string> mapping = new List<string>();
			foreach (string pygameKey in pygameKeys)
			{
				string[] parts = pygameKey.Split('|');
				mapping.Add("pygame." + parts[0] + ": \"" + parts[1] + "\"");
			}

			this.TheseLines(indent, buffer,
				"if sh_PJEvent.keyCodeMapping == None:",
				"	sh_PJEvent.keyCodeMapping = { " + string.Join(", ", mapping) + " }",
				"return sh_PJEvent.keyCodeMapping.get(rawKeyCode, None)"
				);
		}

		protected override void Serialize_PjImage_init(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"if filepath == None:",
				"	self.width = width",
				"	self.height = height",
				"	self.pgImage = pygame.Surface((width, height))",
				"else:",
				"	self.pgImage = pygame.image.load(filepath.replace(\"/\", os.sep).replace(\"\\\\\", os.sep))",
				"	self.width = self.pgImage.get_width()",
				"	self.height = self.pgImage.get_height()"
				);
		}

		protected override void Serialize_PjImage_blit(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"self.pgImage.blit(img.pgImage, (x, y))"
				);
		}

		protected override void Serialize_PjImage_loadFromFile(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"return PJImage(path, 0, 0)"
				);
		}

		protected override void Serialize_PjImage_createBlank(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"return PJImage(None, width, height)"
				);
		}

		protected override void Serialize_PjGame_init(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"self.postInit = postInit",
				"self.updater = updater",
				"self.renderer = renderer",
				"self.width = screenWidth",
				"self.height = screenHeight",
				"self.fps = targetFps",
				"self.quitAttempt = False");
		}

		protected override void Serialize_PjGame_start(string indent, List<string> buffer, List<string> imagesToPreload)
		{
			// preloading images is not necessary in Python

			this.TheseLines(indent, buffer,
				"pygame.init()",
				"screen = pygame.display.set_mode((self.width, self.height))",
				"self.postInit()",
				"self._screen = PJImage(None, self.width, self.height)",
				"self._screen.pgImage = screen",
				"",
				"sm_PJEvent__convertKeyCode(pygame.K_a)",
				"self.pressedKeys = {}",
				"for key in sh_PJEvent.keyCodeMapping.values():",
				"	self.pressedKeys[key] = False",
				"",
				"while True:",
				"	start = time.time()",
				"	self.tick()",
				"	if self.quitAttempt:",
				"		return",
				"	end = time.time()",
				"	diff = end - start",
				"	delay = 1.0 / self.fps - diff",
				"	if delay > 0:",
				"		time.sleep(delay)"
			);
		}

		protected override void Serialize_PjGame_tick(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"events = []",
				"for ev in pygame.event.get():",
				"	myEvent = None",
				"	if ev.type == pygame.QUIT:",
				"		self.quitAttempt = True",
				"	elif ev.type == pygame.KEYDOWN:",
				"		keycode = sm_PJEvent__convertKeyCode(ev.key)",
				"		myEvent = PJEvent(\"keydown\", keycode, 0, 0, False)",
				"		self.pressedKeys[keycode] = True",
				"		if keycode == 'F4' and self.pressedKeys['alt']:",
				"			self.quitAttempt = True",
				"	elif ev.type == pygame.KEYUP:",
				"		keycode = sm_PJEvent__convertKeyCode(ev.key)",
				"		myEvent = PJEvent(\"keyup\", keycode, 0, 0, False)",
				"		self.pressedKeys[keycode] = False",
				"	elif ev.type == pygame.MOUSEBUTTONDOWN:",
				"		myEvent = PJEvent(\"mousedown\", '', ev.pos[0], ev.pos[1], ev.button == 1)",
				"	elif ev.type == pygame.MOUSEBUTTONUP:",
				"		myEvent = PJEvent(\"mouseup\", '', ev.pos[0], ev.pos[1], ev.button == 1)",
				"	elif ev.type == pygame.MOUSEMOTION:",
				"		myEvent = PJEvent(\"mousemove\", '', ev.pos[0], ev.pos[1], False)",
				"",
				"	if myEvent != None:",
				"		events.append(myEvent)",
				"",
				"self.updater(events, self.pressedKeys)",
				"",
				"self._screen.pgImage.fill((0, 0, 0))",
				"self.renderer(self._screen)",
				"",
				"pygame.display.flip()",
				""
				);
		}

		protected override void Serialize_PjDraw_rectangle(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				// silly silly pygame.
				"if height == 1:",
				"	pygame.draw.line(image.pgImage, (r, g, b), (x, y), (x + width - 1, y))",
				"else:",
				"	pygame.draw.rect(image.pgImage, (r, g, b), pygame.Rect(x, y, width, height), border)");
		}

		protected override void Serialize_PjDraw_ellipse(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"pygame.draw.ellipse(image.pgImage, (r, g, b), pygame.Rect(centerX - radiusX, centerY - radiusY, radiusX * 2, radiusY * 2), border)");
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
				"self.pgImage.fill((red, green, blue))");
		}

		protected override void Serialize_Time_currentFloat(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"return time.time()");
		}

		protected override void Serialize_Time_currentInt(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"return int(time.time())");
		}

		protected override void Serialize_Pj_print(string indent, List<string> buffer)
		{
			this.TheseLines(indent, buffer,
				"print(value)");
		}
	}
}
