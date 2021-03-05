var Q = {};
Q._hexDigits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'];
Q._image_load_queue = [];
Q._loading_currently = null;
Q._images = {};
Q._functions = {};

Q._useFrameRate = 60;

Q.setFrameRate = function(fps) {
	Q._useFrameRate = fps;
};

Q.setUpdateMethod = function(pointer) {
	Q._functions['updater'] = pointer;
};

Q.setRenderMethod = function(pointer) {
	Q._functions['renderer'] = pointer;
};

Q.setMainMethod = function(pointer) {
	Q._functions['main'] = pointer;
};

Q.setKeyDown = function(pointer) {
	Q._functions['keydown'] = pointer;
};

Q.setKeyUp = function(pointer) {
	Q._functions['keyup'] = pointer;
};

Q.setMouseDown = function(pointer) {
	Q._functions['mousedown'] = pointer;
};

Q.setMouseUp = function(pointer) {
	Q._functions['mouseup'] = pointer;
};

Q.setMouseMove = function(pointer) {
	Q._functions['mousemove'] = pointer;
};

Q.setMouseDrag = function(pointer) {
	Q._functions['mousedrag'] = pointer;
};

Q.setMouseDoubleClick = function(pointer) {
	Q._functions['mousedoubleclick'] = pointer;
};

Q.setMainMethod(null);
Q.setMouseDown(null);
Q.setMouseUp(null);
Q.setMouseMove(null);
Q.setMouseDrag(null);
Q.setMouseDoubleClick(null);
Q.setKeyDown(null);
Q.setKeyUp(null);

Q.setHostDiv = function(hostId) {
	var div = document.getElementById(hostId);
	div.innerHTML = 
		'<canvas id="screen"></canvas>' +
		'<div style="display:none;">' +
			'<img id="image_loader" onload="Q._finish_loading()" crossOrigin="anonymous" />' +
			'<div id="image_store"></div>' +
			'<div id="temp_image"></div>' +
		'</div>' +
		'<div style="font-family:&quot;Courier New&quot;; font-size:11px;" id="_q_debug_output"></div>';
};

Q._htmlspecialchars = function(value) {
	var output = [];
	var c;
	for (var i = 0; i < value.length; ++i) {
		c = value.charAt(i);
		if (c == '<') { c = '&lt;'; }
		else if (c == '>') { c = '&gt;'; }
		else if (c == '&') { c = '&amp;'; }
		else if (c == '"') { c = '&quot;'; }
		else if (c == '\n') { c = '<br />'; }
		else if (c == '\t') { c = '&nbsp;&nbsp;&nbsp;&nbsp;'; }
		else if (c == ' ') { c = '&nbsp;'; }
		
		output.push(c);
	}
	
	return output.join('');
}

Q._printedLines = [];
Q.print = function(value) {
	var i;
	Q._printedLines.push('' + value);
	if (Q._printedLines.length > 20) {
		var newLines = [];
		for (i = 20; i > 0; --i) {
			newLines.push(Q._printedLines[Q._printedLines.length - i]);
		}
		Q._printedLines = newLines;
	}
	
	
	var output = '';
	for (i = 0; i < Q._printedLines.length; ++i) {
		output += Q._htmlspecialchars(Q._printedLines[i] + '\n');
	}
	
	document.getElementById('_q_debug_output').innerHTML = output;
}

Q.loadImage = function(key, path) {
	Q._image_load_queue.push([key, path]);
};

Q.toHex = function(r, g, b) {
	var hd = Q._hexDigits;
	return '#'
		+ hd[r >> 4] + hd[r & 15]
		+ hd[g >> 4] + hd[g & 15]
		+ hd[b >> 4] + hd[b & 15];
};

Q.begin = function(width, height, color) {
	Q._game_width = width;
	Q._game_height = height;
	Q._game_bgcolor = color;
	Q._load_images();
};

Q._load_images = function() {
	if (Q._image_load_queue.length > 0) {
		image = Q._image_load_queue[Q._image_load_queue.length - 1];
		Q._loading_currently = image;
		Q._image_load_queue.length -= 1;
		document.getElementById('image_loader').src = image[1];
	} else {
		Q._load_complete();
	}
};

Q._finish_loading = function() {
	var index = Q._image_load_queue.length;
	document.getElementById('image_store').innerHTML += 
		'<canvas id="image_store_child_' + index + '"></canvas>';
	var loader = document.getElementById('image_loader');
	var canvas = document.getElementById('image_store_child_' + index);
	
	canvas.width = loader.width;
	canvas.height = loader.height
	var context = canvas.getContext('2d');
	context.drawImage(loader, 0, 0)
	
	Q._images[Q._loading_currently[0]] = new Q.Image(canvas);
	Q._load_images();
};

