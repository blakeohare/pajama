import pygame
import math
import os
import random
import time

class PJ_StaticHost: pass

def sm_Time__currentFloat():
	return time.time()

def sm_Time__currentInt():
	return int(time.time())

class Time:
	def __init__(self):
		pass

def sm_Math__randomFloat():
	return 0

def sm_Math__randomInt(exclusiveMax):
	return 0

def sm_Math__floor(value):
	return 0

def sm_Math__parseInt(value):
	return 0

def sm_Math__parseFloat(value):
	return 0

class Math:
	def __init__(self):
		pass

def sm_PJImage__loadFromFile(path):
	return PJImage(path, 0, 0)

def sm_PJImage__createBlank(width, height):
	return PJImage(None, width, height)

class PJImage:
	def __init__(self, filepath, width, height):
		self.width = 0
		self.height = 0
		if filepath == None:
			self.width = width
			self.height = height
			self.pgImage = pygame.Surface((width, height))
		else:
			self.pgImage = pygame.image.load(filepath.replace("/", os.sep).replace("\\", os.sep))
			self.width = self.pgImage.get_width()
			self.height = self.pgImage.get_height()

	def blit(self, img, x, y):
		self.pgImage.blit(img.pgImage, (x, y))

	def fill(self, red, green, blue):
		self.pgImage.fill((red, green, blue))

def sm_PJ__print(value):
	print(value)

class PJ:
	def __init__(self):
		pass

def sm_PJEvent__convertKeyCode(rawKeyCode):
	if sh_PJEvent.keyCodeMapping == None:
		sh_PJEvent.keyCodeMapping = { pygame.K_SPACE: "space", pygame.K_RETURN: "enter", pygame.K_TAB: "tab", pygame.K_UP: "up", pygame.K_DOWN: "down", pygame.K_LEFT: "left", pygame.K_RIGHT: "right", pygame.K_LALT: "alt", pygame.K_RALT: "alt", pygame.K_LCTRL: "ctrl", pygame.K_RCTRL: "ctrl", pygame.K_LSHIFT: "shift", pygame.K_RSHIFT: "shift", pygame.K_a: "a", pygame.K_b: "b", pygame.K_c: "c", pygame.K_d: "d", pygame.K_e: "e", pygame.K_f: "f", pygame.K_g: "g", pygame.K_h: "h", pygame.K_i: "i", pygame.K_j: "j", pygame.K_k: "k", pygame.K_l: "l", pygame.K_m: "m", pygame.K_n: "n", pygame.K_o: "o", pygame.K_p: "p", pygame.K_q: "q", pygame.K_r: "r", pygame.K_s: "s", pygame.K_t: "t", pygame.K_u: "u", pygame.K_v: "v", pygame.K_w: "w", pygame.K_x: "x", pygame.K_y: "y", pygame.K_z: "z", pygame.K_0: "0", pygame.K_1: "1", pygame.K_2: "2", pygame.K_3: "3", pygame.K_4: "4", pygame.K_5: "5", pygame.K_6: "6", pygame.K_7: "7", pygame.K_8: "8", pygame.K_9: "9", pygame.K_F1: "F1", pygame.K_F2: "F2", pygame.K_F3: "F3", pygame.K_F4: "F4", pygame.K_F5: "F5", pygame.K_F6: "F6", pygame.K_F7: "F7", pygame.K_F8: "F8", pygame.K_F9: "F9", pygame.K_F10: "F10", pygame.K_F11: "F11", pygame.K_F12: "F12" }
	return sh_PJEvent.keyCodeMapping.get(rawKeyCode, None)

sh_PJEvent = PJ_StaticHost()
sh_PJEvent.keyCodeMapping = None

class PJEvent:
	def __init__(self, type, keycode, x, y, isPrimary):
		self.type = None
		self.keycode = None
		self.x = 0
		self.y = 0
		self.isPrimary = False
		self.keyCodeMapping = None
		self.type = type
		self.keycode = keycode
		self.x = x
		self.y = y
		self.isPrimary = isPrimary

