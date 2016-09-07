const {app, BrowserWindow} = require('electron');
const fs = require('fs');
const path = require("path");

app.on('ready', () => {
	 let win = new BrowserWindow({width: 800, height: 600, frame: false});
	 
	 let hwnd = win.getNativeWindowHandle();
	 let hwndString = hwnd.readUInt32LE(0).toString(10); // this works only for 32 WPF process and 32 bit Electron process
	 
	 // write the hwnd so that the parent process can use it
	 let hwndFilePath = path.join(
		path.dirname(app.getPath("exe")),
		"../hwnd.txt");
	 fs.writeFileSync(hwndFilePath, hwndString);

	 // showing off for the lulz
	 win.loadURL("https://google.com");
});