Q._load_complete = function() {
	var screenCanvas = document.getElementById('screen');
	screenCanvas.width = Q._game_width;
	screenCanvas.height = Q._game_height;
	
	screenCanvas.addEventListener('mousedown', Q._mousedown);
	screenCanvas.addEventListener('mouseup', Q._mouseup);
	screenCanvas.addEventListener('mousemove', Q._mousemove);
	
	document.onkeydown = Q._keydown;
	document.onkeyup = Q._keyup;
	
	var bg = Q._game_bgcolor;
	Q.screen = new Q.Image(screenCanvas);
	Q.screen.fill(bg[0], bg[1], bg[2]);
	Q._functions['main']();
	if (Q._useFrameRate !== null) {
		Q._doTick();
	}
};

Q._doTick = function() {
	
	var start = (new Date()).getTime();
	Q._functions['updater']();
	Q._functions['renderer'](Q.screen);
	
	var end = (new Date()).getTime();
	var diff = end - start;
	var delay = diff - Math.floor(1000.0 / Q._useFrameRate);
	if (delay < 0) {
		delay = 0;
	}
	window.setTimeout('Q._doTick()', delay);
};

Q._scaleFactor = 1;

Q.setScaleFactor = function(factor) {
	Q._scaleFactor = factor;
}

Q._keydown = function(ev) {
	var key = Q._getKeyCode(ev);
	if (key != null) {
		var fp = Q._functions['keydown'];
		if (fp) {
			fp(key);
		}
		Q._isKeyPressed[key] = true;
	}
};

Q._keyup = function(ev) {
	var key = Q._getKeyCode(ev);
	if (key != null) {
		var fp = Q._functions['keyup'];
		if (fp) {
			fp(key);
		}
		Q._isKeyPressed[key] = false;
	}
};

Q.isKeyPressed = function(key) {
	var output = Q._isKeyPressed[key];
	if (output === undefined) return false;
	return output;
};

Q._keyCodeLookup = {
	'k13': 'enter',
	'k16': 'shift', 'k17': 'ctrl', 'k18': 'alt',
	'k32': 'space',
	
	'k48': '0', 'k49': '1', 'k50': '2', 'k51': '3', 'k52': '4',
	'k53': '5', 'k54': '6', 'k55': '7', 'k56': '8', 'k57': '9',
	
	'k65': 'a', 'k66': 'b', 'k67': 'c', 'k68': 'd', 'k69': 'e',
	'k70': 'f', 'k71': 'g', 'k72': 'h', 'k73': 'i', 'k74': 'j',
	'k75': 'k', 'k76': 'l', 'k77': 'm', 'k78': 'n', 'k79': 'o',
	'k80': 'p', 'k81': 'q', 'k82': 'r', 'k83': 's', 'k84': 't',
	'k85': 'u', 'k86': 'v', 'k87': 'w', 'k88': 'x', 'k89': 'y',
	'k90': 'z',
	
	'k37': 'left', 'k38': 'up', 'k39': 'right', 'k40': 'down',
	
	'k187': '=',
	'k189': '-'
};

Q._isKeyPressed = {};

Q._getKeyCode = function(ev) {
	var keyCode = ev.which ? ev.which : ev.keyCode;
	var output = Q._keyCodeLookup['k' + keyCode];
	return output === undefined ? null : output;
};

Q._last_mouse_down_loc = [0, 0];
Q._last_mouse_down_time = 0;
Q._last_click_was_double = false;

Q._mousedown = function (ev) {
	var pos = Q._mouse_get_pos_from_event(ev);
	Q._mouse_last_x = pos[0];
	Q._mouse_last_y = pos[1];
	Q._is_mouse_down = true;

	var fp = Q._functions['mousedown'];
	if (fp != null) {
		var rightclick = false;
		if (!ev) ev = window.event;
		if (ev.which) rightclick = (ev.which == 3);
		else if (ev.button) rightclick = (ev.button == 2);
		fp(pos[0], pos[1], !rightclick);
	}

	var time = (new Date()).getTime();
	var diff = time - Q._last_mouse_down_time;
	if (Q._last_click_was_double) {
		Q._last_click_was_double = false;
	} else {
		if (diff < 250) {
			var ppos = Q._last_mouse_down_loc;
			var dx = pos[0] - ppos[0];
			var dy = pos[1] - ppos[1];
			if (dx * dx + dy * dy < 100) { // within 10 pixels
				fp = Q._functions['mousedoubleclick'];
				if (fp != null) {
					fp(pos[0], pos[1]);
					Q._last_click_was_double = true;
				}
			}
		}
	}
	Q._last_mouse_down_time = time;
	Q._last_mouse_down_loc = [pos[0], pos[1]];
};