class PJGame:
	def __init__(self, postInit, updater, renderer, screenWidth, screenHeight, targetFps):
		self.postInit = postInit
		self.updater = updater
		self.renderer = renderer
		self.width = screenWidth
		self.height = screenHeight
		self.fps = targetFps
		self.quitAttempt = False

	def start(self):
		pygame.init()
		screen = pygame.display.set_mode((self.width, self.height))
		self.postInit()
		self._screen = PJImage(None, self.width, self.height)
		self._screen.pgImage = screen
		
		sm_PJEvent__convertKeyCode(pygame.K_a)
		self.pressedKeys = {}
		for key in sh_PJEvent.keyCodeMapping.values():
			self.pressedKeys[key] = False
		
		while True:
			start = time.time()
			self.tick()
			if self.quitAttempt:
				return
			end = time.time()
			diff = end - start
			delay = 1.0 / self.fps - diff
			if delay > 0:
				time.sleep(delay)

	def tick(self):
		events = []
		for ev in pygame.event.get():
			myEvent = None
			if ev.type == pygame.QUIT:
				self.quitAttempt = True
			elif ev.type == pygame.KEYDOWN:
				keycode = sm_PJEvent__convertKeyCode(ev.key)
				myEvent = PJEvent("keydown", keycode, 0, 0, False)
				self.pressedKeys[keycode] = True
				if keycode == 'F4' and self.pressedKeys['alt']:
					self.quitAttempt = True
			elif ev.type == pygame.KEYUP:
				keycode = sm_PJEvent__convertKeyCode(ev.key)
				myEvent = PJEvent("keyup", keycode, 0, 0, False)
				self.pressedKeys[keycode] = False
			elif ev.type == pygame.MOUSEBUTTONDOWN:
				myEvent = PJEvent("mousedown", '', ev.pos[0], ev.pos[1], ev.button == 1)
			elif ev.type == pygame.MOUSEBUTTONUP:
				myEvent = PJEvent("mouseup", '', ev.pos[0], ev.pos[1], ev.button == 1)
			elif ev.type == pygame.MOUSEMOTION:
				myEvent = PJEvent("mousemove", '', ev.pos[0], ev.pos[1], False)
		
			if myEvent != None:
				events.append(myEvent)
		
		self.updater(events, self.pressedKeys)
		
		self._screen.pgImage.fill((0, 0, 0))
		self.renderer(self._screen)
		
		pygame.display.flip()
		

def sm_PJDraw__rectangle(image, r, g, b, x, y, width, height, border):
	if height == 1:
		pygame.draw.line(image.pgImage, (r, g, b), (x, y), (x + width - 1, y))
	else:
		pygame.draw.rect(image.pgImage, (r, g, b), pygame.Rect(x, y, width, height), border)

def sm_PJDraw__ellipse(image, r, g, b, centerX, centerY, radiusX, radiusY, border):
	pygame.draw.ellipse(image.pgImage, (r, g, b), pygame.Rect(centerX - radiusX, centerY - radiusY, radiusX * 2, radiusY * 2), border)

def sm_PJDraw__line(image, r, g, b, startX, startY, endX, endY, width):
	pass

class PJDraw:
	def __init__(self):
		pass

def sm_Program__createNumberGrid(width, height, startingValue):
	columns = []
	x = (width) - (1)
	while (x) >= (0):
		column = []
		y = (height) - (1)
		while (y) >= (0):
			(column).append(startingValue)
			y = (y) - (1)
		(columns).append(column)
		x = (x) - (1)
	return columns

sh_Program = PJ_StaticHost()
sh_Program.MaxMines = 99

