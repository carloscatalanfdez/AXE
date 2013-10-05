#include "Game.h"

Game::Game(PolycodeView *view) {
	core = new Win32Core(view, 640,480,false, false, 0, 0,60);	  
	CoreServices::getInstance()->getResourceManager()->addArchive("default.pak");
	CoreServices::getInstance()->getResourceManager()->addDirResource("default", false);

	// Write your code here
}
Game::~Game() {
    
}

bool Game::Update() {
	return core->updateAndRender();
}