Q._mouseup = function(ev) {
	var pos = Q._mouse_get_pos_from_event(ev);
	Q._mouse_last_x = pos[0];
	Q._mouse_last_y = pos[1];
	Q._is_mouse_down = false;
	
	var fp = Q._functions['mouseup'];
	if (fp != null) {
		var rightclick = false;
		if (!ev) ev = window.event;
		if (ev.which) rightclick = (ev.which == 3);
		else if (ev.button) rightclick = (ev.button == 2);
		fp(pos[0], pos[1], !rightclick);
	}
};

Q._mousemove = function(ev) {
	var orig_x = Q._mouse_last_x;
	var orig_y = Q._mouse_last_y;
	var pos = Q._mouse_get_pos_from_event(ev);
	Q._mouse_last_x = pos[0];
	Q._mouse_last_y = pos[1];
	
	var fp = Q._functions['mousemove'];
	if (fp != null) {
		fp(orig_x, orig_y, pos[0], pos[1]);
	}
	
	if (Q._is_mouse_down) {
		fp = Q._functions['mousedrag'];
		if (fp != null) {
			fp(orig_x, orig_y, pos[0] - orig_x, pos[1] - orig_y);
		}
	}
};

Q._is_mouse_down = false;
Q._mouse_last_x = 0;
Q._mouse_last_y = 0;

Q._mouse_get_pos_from_event = function (ev) {
	var obj = Q.screen.canvas;
	var obj_left = 0;
	var obj_top = 0;
	var xpos;
	var ypos;
	while (obj.offsetParent) {
		obj_left += obj.offsetLeft;
		obj_top += obj.offsetTop;
		obj = obj.offsetParent;
	}
	if (ev) {
		//FireFox
		xpos = ev.pageX;
		ypos = ev.pageY;
	} else {
		//IE
		xpos = window.event.x + document.body.scrollLeft - 2;
		ypos = window.event.y + document.body.scrollTop - 2;
	}
	xpos -= obj_left;
	ypos -= obj_top;
	return [xpos, ypos];
};

Q.getImage = function(name) {
	var output = Q._images[name];
	if (output === undefined) return null;
	return output;
}

Q.setImage = function(name, image) {
	Q._images[name] = image;
}

Q.Image = function() {
	this.imageData = null;
	if (arguments.length == 2) {
		this.width = arguments[0];
		this.height = arguments[1];
		var temp_image = document.getElementById('temp_image');
		temp_image.innerHTML = '<canvas id="temp_image_child"></canvas>';
		this.canvas = document.getElementById('temp_image_child');
		this.canvas.width = this.width;
		this.canvas.height = this.height;
		temp_image.innerHTML = '';
	} else if (arguments.length == 1) {
		this.canvas = arguments[0];
		this.width = this.canvas.width;
		this.height = this.canvas.height;
	}
	this.context = this.canvas.getContext('2d');
};

Q.Image.prototype.blit = function(image, x, y) {
	this.context.drawImage(image.canvas, x, y);
};

Q.Image.prototype.fill = function(r, g, b) {
	this.context.fillStyle = Q.toHex(r, g, b);
	this.context.fillRect(0, 0, this.width, this.height);
}

Q.Image.prototype.beginPixelEditing = function() {
	this.imageData = this.context.getImageData(0, 0, this.width, this.height);
};

Q.Image.prototype.endPixelEditing = function() {
	this.context.putImageData(this.imageData, 0, 0);
	this.imageData = null;
};

Q.Image.prototype.setPixel = function(x, y, r, g, b, a) {
	var index = (x + y * this.width) * 4
	this.imageData.data[index] = r;
	this.imageData.data[index + 1] = g;
	this.imageData.data[index + 2] = b;
	this.imageData.data[index + 3] = a;
};

Q.Image.prototype.swapColor = function(colorA, colorB) {
	this.beginPixelEditing();
	
	var pixelCount = this.width * this.height;
	
	var oldR = colorA[0];
	var oldG = colorA[1];
	var oldB = colorA[2];
	var oldA = colorA[3];
	
	var newR = colorB[0];
	var newG = colorB[1];
	var newB = colorB[2];
	var newA = colorB[3];
	
	var totalBytes = pixelCount * 4;
	for (var i = 0; i < totalBytes; i += 4) {
		if (this.imageData.data[i] == oldR &&
			this.imageData.data[i + 1] == oldG &&
			this.imageData.data[i + 2] == oldB &&
			this.imageData.data[i + 3] == oldA) {
			this.imageData.data[i] = newR;
			this.imageData.data[i + 1] = newG;
			this.imageData.data[i + 2] = newB;
			this.imageData.data[i + 3] = newA;
		}
	}
	
	this.endPixelEditing();
};

J = {};

J.sm_Time__currentFloat = function() {
	return Date.now() / 1000;
};

J.sm_Time__currentInt = function() {
	return Math.floor(Date.now() / 1000);
};