class Program:
	def __init__(self):
		self.MaxMines = 99
		self.minesLeft = 0
		self.mineImage = None
		self.mineDetonated = None
		self.mineWrong = None
		self.flagImage = None
		self.coveredImage = None
		self.smileyButtonNormal = None
		self.smileyButtonDead = None
		self.smileyButtonShades = None
		self.squareByNumber = None
		self.timerStarted = False
		self.board = None
		self.states = None
		self.gameState = None
		self.mouseHeld = False
		self.mouseHeldX = 0
		self.mouseHeldY = 0
		self.smileyMouseHeld = False
		self.overSmiley = False
		self.smileyButtonOoh = None
		self.uiNumbers = None
		self.startTime = 0
		self.clearedBoxes = 0
		self.finalTime = 0

	def init(self):
		game = PJGame(self.postInit, self.update, self.render, 504, 323, 30)
		game.start()

	def makeNumberImage(self, sheet, sheetX, sheetY):
		output = sm_PJImage__createBlank(16, 16)
		output.blit(sheet, -(sheetX), -(sheetY))
		return output

	def initializeBoard(self):
		self.board = sm_Program__createNumberGrid(30, 16, 0)
		self.states = sm_Program__createNumberGrid(30, 16, 0)
		self.minesLeft = sh_Program.MaxMines
		self.gameState = "play"
		self.timerStarted = False
		totalMines = self.minesLeft
		minesLaid = 0
		width = 30
		height = 16
		while (minesLaid) < (totalMines):
			x = random.randint(0, (width) - 1)
			y = random.randint(0, (height) - 1)
			if (self.board[x][y]) == (0):
				self.board[x][y] = 10
				minesLaid = (minesLaid) + (1)
		y = (height) - (1)
		while (y) >= (0):
			x = (width) - (1)
			while (x) >= (0):
				if (self.board[x][y]) != (10):
					surroundCount = 0
					dx = -(1)
					while (dx) <= (1):
						dy = -(1)
						while (dy) <= (1):
							if ((dx) != (0)) or ((dy) != (0)):
								newX = (x) + (dx)
								newY = (y) + (dy)
								if ((((newX) >= (0)) and ((newY) >= (0))) and ((newX) < (width))) and ((newY) < (height)):
									if (self.board[newX][newY]) == (10):
										surroundCount = (surroundCount) + (1)
							dy = (dy) + (1)
						dx = (dx) + (1)
					self.board[x][y] = surroundCount
				x = (x) - (1)
			y = (y) - (1)
		self.startTime = sm_Time__currentInt()

	def postInit(self):
		sheet = sm_PJImage__loadFromFile("image_sheet.png")
		left = 45
		top = 24
		self.squareByNumber = []
		y = 0
		while (y) < (3):
			x = 0
			while (x) < (3):
				(self.squareByNumber).append(self.makeNumberImage(sheet, (45) + ((x) * (17)), (24) + ((y) * (17))))
				x = (x) + (1)
			y = (y) + (1)
		self.mineImage = sm_PJImage__createBlank(16, 16)
		self.mineImage.blit(sheet, -(96), -(58))
		self.coveredImage = sm_PJImage__createBlank(16, 16)
		self.coveredImage.blit(sheet, -(28), -(24))
		self.flagImage = sm_PJImage__createBlank(16, 16)
		self.flagImage.blit(sheet, -(28), -(41))
		self.mineDetonated = sm_PJImage__createBlank(16, 16)
		self.mineDetonated.blit(sheet, -(96), -(41))
		self.mineWrong = sm_PJImage__createBlank(16, 16)
		self.mineWrong.blit(sheet, -(96), -(24))
		self.smileyButtonNormal = sm_PJImage__createBlank(26, 26)
		self.smileyButtonNormal.blit(sheet, 0, -(24))
		self.smileyButtonOoh = sm_PJImage__createBlank(26, 26)
		self.smileyButtonOoh.blit(sheet, -(27), -(78))
		self.smileyButtonDead = sm_PJImage__createBlank(26, 26)
		self.smileyButtonDead.blit(sheet, 0, -(78))
		self.smileyButtonShades = sm_PJImage__createBlank(26, 26)
		self.smileyButtonShades.blit(sheet, 0, -(51))
		self.uiNumbers = {}
		i = 0
		while (i) <= (9):
			img = sm_PJImage__createBlank(13, 23)
			img.blit(sheet, (-(i)) * (14), 0)
			self.uiNumbers[str((i)) + ("")] = img
			i = (i) + (1)
		self.initializeBoard()

	def update(self, events, pressedKeys):
		smileyLeft = 239
		smileyRight = (smileyLeft) + (26)
		smileyTop = 15
		smileyBottom = (smileyTop) + (26)
		i = 0
		while (i) < (len(events)):
			e = events[i]
			col = 0
			row = 0
			inRange = False
			if (((e.type) == ("mousedown")) or ((e.type) == ("mouseup"))) or ((e.type) == ("mousemove")):
				col = ((e.x) - (12)) // (16)
				row = ((e.y) - (55)) // (16)
				if ((((col) >= (0)) and ((col) < (30))) and ((row) >= (0))) and ((row) < (16)):
					inRange = True
				self.overSmiley = ((((e.y) < (smileyBottom)) and ((e.x) >= (smileyLeft))) and ((e.x) < (smileyRight))) and ((e.y) >= (smileyTop))
			if (e.type) == ("mousedown"):
				if (self.overSmiley) and (e.isPrimary):
					self.smileyMouseHeld = True
				if ((self.gameState) == ("play")) and (inRange):
					if e.isPrimary:
						self.mouseHeld = True
						self.mouseHeldX = col
						self.mouseHeldY = row
					elif self.mouseHeld:
						self.mouseHeld = False
					else:
						self.markMine(col, row)
						self.ensureTimerStarted()
			elif (e.type) == ("mouseup"):
				if e.isPrimary:
					if self.smileyMouseHeld:
						if self.overSmiley:
							self.initializeBoard()
						self.smileyMouseHeld = False
					if (((self.gameState) == ("play")) and (inRange)) and (self.mouseHeld):
						self.performClick(col, row)
						self.ensureTimerStarted()
					self.mouseHeld = False
			elif (e.type) == ("mousemove"):
				if self.mouseHeld:
					self.mouseHeldX = col
					self.mouseHeldY = row
			i = (i) + (1)

	def ensureTimerStarted(self):
		if not (self.timerStarted):
			self.startTime = sm_Time__currentInt()
			self.timerStarted = True

	def performClick(self, col, row):
		if (self.states[col][row]) == (0):
			if (self.board[col][row]) == (0):
				self.cascadingClick(col, row)
			elif (self.board[col][row]) < (9):
				self.states[col][row] = 1
			elif (self.board[col][row]) == (10):
				self.states[col][row] = 4
				self.setGameLose()

	def setGameLose(self):
		self.gameState = "lose"
		self.finalTime = (sm_Time__currentInt()) - (self.startTime)
		y = 0
		while (y) < (16):
			x = 0
			while (x) < (30):
				state = self.states[x][y]
				if (state) == (0):
					self.states[x][y] = 1
				elif (state) == (2):
					if (self.board[x][y]) != (10):
						self.states[x][y] = 3
				x = (x) + (1)
			y = (y) + (1)

	def cascadingClick(self, col, row):
		traversed = {}
		traversedCoords = [[col, row]]
		queue = [[col, row]]
		while (len(queue)) > (0):
			current = queue[0]
			(queue).pop(0)
			neighbors = []
			dx = -(1)
			while (dx) <= (1):
				dy = -(1)
				while (dy) <= (1):
					if ((dx) != (0)) or ((dy) != (0)):
						newx = (current[0]) + (dx)
						newy = (current[1]) + (dy)
						if ((((newx) >= (0)) and ((newx) < (30))) and ((newy) >= (0))) and ((newy) < (16)):
							(neighbors).append([(current[0]) + (dx), (current[1]) + (dy)])
					dy = (dy) + (1)
				dx = (dx) + (1)
			i = 0
			while (i) < (len(neighbors)):
				neighbor = neighbors[i]
				x = neighbor[0]
				y = neighbor[1]
				key = (str((neighbor[0])) + ("|")) + str((neighbor[1]))
				if not (((key) in traversed)):
					traversed[key] = True
					if ((self.states[x][y]) == (0)) and ((self.board[x][y]) < (9)):
						(traversedCoords).append(neighbor)
						if (self.board[x][y]) == (0):
							(queue).append(neighbor)
				i = (i) + (1)
		i = 0
		while (i) < (len(traversedCoords)):
			coord = traversedCoords[i]
			x = coord[0]
			y = coord[1]
			self.states[x][y] = 1
			i = (i) + (1)

	def markMine(self, col, row):
		if (self.states[col][row]) == (0):
			self.states[col][row] = 2
			self.minesLeft = (self.minesLeft) - (1)
		elif (self.states[col][row]) == (2):
			self.states[col][row] = 0
			self.minesLeft = (self.minesLeft) + (1)

	def drawBevelBox(self, screen, left, top, width, height, thickness, flipped):
		if (thickness) == (0):
			return None
		color1 = 255
		color2 = 128
		if flipped:
			color1 = 128
			color2 = 255
		sm_PJDraw__rectangle(screen, color1, color1, color1, left, top, 1, (height) - (1), 0)
		sm_PJDraw__rectangle(screen, color1, color1, color1, left, top, (width) - (1), 1, 0)
		sm_PJDraw__rectangle(screen, color2, color2, color2, (left) + (1), ((top) + (height)) - (1), (width) - (1), 1, 0)
		sm_PJDraw__rectangle(screen, color2, color2, color2, ((left) + (width)) - (1), (top) + (1), 1, (height) - (1), 0)
		self.drawBevelBox(screen, (left) + (1), (top) + (1), (width) - (2), (height) - (2), (thickness) - (1), flipped)

	def getSmileyButton(self):
		if (self.gameState) == ("play"):
			if self.mouseHeld:
				return self.smileyButtonOoh
			return self.smileyButtonNormal
		elif (self.gameState) == ("lose"):
			return self.smileyButtonDead
		else:
			return self.smileyButtonShades

	def renderNumber(self, screen, value, x, y):
		if (value) > (999):
			value = 999
		elif (value) < (0):
			value = 0
		valueString = ("") + str((value))
		while (len(valueString)) < (3):
			valueString = ("0") + (valueString)
		screen.blit(self.uiNumbers[valueString[0]], x, y)
		screen.blit(self.uiNumbers[valueString[1]], (x) + (13), y)
		screen.blit(self.uiNumbers[valueString[2]], (x) + (26), y)

	def render(self, screen):
		screen.fill(192, 192, 192)
		self.drawBevelBox(screen, 0, 0, screen.width, screen.height, 3, False)
		self.drawBevelBox(screen, 9, 9, (screen.width) - (18), 37, 2, True)
		self.drawBevelBox(screen, 9, 52, (screen.width) - (18), ((screen.height) - (52)) - (9), 3, True)
		self.drawBevelBox(screen, 16, 15, 41, 25, 1, True)
		self.drawBevelBox(screen, 445, 15, 41, 25, 1, True)
		self.renderNumber(screen, self.minesLeft, 17, 16)
		time = (sm_Time__currentInt()) - (self.startTime)
		if (self.gameState) != ("play"):
			time = self.finalTime
		elif not (self.timerStarted):
			time = 0
		self.renderNumber(screen, time, 446, 16)
		smiley = self.getSmileyButton()
		screen.blit(smiley, 239, 15)
		if (self.smileyMouseHeld) and (self.overSmiley):
			self.drawBevelBox(screen, 240, 16, (smiley.width) - (2), (smiley.height) - (2), 2, True)
		self.renderBoard(screen, 12, 55)

	def renderBoard(self, screen, left, top):
		width = len(self.board)
		height = len(self.board[0])
		pixelX = 0
		pixelY = 0
		value = 0
		state = 0
		self.clearedBoxes = 0
		y = 0
		while (y) < (height):
			x = 0
			while (x) < (width):
				value = self.board[x][y]
				state = self.states[x][y]
				pixelX = (left) + ((x) * (16))
				pixelY = (top) + ((y) * (16))
				if (state) == (0):
					if ((self.mouseHeld) and ((self.mouseHeldX) == (x))) and ((self.mouseHeldY) == (y)):
						screen.blit(self.squareByNumber[0], pixelX, pixelY)
					else:
						screen.blit(self.coveredImage, pixelX, pixelY)
				elif (state) == (1):
					self.clearedBoxes = (self.clearedBoxes) + (1)
					if (value) < (9):
						screen.blit(self.squareByNumber[self.board[x][y]], pixelX, pixelY)
					elif (value) == (10):
						screen.blit(self.mineImage, pixelX, pixelY)
				elif (state) == (2):
					screen.blit(self.flagImage, pixelX, pixelY)
				elif (state) == (3):
					screen.blit(self.mineWrong, pixelX, pixelY)
				elif (state) == (4):
					screen.blit(self.mineDetonated, pixelX, pixelY)
				x = (x) + (1)
			y = (y) + (1)
		if ((self.gameState) == ("play")) and ((self.clearedBoxes) == (((30) * (16)) - (sh_Program.MaxMines))):
			self.setStateWin()

	def setStateWin(self):
		self.gameState = "win"
		self.finalTime = (sm_Time__currentInt()) - (self.startTime)
		self.clearedBoxes = 0
		y = 0
		while (y) < (16):
			x = 0
			while (x) < (30):
				if (self.states[x][y]) == (0):
					self.states[x][y] = 1
				x = (x) + (1)
			y = (y) + (1)


app = Program()
app.init()
