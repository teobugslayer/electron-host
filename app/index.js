const {app, BrowserWindow} = require('electron');
const fs = require('fs');
const path = require("path");

app.on('ready', () => {
	 let win = new BrowserWindow({width: 800, height: 600, frame: false});
	 
	 let hwnd = win.getNativeWindowHandle();
	 let hwndFilePath = path.join(
		path.dirname(app.getPath("exe")),
		"../hwnd.txt");
	 fs.writeFileSync(hwndFilePath, hwnd.readUInt32LE(0).toString(10));

	 win.loadURL("https://google.com");
});