J.Time = function() {
	var _jsThis = this;
};

J.sm_Math__randomFloat = function() {
	return 0;
};

J.sm_Math__randomInt = function(exclusiveMax) {
	return 0;
};

J.sm_Math__floor = function(value) {
	return 0;
};

J.sm_Math__parseInt = function(value) {
	return 0;
};

J.sm_Math__parseFloat = function(value) {
	return 0;
};

J.Math = function() {
	var _jsThis = this;
};

J.sm_PJImage__loadFromFile = function(path) {
	return new J.PJImage(path, 0, 0)
};

J.sm_PJImage__createBlank = function(width, height) {
	return new J.PJImage(null, width, height);
};

J.PJImage = function(filepath, width, height) {
	var _jsThis = this;
	this.blit = function(img, x, y) {
		this.qImage.blit(img.qImage, x, y);
	};

	this.fill = function(red, green, blue) {
		this.qImage.fill(red, green, blue);
	};

	this.width = 0;
	this.height = 0;
	if (filepath != null) {
		var original = Q.getImage(filepath);
		this.qImage = new Q.Image(original.width, original.height);
		this.qImage.blit(original, 0, 0);
		this.width = this.qImage.width;
		this.height = this.qImage.height;
	} else {
		this.qImage = new Q.Image(width, height);
		this.width = width;
		this.height = height;
	}
};

J.sm_PJ__print = function(value) {
	Q.print(value);
};

J.PJ = function() {
	var _jsThis = this;
};

J.sm_PJEvent__convertKeyCode = function(rawKeyCode) {
	if (J.sh_PJEvent.keyCodeMapping == null) {
		J.sh_PJEvent.keyCodeMapping = { "enter": "enter", "space": "space", "ctrl": "ctrl", "up": "up", "down": "down", "left": "left", "right": "right" };
	}
	var output = J.sh_PJEvent.keyCodeMapping[rawKeyCode];
	return output === undefined ? null : output;
};

J.sh_PJEvent = {};
J.sh_PJEvent.keyCodeMapping = null;

J.PJEvent = function(type, keycode, x, y, isPrimary) {
	var _jsThis = this;
	this.type = null;
	this.keycode = null;
	this.x = 0;
	this.y = 0;
	this.isPrimary = false;
	this.keyCodeMapping = null;
	_jsThis.type = type;
	_jsThis.keycode = keycode;
	_jsThis.x = x;
	_jsThis.y = y;
	_jsThis.isPrimary = isPrimary;
};

J.PJGame = function(postInit, updater, renderer, screenWidth, screenHeight, targetFps) {
	var _jsThis = this;
	this.start = function() {
		Q.loadImage("image_sheet.png", "image_sheet.png");
		Q.setHostDiv('pj_host');
		var postInit = this.postInit;
		Q.setMainMethod(function() { postInit(); });
		Q.setScaleFactor(1.0);
		Q.setFrameRate(this.fps);
		var eventQueue = [];
		var pressedKeys = {};
		J.sm_PJEvent__convertKeyCode('space');
		var game = this;
		Q.setKeyDown(function(key) {
			var code = J.sm_PJEvent__convertKeyCode(key);
			eventQueue.push(new J.PJEvent('keydown', code, 0, 0, false));
			pressedKeys[code] = true;
		});
		Q.setKeyUp(function(key) {
			var code = J.sm_PJEvent__convertKeyCode(key);
			eventQueue.push(new J.PJEvent('keyup', code, 0, 0, false));
			pressedKeys[code] = false;
		});
		Q.setMouseDown(function(x, y, isPrimary) {
			eventQueue.push(new J.PJEvent('mousedown', '', x, y, isPrimary));
		});
		Q.setMouseUp(function(x, y, isPrimary) {
			eventQueue.push(new J.PJEvent('mouseup', '', x, y, isPrimary));
		});
		Q.setMouseMove(function(startX, startY, finalX, finalY) {
			eventQueue.push(new J.PJEvent('mousemove', '', finalX, finalY, false));
		});
		var updater = this.updater;
		Q.setUpdateMethod(function() {
			updater(eventQueue, pressedKeys);
			eventQueue.length = 0;
		});
		var screenAsPjImg = null;
		var renderer = this.renderer;
		Q.setRenderMethod(function(screen) {
			if (screenAsPjImg == null) {
				screenAsPjImg = new J.PJImage(null, screen.width, screen.height);
				screenAsPjImg.qImage = screen;
			}
			screenAsPjImg.qImage.fill(0, 0, 0);
			renderer(screenAsPjImg);
		});
		Q.begin(this.width, this.height, [0, 0, 0]);
	};

	this.tick = function() {
		
	};

	this.postInit = postInit;
	this.updater = updater;
	this.renderer = renderer;
	this.width = screenWidth;
	this.height = screenHeight;
	this.fps = targetFps;
	this.quitAttempt = false;
};

J.sm_PJDraw__rectangle = function(image, r, g, b, x, y, width, height, border) {
	var context = image.qImage.context;
	context.fillStyle = Q.toHex(r, g, b);
	if (border == 0) {
		context.fillRect(x, y, width, height);
	} else {
		context.fillRect(x, y, 1, height);
		context.fillRect(x, y, width, 1);
		context.fillRect(x + width - 1, y, 1, height);
		context.fillRect(x, y + height - 1, width, 1);
	}
};

J.sm_PJDraw__ellipse = function(image, r, g, b, centerX, centerY, radiusX, radiusY, border) {
	radiusX = radiusX * 4 / 3;
	var context = image.qImage.context;
	context.beginPath();
	context.moveTo(centerX, centerY - radiusY);
	context.bezierCurveTo(
		centerX + radiusX, centerY - radiusY,
		centerX + radiusX, centerY + radiusY,
		centerX, centerY + radiusY);
	context.bezierCurveTo(
		centerX - radiusX, centerY + radiusY,
		centerX - radiusX, centerY - radiusY,
		centerX, centerY - radiusY);
	context.fillStyle = Q.toHex(r, g, b);
	context.fill();
	context.closePath();
};

J.sm_PJDraw__line = function(image, r, g, b, startX, startY, endX, endY, width) {
};

J.PJDraw = function() {
	var _jsThis = this;
};

J.sm_Program__createNumberGrid = function(width, height, startingValue) {
	var columns = [];
	var x = (width) - (1);
	while ((x) >= (0)) {
		var column = [];
		var y = (height) - (1);
		while ((y) >= (0)) {
			(column).push(startingValue);
			y = (y) - (1);
		}
		(columns).push(column);
		x = (x) - (1);
	}
	return columns;
};

J.sh_Program = {};
J.sh_Program.MaxMines = 99;

J.Program = function() {
	var _jsThis = this;
	this.init = function() {
		var game = new J.PJGame(_jsThis.postInit, _jsThis.update, _jsThis.render, 504, 323, 30);
		game.start();
	};

	this.makeNumberImage = function(sheet, sheetX, sheetY) {
		var output = J.sm_PJImage__createBlank(16, 16);
		output.blit(sheet, -(sheetX), -(sheetY));
		return output;
	};

	this.initializeBoard = function() {
		_jsThis.board = J.sm_Program__createNumberGrid(30, 16, 0);
		_jsThis.states = J.sm_Program__createNumberGrid(30, 16, 0);
		_jsThis.minesLeft = J.sh_Program.MaxMines;
		_jsThis.gameState = "play";
		_jsThis.timerStarted = false;
		var totalMines = _jsThis.minesLeft;
		var minesLaid = 0;
		var width = 30;
		var height = 16;
		while ((minesLaid) < (totalMines)) {
			var x = Math.floor(Math.random() * (width));
			var y = Math.floor(Math.random() * (height));
			if ((_jsThis.board[x][y]) == (0)) {
				_jsThis.board[x][y] = 10;
				minesLaid = (minesLaid) + (1);
			}
		}
		var y = (height) - (1);
		while ((y) >= (0)) {
			var x = (width) - (1);
			while ((x) >= (0)) {
				if ((_jsThis.board[x][y]) != (10)) {
					var surroundCount = 0;
					var dx = -(1);
					while ((dx) <= (1)) {
						var dy = -(1);
						while ((dy) <= (1)) {
							if (((dx) != (0)) || ((dy) != (0))) {
								var newX = (x) + (dx);
								var newY = (y) + (dy);
								if (((((newX) >= (0)) && ((newY) >= (0))) && ((newX) < (width))) && ((newY) < (height))) {
									if ((_jsThis.board[newX][newY]) == (10)) {
										surroundCount = (surroundCount) + (1);
									}
								}
							}
							dy = (dy) + (1);
						}
						dx = (dx) + (1);
					}
					_jsThis.board[x][y] = surroundCount;
				}
				x = (x) - (1);
			}
			y = (y) - (1);
		}
		_jsThis.startTime = J.sm_Time__currentInt();
	};

	this.postInit = function() {
		var sheet = J.sm_PJImage__loadFromFile("image_sheet.png");
		var left = 45;
		var top = 24;
		_jsThis.squareByNumber = [];
		var y = 0;
		while ((y) < (3)) {
			var x = 0;
			while ((x) < (3)) {
				(_jsThis.squareByNumber).push(_jsThis.makeNumberImage(sheet, (45) + ((x) * (17)), (24) + ((y) * (17))));
				x = (x) + (1);
			}
			y = (y) + (1);
		}
		_jsThis.mineImage = J.sm_PJImage__createBlank(16, 16);
		_jsThis.mineImage.blit(sheet, -(96), -(58));
		_jsThis.coveredImage = J.sm_PJImage__createBlank(16, 16);
		_jsThis.coveredImage.blit(sheet, -(28), -(24));
		_jsThis.flagImage = J.sm_PJImage__createBlank(16, 16);
		_jsThis.flagImage.blit(sheet, -(28), -(41));
		_jsThis.mineDetonated = J.sm_PJImage__createBlank(16, 16);
		_jsThis.mineDetonated.blit(sheet, -(96), -(41));
		_jsThis.mineWrong = J.sm_PJImage__createBlank(16, 16);
		_jsThis.mineWrong.blit(sheet, -(96), -(24));
		_jsThis.smileyButtonNormal = J.sm_PJImage__createBlank(26, 26);
		_jsThis.smileyButtonNormal.blit(sheet, 0, -(24));
		_jsThis.smileyButtonOoh = J.sm_PJImage__createBlank(26, 26);
		_jsThis.smileyButtonOoh.blit(sheet, -(27), -(78));
		_jsThis.smileyButtonDead = J.sm_PJImage__createBlank(26, 26);
		_jsThis.smileyButtonDead.blit(sheet, 0, -(78));
		_jsThis.smileyButtonShades = J.sm_PJImage__createBlank(26, 26);
		_jsThis.smileyButtonShades.blit(sheet, 0, -(51));
		_jsThis.uiNumbers = {};
		var i = 0;
		while ((i) <= (9)) {
			var img = J.sm_PJImage__createBlank(13, 23);
			img.blit(sheet, (-(i)) * (14), 0);
			_jsThis.uiNumbers[((i) ) + ("")] = img;
			i = (i) + (1);
		}
		_jsThis.initializeBoard();
	};

	this.update = function(events, pressedKeys) {
		var smileyLeft = 239;
		var smileyRight = (smileyLeft) + (26);
		var smileyTop = 15;
		var smileyBottom = (smileyTop) + (26);
		var i = 0;
		while ((i) < ((events).length)) {
			var e = events[i];
			var col = 0;
			var row = 0;
			var inRange = false;
			if ((((e.type) == ("mousedown")) || ((e.type) == ("mouseup"))) || ((e.type) == ("mousemove"))) {
				col = Math.floor(((e.x) - (12))  /  (16));
				row = Math.floor(((e.y) - (55))  /  (16));
				if (((((col) >= (0)) && ((col) < (30))) && ((row) >= (0))) && ((row) < (16))) {
					inRange = true;
				}
				_jsThis.overSmiley = ((((e.y) < (smileyBottom)) && ((e.x) >= (smileyLeft))) && ((e.x) < (smileyRight))) && ((e.y) >= (smileyTop));
			}
			if ((e.type) == ("mousedown")) {
				if ((_jsThis.overSmiley) && (e.isPrimary)) {
					_jsThis.smileyMouseHeld = true;
				}
				if (((_jsThis.gameState) == ("play")) && (inRange)) {
					if (e.isPrimary) {
						_jsThis.mouseHeld = true;
						_jsThis.mouseHeldX = col;
						_jsThis.mouseHeldY = row;
					} else if (_jsThis.mouseHeld) {
						_jsThis.mouseHeld = false;
					} else {
						_jsThis.markMine(col, row);
						_jsThis.ensureTimerStarted();
					}
				}
			} else if ((e.type) == ("mouseup")) {
				if (e.isPrimary) {
					if (_jsThis.smileyMouseHeld) {
						if (_jsThis.overSmiley) {
							_jsThis.initializeBoard();
						}
						_jsThis.smileyMouseHeld = false;
					}
					if ((((_jsThis.gameState) == ("play")) && (inRange)) && (_jsThis.mouseHeld)) {
						_jsThis.performClick(col, row);
						_jsThis.ensureTimerStarted();
					}
					_jsThis.mouseHeld = false;
				}
			} else if ((e.type) == ("mousemove")) {
				if (_jsThis.mouseHeld) {
					_jsThis.mouseHeldX = col;
					_jsThis.mouseHeldY = row;
				}
			}
			i = (i) + (1);
		}
	};

	this.ensureTimerStarted = function() {
		if (!(_jsThis.timerStarted)) {
			_jsThis.startTime = J.sm_Time__currentInt();
			_jsThis.timerStarted = true;
		}
	};

	this.performClick = function(col, row) {
		if ((_jsThis.states[col][row]) == (0)) {
			if ((_jsThis.board[col][row]) == (0)) {
				_jsThis.cascadingClick(col, row);
			} else if ((_jsThis.board[col][row]) < (9)) {
				_jsThis.states[col][row] = 1;
			} else if ((_jsThis.board[col][row]) == (10)) {
				_jsThis.states[col][row] = 4;
				_jsThis.setGameLose();
			}
		}
	};

	this.setGameLose = function() {
		_jsThis.gameState = "lose";
		_jsThis.finalTime = (J.sm_Time__currentInt()) - (_jsThis.startTime);
		var y = 0;
		while ((y) < (16)) {
			var x = 0;
			while ((x) < (30)) {
				var state = _jsThis.states[x][y];
				if ((state) == (0)) {
					_jsThis.states[x][y] = 1;
				} else if ((state) == (2)) {
					if ((_jsThis.board[x][y]) != (10)) {
						_jsThis.states[x][y] = 3;
					}
				}
				x = (x) + (1);
			}
			y = (y) + (1);
		}
	};

	this.cascadingClick = function(col, row) {
		var traversed = {};
		var traversedCoords = [[col, row]];
		var queue = [[col, row]];
		while (((queue).length) > (0)) {
			var current = queue[0];
			(queue).splice(0, 1);
			var neighbors = [];
			var dx = -(1);
			while ((dx) <= (1)) {
				var dy = -(1);
				while ((dy) <= (1)) {
					if (((dx) != (0)) || ((dy) != (0))) {
						var newx = (current[0]) + (dx);
						var newy = (current[1]) + (dy);
						if (((((newx) >= (0)) && ((newx) < (30))) && ((newy) >= (0))) && ((newy) < (16))) {
							(neighbors).push([(current[0]) + (dx), (current[1]) + (dy)]);
						}
					}
					dy = (dy) + (1);
				}
				dx = (dx) + (1);
			}
			var i = 0;
			while ((i) < ((neighbors).length)) {
				var neighbor = neighbors[i];
				var x = neighbor[0];
				var y = neighbor[1];
				var key = ((((neighbor[0]) ) + ("|")) ) + (neighbor[1]);
				if (!((traversed[key] !== undefined))) {
					traversed[key] = true;
					if (((_jsThis.states[x][y]) == (0)) && ((_jsThis.board[x][y]) < (9))) {
						(traversedCoords).push(neighbor);
						if ((_jsThis.board[x][y]) == (0)) {
							(queue).push(neighbor);
						}
					}
				}
				i = (i) + (1);
			}
		}
		var i = 0;
		while ((i) < ((traversedCoords).length)) {
			var coord = traversedCoords[i];
			var x = coord[0];
			var y = coord[1];
			_jsThis.states[x][y] = 1;
			i = (i) + (1);
		}
	};

	this.markMine = function(col, row) {
		if ((_jsThis.states[col][row]) == (0)) {
			_jsThis.states[col][row] = 2;
			_jsThis.minesLeft = (_jsThis.minesLeft) - (1);
		} else if ((_jsThis.states[col][row]) == (2)) {
			_jsThis.states[col][row] = 0;
			_jsThis.minesLeft = (_jsThis.minesLeft) + (1);
		}
	};

	this.drawBevelBox = function(screen, left, top, width, height, thickness, flipped) {
		if ((thickness) == (0)) {
			return null;
		}
		var color1 = 255;
		var color2 = 128;
		if (flipped) {
			color1 = 128;
			color2 = 255;
		}
		J.sm_PJDraw__rectangle(screen, color1, color1, color1, left, top, 1, (height) - (1), 0);
		J.sm_PJDraw__rectangle(screen, color1, color1, color1, left, top, (width) - (1), 1, 0);
		J.sm_PJDraw__rectangle(screen, color2, color2, color2, (left) + (1), ((top) + (height)) - (1), (width) - (1), 1, 0);
		J.sm_PJDraw__rectangle(screen, color2, color2, color2, ((left) + (width)) - (1), (top) + (1), 1, (height) - (1), 0);
		_jsThis.drawBevelBox(screen, (left) + (1), (top) + (1), (width) - (2), (height) - (2), (thickness) - (1), flipped);
	};

	this.getSmileyButton = function() {
		if ((_jsThis.gameState) == ("play")) {
			if (_jsThis.mouseHeld) {
				return _jsThis.smileyButtonOoh;
			}
			return _jsThis.smileyButtonNormal;
		} else if ((_jsThis.gameState) == ("lose")) {
			return _jsThis.smileyButtonDead;
		} else {
			return _jsThis.smileyButtonShades;
		}
	};

	this.renderNumber = function(screen, value, x, y) {
		if ((value) > (999)) {
			value = 999;
		} else if ((value) < (0)) {
			value = 0;
		}
		var valueString = (("") ) + (value);
		while (((valueString).length) < (3)) {
			valueString = (("0") ) + (valueString);
		}
		screen.blit(_jsThis.uiNumbers[valueString[0]], x, y);
		screen.blit(_jsThis.uiNumbers[valueString[1]], (x) + (13), y);
		screen.blit(_jsThis.uiNumbers[valueString[2]], (x) + (26), y);
	};

	this.render = function(screen) {
		screen.fill(192, 192, 192);
		_jsThis.drawBevelBox(screen, 0, 0, screen.width, screen.height, 3, false);
		_jsThis.drawBevelBox(screen, 9, 9, (screen.width) - (18), 37, 2, true);
		_jsThis.drawBevelBox(screen, 9, 52, (screen.width) - (18), ((screen.height) - (52)) - (9), 3, true);
		_jsThis.drawBevelBox(screen, 16, 15, 41, 25, 1, true);
		_jsThis.drawBevelBox(screen, 445, 15, 41, 25, 1, true);
		_jsThis.renderNumber(screen, _jsThis.minesLeft, 17, 16);
		var time = (J.sm_Time__currentInt()) - (_jsThis.startTime);
		if ((_jsThis.gameState) != ("play")) {
			time = _jsThis.finalTime;
		} else if (!(_jsThis.timerStarted)) {
			time = 0;
		}
		_jsThis.renderNumber(screen, time, 446, 16);
		var smiley = _jsThis.getSmileyButton();
		screen.blit(smiley, 239, 15);
		if ((_jsThis.smileyMouseHeld) && (_jsThis.overSmiley)) {
			_jsThis.drawBevelBox(screen, 240, 16, (smiley.width) - (2), (smiley.height) - (2), 2, true);
		}
		_jsThis.renderBoard(screen, 12, 55);
	};

	this.renderBoard = function(screen, left, top) {
		var width = (_jsThis.board).length;
		var height = (_jsThis.board[0]).length;
		var pixelX = 0;
		var pixelY = 0;
		var value = 0;
		var state = 0;
		_jsThis.clearedBoxes = 0;
		var y = 0;
		while ((y) < (height)) {
			var x = 0;
			while ((x) < (width)) {
				value = _jsThis.board[x][y];
				state = _jsThis.states[x][y];
				pixelX = (left) + ((x) * (16));
				pixelY = (top) + ((y) * (16));
				if ((state) == (0)) {
					if (((_jsThis.mouseHeld) && ((_jsThis.mouseHeldX) == (x))) && ((_jsThis.mouseHeldY) == (y))) {
						screen.blit(_jsThis.squareByNumber[0], pixelX, pixelY);
					} else {
						screen.blit(_jsThis.coveredImage, pixelX, pixelY);
					}
				} else if ((state) == (1)) {
					_jsThis.clearedBoxes = (_jsThis.clearedBoxes) + (1);
					if ((value) < (9)) {
						screen.blit(_jsThis.squareByNumber[_jsThis.board[x][y]], pixelX, pixelY);
					} else if ((value) == (10)) {
						screen.blit(_jsThis.mineImage, pixelX, pixelY);
					}
				} else if ((state) == (2)) {
					screen.blit(_jsThis.flagImage, pixelX, pixelY);
				} else if ((state) == (3)) {
					screen.blit(_jsThis.mineWrong, pixelX, pixelY);
				} else if ((state) == (4)) {
					screen.blit(_jsThis.mineDetonated, pixelX, pixelY);
				}
				x = (x) + (1);
			}
			y = (y) + (1);
		}
		if (((_jsThis.gameState) == ("play")) && ((_jsThis.clearedBoxes) == (((30) * (16)) - (J.sh_Program.MaxMines)))) {
			_jsThis.setStateWin();
		}
	};

	this.setStateWin = function() {
		_jsThis.gameState = "win";
		_jsThis.finalTime = (J.sm_Time__currentInt()) - (_jsThis.startTime);
		_jsThis.clearedBoxes = 0;
		var y = 0;
		while ((y) < (16)) {
			var x = 0;
			while ((x) < (30)) {
				if ((_jsThis.states[x][y]) == (0)) {
					_jsThis.states[x][y] = 1;
				}
				x = (x) + (1);
			}
			y = (y) + (1);
		}
	};

	this.MaxMines = 99;
	this.minesLeft = 0;
	this.mineImage = null;
	this.mineDetonated = null;
	this.mineWrong = null;
	this.flagImage = null;
	this.coveredImage = null;
	this.smileyButtonNormal = null;
	this.smileyButtonDead = null;
	this.smileyButtonShades = null;
	this.squareByNumber = null;
	this.timerStarted = false;
	this.board = null;
	this.states = null;
	this.gameState = null;
	this.mouseHeld = false;
	this.mouseHeldX = 0;
	this.mouseHeldY = 0;
	this.smileyMouseHeld = false;
	this.overSmiley = false;
	this.smileyButtonOoh = null;
	this.uiNumbers = null;
	this.startTime = 0;
	this.clearedBoxes = 0;
	this.finalTime = 0;
};


function setup() {
	var program = new J.Program();
	program.init();
}
